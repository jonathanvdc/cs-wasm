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

            Unreachable = Register<NullaryOperator>(new NullaryOperator(0x00, WasmType.Empty, "unreachable"));
            Nop = Register<NullaryOperator>(new NullaryOperator(0x01, WasmType.Empty, "nop"));
            Block = Register<BlockOperator>(new BlockOperator(0x02, WasmType.Empty, "block"));
            Loop = Register<BlockOperator>(new BlockOperator(0x03, WasmType.Empty, "loop"));
            If = Register<IfElseOperator>(new IfElseOperator(0x04, WasmType.Empty, "if"));
            Br = Register<VarUInt32Operator>(new VarUInt32Operator(0x0c, WasmType.Empty, "br"));
            BrIf = Register<VarUInt32Operator>(new VarUInt32Operator(0x0d, WasmType.Empty, "br_if"));
            BrTable = Register<BrTableOperator>(new BrTableOperator(0x0e, WasmType.Empty, "br_table"));
            Return = Register<NullaryOperator>(new NullaryOperator(0x0f, WasmType.Empty, "return"));
            Drop = Register<NullaryOperator>(new NullaryOperator(0x1a, WasmType.Empty, "drop"));
            Select = Register<NullaryOperator>(new NullaryOperator(0x1b, WasmType.Empty, "select"));

            Call = Register<VarUInt32Operator>(new VarUInt32Operator(0x10, WasmType.Empty, "call"));
            CallIndirect = Register<CallIndirectOperator>(new CallIndirectOperator(0x11, WasmType.Empty, "call_indirect"));

            GetLocal = Register<VarUInt32Operator>(new VarUInt32Operator(0x20, WasmType.Empty, "get_local"));
            SetLocal = Register<VarUInt32Operator>(new VarUInt32Operator(0x21, WasmType.Empty, "set_local"));
            TeeLocal = Register<VarUInt32Operator>(new VarUInt32Operator(0x22, WasmType.Empty, "tee_local"));
            GetGlobal = Register<VarUInt32Operator>(new VarUInt32Operator(0x23, WasmType.Empty, "get_global"));
            SetGlobal = Register<VarUInt32Operator>(new VarUInt32Operator(0x24, WasmType.Empty, "set_global"));

            Int32Const = Register<VarInt32Operator>(new VarInt32Operator(0x41, WasmType.Int32, "const"));
            Int64Const = Register<VarInt64Operator>(new VarInt64Operator(0x42, WasmType.Int64, "const"));
            Float32Const = Register<Float32Operator>(new Float32Operator(0x43, WasmType.Float32, "const"));
            Float64Const = Register<Float64Operator>(new Float64Operator(0x44, WasmType.Float64, "const"));

            Int32Load = Register<MemoryOperator>(new MemoryOperator(0x28, WasmType.Int32, "load"));
            Int64Load = Register<MemoryOperator>(new MemoryOperator(0x29, WasmType.Int64, "load"));
            Float32Load = Register<MemoryOperator>(new MemoryOperator(0x2a, WasmType.Float32, "load"));
            Float64Load = Register<MemoryOperator>(new MemoryOperator(0x2b, WasmType.Float64, "load"));
            Int32Load8S = Register<MemoryOperator>(new MemoryOperator(0x2c, WasmType.Int32, "load8_s"));
            Int32Load8U = Register<MemoryOperator>(new MemoryOperator(0x2d, WasmType.Int32, "load8_u"));
            Int32Load16S = Register<MemoryOperator>(new MemoryOperator(0x2e, WasmType.Int32, "load16_s"));
            Int32Load16U = Register<MemoryOperator>(new MemoryOperator(0x2f, WasmType.Int32, "load16_u"));
            Int64Load8S = Register<MemoryOperator>(new MemoryOperator(0x30, WasmType.Int64, "load8_s"));
            Int64Load8U = Register<MemoryOperator>(new MemoryOperator(0x31, WasmType.Int64, "load8_u"));
            Int64Load16S = Register<MemoryOperator>(new MemoryOperator(0x32, WasmType.Int64, "load16_s"));
            Int64Load16U = Register<MemoryOperator>(new MemoryOperator(0x33, WasmType.Int64, "load16_u"));
            Int64Load32S = Register<MemoryOperator>(new MemoryOperator(0x34, WasmType.Int64, "load32_s"));
            Int64Load32U = Register<MemoryOperator>(new MemoryOperator(0x35, WasmType.Int64, "load32_u"));
            Int32Store = Register<MemoryOperator>(new MemoryOperator(0x36, WasmType.Int32, "store"));
            Int64Store = Register<MemoryOperator>(new MemoryOperator(0x37, WasmType.Int64, "store"));
            Float32Store = Register<MemoryOperator>(new MemoryOperator(0x38, WasmType.Float32, "store"));
            Float64Store = Register<MemoryOperator>(new MemoryOperator(0x39, WasmType.Float64, "store"));
            Int32Store8 = Register<MemoryOperator>(new MemoryOperator(0x3a, WasmType.Int32, "store8"));
            Int32Store16 = Register<MemoryOperator>(new MemoryOperator(0x3b, WasmType.Int32, "store16"));
            Int64Store8 = Register<MemoryOperator>(new MemoryOperator(0x3c, WasmType.Int64, "store8"));
            Int64Store16 = Register<MemoryOperator>(new MemoryOperator(0x3d, WasmType.Int64, "store16"));
            Int64Store32 = Register<MemoryOperator>(new MemoryOperator(0x3e, WasmType.Int64, "store32"));
            CurrentMemory = Register<VarUInt32Operator>(new VarUInt32Operator(0x3f, WasmType.Empty, "current_memory"));
            GrowMemory = Register<VarUInt32Operator>(new VarUInt32Operator(0x40, WasmType.Empty, "grow_memory"));

            // The code below has been auto-generated by nullary-opcode-generator.
            Int32Eqz = Register<NullaryOperator>(new NullaryOperator(0x45, WasmType.Int32, "eqz"));
            Int32Eq = Register<NullaryOperator>(new NullaryOperator(0x46, WasmType.Int32, "eq"));
            Int32Ne = Register<NullaryOperator>(new NullaryOperator(0x47, WasmType.Int32, "ne"));
            Int32LtS = Register<NullaryOperator>(new NullaryOperator(0x48, WasmType.Int32, "lt_s"));
            Int32LtU = Register<NullaryOperator>(new NullaryOperator(0x49, WasmType.Int32, "lt_u"));
            Int32GtS = Register<NullaryOperator>(new NullaryOperator(0x4a, WasmType.Int32, "gt_s"));
            Int32GtU = Register<NullaryOperator>(new NullaryOperator(0x4b, WasmType.Int32, "gt_u"));
            Int32LeS = Register<NullaryOperator>(new NullaryOperator(0x4c, WasmType.Int32, "le_s"));
            Int32LeU = Register<NullaryOperator>(new NullaryOperator(0x4d, WasmType.Int32, "le_u"));
            Int32GeS = Register<NullaryOperator>(new NullaryOperator(0x4e, WasmType.Int32, "ge_s"));
            Int32GeU = Register<NullaryOperator>(new NullaryOperator(0x4f, WasmType.Int32, "ge_u"));
            Int64Eqz = Register<NullaryOperator>(new NullaryOperator(0x50, WasmType.Int64, "eqz"));
            Int64Eq = Register<NullaryOperator>(new NullaryOperator(0x51, WasmType.Int64, "eq"));
            Int64Ne = Register<NullaryOperator>(new NullaryOperator(0x52, WasmType.Int64, "ne"));
            Int64LtS = Register<NullaryOperator>(new NullaryOperator(0x53, WasmType.Int64, "lt_s"));
            Int64LtU = Register<NullaryOperator>(new NullaryOperator(0x54, WasmType.Int64, "lt_u"));
            Int64GtS = Register<NullaryOperator>(new NullaryOperator(0x55, WasmType.Int64, "gt_s"));
            Int64GtU = Register<NullaryOperator>(new NullaryOperator(0x56, WasmType.Int64, "gt_u"));
            Int64LeS = Register<NullaryOperator>(new NullaryOperator(0x57, WasmType.Int64, "le_s"));
            Int64LeU = Register<NullaryOperator>(new NullaryOperator(0x58, WasmType.Int64, "le_u"));
            Int64GeS = Register<NullaryOperator>(new NullaryOperator(0x59, WasmType.Int64, "ge_s"));
            Int64GeU = Register<NullaryOperator>(new NullaryOperator(0x5a, WasmType.Int64, "ge_u"));
            Float32Eq = Register<NullaryOperator>(new NullaryOperator(0x5b, WasmType.Float32, "eq"));
            Float32Ne = Register<NullaryOperator>(new NullaryOperator(0x5c, WasmType.Float32, "ne"));
            Float32Lt = Register<NullaryOperator>(new NullaryOperator(0x5d, WasmType.Float32, "lt"));
            Float32Gt = Register<NullaryOperator>(new NullaryOperator(0x5e, WasmType.Float32, "gt"));
            Float32Le = Register<NullaryOperator>(new NullaryOperator(0x5f, WasmType.Float32, "le"));
            Float32Ge = Register<NullaryOperator>(new NullaryOperator(0x60, WasmType.Float32, "ge"));
            Float64Eq = Register<NullaryOperator>(new NullaryOperator(0x61, WasmType.Float64, "eq"));
            Float64Ne = Register<NullaryOperator>(new NullaryOperator(0x62, WasmType.Float64, "ne"));
            Float64Lt = Register<NullaryOperator>(new NullaryOperator(0x63, WasmType.Float64, "lt"));
            Float64Gt = Register<NullaryOperator>(new NullaryOperator(0x64, WasmType.Float64, "gt"));
            Float64Le = Register<NullaryOperator>(new NullaryOperator(0x65, WasmType.Float64, "le"));
            Float64Ge = Register<NullaryOperator>(new NullaryOperator(0x66, WasmType.Float64, "ge"));
            Int32Clz = Register<NullaryOperator>(new NullaryOperator(0x67, WasmType.Int32, "clz"));
            Int32Ctz = Register<NullaryOperator>(new NullaryOperator(0x68, WasmType.Int32, "ctz"));
            Int32Popcnt = Register<NullaryOperator>(new NullaryOperator(0x69, WasmType.Int32, "popcnt"));
            Int32Add = Register<NullaryOperator>(new NullaryOperator(0x6a, WasmType.Int32, "add"));
            Int32Sub = Register<NullaryOperator>(new NullaryOperator(0x6b, WasmType.Int32, "sub"));
            Int32Mul = Register<NullaryOperator>(new NullaryOperator(0x6c, WasmType.Int32, "mul"));
            Int32DivS = Register<NullaryOperator>(new NullaryOperator(0x6d, WasmType.Int32, "div_s"));
            Int32DivU = Register<NullaryOperator>(new NullaryOperator(0x6e, WasmType.Int32, "div_u"));
            Int32RemS = Register<NullaryOperator>(new NullaryOperator(0x6f, WasmType.Int32, "rem_s"));
            Int32RemU = Register<NullaryOperator>(new NullaryOperator(0x70, WasmType.Int32, "rem_u"));
            Int32And = Register<NullaryOperator>(new NullaryOperator(0x71, WasmType.Int32, "and"));
            Int32Or = Register<NullaryOperator>(new NullaryOperator(0x72, WasmType.Int32, "or"));
            Int32Xor = Register<NullaryOperator>(new NullaryOperator(0x73, WasmType.Int32, "xor"));
            Int32Shl = Register<NullaryOperator>(new NullaryOperator(0x74, WasmType.Int32, "shl"));
            Int32ShrS = Register<NullaryOperator>(new NullaryOperator(0x75, WasmType.Int32, "shr_s"));
            Int32ShrU = Register<NullaryOperator>(new NullaryOperator(0x76, WasmType.Int32, "shr_u"));
            Int32Rotl = Register<NullaryOperator>(new NullaryOperator(0x77, WasmType.Int32, "rotl"));
            Int32Rotr = Register<NullaryOperator>(new NullaryOperator(0x78, WasmType.Int32, "rotr"));
            Int64Clz = Register<NullaryOperator>(new NullaryOperator(0x79, WasmType.Int64, "clz"));
            Int64Ctz = Register<NullaryOperator>(new NullaryOperator(0x7a, WasmType.Int64, "ctz"));
            Int64Popcnt = Register<NullaryOperator>(new NullaryOperator(0x7b, WasmType.Int64, "popcnt"));
            Int64Add = Register<NullaryOperator>(new NullaryOperator(0x7c, WasmType.Int64, "add"));
            Int64Sub = Register<NullaryOperator>(new NullaryOperator(0x7d, WasmType.Int64, "sub"));
            Int64Mul = Register<NullaryOperator>(new NullaryOperator(0x7e, WasmType.Int64, "mul"));
            Int64DivS = Register<NullaryOperator>(new NullaryOperator(0x7f, WasmType.Int64, "div_s"));
            Int64DivU = Register<NullaryOperator>(new NullaryOperator(0x80, WasmType.Int64, "div_u"));
            Int64RemS = Register<NullaryOperator>(new NullaryOperator(0x81, WasmType.Int64, "rem_s"));
            Int64RemU = Register<NullaryOperator>(new NullaryOperator(0x82, WasmType.Int64, "rem_u"));
            Int64And = Register<NullaryOperator>(new NullaryOperator(0x83, WasmType.Int64, "and"));
            Int64Or = Register<NullaryOperator>(new NullaryOperator(0x84, WasmType.Int64, "or"));
            Int64Xor = Register<NullaryOperator>(new NullaryOperator(0x85, WasmType.Int64, "xor"));
            Int64Shl = Register<NullaryOperator>(new NullaryOperator(0x86, WasmType.Int64, "shl"));
            Int64ShrS = Register<NullaryOperator>(new NullaryOperator(0x87, WasmType.Int64, "shr_s"));
            Int64ShrU = Register<NullaryOperator>(new NullaryOperator(0x88, WasmType.Int64, "shr_u"));
            Int64Rotl = Register<NullaryOperator>(new NullaryOperator(0x89, WasmType.Int64, "rotl"));
            Int64Rotr = Register<NullaryOperator>(new NullaryOperator(0x8a, WasmType.Int64, "rotr"));
            Float32Abs = Register<NullaryOperator>(new NullaryOperator(0x8b, WasmType.Float32, "abs"));
            Float32Neg = Register<NullaryOperator>(new NullaryOperator(0x8c, WasmType.Float32, "neg"));
            Float32Ceil = Register<NullaryOperator>(new NullaryOperator(0x8d, WasmType.Float32, "ceil"));
            Float32Floor = Register<NullaryOperator>(new NullaryOperator(0x8e, WasmType.Float32, "floor"));
            Float32Trunc = Register<NullaryOperator>(new NullaryOperator(0x8f, WasmType.Float32, "trunc"));
            Float32Nearest = Register<NullaryOperator>(new NullaryOperator(0x90, WasmType.Float32, "nearest"));
            Float32Sqrt = Register<NullaryOperator>(new NullaryOperator(0x91, WasmType.Float32, "sqrt"));
            Float32Add = Register<NullaryOperator>(new NullaryOperator(0x92, WasmType.Float32, "add"));
            Float32Sub = Register<NullaryOperator>(new NullaryOperator(0x93, WasmType.Float32, "sub"));
            Float32Mul = Register<NullaryOperator>(new NullaryOperator(0x94, WasmType.Float32, "mul"));
            Float32Div = Register<NullaryOperator>(new NullaryOperator(0x95, WasmType.Float32, "div"));
            Float32Min = Register<NullaryOperator>(new NullaryOperator(0x96, WasmType.Float32, "min"));
            Float32Max = Register<NullaryOperator>(new NullaryOperator(0x97, WasmType.Float32, "max"));
            Float32Copysign = Register<NullaryOperator>(new NullaryOperator(0x98, WasmType.Float32, "copysign"));
            Float64Abs = Register<NullaryOperator>(new NullaryOperator(0x99, WasmType.Float64, "abs"));
            Float64Neg = Register<NullaryOperator>(new NullaryOperator(0x9a, WasmType.Float64, "neg"));
            Float64Ceil = Register<NullaryOperator>(new NullaryOperator(0x9b, WasmType.Float64, "ceil"));
            Float64Floor = Register<NullaryOperator>(new NullaryOperator(0x9c, WasmType.Float64, "floor"));
            Float64Trunc = Register<NullaryOperator>(new NullaryOperator(0x9d, WasmType.Float64, "trunc"));
            Float64Nearest = Register<NullaryOperator>(new NullaryOperator(0x9e, WasmType.Float64, "nearest"));
            Float64Sqrt = Register<NullaryOperator>(new NullaryOperator(0x9f, WasmType.Float64, "sqrt"));
            Float64Add = Register<NullaryOperator>(new NullaryOperator(0xa0, WasmType.Float64, "add"));
            Float64Sub = Register<NullaryOperator>(new NullaryOperator(0xa1, WasmType.Float64, "sub"));
            Float64Mul = Register<NullaryOperator>(new NullaryOperator(0xa2, WasmType.Float64, "mul"));
            Float64Div = Register<NullaryOperator>(new NullaryOperator(0xa3, WasmType.Float64, "div"));
            Float64Min = Register<NullaryOperator>(new NullaryOperator(0xa4, WasmType.Float64, "min"));
            Float64Max = Register<NullaryOperator>(new NullaryOperator(0xa5, WasmType.Float64, "max"));
            Float64Copysign = Register<NullaryOperator>(new NullaryOperator(0xa6, WasmType.Float64, "copysign"));
            Int32WrapInt64 = Register<NullaryOperator>(new NullaryOperator(0xa7, WasmType.Int32, "wrap/i64"));
            Int32TruncSFloat32 = Register<NullaryOperator>(new NullaryOperator(0xa8, WasmType.Int32, "trunc_s/f32"));
            Int32TruncUFloat32 = Register<NullaryOperator>(new NullaryOperator(0xa9, WasmType.Int32, "trunc_u/f32"));
            Int32TruncSFloat64 = Register<NullaryOperator>(new NullaryOperator(0xaa, WasmType.Int32, "trunc_s/f64"));
            Int32TruncUFloat64 = Register<NullaryOperator>(new NullaryOperator(0xab, WasmType.Int32, "trunc_u/f64"));
            Int64ExtendSInt32 = Register<NullaryOperator>(new NullaryOperator(0xac, WasmType.Int64, "extend_s/i32"));
            Int64ExtendUInt32 = Register<NullaryOperator>(new NullaryOperator(0xad, WasmType.Int64, "extend_u/i32"));
            Int64TruncSFloat32 = Register<NullaryOperator>(new NullaryOperator(0xae, WasmType.Int64, "trunc_s/f32"));
            Int64TruncUFloat32 = Register<NullaryOperator>(new NullaryOperator(0xaf, WasmType.Int64, "trunc_u/f32"));
            Int64TruncSFloat64 = Register<NullaryOperator>(new NullaryOperator(0xb0, WasmType.Int64, "trunc_s/f64"));
            Int64TruncUFloat64 = Register<NullaryOperator>(new NullaryOperator(0xb1, WasmType.Int64, "trunc_u/f64"));
            Float32ConvertSInt32 = Register<NullaryOperator>(new NullaryOperator(0xb2, WasmType.Float32, "convert_s/i32"));
            Float32ConvertUInt32 = Register<NullaryOperator>(new NullaryOperator(0xb3, WasmType.Float32, "convert_u/i32"));
            Float32ConvertSInt64 = Register<NullaryOperator>(new NullaryOperator(0xb4, WasmType.Float32, "convert_s/i64"));
            Float32ConvertUInt64 = Register<NullaryOperator>(new NullaryOperator(0xb5, WasmType.Float32, "convert_u/i64"));
            Float32DemoteFloat64 = Register<NullaryOperator>(new NullaryOperator(0xb6, WasmType.Float32, "demote/f64"));
            Float64ConvertSInt32 = Register<NullaryOperator>(new NullaryOperator(0xb7, WasmType.Float64, "convert_s/i32"));
            Float64ConvertUInt32 = Register<NullaryOperator>(new NullaryOperator(0xb8, WasmType.Float64, "convert_u/i32"));
            Float64ConvertSInt64 = Register<NullaryOperator>(new NullaryOperator(0xb9, WasmType.Float64, "convert_s/i64"));
            Float64ConvertUInt64 = Register<NullaryOperator>(new NullaryOperator(0xba, WasmType.Float64, "convert_u/i64"));
            Float64PromoteFloat32 = Register<NullaryOperator>(new NullaryOperator(0xbb, WasmType.Float64, "promote/f32"));
            Int32ReinterpretFloat32 = Register<NullaryOperator>(new NullaryOperator(0xbc, WasmType.Int32, "reinterpret/f32"));
            Int64ReinterpretFloat64 = Register<NullaryOperator>(new NullaryOperator(0xbd, WasmType.Int64, "reinterpret/f64"));
            Float32ReinterpretInt32 = Register<NullaryOperator>(new NullaryOperator(0xbe, WasmType.Float32, "reinterpret/i32"));
            Float64ReinterpretInt64 = Register<NullaryOperator>(new NullaryOperator(0xbf, WasmType.Float64, "reinterpret/i64"));
        }

        /// <summary>
        /// A map of opcodes to the operators that define them.
        /// </summary>
        private static Dictionary<byte, Operator> opsByOpCode;

        /// <summary>
        /// Gets a map of opcodes to the operators that define them.
        /// </summary>
        public static IReadOnlyDictionary<byte, Operator> OperatorsByOpCode => opsByOpCode;

        /// <summary>
        /// Gets a sequence that contains all WebAssembly operators defined by this class.
        /// </summary>
        public static IEnumerable<Operator> AllOperators => opsByOpCode.Values;

        /// <summary>
        /// Registers the given operator.
        /// </summary>
        /// <param name="Op">The operator to register.</param>
        /// <returns>The operator.</returns>
        private static T Register<T>(T Op)
            where T : Operator
        {
            opsByOpCode.Add(Op.OpCode, Op);
            return Op;
        }

        /// <summary>
        /// Gets the operator with the given opcode.
        /// </summary>
        /// <param name="OpCode">The opcode to find an operator for.</param>
        /// <returns>The operator with the given opcode.</returns>
        public static Operator GetOperatorByOpCode(byte OpCode)
        {
            Operator result;
            if (OperatorsByOpCode.TryGetValue(OpCode, out result))
            {
                return result;
            }
            else
            {
                throw new WasmException(
                    string.Format("Unknown opcode: {0}", DumpHelpers.FormatHex(OpCode)));
            }
        }

        /// <summary>
        /// The 'unreachable' operator, which traps immediately.
        /// </summary>
        public static readonly NullaryOperator Unreachable;

        /// <summary>
        /// The 'nop' operator, which does nothing.
        /// </summary>
        public static readonly NullaryOperator Nop;

        /// <summary>
        /// The 'block' operator, which begins a sequence of expressions, yielding 0 or 1 values.
        /// </summary>
        public static readonly BlockOperator Block;

        /// <summary>
        /// The 'loop' operator, which begins a block which can also form control flow loops
        /// </summary>
        public static readonly BlockOperator Loop;

        /// <summary>
        /// The 'if' operator, which runs one of two sequences of expressions.
        /// </summary>
        public static readonly IfElseOperator If;

        /// <summary>
        /// The 'br' operator: a break that targets an outer nested block.
        /// </summary>
        public static readonly VarUInt32Operator Br;

        /// <summary>
        /// The 'br_if' operator: a conditional break that targets an outer nested block.
        /// </summary>
        public static readonly VarUInt32Operator BrIf;

        /// <summary>
        /// The 'br_table' operator, which begins a break table.
        /// </summary>
        public static readonly BrTableOperator BrTable;

        /// <summary>
        /// The 'return' operator, which returns zero or one value from a function.
        /// </summary>
        public static readonly NullaryOperator Return;

        /// <summary>
        /// The 'drop' operator, which pops the top-of-stack value and ignores it.
        /// </summary>
        public static readonly NullaryOperator Drop;

        /// <summary>
        /// The 'select' operator, which selects one of two values based on a condition.
        /// </summary>
        public static readonly NullaryOperator Select;

        /// <summary>
        /// The 'call' operator, which calls a function by its index.
        /// </summary>
        public static readonly VarUInt32Operator Call;

        /// <summary>
        /// The 'call_indirect' operator, which calls a function pointer.
        /// </summary>
        public static readonly CallIndirectOperator CallIndirect;

        /// <summary>
        /// The 'get_local' operator, which reads a local variable or parameter.
        /// </summary>
        public static readonly VarUInt32Operator GetLocal;

        /// <summary>
        /// The 'set_local' operator, which writes a value to a local variable or parameter.
        /// </summary>
        public static readonly VarUInt32Operator SetLocal;

        /// <summary>
        /// The 'tee_local' operator, which writes a value to a local variable or parameter
        /// and then returns the same value.
        /// </summary>
        public static readonly VarUInt32Operator TeeLocal;

        /// <summary>
        /// The 'get_global' operator, which reads a global variable.
        /// </summary>
        public static readonly VarUInt32Operator GetGlobal;

        /// <summary>
        /// The 'set_global' operator, which writes a value to a global variable.
        /// </summary>
        public static readonly VarUInt32Operator SetGlobal;

        /// <summary>
        /// The 'i32.load' operator, which loads a 32-bit integer from linear memory.
        /// </summary>
        public static readonly MemoryOperator Int32Load;

        /// <summary>
        /// The 'i64.load' operator, which loads a 64-bit integer from linear memory.
        /// </summary>
        public static readonly MemoryOperator Int64Load;

        /// <summary>
        /// The 'f32.load' operator, which loads a 32-bit floating-point number from linear memory.
        /// </summary>
        public static readonly MemoryOperator Float32Load;

        /// <summary>
        /// The 'f64.load' operator, which loads a 64-bit floating-point number from linear memory.
        /// </summary>
        public static readonly MemoryOperator Float64Load;

        /// <summary>
        /// The 'i32.load8_s' operator, which loads a byte from memory and sign-extends it to
        /// a 32-bit integer.
        /// </summary>
        public static readonly MemoryOperator Int32Load8S;

        /// <summary>
        /// The 'i32.load8_u' operator, which loads a byte from memory and zero-extends it to
        /// a 32-bit integer.
        /// </summary>
        public static readonly MemoryOperator Int32Load8U;

        /// <summary>
        /// The 'i32.load16_s' operator, which loads a 16-bit integer from memory and
        /// sign-extends it to a 32-bit integer.
        /// </summary>
        public static readonly MemoryOperator Int32Load16S;

        /// <summary>
        /// The 'i32.load16_u' operator, which loads a 16-bit integer from memory and
        /// zero-extends it to a 32-bit integer.
        /// </summary>
        public static readonly MemoryOperator Int32Load16U;

        /// <summary>
        /// The 'i64.load8_s' operator, which loads a byte from memory and sign-extends it to
        /// a 64-bit integer.
        /// </summary>
        public static readonly MemoryOperator Int64Load8S;

        /// <summary>
        /// The 'i64.load8_u' operator, which loads a byte from memory and zero-extends it to
        /// a 64-bit integer.
        /// </summary>
        public static readonly MemoryOperator Int64Load8U;

        /// <summary>
        /// The 'i64.load16_s' operator, which loads a 16-bit integer from memory and
        /// sign-extends it to a 64-bit integer.
        /// </summary>
        public static readonly MemoryOperator Int64Load16S;

        /// <summary>
        /// The 'i64.load16_u' operator, which loads a 16-bit integer from memory and
        /// zero-extends it to a 64-bit integer.
        /// </summary>
        public static readonly MemoryOperator Int64Load16U;

        /// <summary>
        /// The 'i64.load32_s' operator, which loads a 32-bit integer from memory and
        /// sign-extends it to a 64-bit integer.
        /// </summary>
        public static readonly MemoryOperator Int64Load32S;

        /// <summary>
        /// The 'i64.load32_u' operator, which loads a 32-bit integer from memory and
        /// zero-extends it to a 64-bit integer.
        /// </summary>
        public static readonly MemoryOperator Int64Load32U;

        /// <summary>
        /// The 'i32.store' operator, which stores a 32-bit integer in linear memory.
        /// </summary>
        public static readonly MemoryOperator Int32Store;

        /// <summary>
        /// The 'i64.store' operator, which stores a 64-bit integer in linear memory.
        /// </summary>
        public static readonly MemoryOperator Int64Store;

        /// <summary>
        /// The 'f32.store' operator, which stores a 32-bit floating-point number in
        /// linear memory.
        /// </summary>
        public static readonly MemoryOperator Float32Store;

        /// <summary>
        /// The 'f64.store' operator, which stores a 64-bit floating-point number in
        /// linear memory.
        /// </summary>
        public static readonly MemoryOperator Float64Store;

        /// <summary>
        /// The 'i32.store' operator, which truncates a 32-bit integer to a byte and stores
        /// it in linear memory.
        /// </summary>
        public static readonly MemoryOperator Int32Store8;

        /// <summary>
        /// The 'i32.store' operator, which truncates a 32-bit integer to a 16-bit integer
        /// and stores it in linear memory.
        /// </summary>
        public static readonly MemoryOperator Int32Store16;

        /// <summary>
        /// The 'i64.store' operator, which truncates a 64-bit integer to a byte and stores
        /// it in linear memory.
        /// </summary>
        public static readonly MemoryOperator Int64Store8;

        /// <summary>
        /// The 'i64.store' operator, which truncates a 64-bit integer to a 16-bit integer
        /// and stores it in linear memory.
        /// </summary>
        public static readonly MemoryOperator Int64Store16;

        /// <summary>
        /// The 'i64.store' operator, which truncates a 64-bit integer to a 32-bit integer
        /// and stores it in linear memory.
        /// </summary>
        public static readonly MemoryOperator Int64Store32;

        /// <summary>
        /// The 'current_memory' operator, which queries the memory size.
        /// </summary>
        public static readonly VarUInt32Operator CurrentMemory;

        /// <summary>
        /// The 'grow_memory' operator, which grows the memory size.
        /// </summary>
        public static readonly VarUInt32Operator GrowMemory;

        /// <summary>
        /// The 'i32.const' operator, which loads a constant 32-bit integer onto the stack.
        /// </summary>
        public static readonly VarInt32Operator Int32Const;

        /// <summary>
        /// The 'i64.const' operator, which loads a constant 64-bit integer onto the stack.
        /// </summary>
        public static readonly VarInt64Operator Int64Const;

        /// <summary>
        /// The 'f32.const' operator, which loads a constant 32-bit floating-point number onto the stack.
        /// </summary>
        public static readonly Float32Operator Float32Const;

        /// <summary>
        /// The 'f64.const' operator, which loads a constant 64-bit floating-point number onto the stack.
        /// </summary>
        public static readonly Float64Operator Float64Const;

        /// <summary>
        /// The 'else' opcode, which begins an 'if' expression's 'else' block.
        /// </summary>
        public const byte ElseOpCode = 0x05;

        /// <summary>
        /// The 'end' opcode, which ends a block, loop or if.
        /// </summary>
        public const byte EndOpCode = 0x0b;

        #region Auto-generated nullaries
        // This region was auto-generated by nullary-opcode-generator. Please don't make any
        // manual changes.

        /// <summary>
        /// The 'i32.eqz' operator: compare equal to zero (return 1 if operand is zero, 0 otherwise).
        /// </summary>
        public static readonly NullaryOperator Int32Eqz;

        /// <summary>
        /// The 'i32.eq' operator: sign-agnostic compare equal.
        /// </summary>
        public static readonly NullaryOperator Int32Eq;

        /// <summary>
        /// The 'i32.ne' operator: sign-agnostic compare unequal.
        /// </summary>
        public static readonly NullaryOperator Int32Ne;

        /// <summary>
        /// The 'i32.lt_s' operator: signed less than.
        /// </summary>
        public static readonly NullaryOperator Int32LtS;

        /// <summary>
        /// The 'i32.lt_u' operator: unsigned less than.
        /// </summary>
        public static readonly NullaryOperator Int32LtU;

        /// <summary>
        /// The 'i32.gt_s' operator: signed greater than.
        /// </summary>
        public static readonly NullaryOperator Int32GtS;

        /// <summary>
        /// The 'i32.gt_u' operator: unsigned greater than.
        /// </summary>
        public static readonly NullaryOperator Int32GtU;

        /// <summary>
        /// The 'i32.le_s' operator: signed less than or equal.
        /// </summary>
        public static readonly NullaryOperator Int32LeS;

        /// <summary>
        /// The 'i32.le_u' operator: unsigned less than or equal.
        /// </summary>
        public static readonly NullaryOperator Int32LeU;

        /// <summary>
        /// The 'i32.ge_s' operator: signed greater than or equal.
        /// </summary>
        public static readonly NullaryOperator Int32GeS;

        /// <summary>
        /// The 'i32.ge_u' operator: unsigned greater than or equal.
        /// </summary>
        public static readonly NullaryOperator Int32GeU;

        /// <summary>
        /// The 'i64.eqz' operator: compare equal to zero (return 1 if operand is zero, 0 otherwise).
        /// </summary>
        public static readonly NullaryOperator Int64Eqz;

        /// <summary>
        /// The 'i64.eq' operator: sign-agnostic compare equal.
        /// </summary>
        public static readonly NullaryOperator Int64Eq;

        /// <summary>
        /// The 'i64.ne' operator: sign-agnostic compare unequal.
        /// </summary>
        public static readonly NullaryOperator Int64Ne;

        /// <summary>
        /// The 'i64.lt_s' operator: signed less than.
        /// </summary>
        public static readonly NullaryOperator Int64LtS;

        /// <summary>
        /// The 'i64.lt_u' operator: unsigned less than.
        /// </summary>
        public static readonly NullaryOperator Int64LtU;

        /// <summary>
        /// The 'i64.gt_s' operator: signed greater than.
        /// </summary>
        public static readonly NullaryOperator Int64GtS;

        /// <summary>
        /// The 'i64.gt_u' operator: unsigned greater than.
        /// </summary>
        public static readonly NullaryOperator Int64GtU;

        /// <summary>
        /// The 'i64.le_s' operator: signed less than or equal.
        /// </summary>
        public static readonly NullaryOperator Int64LeS;

        /// <summary>
        /// The 'i64.le_u' operator: unsigned less than or equal.
        /// </summary>
        public static readonly NullaryOperator Int64LeU;

        /// <summary>
        /// The 'i64.ge_s' operator: signed greater than or equal.
        /// </summary>
        public static readonly NullaryOperator Int64GeS;

        /// <summary>
        /// The 'i64.ge_u' operator: unsigned greater than or equal.
        /// </summary>
        public static readonly NullaryOperator Int64GeU;

        /// <summary>
        /// The 'f32.eq' operator: compare ordered and equal.
        /// </summary>
        public static readonly NullaryOperator Float32Eq;

        /// <summary>
        /// The 'f32.ne' operator: compare unordered or unequal.
        /// </summary>
        public static readonly NullaryOperator Float32Ne;

        /// <summary>
        /// The 'f32.lt' operator: compare ordered and less than.
        /// </summary>
        public static readonly NullaryOperator Float32Lt;

        /// <summary>
        /// The 'f32.gt' operator: compare ordered and greater than.
        /// </summary>
        public static readonly NullaryOperator Float32Gt;

        /// <summary>
        /// The 'f32.le' operator: compare ordered and less than or equal.
        /// </summary>
        public static readonly NullaryOperator Float32Le;

        /// <summary>
        /// The 'f32.ge' operator: compare ordered and greater than or equal.
        /// </summary>
        public static readonly NullaryOperator Float32Ge;

        /// <summary>
        /// The 'f64.eq' operator: compare ordered and equal.
        /// </summary>
        public static readonly NullaryOperator Float64Eq;

        /// <summary>
        /// The 'f64.ne' operator: compare unordered or unequal.
        /// </summary>
        public static readonly NullaryOperator Float64Ne;

        /// <summary>
        /// The 'f64.lt' operator: compare ordered and less than.
        /// </summary>
        public static readonly NullaryOperator Float64Lt;

        /// <summary>
        /// The 'f64.gt' operator: compare ordered and greater than.
        /// </summary>
        public static readonly NullaryOperator Float64Gt;

        /// <summary>
        /// The 'f64.le' operator: compare ordered and less than or equal.
        /// </summary>
        public static readonly NullaryOperator Float64Le;

        /// <summary>
        /// The 'f64.ge' operator: compare ordered and greater than or equal.
        /// </summary>
        public static readonly NullaryOperator Float64Ge;

        /// <summary>
        /// The 'i32.clz' operator: sign-agnostic count leading zero bits (All zero bits are considered leading if the value is zero).
        /// </summary>
        public static readonly NullaryOperator Int32Clz;

        /// <summary>
        /// The 'i32.ctz' operator: sign-agnostic count trailing zero bits (All zero bits are considered trailing if the value is zero).
        /// </summary>
        public static readonly NullaryOperator Int32Ctz;

        /// <summary>
        /// The 'i32.popcnt' operator: sign-agnostic count number of one bits.
        /// </summary>
        public static readonly NullaryOperator Int32Popcnt;

        /// <summary>
        /// The 'i32.add' operator: sign-agnostic addition.
        /// </summary>
        public static readonly NullaryOperator Int32Add;

        /// <summary>
        /// The 'i32.sub' operator: sign-agnostic subtraction.
        /// </summary>
        public static readonly NullaryOperator Int32Sub;

        /// <summary>
        /// The 'i32.mul' operator: sign-agnostic multiplication (lower 32-bits).
        /// </summary>
        public static readonly NullaryOperator Int32Mul;

        /// <summary>
        /// The 'i32.div_s' operator: signed division (result is truncated toward zero).
        /// </summary>
        public static readonly NullaryOperator Int32DivS;

        /// <summary>
        /// The 'i32.div_u' operator: unsigned division (result is floored).
        /// </summary>
        public static readonly NullaryOperator Int32DivU;

        /// <summary>
        /// The 'i32.rem_s' operator: signed remainder (result has the sign of the dividend).
        /// </summary>
        public static readonly NullaryOperator Int32RemS;

        /// <summary>
        /// The 'i32.rem_u' operator: unsigned remainder.
        /// </summary>
        public static readonly NullaryOperator Int32RemU;

        /// <summary>
        /// The 'i32.and' operator: sign-agnostic bitwise and.
        /// </summary>
        public static readonly NullaryOperator Int32And;

        /// <summary>
        /// The 'i32.or' operator: sign-agnostic bitwise inclusive or.
        /// </summary>
        public static readonly NullaryOperator Int32Or;

        /// <summary>
        /// The 'i32.xor' operator: sign-agnostic bitwise exclusive or.
        /// </summary>
        public static readonly NullaryOperator Int32Xor;

        /// <summary>
        /// The 'i32.shl' operator: sign-agnostic shift left.
        /// </summary>
        public static readonly NullaryOperator Int32Shl;

        /// <summary>
        /// The 'i32.shr_s' operator: sign-replicating (arithmetic) shift right.
        /// </summary>
        public static readonly NullaryOperator Int32ShrS;

        /// <summary>
        /// The 'i32.shr_u' operator: zero-replicating (logical) shift right.
        /// </summary>
        public static readonly NullaryOperator Int32ShrU;

        /// <summary>
        /// The 'i32.rotl' operator: sign-agnostic rotate left.
        /// </summary>
        public static readonly NullaryOperator Int32Rotl;

        /// <summary>
        /// The 'i32.rotr' operator: sign-agnostic rotate right.
        /// </summary>
        public static readonly NullaryOperator Int32Rotr;

        /// <summary>
        /// The 'i64.clz' operator: sign-agnostic count leading zero bits (All zero bits are considered leading if the value is zero).
        /// </summary>
        public static readonly NullaryOperator Int64Clz;

        /// <summary>
        /// The 'i64.ctz' operator: sign-agnostic count trailing zero bits (All zero bits are considered trailing if the value is zero).
        /// </summary>
        public static readonly NullaryOperator Int64Ctz;

        /// <summary>
        /// The 'i64.popcnt' operator: sign-agnostic count number of one bits.
        /// </summary>
        public static readonly NullaryOperator Int64Popcnt;

        /// <summary>
        /// The 'i64.add' operator: sign-agnostic addition.
        /// </summary>
        public static readonly NullaryOperator Int64Add;

        /// <summary>
        /// The 'i64.sub' operator: sign-agnostic subtraction.
        /// </summary>
        public static readonly NullaryOperator Int64Sub;

        /// <summary>
        /// The 'i64.mul' operator: sign-agnostic multiplication (lower 32-bits).
        /// </summary>
        public static readonly NullaryOperator Int64Mul;

        /// <summary>
        /// The 'i64.div_s' operator: signed division (result is truncated toward zero).
        /// </summary>
        public static readonly NullaryOperator Int64DivS;

        /// <summary>
        /// The 'i64.div_u' operator: unsigned division (result is floored).
        /// </summary>
        public static readonly NullaryOperator Int64DivU;

        /// <summary>
        /// The 'i64.rem_s' operator: signed remainder (result has the sign of the dividend).
        /// </summary>
        public static readonly NullaryOperator Int64RemS;

        /// <summary>
        /// The 'i64.rem_u' operator: unsigned remainder.
        /// </summary>
        public static readonly NullaryOperator Int64RemU;

        /// <summary>
        /// The 'i64.and' operator: sign-agnostic bitwise and.
        /// </summary>
        public static readonly NullaryOperator Int64And;

        /// <summary>
        /// The 'i64.or' operator: sign-agnostic bitwise inclusive or.
        /// </summary>
        public static readonly NullaryOperator Int64Or;

        /// <summary>
        /// The 'i64.xor' operator: sign-agnostic bitwise exclusive or.
        /// </summary>
        public static readonly NullaryOperator Int64Xor;

        /// <summary>
        /// The 'i64.shl' operator: sign-agnostic shift left.
        /// </summary>
        public static readonly NullaryOperator Int64Shl;

        /// <summary>
        /// The 'i64.shr_s' operator: sign-replicating (arithmetic) shift right.
        /// </summary>
        public static readonly NullaryOperator Int64ShrS;

        /// <summary>
        /// The 'i64.shr_u' operator: zero-replicating (logical) shift right.
        /// </summary>
        public static readonly NullaryOperator Int64ShrU;

        /// <summary>
        /// The 'i64.rotl' operator: sign-agnostic rotate left.
        /// </summary>
        public static readonly NullaryOperator Int64Rotl;

        /// <summary>
        /// The 'i64.rotr' operator: sign-agnostic rotate right.
        /// </summary>
        public static readonly NullaryOperator Int64Rotr;

        /// <summary>
        /// The 'f32.abs' operator: absolute value.
        /// </summary>
        public static readonly NullaryOperator Float32Abs;

        /// <summary>
        /// The 'f32.neg' operator: negation.
        /// </summary>
        public static readonly NullaryOperator Float32Neg;

        /// <summary>
        /// The 'f32.ceil' operator: ceiling operator.
        /// </summary>
        public static readonly NullaryOperator Float32Ceil;

        /// <summary>
        /// The 'f32.floor' operator: floor operator.
        /// </summary>
        public static readonly NullaryOperator Float32Floor;

        /// <summary>
        /// The 'f32.trunc' operator: round to nearest integer towards zero.
        /// </summary>
        public static readonly NullaryOperator Float32Trunc;

        /// <summary>
        /// The 'f32.nearest' operator: round to nearest integer, ties to even.
        /// </summary>
        public static readonly NullaryOperator Float32Nearest;

        /// <summary>
        /// The 'f32.sqrt' operator: square root.
        /// </summary>
        public static readonly NullaryOperator Float32Sqrt;

        /// <summary>
        /// The 'f32.add' operator: addition.
        /// </summary>
        public static readonly NullaryOperator Float32Add;

        /// <summary>
        /// The 'f32.sub' operator: subtraction.
        /// </summary>
        public static readonly NullaryOperator Float32Sub;

        /// <summary>
        /// The 'f32.mul' operator: multiplication.
        /// </summary>
        public static readonly NullaryOperator Float32Mul;

        /// <summary>
        /// The 'f32.div' operator: division.
        /// </summary>
        public static readonly NullaryOperator Float32Div;

        /// <summary>
        /// The 'f32.min' operator: minimum (binary operator); if either operand is NaN, returns NaN.
        /// </summary>
        public static readonly NullaryOperator Float32Min;

        /// <summary>
        /// The 'f32.max' operator: maximum (binary operator); if either operand is NaN, returns NaN.
        /// </summary>
        public static readonly NullaryOperator Float32Max;

        /// <summary>
        /// The 'f32.copysign' operator: copysign.
        /// </summary>
        public static readonly NullaryOperator Float32Copysign;

        /// <summary>
        /// The 'f64.abs' operator: absolute value.
        /// </summary>
        public static readonly NullaryOperator Float64Abs;

        /// <summary>
        /// The 'f64.neg' operator: negation.
        /// </summary>
        public static readonly NullaryOperator Float64Neg;

        /// <summary>
        /// The 'f64.ceil' operator: ceiling operator.
        /// </summary>
        public static readonly NullaryOperator Float64Ceil;

        /// <summary>
        /// The 'f64.floor' operator: floor operator.
        /// </summary>
        public static readonly NullaryOperator Float64Floor;

        /// <summary>
        /// The 'f64.trunc' operator: round to nearest integer towards zero.
        /// </summary>
        public static readonly NullaryOperator Float64Trunc;

        /// <summary>
        /// The 'f64.nearest' operator: round to nearest integer, ties to even.
        /// </summary>
        public static readonly NullaryOperator Float64Nearest;

        /// <summary>
        /// The 'f64.sqrt' operator: square root.
        /// </summary>
        public static readonly NullaryOperator Float64Sqrt;

        /// <summary>
        /// The 'f64.add' operator: addition.
        /// </summary>
        public static readonly NullaryOperator Float64Add;

        /// <summary>
        /// The 'f64.sub' operator: subtraction.
        /// </summary>
        public static readonly NullaryOperator Float64Sub;

        /// <summary>
        /// The 'f64.mul' operator: multiplication.
        /// </summary>
        public static readonly NullaryOperator Float64Mul;

        /// <summary>
        /// The 'f64.div' operator: division.
        /// </summary>
        public static readonly NullaryOperator Float64Div;

        /// <summary>
        /// The 'f64.min' operator: minimum (binary operator); if either operand is NaN, returns NaN.
        /// </summary>
        public static readonly NullaryOperator Float64Min;

        /// <summary>
        /// The 'f64.max' operator: maximum (binary operator); if either operand is NaN, returns NaN.
        /// </summary>
        public static readonly NullaryOperator Float64Max;

        /// <summary>
        /// The 'f64.copysign' operator: copysign.
        /// </summary>
        public static readonly NullaryOperator Float64Copysign;

        /// <summary>
        /// The 'i32.wrap/i64' operator: wrap a 64-bit integer to a 32-bit integer.
        /// </summary>
        public static readonly NullaryOperator Int32WrapInt64;

        /// <summary>
        /// The 'i32.trunc_s/f32' operator: truncate a 32-bit float to a signed 32-bit integer.
        /// </summary>
        public static readonly NullaryOperator Int32TruncSFloat32;

        /// <summary>
        /// The 'i32.trunc_u/f32' operator: truncate a 32-bit float to an unsigned 32-bit integer.
        /// </summary>
        public static readonly NullaryOperator Int32TruncUFloat32;

        /// <summary>
        /// The 'i32.trunc_s/f64' operator: truncate a 64-bit float to a signed 32-bit integer.
        /// </summary>
        public static readonly NullaryOperator Int32TruncSFloat64;

        /// <summary>
        /// The 'i32.trunc_u/f64' operator: truncate a 64-bit float to an unsigned 32-bit integer.
        /// </summary>
        public static readonly NullaryOperator Int32TruncUFloat64;

        /// <summary>
        /// The 'i64.extend_s/i32' operator: extend a signed 32-bit integer to a 64-bit integer.
        /// </summary>
        public static readonly NullaryOperator Int64ExtendSInt32;

        /// <summary>
        /// The 'i64.extend_u/i32' operator: extend an unsigned 32-bit integer to a 64-bit integer.
        /// </summary>
        public static readonly NullaryOperator Int64ExtendUInt32;

        /// <summary>
        /// The 'i64.trunc_s/f32' operator: truncate a 32-bit float to a signed 64-bit integer.
        /// </summary>
        public static readonly NullaryOperator Int64TruncSFloat32;

        /// <summary>
        /// The 'i64.trunc_u/f32' operator: truncate a 32-bit float to an unsigned 64-bit integer.
        /// </summary>
        public static readonly NullaryOperator Int64TruncUFloat32;

        /// <summary>
        /// The 'i64.trunc_s/f64' operator: truncate a 64-bit float to a signed 64-bit integer.
        /// </summary>
        public static readonly NullaryOperator Int64TruncSFloat64;

        /// <summary>
        /// The 'i64.trunc_u/f64' operator: truncate a 64-bit float to an unsigned 64-bit integer.
        /// </summary>
        public static readonly NullaryOperator Int64TruncUFloat64;

        /// <summary>
        /// The 'f32.convert_s/i32' operator: convert a signed 32-bit integer to a 32-bit float.
        /// </summary>
        public static readonly NullaryOperator Float32ConvertSInt32;

        /// <summary>
        /// The 'f32.convert_u/i32' operator: convert an unsigned 32-bit integer to a 32-bit float.
        /// </summary>
        public static readonly NullaryOperator Float32ConvertUInt32;

        /// <summary>
        /// The 'f32.convert_s/i64' operator: convert a signed 64-bit integer to a 32-bit float.
        /// </summary>
        public static readonly NullaryOperator Float32ConvertSInt64;

        /// <summary>
        /// The 'f32.convert_u/i64' operator: convert an unsigned 64-bit integer to a 32-bit float.
        /// </summary>
        public static readonly NullaryOperator Float32ConvertUInt64;

        /// <summary>
        /// The 'f32.demote/f64' operator: demote a 64-bit float to a 32-bit float.
        /// </summary>
        public static readonly NullaryOperator Float32DemoteFloat64;

        /// <summary>
        /// The 'f64.convert_s/i32' operator: convert a signed 32-bit integer to a 64-bit float.
        /// </summary>
        public static readonly NullaryOperator Float64ConvertSInt32;

        /// <summary>
        /// The 'f64.convert_u/i32' operator: convert an unsigned 32-bit integer to a 64-bit float.
        /// </summary>
        public static readonly NullaryOperator Float64ConvertUInt32;

        /// <summary>
        /// The 'f64.convert_s/i64' operator: convert a signed 64-bit integer to a 64-bit float.
        /// </summary>
        public static readonly NullaryOperator Float64ConvertSInt64;

        /// <summary>
        /// The 'f64.convert_u/i64' operator: convert an unsigned 64-bit integer to a 64-bit float.
        /// </summary>
        public static readonly NullaryOperator Float64ConvertUInt64;

        /// <summary>
        /// The 'f64.promote/f32' operator: promote a 32-bit float to a 64-bit float.
        /// </summary>
        public static readonly NullaryOperator Float64PromoteFloat32;

        /// <summary>
        /// The 'i32.reinterpret/f32' operator: reinterpret the bits of a 32-bit float as a 32-bit integer.
        /// </summary>
        public static readonly NullaryOperator Int32ReinterpretFloat32;

        /// <summary>
        /// The 'i64.reinterpret/f64' operator: reinterpret the bits of a 64-bit float as a 64-bit integer.
        /// </summary>
        public static readonly NullaryOperator Int64ReinterpretFloat64;

        /// <summary>
        /// The 'f32.reinterpret/i32' operator: reinterpret the bits of a 32-bit integer as a 32-bit float.
        /// </summary>
        public static readonly NullaryOperator Float32ReinterpretInt32;

        /// <summary>
        /// The 'f64.reinterpret/i64' operator: reinterpret the bits of a 64-bit integer as a 64-bit float.
        /// </summary>
        public static readonly NullaryOperator Float64ReinterpretInt64;

        #endregion
    }
}