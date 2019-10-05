using System;
using System.Linq;
using System.Numerics;
using Loyc.MiniTest;

namespace Wasm.Text
{
    [TestFixture]
    public class LexerTests
    {
        [Test]
        public void ParseStrings()
        {
            AssertParsesAsKind(Lexer.TokenKind.String, "\"hi\"");
            AssertParsesAs("hi", "\"hi\"");
            AssertParsesAs("hello there", "\"hello there\"");
            AssertParsesAs("hello there", "\"hello\\u{20}there\"");
            AssertParsesAs("hello there", "\"hello\\20there\"");
            AssertParsesAs("hello\tthere", "\"hello\\tthere\"");
            AssertParsesAs("hello\rthere", "\"hello\\rthere\"");
            AssertParsesAs("hello\nthere", "\"hello\\nthere\"");
            AssertParsesAs("hello\'there", "\"hello\\'there\"");
            AssertParsesAs("hello\"there", "\"hello\\\"there\"");
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
            AssertParsesAs("hi", " \"hi\"");
            AssertParsesAs("hi", "\t\"hi\"");
            AssertParsesAs("hi", "\n\"hi\"");
            AssertParsesAs("hi", "\r\"hi\"");
            AssertParsesAs("hi", " \r\n\"hi\"");
            AssertParsesAs("hi", "(; block comment! ;)\"hi\"");
            AssertParsesAs("hi", "(; (; nested block comment! ;) ;)\"hi\"");
            AssertParsesAs("hi", " \"hi\" ");
            AssertParsesAs("hi", "\"hi\" ;; line comment!");
        }

        private void AssertParsesAs(object expected, string text)
        {
            Assert.AreEqual(expected, ParseSingleToken(text).Value);
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
