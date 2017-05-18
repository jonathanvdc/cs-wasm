using Wasm.Instructions;

namespace Wasm.Interpret
{
    /// <summary>
    /// A class that defines operator implementations.
    /// </summary>
    public static class OperatorImpls
    {
        /// <summary>
        /// Executes a 'unreachable' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Unreachable(Instruction Value, InterpreterContext Context)
        {
            throw new WasmException("'unreachable' instruction was reached.");
        }

        /// <summary>
        /// Executes a 'nop' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Nop(Instruction Value, InterpreterContext Context)
        {
        }

        /// <summary>
        /// Executes a 'drop' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Drop(Instruction Value, InterpreterContext Context)
        {
            Context.Pop<object>();
        }
    }
}