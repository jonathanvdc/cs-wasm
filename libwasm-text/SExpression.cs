using System;
using System.Collections.Generic;

namespace Wasm.Text
{
    /// <summary>
    /// An S-expression: a data structure that is either a single token or a keyword
    /// token followed by a tail.
    /// </summary>
    public struct SExpression
    {
        internal static SExpression Create(Lexer.Token head, IReadOnlyList<SExpression> tail)
        {
            return new SExpression
            {
                IsCall = true,
                Head = head,
                Tail = tail
            };
        }

        internal static SExpression Create(Lexer.Token head)
        {
            return new SExpression
            {
                IsCall = false,
                Head = head,
                Tail = Array.Empty<SExpression>()
            };
        }

        /// <summary>
        /// Tests if this S-expression represents a call .
        /// </summary>
        public bool IsCall { get; private set; }

        /// <summary>
        /// Gets the keyword token that is the head of this S-expression if the S-expression is a call;
        /// otherwise, the token that corresponds to the S-expression itself.
        /// </summary>
        public Lexer.Token Head { get; private set; }

        /// <summary>
        /// Gets the S-expression's tail: a sequence of S-expressions that trail the S-expression's head.
        /// Note that this tail may be empty even for S-expressions that are calls.
        /// </summary>
        public IReadOnlyList<SExpression> Tail { get; private set; }

        /// <summary>
        /// Tests if this S-expression is a call to a keyword with a particular name.
        /// </summary>
        /// <param name="keyword">The keyword to check for.</param>
        /// <returns>
        /// <c>true</c> if the S-expression is a call to <paramref name="keyword"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool IsCallTo(string keyword)
        {
            return IsCall && Head.Kind == Lexer.TokenKind.Keyword && (string)Head.Value == keyword;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return IsCall ? Head.Span.Text : $"({Head.Span.Text} {string.Join(" ", Tail)})";
        }
    }
}
