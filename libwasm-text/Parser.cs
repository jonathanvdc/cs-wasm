using System.Collections.Generic;
using Pixie;
using Pixie.Code;
using Pixie.Markup;

namespace Wasm.Text
{
    /// <summary>
    /// A parser for the WebAssembly text format.
    /// </summary>
    public sealed partial class Parser
    {
        /// <summary>
        /// Parses a sequence of tokens as S-expressions.
        /// </summary>
        /// <param name="tokens">The tokens to parse.</param>
        /// <param name="log">A log to send errors to.</param>
        /// <returns>A list of parsed S-expressions.</returns>
        public static IReadOnlyList<SExpression> ParseAsSExpressions(IEnumerable<Lexer.Token> tokens, ILog log)
        {
            using (var enumerator = tokens.GetEnumerator())
            {
                return ParseAsSExpressions(enumerator, log, false);
            }
        }

        /// <summary>
        /// Parses a sequence of tokens as S-expressions.
        /// </summary>
        /// <param name="tokens">The tokens to parse.</param>
        /// <param name="log">A log to send errors to.</param>
        /// <param name="isNested">Tells if this parsing action is a nested rather than a top-level action.</param>
        /// <returns>A list of parsed S-expressions.</returns>
        private static IReadOnlyList<SExpression> ParseAsSExpressions(IEnumerator<Lexer.Token> tokens, ILog log, bool isNested)
        {
            var results = new List<SExpression>();
            while (tokens.MoveNext())
            {
                var token = tokens.Current;
                if (token.Kind == Lexer.TokenKind.LeftParenthesis)
                {
                    if (tokens.MoveNext())
                    {
                        var head = tokens.Current;
                        if (head.Kind != Lexer.TokenKind.Keyword)
                        {
                            log.Log(
                                new LogEntry(
                                    Severity.Error,
                                    "expected a keyword",
                                    "all S-expressions should begin with a keyword, but this one doesn't.",
                                    new HighlightedSource(new SourceRegion(head.Span))));
                        }
                        var tail = ParseAsSExpressions(tokens, log, true);
                        if (tokens.Current.Kind != Lexer.TokenKind.RightParenthesis)
                        {
                            log.Log(
                                new LogEntry(
                                    Severity.Error,
                                    "no closing parenthesis",
                                    "left parenthesis indicates the start of an S-expression, but that expression is never closed.",
                                    new HighlightedSource(new SourceRegion(token.Span))));
                        }
                        results.Add(SExpression.Create(head, tail));
                    }
                    else
                    {
                        log.Log(
                            new LogEntry(
                                Severity.Error,
                                "no closing parenthesis",
                                "left parenthesis indicates the start of an S-expression, but the file ends immediately after.",
                                new HighlightedSource(new SourceRegion(token.Span))));
                    }
                }
                else if (token.Kind == Lexer.TokenKind.RightParenthesis)
                {
                    if (!isNested)
                    {
                        log.Log(
                            new LogEntry(
                                Severity.Error,
                                "excess parenthesis",
                                "right parenthesis does not close a left parenthesis.",
                                new HighlightedSource(new SourceRegion(token.Span))));
                    }
                    break;
                }
                else
                {
                    results.Add(SExpression.Create(token));
                }
            }
            return results;
        }
    }
}
