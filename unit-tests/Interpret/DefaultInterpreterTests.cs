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
    }
}

