using System;
using System.Linq;
using Loyc.MiniTest;

namespace Wasm.Text
{
    [TestFixture]
    public class LexerTests
    {
        [Test]
        public void ParseStrings()
        {
            AssertParsesAs("hi", "\"hi\"");
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

        private Lexer.Token ParseSingleToken(string text)
        {
            var tokens = Lexer.Tokenize(text).ToArray();
            return tokens.Single();
        }
    }
}
