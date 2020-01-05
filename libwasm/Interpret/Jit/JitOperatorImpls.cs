using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Wasm.Instructions;

namespace Wasm.Interpret.Jit
{
    using InstructionImpl = Action<CompilerContext, ILGenerator>;

    /// <summary>
    /// A collection of methods that compiler WebAssembly instructions to IL.
    /// </summary>
    public static class JitOperatorImpls
    {
        private static InstructionImpl ImplementAsOpCode(OpCode op, WasmValueType? resultType = null, params WasmValueType[] parameterTypes)
        {
            return (context, gen) =>
            {
                var paramTypesOnStack = context.Pop(parameterTypes.Length);
                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    if (parameterTypes[i] != paramTypesOnStack[i])
                    {
                        throw new InvalidOperationException($"Expected type '{parameterTypes[i]}' on stack for argument {i} of opcode '{op}', but got type '{paramTypesOnStack[i]}' instead.");
                    }
                }
                gen.Emit(op);
                if (resultType.HasValue)
                {
                    context.Push(resultType.Value);
                }
            };
        }

        private static InstructionImpl ImplementAsCall(MethodInfo callee)
        {
            return (context, gen) =>
            {
                var parameterTypes = callee.GetParameters().Select(p => ValueHelpers.ToWasmValueType(p.ParameterType)).ToArray();
                var paramTypesOnStack = context.Pop(parameterTypes.Length);
                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    if (parameterTypes[i] != paramTypesOnStack[i])
                    {
                        throw new InvalidOperationException($"Expected type '{parameterTypes[i]}' on stack for argument {i} of method '{callee}', but got type '{paramTypesOnStack[i]}' instead.");
                    }
                }
                gen.Emit(OpCodes.Call, callee);
                if (callee.ReturnType != null && callee.ReturnType != typeof(void))
                {
                    context.Push(ValueHelpers.ToWasmValueType(callee.ReturnType));
                }
            };
        }

        private static InstructionImpl Chain(params InstructionImpl[] impls)
        {
            return (context, gen) =>
            {
                foreach (var impl in impls)
                {
                    impl(context, gen);
                }
            };
        }

        private static InstructionImpl ImplementAsBinaryOpCode(OpCode op, WasmValueType type)
        {
            return ImplementAsOpCode(op, type, type, type);
        }

        private static InstructionImpl ImplementAsComparisonOpCode(OpCode op, WasmValueType type)
        {
            return ImplementAsOpCode(op, WasmValueType.Int32, type, type);
        }

        private static InstructionImpl ImplementAsUnaryOpCode(OpCode op, WasmValueType type)
        {
            return ImplementAsOpCode(op, type, type);
        }

        /// <summary>
        /// Compiles an 'i32.add' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32Add(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Add, WasmValueType.Int32);
        }

        /// <summary>
        /// Compiles an 'i32.clz' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32Clz(Instruction instruction)
        {
            return ImplementAsCall(
                typeof(ValueHelpers).GetMethod(
                    nameof(ValueHelpers.CountLeadingZeros),
                    new[] { typeof(int) }));
        }

        /// <summary>
        /// Compiles an 'i32.ctz' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32Ctz(Instruction instruction)
        {
            return ImplementAsCall(
                typeof(ValueHelpers).GetMethod(
                    nameof(ValueHelpers.CountTrailingZeros),
                    new[] { typeof(int) }));
        }

        /// <summary>
        /// Compiles an 'i32.sub' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32Sub(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Sub, WasmValueType.Int32);
        }

        /// <summary>
        /// Compiles an 'i32.mul' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32Mul(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Mul, WasmValueType.Int32);
        }

        /// <summary>
        /// Compiles an 'i32.div_s' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32DivS(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Div, WasmValueType.Int32);
        }

        /// <summary>
        /// Compiles an 'i32.div_u' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32DivU(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Div_Un, WasmValueType.Int32);
        }

        /// <summary>
        /// Compiles an 'i32.eq' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32Eq(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Ceq, WasmValueType.Int32);
        }

        /// <summary>
        /// Compiles an 'i32.eqz' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32Eqz(Instruction instruction)
        {
            return Chain(Int32Const(Operators.Int32Const.Create(0)), Int32Eq(instruction));
        }

        /// <summary>
        /// Compiles an 'i32.ne' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32Ne(Instruction instruction)
        {
            return Chain(Int32Eq(instruction), Int32Eqz(instruction));
        }

        /// <summary>
        /// Compiles an 'i32.ge_s' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32GeS(Instruction instruction)
        {
            return Chain(Int32LtS(instruction), Int32Eqz(instruction));
        }

        /// <summary>
        /// Compiles an 'i32.ge_u' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32GeU(Instruction instruction)
        {
            return Chain(Int32LtU(instruction), Int32Eqz(instruction));
        }

        /// <summary>
        /// Compiles an 'i32.le_s' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32LeS(Instruction instruction)
        {
            return Chain(Int32GtS(instruction), Int32Eqz(instruction));
        }

        /// <summary>
        /// Compiles an 'i32.le_u' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32LeU(Instruction instruction)
        {
            return Chain(Int32GtU(instruction), Int32Eqz(instruction));
        }

        /// <summary>
        /// Compiles an 'i32.lt_s' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32LtS(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Clt, WasmValueType.Int32);
        }

        /// <summary>
        /// Compiles an 'i32.lt_u' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32LtU(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Clt_Un, WasmValueType.Int32);
        }

        /// <summary>
        /// Compiles an 'i32.gt_s' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32GtS(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Cgt, WasmValueType.Int32);
        }

        /// <summary>
        /// Compiles an 'i32.gt_u' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32GtU(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Cgt_Un, WasmValueType.Int32);
        }

        /// <summary>
        /// Compiles an 'i32.rem_s' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32RemS(Instruction instruction)
        {
            return ImplementAsCall(
                typeof(ValueHelpers).GetMethod(
                    nameof(ValueHelpers.RemS),
                    new[] { typeof(int), typeof(int) }));
        }

        /// <summary>
        /// Compiles an 'i32.rem_u' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32RemU(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Rem_Un, WasmValueType.Int32);
        }

        /// <summary>
        /// Compiles an 'i32.rotl' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32Rotl(Instruction instruction)
        {
            return ImplementAsCall(
                typeof(ValueHelpers).GetMethod(
                    nameof(ValueHelpers.RotateLeft),
                    new[] { typeof(int), typeof(int) }));
        }

        /// <summary>
        /// Compiles an 'i32.rotr' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32Rotr(Instruction instruction)
        {
            return ImplementAsCall(
                typeof(ValueHelpers).GetMethod(
                    nameof(ValueHelpers.RotateRight),
                    new[] { typeof(int), typeof(int) }));
        }

        /// <summary>
        /// Compiles an 'i32.and' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32And(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.And, WasmValueType.Int32);
        }

        /// <summary>
        /// Compiles an 'i32.or' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32Or(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Or, WasmValueType.Int32);
        }

        /// <summary>
        /// Compiles an 'i32.popcnt' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32Popcnt(Instruction instruction)
        {
            return ImplementAsCall(
                typeof(ValueHelpers).GetMethod(
                    nameof(ValueHelpers.PopCount),
                    new[] { typeof(int) }));
        }

        /// <summary>
        /// Compiles an 'i32.wrap/i64' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32WrapInt64(Instruction instruction)
        {
            return ImplementAsOpCode(OpCodes.Conv_I4, WasmValueType.Int32, WasmValueType.Int64);
        }

        /// <summary>
        /// Compiles an 'i32.xor' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32Xor(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Xor, WasmValueType.Int32);
        }

        /// <summary>
        /// Compiles an 'i32.shl' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32Shl(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Shl, WasmValueType.Int32);
        }

        /// <summary>
        /// Compiles an 'i32.shl_s' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32ShrS(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Shr, WasmValueType.Int32);
        }

        /// <summary>
        /// Compiles an 'i32.shl_u' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32ShrU(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Shr_Un, WasmValueType.Int32);
        }

        /// <summary>
        /// Compiles an 'i64.add' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64Add(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Add, WasmValueType.Int64);
        }

        /// <summary>
        /// Compiles an 'i64.clz' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64Clz(Instruction instruction)
        {
            return Chain(
                ImplementAsCall(
                    typeof(ValueHelpers).GetMethod(
                        nameof(ValueHelpers.CountLeadingZeros),
                        new[] { typeof(long) })),
                Int64ExtendUInt32(instruction));
        }

        /// <summary>
        /// Compiles an 'i64.ctz' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64Ctz(Instruction instruction)
        {
            return Chain(
                ImplementAsCall(
                    typeof(ValueHelpers).GetMethod(
                        nameof(ValueHelpers.CountTrailingZeros),
                        new[] { typeof(long) })),
                Int64ExtendUInt32(instruction));
        }

        /// <summary>
        /// Compiles an 'i64.sub' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64Sub(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Sub, WasmValueType.Int64);
        }

        /// <summary>
        /// Compiles an 'i64.mul' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64Mul(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Mul, WasmValueType.Int64);
        }

        /// <summary>
        /// Compiles an 'i64.div_s' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64DivS(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Div, WasmValueType.Int64);
        }

        /// <summary>
        /// Compiles an 'i64.div_u' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64DivU(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Div_Un, WasmValueType.Int64);
        }

        /// <summary>
        /// Compiles an 'i64.eq' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64Eq(Instruction instruction)
        {
            return ImplementAsComparisonOpCode(OpCodes.Ceq, WasmValueType.Int64);
        }

        /// <summary>
        /// Compiles an 'i64.eqz' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64Eqz(Instruction instruction)
        {
            return Chain(Int64Const(Operators.Int64Const.Create(0)), Int64Eq(instruction));
        }

        /// <summary>
        /// Compiles an 'i64.extend_s/i32' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64ExtendSInt32(Instruction instruction)
        {
            return ImplementAsOpCode(OpCodes.Conv_I8, WasmValueType.Int64, WasmValueType.Int32);
        }

        /// <summary>
        /// Compiles an 'i64.extend_u/i32' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64ExtendUInt32(Instruction instruction)
        {
            return ImplementAsOpCode(OpCodes.Conv_U8, WasmValueType.Int64, WasmValueType.Int32);
        }

        /// <summary>
        /// Compiles an 'i64.ne' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64Ne(Instruction instruction)
        {
            return Chain(Int64Eq(instruction), Int32Eqz(instruction));
        }

        /// <summary>
        /// Compiles an 'i64.ge_s' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64GeS(Instruction instruction)
        {
            return Chain(Int64LtS(instruction), Int32Eqz(instruction));
        }

        /// <summary>
        /// Compiles an 'i64.ge_u' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64GeU(Instruction instruction)
        {
            return Chain(Int64LtU(instruction), Int32Eqz(instruction));
        }

        /// <summary>
        /// Compiles an 'i64.le_s' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64LeS(Instruction instruction)
        {
            return Chain(Int64GtS(instruction), Int32Eqz(instruction));
        }

        /// <summary>
        /// Compiles an 'i64.le_u' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64LeU(Instruction instruction)
        {
            return Chain(Int64GtU(instruction), Int32Eqz(instruction));
        }

        /// <summary>
        /// Compiles an 'i64.lt_s' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64LtS(Instruction instruction)
        {
            return ImplementAsComparisonOpCode(OpCodes.Clt, WasmValueType.Int64);
        }

        /// <summary>
        /// Compiles an 'i64.lt_u' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64LtU(Instruction instruction)
        {
            return ImplementAsComparisonOpCode(OpCodes.Clt_Un, WasmValueType.Int64);
        }

        /// <summary>
        /// Compiles an 'i64.gt_s' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64GtS(Instruction instruction)
        {
            return ImplementAsComparisonOpCode(OpCodes.Cgt, WasmValueType.Int64);
        }

        /// <summary>
        /// Compiles an 'i64.gt_u' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64GtU(Instruction instruction)
        {
            return ImplementAsComparisonOpCode(OpCodes.Cgt_Un, WasmValueType.Int64);
        }

        /// <summary>
        /// Compiles an 'i64.rem_s' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64RemS(Instruction instruction)
        {
            return ImplementAsCall(
                typeof(ValueHelpers).GetMethod(
                    nameof(ValueHelpers.RemS),
                    new[] { typeof(long), typeof(long) }));
        }

        /// <summary>
        /// Compiles an 'i64.rem_u' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64RemU(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Rem_Un, WasmValueType.Int64);
        }

        /// <summary>
        /// Compiles an 'i64.rotl' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64Rotl(Instruction instruction)
        {
            return ImplementAsCall(
                typeof(ValueHelpers).GetMethod(
                    nameof(ValueHelpers.RotateLeft),
                    new[] { typeof(long), typeof(long) }));
        }

        /// <summary>
        /// Compiles an 'i64.rotr' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64Rotr(Instruction instruction)
        {
            return ImplementAsCall(
                typeof(ValueHelpers).GetMethod(
                    nameof(ValueHelpers.RotateRight),
                    new[] { typeof(long), typeof(long) }));
        }

        /// <summary>
        /// Compiles an 'i64.and' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64And(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.And, WasmValueType.Int64);
        }

        /// <summary>
        /// Compiles an 'i64.or' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64Or(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Or, WasmValueType.Int64);
        }

        /// <summary>
        /// Compiles an 'i64.popcnt' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64Popcnt(Instruction instruction)
        {
            return Chain(
                ImplementAsCall(
                    typeof(ValueHelpers).GetMethod(
                        nameof(ValueHelpers.PopCount),
                        new[] { typeof(long) })),
                Int64ExtendUInt32(instruction));
        }

        /// <summary>
        /// Compiles an 'i64.xor' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64Xor(Instruction instruction)
        {
            return ImplementAsBinaryOpCode(OpCodes.Xor, WasmValueType.Int64);
        }

        /// <summary>
        /// Compiles an 'i64.shl' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64Shl(Instruction instruction)
        {
            return Chain(
                Int32WrapInt64(instruction),
                ImplementAsOpCode(OpCodes.Shl, WasmValueType.Int64, WasmValueType.Int64, WasmValueType.Int32));
        }

        /// <summary>
        /// Compiles an 'i64.shl_s' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64ShrS(Instruction instruction)
        {
            return Chain(
                Int32WrapInt64(instruction),
                ImplementAsOpCode(OpCodes.Shr, WasmValueType.Int64, WasmValueType.Int64, WasmValueType.Int32));
        }

        /// <summary>
        /// Compiles an 'i64.shl_u' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64ShrU(Instruction instruction)
        {
            return Chain(
                Int32WrapInt64(instruction),
                ImplementAsOpCode(OpCodes.Shr_Un, WasmValueType.Int64, WasmValueType.Int64, WasmValueType.Int32));
        }

        /// <summary>
        /// Compiles an 'i32.const' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int32Const(Instruction instruction)
        {
            var immediate = Operators.Int32Const.CastInstruction(instruction).Immediate;
            return (context, gen) =>
            {
                gen.Emit(OpCodes.Ldc_I4, immediate);
                context.Push(WasmValueType.Int32);
            };
        }

        /// <summary>
        /// Compiles an 'i64.const' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Int64Const(Instruction instruction)
        {
            var immediate = Operators.Int64Const.CastInstruction(instruction).Immediate;
            return (context, gen) =>
            {
                gen.Emit(OpCodes.Ldc_I8, immediate);
                context.Push(WasmValueType.Int64);
            };
        }

        /// <summary>
        /// Compiles an 'f32.const' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Float32Const(Instruction instruction)
        {
            var immediate = Operators.Float32Const.CastInstruction(instruction).Immediate;
            return (context, gen) =>
            {
                gen.Emit(OpCodes.Ldc_R4, immediate);
                context.Push(WasmValueType.Float32);
            };
        }

        /// <summary>
        /// Compiles an 'f64.const' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Float64Const(Instruction instruction)
        {
            var immediate = Operators.Float64Const.CastInstruction(instruction).Immediate;
            return (context, gen) =>
            {
                gen.Emit(OpCodes.Ldc_R8, immediate);
                context.Push(WasmValueType.Float64);
            };
        }

        /// <summary>
        /// Compiles a 'nop' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Nop(Instruction instruction)
        {
            return (context, gen) => { };
        }

        /// <summary>
        /// Compiles a 'drop' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Drop(Instruction instruction)
        {
            return (context, gen) =>
            {
                context.Pop();
                gen.Emit(OpCodes.Pop);
            };
        }

        /// <summary>
        /// Compiles a 'select' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl Select(Instruction instruction)
        {
            return (context, gen) =>
            {
                context.Pop();
                var rhsType = context.Pop();
                var lhsType = context.Pop();
                var ifLabel = gen.DefineLabel();
                var endLabel = gen.DefineLabel();
                gen.Emit(OpCodes.Brtrue, ifLabel);

                var rhsLocal = gen.DeclareLocal(ValueHelpers.ToClrType(rhsType));
                gen.Emit(OpCodes.Stloc, rhsLocal);
                gen.Emit(OpCodes.Pop);
                gen.Emit(OpCodes.Ldloc, rhsLocal);
                gen.Emit(OpCodes.Br, endLabel);

                gen.MarkLabel(ifLabel);
                gen.Emit(OpCodes.Pop);

                gen.MarkLabel(endLabel);
                context.Push(lhsType);
            };
        }

        /// <summary>
        /// Compiles a 'get_local' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl GetLocal(Instruction instruction)
        {
            var index = Operators.GetLocal.CastInstruction(instruction).Immediate;
            return (context, gen) =>
            {
                if (index < context.ParameterCount)
                {
                    gen.Emit(OpCodes.Ldarg, (int)index + 1);
                }
                else
                {
                    gen.Emit(OpCodes.Ldloc, context.Locals[index]);
                }
                context.Push(context.LocalTypes[(int)index]);
            };
        }

        /// <summary>
        /// Compiles a 'set_local' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl SetLocal(Instruction instruction)
        {
            var index = Operators.SetLocal.CastInstruction(instruction).Immediate;
            return (context, gen) =>
            {
                if (index < context.ParameterCount)
                {
                    gen.Emit(OpCodes.Starg, (int)index + 1);
                }
                else
                {
                    gen.Emit(OpCodes.Stloc, context.Locals[index]);
                }
            };
        }

        /// <summary>
        /// Compiles a 'tee_local' instruction.
        /// </summary>
        /// <param name="instruction">The instruction to compile to an implementation.</param>
        public static InstructionImpl TeeLocal(Instruction instruction)
        {
            return (context, gen) =>
            {
                gen.Emit(OpCodes.Dup);
                var type = context.Pop();
                context.Push(type);
                context.Push(type);
                SetLocal(instruction)(context, gen);
            };
        }
    }
}
