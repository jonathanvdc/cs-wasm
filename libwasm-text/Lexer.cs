using System;
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
                var startOffset = offset;
                if (firstChar == '(' || firstChar == ')')
                {
                    SkipChar();
                    token = new Token(
                        firstChar == '(' ? TokenKind.LeftParenthesis : TokenKind.RightParenthesis,
                        new SourceSpan(document, startOffset, 1),
                        null);
                    return true;
                }
                else if (firstChar == '"')
                {
                    token = ReadStringToken();
                }
                else if (firstChar == '$')
                {
                    token = ReadIdentifierToken();
                }
                else if (firstChar >= 'a' && firstChar <= 'z')
                {
                    token = ReadKeywordToken();
                }
                else
                {
                    token = ReadNumberToken();
                }
                if (!SkipWhitespace() && TryPeekChar(out firstChar) && firstChar != '(' && firstChar != ')')
                {
                    // According to the spec:
                    //
                    // The effect of defining the set of reserved tokens is that all tokens must be
                    // separated by either parentheses or white space. For example, ‘𝟶$𝚡’ is a single
                    // reserved token. Consequently, it is not recognized as two separate tokens ‘𝟶’
                    // and ‘$𝚡’, but instead disallowed. This property of tokenization is not affected
                    // by the fact that the definition of reserved tokens overlaps with other token classes.
                    token = ReadReservedToken(startOffset);
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

        private bool Expect(string expected)
        {
            if (IsNext(expected))
            {
                SkipChars(expected.Length);
                return true;
            }
            else
            {
                return false;
            }
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
            return Expect(c => c == expected);
        }

        private bool Expect(Predicate<char> predicate)
        {
            char c;
            if (TryPeekChar(out c) && predicate(c))
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

        private Token ReadNumberToken()
        {
            var spanStart = offset;

            bool isNegated = false;
            bool isSigned = false;
            if (Expect('+'))
            {
                isSigned = true;
            }
            else if (Expect('-'))
            {
                isSigned = true;
                isNegated = true;
            }

            object val;
            if (TryReadUnsignedNumber(isNegated, out val))
            {
                return new Token(
                    val is BigInteger ? (isSigned ? TokenKind.SignedInteger : TokenKind.UnsignedInteger) : TokenKind.Float,
                    new SourceSpan(document, spanStart, offset - spanStart),
                    val);
            }
            else
            {
                return ReadReservedToken(spanStart);
            }
        }

        private bool TryReadUnsignedNumber(bool negate, out object result)
        {
            if (Expect("nan:0x"))
            {
                BigInteger hexNum;
                if (TryReadHexNum(out hexNum))
                {
                    result = MaybeNegate(CreateNaNWithSignificand((long)hexNum), negate);
                    return true;
                }
                else
                {
                    result = 0.0;
                    return false;
                }
            }
            else if (Expect("nan"))
            {
                result = MaybeNegate(double.NaN, negate);
                return true;
            }
            else if (Expect("inf"))
            {
                result = MaybeNegate(double.PositiveInfinity, negate);
                return true;
            }
            else if (Expect("0x"))
            {
                return TryReadUnsignedNumber(
                    negate,
                    out result,
                    TryReadHexNum,
                    TryReadHexFrac,
                    'p',
                    2);
            }
            else
            {
                return TryReadUnsignedNumber(
                    negate,
                    out result,
                    TryReadNum,
                    TryReadFrac,
                    'e',
                    10);
            }
        }

        private double MaybeNegate(double v, bool negate)
        {
            return negate ? -v : v;
        }

        private BigInteger MaybeNegate(BigInteger v, bool negate)
        {
            return negate ? -v : v;
        }

        private static double CreateNaNWithSignificand(long significand)
        {
            // We're going to create a NaN with a special significand.
            long bits = BitConverter.DoubleToInt64Bits(double.NaN);
            long oldSignificand = bits & 0xfffffffffffffL;
            // Wipe out the bits originally in the significand.
            bits ^= oldSignificand;
            // Put in our bits.
            bits |= (long)significand;
            return BitConverter.Int64BitsToDouble(bits);
        }

        private delegate bool IntegerReader(out BigInteger result);
        private delegate bool FloatReader(out double result);

        private bool TryReadUnsignedNumber(
            bool negate,
            out object result,
            IntegerReader tryReadNum,
            FloatReader tryReadFrac,
            char exponentChar,
            int exponent)
        {
            BigInteger num;
            if (tryReadNum(out num))
            {
                if (Expect('.'))
                {
                    double frac;
                    if (!tryReadFrac(out frac))
                    {
                        frac = 0.0;
                    }
                    var floatNum = (double)num + frac;

                    if (Expect(exponentChar) || Expect(char.ToUpperInvariant(exponentChar)))
                    {
                        if (!TryAppendExponent(floatNum, exponent, out floatNum))
                        {
                            result = null;
                            return false;
                        }
                    }

                    result = MaybeNegate(floatNum, negate);
                }
                else if (Expect(exponentChar) || Expect(char.ToUpperInvariant(exponentChar)))
                {
                    double floatNum;
                    if (!TryAppendExponent((double)num, exponent, out floatNum))
                    {
                        result = null;
                        return false;
                    }

                    result = MaybeNegate(floatNum, negate);
                }
                else
                {
                    result = MaybeNegate(num, negate);
                }
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        private bool TryAppendExponent(
            double floatNum,
            int exponent,
            out double result)
        {
            bool negateExp = false;
            if (Expect('-'))
            {
                negateExp = true;
            }
            else
            {
                Expect('+');
            }
            BigInteger exp;
            if (!TryReadNum(out exp))
            {
                result = 0.0;
                return false;
            }
            else
            {
                result = floatNum * Math.Pow(exponent, (double)MaybeNegate(exp, negateExp));
                return true;
            }
        }

        private Token ReadKeywordToken()
        {
            var spanStart = offset;
            char c;
            if (!TryPeekChar(out c) || c < 'a' || c > 'z')
            {
                return ReadReservedToken(spanStart);
            }

            var builder = new StringBuilder();
            while (TryReadIdentifierChar(out c))
            {
                builder.Append(c);
            }

            var span = new SourceSpan(document, spanStart, offset - spanStart);
            var result = builder.ToString();

            // Some floating point tokens look like keywords, so we'll handle
            // them here as well as in the FP parsing routine.
            if (result == "nan")
            {
                return new Token(TokenKind.Float, span, double.NaN);
            }
            else if (result.StartsWith("nan:0x", StringComparison.Ordinal))
            {
                var payload = result.Substring("nan:0x".Length);
                long newBits = 0;
                for (int i = 0; i < payload.Length; i++)
                {
                    int digit;
                    if (payload[i] == '_')
                    {
                        continue;
                    }
                    else if (TryParseHexDigit(payload[i], out digit))
                    {
                        newBits = newBits * 16 + digit;
                    }
                    else
                    {
                        return new Token(TokenKind.Keyword, span, result);
                    }
                }
                return new Token(TokenKind.Float, span, CreateNaNWithSignificand(newBits));
            }
            else if (result == "inf")
            {
                return new Token(TokenKind.Float, span, double.PositiveInfinity);
            }
            else
            {
                return new Token(TokenKind.Keyword, span, result);
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

            var builder = new List<byte>();
            char c;
            while (TryReadChar(out c))
            {
                if (c == '"')
                {
                    return new Token(
                        TokenKind.String,
                        new SourceSpan(document, spanStart, offset - spanStart),
                        builder.ToArray());
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
                            builder.Add((byte)'\\');
                            break;
                        case 'n':
                            builder.Add((byte)'\n');
                            break;
                        case 'r':
                            builder.Add((byte)'\r');
                            break;
                        case 't':
                            builder.Add((byte)'\t');
                            break;
                        case '"':
                            builder.Add((byte)'"');
                            break;
                        case '\'':
                            builder.Add((byte)'\'');
                            break;
                        case 'u':
                            BigInteger num;
                            if (Expect('{') && TryReadHexNum(out num) && Expect('}'))
                            {
                                builder.AddRange(Encoding.UTF8.GetBytes(char.ConvertFromUtf32((int)num)));
                                continue;
                            }
                            return ReadReservedToken(spanStart);
                        default:
                            int firstDigit, secondDigit;
                            if (TryParseHexDigit(c, out firstDigit) && TryReadHexDigit(out secondDigit))
                            {
                                builder.Add((byte)(16 * firstDigit + secondDigit));
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
                else if (char.IsHighSurrogate(c))
                {
                    var high = c;
                    if (!TryReadChar(out c) || !char.IsLowSurrogate(c))
                    {
                        return ReadReservedToken(spanStart);
                    }
                    var low = c;
                    builder.AddRange(Encoding.UTF8.GetBytes(new string(new[] { high, low })));
                }
                else
                {
                    builder.AddRange(Encoding.UTF8.GetBytes(c.ToString()));
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

        private bool TryReadHexFrac(out double frac)
        {
            return TryReadFrac(out frac, 16, (i, c) =>
            {
                int digit;
                if (TryParseHexDigit(c, out digit))
                {
                    return i * 16 + digit;
                }
                else
                {
                    return null;
                }
            });
        }

        private bool TryReadFrac(out double frac)
        {
            return TryReadFrac(out frac, 10, (i, c) =>
            {
                if (c >= '0' && c <= '9')
                {
                    return i * 10 + (c - '0');
                }
                else
                {
                    return null;
                }
            });
        }

        private bool TryReadFrac(
            out double frac,
            int fracBase,
            Func<BigInteger, char, BigInteger?> tryAccumulateFracDigit)
        {
            (BigInteger, int) pair;
            bool parsed = TryReadNum(out pair, (BigInteger.Zero, 0), (acc, c) =>
            {
                var (i, n) = acc;
                var maybeAcc = tryAccumulateFracDigit(i, c);
                if (maybeAcc.HasValue)
                {
                    return (maybeAcc.Value, n + 1);
                }
                else
                {
                    return null;
                }
            });

            if (parsed)
            {
                var (i, n) = pair;
                frac = (double)i / Math.Pow(fracBase, n);
                return true;
            }
            else
            {
                frac = 0.0;
                return false;
            }
        }

        private bool TryReadNum<T>(
            out T num,
            T init,
            Func<T, char, T?> tryAccumulate)
            where T : struct
        {
            bool parsed = false;
            var acc = init;
            char c;
            while (TryPeekChar(out c))
            {
                if (c == '_')
                {
                    parsed = true;
                    SkipChar();
                }
                else
                {
                    var maybeAcc = tryAccumulate(acc, c);
                    if (maybeAcc.HasValue)
                    {
                        acc = maybeAcc.Value;
                        parsed = true;
                        SkipChar();
                    }
                    else
                    {
                        break;
                    }
                }
            }
            num = acc;
            return parsed;
        }

        private bool TryReadNum(
            out BigInteger num,
            Func<BigInteger, char, BigInteger?> tryAccumulate)
        {
            return TryReadNum(out num, BigInteger.Zero, tryAccumulate);
        }

        private bool TryReadNum(out BigInteger num)
        {
            return TryReadNum(out num, (i, c) =>
            {
                if (c >= '0' && c <= '9')
                {
                    return i * 10 + (c - '0');
                }
                else
                {
                    return null;
                }
            });
        }

        private bool TryReadHexNum(out BigInteger num)
        {
            return TryReadNum(out num, (i, c) =>
            {
                int digit;
                if (TryParseHexDigit(c, out digit))
                {
                    return i * 16 + digit;
                }
                else
                {
                    return null;
                }
            });
        }

        private bool TryReadHexDigit(out int result)
        {
            char c;
            if (TryPeekChar(out c) && TryParseHexDigit(c, out result))
            {
                SkipChar();
                return true;
            }
            else
            {
                result = 0;
                return false;
            }
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
            /// Creates a synthetic token.
            /// </summary>
            /// <param name="value">The token's value.</param>
            /// <param name="kind">The type of the token to synthesize.</param>
            /// <returns>A synthetic token.</returns>
            public static Token Synthesize(object value, TokenKind kind = TokenKind.Synthetic)
            {
                var doc = new StringDocument("<synthetic>", value.ToString());
                return new Token(kind, new SourceSpan(doc, 0, doc.Length), value);
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
            /// Indicates that a token represents a keyword.
            /// </summary>
            Keyword,

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
            Reserved,

            /// <summary>
            /// A token that was generated by some component other than the lexer.
            /// Synthetic tokens are never user-created.
            /// </summary>
            Synthetic
        }
    }
}
