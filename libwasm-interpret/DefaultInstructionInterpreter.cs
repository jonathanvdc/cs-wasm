using System;
using Wasm.Instructions;

namespace Wasm.Interpret
{
    /// <summary>
    /// The default instruction interpreter implementation.
    /// </summary>
    public class DefaultInstructionInterpreter : InstructionInterpreter
    {
        /// <summary>
        /// Interprets the given instruction within the specified context.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter context.</param>
        public override void Interpret(Instruction Value, InterpreterContext Context)
        {
            throw new NotImplementedException();
        }
    }
}