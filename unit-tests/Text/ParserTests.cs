using System.Linq;
using Loyc.MiniTest;
using Pixie;

namespace Wasm.Text
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void ParseSExpressions()
        {
            var exprWithoutTail = ParseSingleSExpression("module");
            Assert.IsFalse(exprWithoutTail.IsCall);
            Assert.AreEqual(Lexer.TokenKind.Keyword, exprWithoutTail.Head.Kind);
            Assert.AreEqual(0, exprWithoutTail.Tail.Count);

            var exprWithEmptyTail = ParseSingleSExpression("(module)");
            Assert.IsTrue(exprWithEmptyTail.IsCall);
            Assert.AreEqual(Lexer.TokenKind.Keyword, exprWithoutTail.Head.Kind);
            Assert.AreEqual(0, exprWithEmptyTail.Tail.Count);

            var exprWithNonEmptyTail = ParseSingleSExpression("(module 10 2e4)");
            Assert.IsTrue(exprWithNonEmptyTail.IsCall);
            Assert.AreEqual(Lexer.TokenKind.Keyword, exprWithNonEmptyTail.Head.Kind);
            Assert.AreEqual(2, exprWithNonEmptyTail.Tail.Count);
            Assert.AreEqual(Lexer.TokenKind.UnsignedInteger, exprWithNonEmptyTail.Tail[0].Head.Kind);
            Assert.AreEqual(Lexer.TokenKind.Float, exprWithNonEmptyTail.Tail[1].Head.Kind);

            var nestedExpr = ParseSingleSExpression("(module (limits 10 20))");
            Assert.IsTrue(nestedExpr.IsCall);
            Assert.AreEqual(Lexer.TokenKind.Keyword, nestedExpr.Head.Kind);
            Assert.AreEqual(1, nestedExpr.Tail.Count);
            Assert.IsTrue(nestedExpr.Tail[0].IsCall);
            Assert.AreEqual(Lexer.TokenKind.Keyword, nestedExpr.Tail[0].Head.Kind);
            Assert.IsTrue(nestedExpr.Tail[0].IsCall);
            Assert.AreEqual(Lexer.TokenKind.UnsignedInteger, nestedExpr.Tail[0].Tail[0].Head.Kind);
            Assert.AreEqual(Lexer.TokenKind.UnsignedInteger, nestedExpr.Tail[0].Tail[1].Head.Kind);
        }

        private SExpression ParseSingleSExpression(string text)
        {
            var tokens = Lexer.Tokenize(text).ToArray();
            var log = new TestLog(new[] { Severity.Error }, NullLog.Instance);
            return Parser.ParseAsSExpressions(tokens, log).Single();
        }
    }
}
