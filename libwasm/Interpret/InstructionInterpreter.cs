using Wasm.Instructions;

namespace Wasm.Interpret
{
    /// <summary>
    /// A type that handles the execution of instructions.
    /// </summary>
    public abstract class InstructionInterpreter
    {
        /// <summary>
        /// Interprets the given instruction within the specified context.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter context.</param>
        public abstract void Interpret(Instruction value, InterpreterContext context);
    }
}
