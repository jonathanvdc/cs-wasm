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
            Drop = Register(new NullaryOperator(0x1a, WasmType.Empty, "drop"));
            Select = Register(new NullaryOperator(0x1b, WasmType.Empty, "select"));
            GetLocal = Register(new VarUInt32Operator(0x20, WasmType.Empty, "get_local"));
            SetLocal = Register(new VarUInt32Operator(0x21, WasmType.Empty, "set_local"));
            TeeLocal = Register(new VarUInt32Operator(0x22, WasmType.Empty, "tee_local"));
            GetGlobal = Register(new VarUInt32Operator(0x23, WasmType.Empty, "get_global"));
            SetGlobal = Register(new VarUInt32Operator(0x24, WasmType.Empty, "set_global"));
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
        /// The 'drop' operator, which pops the top-of-stack value and ignores it.
        /// </summary>
        public static readonly Operator Drop;

        /// <summary>
        /// The 'select' operator, which selects one of two values based on a condition.
        /// </summary>
        public static readonly Operator Select;

        /// <summary>
        /// The 'get_local' operator, which reads a local variable or parameter.
        /// </summary>
        public static readonly Operator GetLocal;

        /// <summary>
        /// The 'set_local' operator, which writes a value to a local variable or parameter.
        /// </summary>
        public static readonly Operator SetLocal;

        /// <summary>
        /// The 'tee_local' operator, which writes a value to a local variable or parameter
        /// and then returns the same value.
        /// </summary>
        public static readonly Operator TeeLocal;

        /// <summary>
        /// The 'get_global' operator, which reads a global variable.
        /// </summary>
        public static readonly Operator GetGlobal;

        /// <summary>
        /// The 'set_global' operator, which reads a global variable.
        /// </summary>
        public static readonly Operator SetGlobal;

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