using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using Pixie.Code;

namespace Wasm.Text
{
    /// <summary>
    /// A lexer for the WebAssembly text format.
    /// </summary>
    public sealed class Lexer
    {
        private Lexer(SourceDocument document, TextReader reader)
        {
            this.document = document;
            this.reader = reader;
            this.offset = 0;
            this.lookaheadBuffer = new List<char>();
        }

        private SourceDocument document;
        private TextReader reader;
        private int offset;
        private List<char> lookaheadBuffer;

        /// <summary>
        /// Tokenizes a string.
        /// </summary>
        /// <param name="document">A string to tokenize.</param>
        /// <param name="fileName">The name of the file in which the string is saved.</param>
        /// <returns>A tokenized string.</returns>
        public static IEnumerable<Token> Tokenize(string document, string fileName = "<string>")
        {
            return Tokenize(new StringDocument(fileName, document));
        }

        /// <summary>
        /// Tokenizes a source document.
        /// </summary>
        /// <param name="document">A source document to tokenize.</param>
        /// <returns>A tokenized source document.</returns>
        public static IEnumerable<Token> Tokenize(SourceDocument document)
        {
            using (var reader = document.Open(0))
            {
                var lexer = new Lexer(document, reader);
                Token token;
                while (lexer.TryReadToken(out token))
                {
                    yield return token;
                }
            }
        }

        /// <summary>
        /// Tries to read the next token from the stream.
        /// </summary>
        /// <param name="token">The next token.</param>
        /// <returns>
        /// <c>true</c> if a token was read; <c>false</c> if the stream is empty.
        /// </returns>
        private bool TryReadToken(out Token token)
        {
            SkipWhitespace();
            char firstChar;
            if (TryPeekChar(out firstChar))
            {
                if (firstChar == '(' || firstChar == ')')
                {
                    SkipChar();
                    token = new Token(
                        firstChar == '(' ? TokenKind.LeftParenthesis : TokenKind.RightParenthesis,
                        new SourceSpan(document, offset - 1, 1),
                        null);
                }
                else if (firstChar == '"')
                {
                    token = ReadStringToken();
                }
                else if (firstChar == '$')
                {
                    token = ReadIdentifierToken();
                }
                else
                {
                    token = ReadReservedToken(offset);
                }
                return true;
            }
            else
            {
                token = default(Token);
                return false;
            }
        }

        /// <summary>
        /// Skips as many whitespace characters as possible.
        /// </summary>
        /// <returns>
        /// <c>true</c> if at least one whitespace character was skipped; otherwise, <c>false</c>.
        /// </returns>
        private bool SkipWhitespace()
        {
            bool skippedAny = false;
            while (true)
            {
                char c;
                if (!TryPeekChar(out c))
                {
                    break;
                }

                if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
                {
                    SkipChar();
                    skippedAny = true;
                }
                else if (IsNext(";;"))
                {
                    // Line comments.
                    SkipChar();
                    SkipChar();
                    while (TryReadChar(out c) && c != '\n')
                    { }
                    skippedAny = true;
                }
                else if (IsNext("(;"))
                {
                    // Block comments.
                    SkipChar();
                    SkipChar();
                    int nest = 1;
                    while (nest > 0)
                    {
                        if (IsNext(";)"))
                        {
                            nest--;
                            SkipChars(2);
                        }
                        else if (IsNext("(;"))
                        {
                            nest++;
                            SkipChars(2);
                        }
                        else
                        {
                            SkipChar();
                        }
                    }
                    skippedAny = true;
                }
                else
                {
                    break;
                }
            }
            return skippedAny;
        }

        private bool SkipChar()
        {
            char c;
            return TryReadChar(out c);
        }

        private void SkipChars(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SkipChar();
            }
        }

        private bool IsNext(string expected)
        {
            string actual;
            return TryPeekString(expected.Length, out actual)
                && actual == expected;
        }

        private bool TryPeekString(int length, out string result)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                char c;
                if (TryPeekChar(i, out c))
                {
                    builder.Append(c);
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            result = builder.ToString();
            return true;
        }

        private bool TryPeekChar(int offset, out char result)
        {
            // Read characters from the text reader into the lookahead buffer
            // until we reach the character at 'offset'.
            for (int i = lookaheadBuffer.Count; i <= offset; i++)
            {
                char c;
                if (CheckReaderResult(reader.Read(), out c))
                {
                    lookaheadBuffer.Add(c);
                }
                else
                {
                    result = default(char);
                    return false;
                }
            }
            result = lookaheadBuffer[offset];
            return true;
        }

        private bool TryPeekChar(out char result)
        {
            return TryPeekChar(0, out result);
        }

        private bool Expect(char expected)
        {
            char c;
            if (TryPeekChar(out c) && c == expected)
            {
                SkipChar();
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool TryReadChar(out char result)
        {
            bool hasRead;
            if (lookaheadBuffer.Count > 0)
            {
                result = lookaheadBuffer[0];
                lookaheadBuffer.RemoveAt(0);
                hasRead = true;
            }
            else
            {
                hasRead = CheckReaderResult(reader.Read(), out result);
            }
            if (hasRead)
            {
                offset++;
            }
            return hasRead;
        }

        private bool CheckReaderResult(int c, out char result)
        {
            if (c <= 0)
            {
                result = default(char);
                return false;
            }
            else
            {
                result = (char)c;
                return true;
            }
        }

        private Token ReadIdentifierToken()
        {
            var spanStart = offset;
            if (!Expect('$'))
            {
                return ReadReservedToken(spanStart);
            }

            var builder = new StringBuilder();
            char c;
            while (TryReadIdentifierChar(out c))
            {
                builder.Append(c);
            }

            if (builder.Length == 0)
            {
                return ReadReservedToken(spanStart);
            }
            else
            {
                return new Token(
                    TokenKind.Identifier,
                    new SourceSpan(document, spanStart, offset - spanStart),
                    builder.ToString());
            }
        }

        private bool TryReadIdentifierChar(out char c)
        {
            if (TryPeekChar(out c))
            {
                bool isIdChar = IsIdentifierChar(c);
                if (isIdChar)
                {
                    SkipChar();
                }
                else
                {
                    c = '\0';
                }
                return isIdChar;
            }
            else
            {
                return false;
            }
        }

        private static bool IsIdentifierChar(char c)
        {
            return (c >= '0' && c <= '9')
                || (c >= 'A' && c <= 'Z')
                || (c >= 'a' && c <= 'z')
                || c == '!' || c == '#' || c == '$' || c == '%'
                || c == '&' || c == '\'' || c == '*' || c == '+'
                || c == '-' || c == '.' || c == '/' || c == ':'
                || c == '<' || c == '=' || c == '>' || c == '?'
                || c == '@' || c == '\\' || c == '^' || c == '_'
                || c == '`' || c == '|' || c == '~';
        }

        private Token ReadStringToken()
        {
            var spanStart = offset;
            if (!Expect('"'))
            {
                return ReadReservedToken(spanStart);
            }

            var builder = new StringBuilder();
            char c;
            while (TryReadChar(out c))
            {
                if (c == '"')
                {
                    return new Token(
                        TokenKind.String,
                        new SourceSpan(document, spanStart, offset - spanStart),
                        builder.ToString());
                }
                else if (c == '\\')
                {
                    if (!TryReadChar(out c))
                    {
                        return ReadReservedToken(spanStart);
                    }
                    switch (c)
                    {
                        case '\\':
                            builder.Append('\\');
                            break;
                        case 'n':
                            builder.Append('\n');
                            break;
                        case 'r':
                            builder.Append('\r');
                            break;
                        case 't':
                            builder.Append('\t');
                            break;
                        case '"':
                            builder.Append('"');
                            break;
                        case '\'':
                            builder.Append('\'');
                            break;
                        case 'u':
                            BigInteger num;
                            if (Expect('{') && TryReadHexNum(out num) && Expect('}'))
                            {
                                builder.Append(char.ConvertFromUtf32((int)num));
                                continue;
                            }
                            return ReadReservedToken(spanStart);
                        default:
                            int firstDigit, secondDigit;
                            if (TryReadHexDigit(out firstDigit) && TryReadHexDigit(out secondDigit))
                            {
                                builder.Append((char)(16 * firstDigit + secondDigit));
                                break;
                            }
                            else
                            {
                                return ReadReservedToken(spanStart);
                            }
                    }
                }
                else if (IsDisallowedInString(c))
                {
                    return ReadReservedToken(spanStart);
                }
                else
                {
                    builder.Append(c);
                }
            }
            return ReadReservedToken(spanStart);
        }

        private static bool IsDisallowedInString(char c)
        {
            return c < '\u0020' || c == '\u007f';
        }

        private Token ReadReservedToken(int start)
        {
            int count = offset - start;
            while (!SkipWhitespace())
            {
                char c;
                if (TryReadChar(out c))
                {
                    if (c == ')' || c == '(' || c == '"')
                    {
                        break;
                    }
                    count++;
                }
                else
                {
                    break;
                }
            }
            return new Token(TokenKind.Reserved, new SourceSpan(document, start, count), null);
        }

        private bool TryReadHexNum(out BigInteger num)
        {
            bool parsed = false;
            var acc = BigInteger.Zero;
            char c;
            while (TryPeekChar(out c))
            {
                int digit;
                if (c == '_')
                {
                    parsed = true;
                    SkipChar();
                }
                else if (TryParseHexDigit(c, out digit))
                {
                    acc = acc * 16 + digit;
                    parsed = true;
                    SkipChar();
                }
                else
                {
                    break;
                }
            }
            num = acc;
            return parsed;
        }

        private bool TryReadHexDigit(out int result)
        {
            result = 0;
            char c;
            return TryPeekChar(out c) && TryParseHexDigit(c, out result);
        }

        private static bool TryParseHexDigit(char c, out int result)
        {
            if (c >= 'a' && c <= 'f')
            {
                result = 10 + c - 'a';
                return true;
            }
            else if (c >= 'A' && c <= 'F')
            {
                result = 10 + c - 'A';
                return true;
            }
            else if (c >= '0' && c <= '9')
            {
                result = c - '0';
                return true;
            }
            else
            {
                result = 0;
                return false;
            }
        }

        /// <summary>
        /// A token as parsed by the lexer.
        /// </summary>
        public struct Token
        {
            /// <summary>
            /// Creates a new token.
            /// </summary>
            /// <param name="kind">The token's kind.</param>
            /// <param name="span">The span in the source document that defines the token.</param>
            /// <param name="value">The token's parsed value, if applicable.</param>
            public Token(TokenKind kind, SourceSpan span, object value)
            {
                this.Kind = kind;
                this.Span = span;
                this.Value = value;
            }

            /// <summary>
            /// Gets the token's kind.
            /// </summary>
            /// <value>A token kind.</value>
            public TokenKind Kind { get; private set; }

            /// <summary>
            /// Gets the span in the source document that defines the token.
            /// </summary>
            /// <value>A source span.</value>
            public SourceSpan Span { get; private set; }

            /// <summary>
            /// Gets the token's parsed value, if applicable.
            /// </summary>
            /// <value>A parsed value.</value>
            public object Value { get; private set; }

            /// <inheritdoc/>
            public override string ToString()
            {
                return $"[{Kind}{(Value == null ? "" : " " + Value)} ('{Span.Text}')]";
            }
        }

        /// <summary>
        /// An enumeration of different kinds of tokens.
        /// </summary>
        public enum TokenKind
        {
            /// <summary>
            /// Indicates that a token represents a signed integer.
            /// </summary>
            SignedInteger,

            /// <summary>
            /// Indicates that a token represents an unsigned integer.
            /// </summary>
            UnsignedInteger,

            /// <summary>
            /// Indicates that a token represents a floating-point number.
            /// </summary>
            Float,

            /// <summary>
            /// Indicates that a token represents a string literal.
            /// </summary>
            String,

            /// <summary>
            /// Indicates that a token represents an identifier.
            /// </summary>
            Identifier,

            /// <summary>
            /// Indicates that a token represents a left parenthesis.
            /// </summary>
            LeftParenthesis,

            /// <summary>
            /// Indicates that a token represents a right parenthesis.
            /// </summary>
            RightParenthesis,

            /// <summary>
            /// Indicates that a token is reserved. These tokens should not show up
            /// in the WebAssembly text format.
            /// </summary>
            Reserved
        }
    }
}
