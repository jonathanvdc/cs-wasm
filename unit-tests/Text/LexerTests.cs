using System;
using System.Linq;
using System.Numerics;
using System.Text;
using Loyc.MiniTest;

namespace Wasm.Text
{
    [TestFixture]
    public class LexerTests
    {
        [Test]
        public void ParseStrings()
        {
            AssertStringParsesAs("hi", "\"hi\"");
            AssertStringParsesAs("hello there", "\"hello there\"");
            AssertStringParsesAs("hello there", "\"hello\\u{20}there\"");
            AssertStringParsesAs("hello there", "\"hello\\20there\"");
            AssertStringParsesAs("hello\tthere", "\"hello\\tthere\"");
            AssertStringParsesAs("hello\rthere", "\"hello\\rthere\"");
            AssertStringParsesAs("hello\nthere", "\"hello\\nthere\"");
            AssertStringParsesAs("hello\'there", "\"hello\\'there\"");
            AssertStringParsesAs("hello\"there", "\"hello\\\"there\"");
            AssertStringParsesAs("hello\"there", "\"hello\\\"there\"");

            var unicode = new string(new[] { (char)55304, (char)56692 });
            AssertStringParsesAs(unicode, $"\"{unicode}\"");
        }

        [Test]
        public void ParseIdentifier()
        {
            AssertParsesAsKind(Lexer.TokenKind.Identifier, "$hi");
            AssertParsesAs("hi", "$hi");
            AssertParsesAs("variable_name", "$variable_name");
            AssertParsesAs("variable_name123ABC", "$variable_name123ABC");
        }

        [Test]
        public void ParseKeyword()
        {
            AssertParsesAsKind(Lexer.TokenKind.Keyword, "module");
            AssertParsesAs("module", "module");
            AssertParsesAs("i32.add", "i32.add");
            AssertParsesAsKind(Lexer.TokenKind.Keyword, "offset=4");
            AssertParsesAsKind(Lexer.TokenKind.Keyword, "i32.load");
            Assert.IsTrue(
                Enumerable.SequenceEqual(
                    new[] { Lexer.TokenKind.Keyword, Lexer.TokenKind.Keyword, Lexer.TokenKind.Keyword },
                    Lexer.Tokenize("i32.load offset=16 align=2").Select(x => x.Kind)));
        }

        [Test]
        public void ParseUnsignedIntegers()
        {
            AssertParsesAsKind(Lexer.TokenKind.UnsignedInteger, "10");
            AssertParsesAsKind(Lexer.TokenKind.UnsignedInteger, "0x10");
            AssertParsesAs(new BigInteger(10), "10");
            AssertParsesAs(new BigInteger(0x10), "0x10");
            AssertParsesAs(new BigInteger(0xff), "0xff");
        }

        [Test]
        public void ParseSignedIntegers()
        {
            AssertParsesAsKind(Lexer.TokenKind.SignedInteger, "+10");
            AssertParsesAsKind(Lexer.TokenKind.SignedInteger, "+0x10");
            AssertParsesAs(new BigInteger(10), "+10");
            AssertParsesAs(new BigInteger(0x10), "+0x10");
            AssertParsesAs(new BigInteger(0xff), "+0xff");
            AssertParsesAsKind(Lexer.TokenKind.SignedInteger, "-10");
            AssertParsesAsKind(Lexer.TokenKind.SignedInteger, "-0x10");
            AssertParsesAs(new BigInteger(-10), "-10");
            AssertParsesAs(new BigInteger(-0x10), "-0x10");
            AssertParsesAs(new BigInteger(-0xff), "-0xff");
        }

        [Test]
        public void ParseFloats()
        {
            AssertParsesAsKind(Lexer.TokenKind.Float, "inf");
            AssertParsesAsKind(Lexer.TokenKind.Float, "+inf");
            AssertParsesAsKind(Lexer.TokenKind.Float, "-inf");
            AssertParsesAsKind(Lexer.TokenKind.Float, "nan");
            AssertParsesAsKind(Lexer.TokenKind.Float, "nan:0x2");
            AssertParsesAsKind(Lexer.TokenKind.Float, "10.");
            AssertParsesAsKind(Lexer.TokenKind.Float, "10.10");
            AssertParsesAsKind(Lexer.TokenKind.Float, "+10.10");
            AssertParsesAsKind(Lexer.TokenKind.Float, "-10.10");
            AssertParsesAsKind(Lexer.TokenKind.Float, "0x10.");
            AssertParsesAsKind(Lexer.TokenKind.Float, "0x10.10");
            AssertParsesAsKind(Lexer.TokenKind.Float, "+0x10.10");
            AssertParsesAsKind(Lexer.TokenKind.Float, "-0x10.10");
            AssertParsesAsKind(Lexer.TokenKind.Float, "10.e1");
            AssertParsesAsKind(Lexer.TokenKind.Float, "10.10e1");
            AssertParsesAsKind(Lexer.TokenKind.Float, "+10.10e1");
            AssertParsesAsKind(Lexer.TokenKind.Float, "-10.10e1");
            AssertParsesAsKind(Lexer.TokenKind.Float, "0x10.p1");
            AssertParsesAsKind(Lexer.TokenKind.Float, "0x10.10p1");
            AssertParsesAsKind(Lexer.TokenKind.Float, "+0x10.10p1");
            AssertParsesAsKind(Lexer.TokenKind.Float, "-0x10.10p1");
            AssertParsesAs(double.NegativeInfinity, "-inf");
            AssertParsesAs(10.0, "10.");
            AssertParsesAs(10.10, "10.10");
            AssertParsesAs(10.10, "+10.10");
            AssertParsesAs(-10.10, "-10.10");
            AssertParsesAs(16.0, "0x10.");
            AssertParsesAs(16.0625, "0x10.10");
            AssertParsesAs(16.0625, "+0x10.10");
            AssertParsesAs(-16.0625, "-0x10.10");
            AssertParsesAs(10.0 * 10, "10E1");
            AssertParsesAs(10.0 * 10, "10.e1");
            AssertParsesAs(10.10 * 10, "10.10e1");
            AssertParsesAs(10.10 * 10, "+10.10e1");
            AssertParsesAs(-10.10 * 10, "-10.10e1");
            AssertParsesAs(16.0 * 2, "0x10P1");
            AssertParsesAs(16.0 * 2, "0x10.p1");
            AssertParsesAs(16.0625 * 2, "0x10.10p1");
            AssertParsesAs(16.0625 * 2, "+0x10.10p1");
            AssertParsesAs(-16.0625 * 2, "-0x10.10p1");
            AssertParsesAs(BitConverter.Int64BitsToDouble(0x0000000000000001), "0x1p-1074");
        }

        [Test]
        public void ParseReserved()
        {
            AssertParsesAsKind(Lexer.TokenKind.Reserved, "0$x");
            AssertParsesAsKind(Lexer.TokenKind.Reserved, "\"hello\\u{20x}there\"");
        }

        [Test]
        public void ParseParens()
        {
            AssertParsesAsKind(Lexer.TokenKind.LeftParenthesis, "(");
            AssertParsesAsKind(Lexer.TokenKind.RightParenthesis, ")");
        }

        [Test]
        public void ParseWhitespace()
        {
            AssertStringParsesAs("hi", " \"hi\"");
            AssertStringParsesAs("hi", "\t\"hi\"");
            AssertStringParsesAs("hi", "\n\"hi\"");
            AssertStringParsesAs("hi", "\r\"hi\"");
            AssertStringParsesAs("hi", " \r\n\"hi\"");
            AssertStringParsesAs("hi", "(; block comment! ;)\"hi\"");
            AssertStringParsesAs("hi", "(; (; nested block comment! ;) ;)\"hi\"");
            AssertStringParsesAs("hi", " \"hi\" ");
            AssertStringParsesAs("hi", "\"hi\" ;; line comment!");
        }

        private void AssertStringParsesAs(string expected, string text)
        {
            var token = ParseSingleToken(text);
            Assert.AreEqual(Lexer.TokenKind.String, token.Kind);
            Assert.AreEqual(expected, Encoding.UTF8.GetString((byte[])token.Value));
        }

        private void AssertParsesAs(object expected, string text)
        {
            Assert.AreEqual(expected, ParseSingleToken(text).Value);
        }

        private void AssertParsesAs(double expected, string text)
        {
            Assert.AreEqual(expected, (double)(FloatLiteral)ParseSingleToken(text).Value);
        }

        private void AssertParsesAs(float expected, string text)
        {
            Assert.AreEqual(expected, (float)(FloatLiteral)ParseSingleToken(text).Value);
        }

        private void AssertParsesAsKind(Lexer.TokenKind kind, string text)
        {
            Assert.AreEqual(kind, ParseSingleToken(text).Kind);
        }

        private Lexer.Token ParseSingleToken(string text)
        {
            var tokens = Lexer.Tokenize(text).ToArray();
            return tokens.Single();
        }
    }
}
