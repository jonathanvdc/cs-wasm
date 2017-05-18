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

        /// <summary>
        /// Executes an 'i32.const' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32Const(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.Int32Const.CastInstruction(Value);
            Context.Push<int>(instr.Immediate);
        }

        /// <summary>
        /// Executes an 'i64.const' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Const(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.Int64Const.CastInstruction(Value);
            Context.Push<long>(instr.Immediate);
        }

        /// <summary>
        /// Executes an 'f32.const' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32Const(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.Float32Const.CastInstruction(Value);
            Context.Push<float>(instr.Immediate);
        }

        /// <summary>
        /// Executes an 'f64.const' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float64Const(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.Float64Const.CastInstruction(Value);
            Context.Push<double>(instr.Immediate);
        }
    }
}