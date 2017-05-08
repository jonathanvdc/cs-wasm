using System.Collections.Generic;

namespace Wasm.Instructions
{
    /// <summary>
    /// A collection of operator definitions.
    /// </summary>
    public static class Operators
    {
        static Operators()
        {
            opsByOpCode = new Dictionary<byte, Operator>();

            Unreachable = Register(new NullaryOperator(0x00, WasmType.Empty, "unreachable"));
            Nop = Register(new NullaryOperator(0x01, WasmType.Empty, "nop"));
            Return = Register(new NullaryOperator(0x0f, WasmType.Empty, "return"));
        }

        /// <summary>
        /// The 'unreachable' operator, which traps immediately.
        /// </summary>
        public static readonly Operator Unreachable;

        /// <summary>
        /// The 'nop' operator, which does nothing.
        /// </summary>
        public static readonly Operator Nop;

        /// <summary>
        /// The 'return' operator, which returns zero or one value from a function.
        /// </summary>
        public static readonly Operator Return;

        /// <summary>
        /// The 'else' opcode, which begins an 'if' expression's 'else' block.
        /// </summary>
        public const byte ElseOpCode = 0x05;

        /// <summary>
        /// The 'end' opcode, which ends a block, loop or if.
        /// </summary>
        public const byte EndOpCode = 0x0b;

        /// <summary>
        /// A map of opcodes to the operators that define them.
        /// </summary>
        private static Dictionary<byte, Operator> opsByOpCode;

        /// <summary>
        /// Gets a map of opcodes to the operators that define them.
        /// </summary>
        public static IReadOnlyDictionary<byte, Operator> OperatorsByOpCode => opsByOpCode;

        /// <summary>
        /// Registers the given operator.
        /// </summary>
        /// <param name="Op">The operator to register.</param>
        /// <returns>The operator.</returns>
        private static Operator Register(Operator Op)
        {
            opsByOpCode.Add(Op.OpCode, Op);
            return Op;
        }
    }
}