using System;
using Loyc.MiniTest;
using Wasm.Instructions;

namespace Wasm.Interpret
{
    [TestFixture]
    public class DefaultInterpreterTests
    {
        [Test]
        public void ImplementationCompleteness()
        {
            var interpreter = DefaultInstructionInterpreter.Default;
            foreach (var op in Operators.AllOperators)
            {
                Assert.IsTrue(interpreter.IsImplemented(op), "Operator not implemented: " + op.ToString());
            }
        }

        private static readonly double float64NegativeZero = Negate(0.0);

        private static double Negate(double value)
        {
            return -value;
        }

        [Test]
        public void Signbit()
        {
            Assert.IsFalse(ValueHelpers.Signbit(1.0));
            Assert.IsTrue(ValueHelpers.Signbit(-1.0));
            Assert.IsTrue(ValueHelpers.Signbit(float64NegativeZero));
            Assert.IsFalse(ValueHelpers.Signbit(0.0));
        }

        [Test]
        public void Copysign()
        {
            Assert.AreEqual(42.0, ValueHelpers.Copysign(42.0, 1.0));
            Assert.AreEqual(42.0, ValueHelpers.Copysign(-42.0, 1.0));
            Assert.AreEqual(-42.0, ValueHelpers.Copysign(-42.0, -1.0));
            Assert.AreEqual(42.0, ValueHelpers.Copysign(-42.0, 0.0));
            Assert.AreEqual(-42.0, ValueHelpers.Copysign(-42.0, float64NegativeZero));
            Assert.AreEqual(42.0, ValueHelpers.Copysign(-42.0, (double)Text.FloatLiteral.NaN(false)));
            Assert.AreEqual(-42.0, ValueHelpers.Copysign(-42.0, (double)Text.FloatLiteral.NaN(true)));
        }
    }
}

