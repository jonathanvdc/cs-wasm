using System;
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
        /// Executes a 'block' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Block(Instruction Value, InterpreterContext Context)
        {
            var contents = Operators.Block.CastInstruction(Value).Contents;
            var interpreter = Context.Module.Interpreter;
            for (int i = 0; i < contents.Count; i++)
            {
                interpreter.Interpret(contents[i], Context);
                if (Context.BreakRequested)
                {
                    Context.BreakDepth--;
                    return;
                }
            }
        }

        /// <summary>
        /// Executes a 'loop' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Loop(Instruction Value, InterpreterContext Context)
        {
            var contents = Operators.Loop.CastInstruction(Value).Contents;
            var interpreter = Context.Module.Interpreter;
            for (int i = 0; i < contents.Count;)
            {
                interpreter.Interpret(contents[i], Context);
                if (Context.BreakRequested)
                {
                    if (Context.BreakDepth > 0)
                    {
                        // This loop is the break's target. We should decrement the
                        // break depth to terminate the break request and then re-start
                        // the loop.
                        Context.BreakDepth--;
                        i = 0;
                    }
                    else
                    {
                        // This loop is not the break's target. We should terminate the loop.
                        Context.BreakDepth--;
                        return;
                    }
                }
                else
                {
                    i++;
                }
            }
        }


        /// <summary>
        /// Executes an 'if' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void If(Instruction Value, InterpreterContext Context)
        {
            // Determine which branch we should take.
            var instr = Operators.If.CastInstruction(Value);
            var condVal = Context.Pop<int>();
            var bodyToRun = condVal != 0
                ? instr.IfBranch
                : instr.ElseBranch;

            if (bodyToRun != null)
            {
                // Create a block and run it.
                var block = Operators.Block.Create(instr.Type, bodyToRun);
                Block(block, Context);
            }
        }

        /// <summary>
        /// Executes a 'br' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Br(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.Br.CastInstruction(Value);
            Context.BreakDepth = (int)instr.Immediate;
        }

        /// <summary>
        /// Executes a 'br_if' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void BrIf(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.BrIf.CastInstruction(Value);
            if (Context.Pop<int>() != 0)
            {
                Context.BreakDepth = (int)instr.Immediate;
            }
        }

        /// <summary>
        /// Executes a 'br_table' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void BrTable(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.BrTable.CastInstruction(Value);
            int index = Context.Pop<int>();
            if (index < 0 || index > instr.TargetTable.Count)
            {
                Context.BreakDepth = (int)instr.DefaultTarget;
            }
            else
            {
                Context.BreakDepth = (int)instr.TargetTable[index];
            }
        }

        /// <summary>
        /// Executes a 'return' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Return(Instruction Value, InterpreterContext Context)
        {
            Context.Return();
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
        /// Executes a 'select' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Select(Instruction Value, InterpreterContext Context)
        {
            // Stack layout:
            //
            //     lhs (any type)
            //     rhs (same type as `lhs`)
            //     condition (i32)
            //

            // Pop operands from the stack.
            int condVal = Context.Pop<int>();
            var rhs = Context.Pop<object>();
            var lhs = Context.Pop<object>();

            // Push the lhs onto the stack if the condition
            // is truthy; otherwise, push the rhs onto the
            // stack.
            Context.Push<object>(condVal != 0 ? lhs : rhs);
        }

        /// <summary>
        /// Executes a 'call' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Call(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.Call.CastInstruction(Value);
            var funcDef = Context.Module.Functions[(int)instr.Immediate];

            var args = Context.Pop<object>(funcDef.ParameterTypes.Count);
            var results = funcDef.Invoke(args);
            Context.Push<object>(results);
        }

        /// <summary>
        /// Executes a 'call_indirect' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void CallIndirect(Instruction Value, InterpreterContext Context)
        {
            var funcDefIndex = Context.Pop<int>();
            var funcDef = Context.Module.Functions[funcDefIndex];

            var args = Context.Pop<object>(funcDef.ParameterTypes.Count);
            var results = funcDef.Invoke(args);
            Context.Push<object>(results);
        }

        /// <summary>
        /// Executes a 'get_local' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void GetLocal(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.GetLocal.CastInstruction(Value);
            Context.Push<object>(Context.Locals[(int)instr.Immediate].Get<object>());
        }

        /// <summary>
        /// Executes a 'set_local' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void SetLocal(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.SetLocal.CastInstruction(Value);
            Context.Locals[(int)instr.Immediate].Set<object>(Context.Pop<object>());
        }

        /// <summary>
        /// Executes a 'tee_local' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void TeeLocal(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.TeeLocal.CastInstruction(Value);
            Context.Locals[(int)instr.Immediate].Set<object>(Context.Peek<object>());
        }

        /// <summary>
        /// Executes a 'get_global' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void GetGlobal(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.GetGlobal.CastInstruction(Value);
            Context.Push<object>(Context.Module.Globals[(int)instr.Immediate].Get<object>());
        }

        /// <summary>
        /// Executes a 'set_global' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void SetGlobal(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.SetGlobal.CastInstruction(Value);
            Context.Module.Globals[(int)instr.Immediate].Set<object>(Context.Pop<object>());
        }

        /// <summary>
        /// Executes an 'i32.load' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32Load(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.Int32Load.CastInstruction(Value);
            var pointer = (uint)Context.Pop<int>() + instr.Offset;
            CheckAlignment(pointer, instr);
            var value = Context.Module.Memories[0].Int32[pointer];
            Context.Push<int>(value);
        }

        /// <summary>
        /// Executes an 'i64.load' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Load(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.Int64Load.CastInstruction(Value);
            var pointer = (uint)Context.Pop<int>() + instr.Offset;
            CheckAlignment(pointer, instr);
            var value = Context.Module.Memories[0].Int64[pointer];
            Context.Push<long>(value);
        }

        /// <summary>
        /// Executes an 'i32.load8_s' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32Load8S(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.Int32Load8S.CastInstruction(Value);
            var pointer = (uint)Context.Pop<int>() + instr.Offset;
            CheckAlignment(pointer, instr);
            var value = Context.Module.Memories[0].Int8[pointer];
            Context.Push<int>(value);
        }

        /// <summary>
        /// Executes an 'i32.load8_u' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32Load8U(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.Int32Load8U.CastInstruction(Value);
            var pointer = (uint)Context.Pop<int>() + instr.Offset;
            CheckAlignment(pointer, instr);
            var value = (byte)Context.Module.Memories[0].Int8[pointer];
            Context.Push<int>(value);
        }

        /// <summary>
        /// Executes an 'i32.load16_s' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32Load16S(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.Int32Load16S.CastInstruction(Value);
            var pointer = (uint)Context.Pop<int>() + instr.Offset;
            CheckAlignment(pointer, instr);
            var value = Context.Module.Memories[0].Int16[pointer];
            Context.Push<int>(value);
        }

        /// <summary>
        /// Executes an 'i32.load16_u' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32Load16U(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.Int32Load16U.CastInstruction(Value);
            var pointer = (uint)Context.Pop<int>() + instr.Offset;
            CheckAlignment(pointer, instr);
            var value = (ushort)Context.Module.Memories[0].Int16[pointer];
            Context.Push<int>(value);
        }

        /// <summary>
        /// Executes an 'i64.load8_s' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Load8S(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.Int64Load8S.CastInstruction(Value);
            var pointer = (uint)Context.Pop<int>() + instr.Offset;
            CheckAlignment(pointer, instr);
            var value = Context.Module.Memories[0].Int8[pointer];
            Context.Push<long>(value);
        }

        /// <summary>
        /// Executes an 'i64.load8_u' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Load8U(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.Int64Load8U.CastInstruction(Value);
            var pointer = (uint)Context.Pop<int>() + instr.Offset;
            CheckAlignment(pointer, instr);
            var value = (byte)Context.Module.Memories[0].Int8[pointer];
            Context.Push<long>(value);
        }

        /// <summary>
        /// Executes an 'i64.load16_s' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Load16S(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.Int64Load16S.CastInstruction(Value);
            var pointer = (uint)Context.Pop<int>() + instr.Offset;
            CheckAlignment(pointer, instr);
            var value = Context.Module.Memories[0].Int16[pointer];
            Context.Push<long>(value);
        }

        /// <summary>
        /// Executes an 'i64.load16_u' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Load16U(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.Int64Load16U.CastInstruction(Value);
            var pointer = (uint)Context.Pop<int>() + instr.Offset;
            CheckAlignment(pointer, instr);
            var value = (ushort)Context.Module.Memories[0].Int16[pointer];
            Context.Push<long>(value);
        }

        /// <summary>
        /// Executes an 'i64.load32_s' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Load32S(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.Int64Load32S.CastInstruction(Value);
            var pointer = (uint)Context.Pop<int>() + instr.Offset;
            CheckAlignment(pointer, instr);
            var value = Context.Module.Memories[0].Int32[pointer];
            Context.Push<long>(value);
        }

        /// <summary>
        /// Executes an 'i64.load32_u' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Load32U(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.Int64Load32U.CastInstruction(Value);
            var pointer = (uint)Context.Pop<int>() + instr.Offset;
            CheckAlignment(pointer, instr);
            var value = (uint)Context.Module.Memories[0].Int32[pointer];
            Context.Push<long>(value);
        }

        /// <summary>
        /// Executes an 'f32.load' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32Load(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.Float32Load.CastInstruction(Value);
            var pointer = (uint)Context.Pop<int>() + instr.Offset;
            CheckAlignment(pointer, instr);
            var value = Context.Module.Memories[0].Float32[pointer];
            Context.Push<float>(value);
        }

        /// <summary>
        /// Executes an 'f64.load' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float64Load(Instruction Value, InterpreterContext Context)
        {
            var instr = Operators.Float64Load.CastInstruction(Value);
            var pointer = (uint)Context.Pop<int>() + instr.Offset;
            CheckAlignment(pointer, instr);
            var value = Context.Module.Memories[0].Float64[pointer];
            Context.Push<double>(value);
        }

        private static void CheckAlignment(uint Pointer, MemoryInstruction Instruction)
        {
            if (Pointer % Instruction.Alignment != 0)
            {
                throw new WasmException(
                    string.Format(
                        "Misaligned memory access at 0x{0:X08}. (alignment: {1})",
                        Pointer,
                        Instruction.Alignment));
            }
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

        #region Int32 nullaries

        /// <summary>
        /// Executes an 'i32.add' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32Add(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<int>();
            var lhs = Context.Pop<int>();
            Context.Push<int>(lhs + rhs);
        }

        /// <summary>
        /// Executes an 'i32.sub' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32Sub(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<int>();
            var lhs = Context.Pop<int>();
            Context.Push<int>(lhs - rhs);
        }

        /// <summary>
        /// Executes an 'i32.mul' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32Mul(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<int>();
            var lhs = Context.Pop<int>();
            Context.Push<int>(lhs * rhs);
        }

        /// <summary>
        /// Executes an 'i32.div_s' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32DivS(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<int>();
            var lhs = Context.Pop<int>();
            Context.Push<int>(lhs / rhs);
        }

        /// <summary>
        /// Executes an 'i32.div_u' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32DivU(Instruction Value, InterpreterContext Context)
        {
            var rhs = (uint)Context.Pop<int>();
            var lhs = (uint)Context.Pop<int>();
            Context.Push<int>((int)(lhs / rhs));
        }

        /// <summary>
        /// Executes an 'i32.rem_s' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32RemS(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<int>();
            var lhs = Context.Pop<int>();
            Context.Push<int>(lhs % rhs);
        }

        /// <summary>
        /// Executes an 'i32.rem_u' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32RemU(Instruction Value, InterpreterContext Context)
        {
            var rhs = (uint)Context.Pop<int>();
            var lhs = (uint)Context.Pop<int>();
            Context.Push<int>((int)(lhs % rhs));
        }

        /// <summary>
        /// Executes an 'i32.and' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32And(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<int>();
            var lhs = Context.Pop<int>();
            Context.Push<int>(lhs & rhs);
        }

        /// <summary>
        /// Executes an 'i32.or' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32Or(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<int>();
            var lhs = Context.Pop<int>();
            Context.Push<int>(lhs | rhs);
        }

        /// <summary>
        /// Executes an 'i32.xor' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32Xor(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<int>();
            var lhs = Context.Pop<int>();
            Context.Push<int>(lhs ^ rhs);
        }

        /// <summary>
        /// Executes an 'i32.shr_s' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32ShrS(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<int>();
            var lhs = Context.Pop<int>();
            Context.Push<int>(lhs >> rhs);
        }

        /// <summary>
        /// Executes an 'i32.shr_u' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32ShrU(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<int>();
            var lhs = (uint)Context.Pop<int>();
            Context.Push<int>((int)(lhs >> rhs));
        }

        /// <summary>
        /// Executes an 'i32.shl' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32Shl(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<int>();
            var lhs = Context.Pop<int>();
            Context.Push<int>(lhs << rhs);
        }


        /// <summary>
        /// Executes an 'i32.rotl' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32Rotl(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<int>();
            var lhs = Context.Pop<int>();
            Context.Push<int>(ValueHelpers.RotateLeft(lhs, rhs));
        }

        /// <summary>
        /// Executes an 'i32.rotr' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32Rotr(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<int>();
            var lhs = Context.Pop<int>();
            Context.Push<int>(ValueHelpers.RotateRight(lhs, rhs));
        }

        /// <summary>
        /// Executes an 'i32.clz' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32Clz(Instruction Value, InterpreterContext Context)
        {
            Context.Push<int>(ValueHelpers.CountLeadingZeros(Context.Pop<int>()));
        }

        /// <summary>
        /// Executes an 'i32.ctz' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32Ctz(Instruction Value, InterpreterContext Context)
        {
            Context.Push<int>(ValueHelpers.CountTrailingZeros(Context.Pop<int>()));
        }

        /// <summary>
        /// Executes an 'i32.popcnt' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32Popcnt(Instruction Value, InterpreterContext Context)
        {
            Context.Push<int>(ValueHelpers.PopCount(Context.Pop<int>()));
        }

        /// <summary>
        /// Executes an 'i32.eq' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32Eq(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<int>();
            var lhs = Context.Pop<int>();
            Context.Push<int>(lhs == rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.ne' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32Ne(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<int>();
            var lhs = Context.Pop<int>();
            Context.Push<int>(lhs != rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.lt_s' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32LtS(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<int>();
            var lhs = Context.Pop<int>();
            Context.Push<int>(lhs < rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.lt_u' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32LtU(Instruction Value, InterpreterContext Context)
        {
            var rhs = (uint)Context.Pop<int>();
            var lhs = (uint)Context.Pop<int>();
            Context.Push<int>(lhs < rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.le_s' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32LeS(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<int>();
            var lhs = Context.Pop<int>();
            Context.Push<int>(lhs <= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.le_u' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32LeU(Instruction Value, InterpreterContext Context)
        {
            var rhs = (uint)Context.Pop<int>();
            var lhs = (uint)Context.Pop<int>();
            Context.Push<int>(lhs <= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.gt_s' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32GtS(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<int>();
            var lhs = Context.Pop<int>();
            Context.Push<int>(lhs > rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.gt_u' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32GtU(Instruction Value, InterpreterContext Context)
        {
            var rhs = (uint)Context.Pop<int>();
            var lhs = (uint)Context.Pop<int>();
            Context.Push<int>(lhs > rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.ge_s' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32GeS(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<int>();
            var lhs = Context.Pop<int>();
            Context.Push<int>(lhs >= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.ge_u' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32GeU(Instruction Value, InterpreterContext Context)
        {
            var rhs = (uint)Context.Pop<int>();
            var lhs = (uint)Context.Pop<int>();
            Context.Push<int>(lhs >= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.eqz' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32Eqz(Instruction Value, InterpreterContext Context)
        {
            Context.Push<int>(Context.Pop<int>() == 0 ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.trunc_s/f32' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32TruncSFloat32(Instruction Value, InterpreterContext Context)
        {
            Context.Push<int>((int)Context.Pop<float>());
        }

        /// <summary>
        /// Executes an 'i32.trunc_u/f32' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32TruncUFloat32(Instruction Value, InterpreterContext Context)
        {
            Context.Push<int>((int)(uint)Context.Pop<float>());
        }

        /// <summary>
        /// Executes an 'i32.trunc_s/f64' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32TruncSFloat64(Instruction Value, InterpreterContext Context)
        {
            Context.Push<int>((int)Context.Pop<double>());
        }

        /// <summary>
        /// Executes an 'i32.trunc_u/f64' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32TruncUFloat64(Instruction Value, InterpreterContext Context)
        {
            Context.Push<int>((int)(uint)Context.Pop<double>());
        }

        /// <summary>
        /// Executes an 'i32.wrap/i64' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32WrapInt64(Instruction Value, InterpreterContext Context)
        {
            Context.Push<int>((int)Context.Pop<long>());
        }

        /// <summary>
        /// Executes an 'i32.reinterpret/f32' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int32ReinterpretFloat32(Instruction Value, InterpreterContext Context)
        {
            Context.Push<int>(ValueHelpers.ReinterpretAsInt32(Context.Pop<float>()));
        }

        #endregion

        #region Int64 nullaries

        /// <summary>
        /// Executes an 'i64.add' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Add(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<long>();
            var lhs = Context.Pop<long>();
            Context.Push<long>(lhs + rhs);
        }

        /// <summary>
        /// Executes an 'i64.sub' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Sub(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<long>();
            var lhs = Context.Pop<long>();
            Context.Push<long>(lhs - rhs);
        }

        /// <summary>
        /// Executes an 'i64.mul' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Mul(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<long>();
            var lhs = Context.Pop<long>();
            Context.Push<long>(lhs * rhs);
        }

        /// <summary>
        /// Executes an 'i64.div_s' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64DivS(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<long>();
            var lhs = Context.Pop<long>();
            Context.Push<long>(lhs / rhs);
        }

        /// <summary>
        /// Executes an 'i64.div_u' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64DivU(Instruction Value, InterpreterContext Context)
        {
            var rhs = (ulong)Context.Pop<long>();
            var lhs = (ulong)Context.Pop<long>();
            Context.Push<long>((long)(lhs / rhs));
        }

        /// <summary>
        /// Executes an 'i64.rem_s' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64RemS(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<long>();
            var lhs = Context.Pop<long>();
            Context.Push<long>(lhs % rhs);
        }

        /// <summary>
        /// Executes an 'i64.rem_u' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64RemU(Instruction Value, InterpreterContext Context)
        {
            var rhs = (ulong)Context.Pop<long>();
            var lhs = (ulong)Context.Pop<long>();
            Context.Push<long>((long)(lhs % rhs));
        }

        /// <summary>
        /// Executes an 'i64.and' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64And(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<long>();
            var lhs = Context.Pop<long>();
            Context.Push<long>(lhs & rhs);
        }

        /// <summary>
        /// Executes an 'i64.or' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Or(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<long>();
            var lhs = Context.Pop<long>();
            Context.Push<long>(lhs | rhs);
        }

        /// <summary>
        /// Executes an 'i64.xor' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Xor(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<long>();
            var lhs = Context.Pop<long>();
            Context.Push<long>(lhs ^ rhs);
        }

        /// <summary>
        /// Executes an 'i64.shr_s' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64ShrS(Instruction Value, InterpreterContext Context)
        {
            var rhs = (int)Context.Pop<long>();
            var lhs = Context.Pop<long>();
            Context.Push<long>(lhs >> rhs);
        }

        /// <summary>
        /// Executes an 'i64.shr_u' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64ShrU(Instruction Value, InterpreterContext Context)
        {
            var rhs = (int)Context.Pop<long>();
            var lhs = (ulong)Context.Pop<long>();
            Context.Push<long>((long)(lhs >> rhs));
        }

        /// <summary>
        /// Executes an 'i64.shl' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Shl(Instruction Value, InterpreterContext Context)
        {
            var rhs = (int)Context.Pop<long>();
            var lhs = Context.Pop<long>();
            Context.Push<long>(lhs << rhs);
        }


        /// <summary>
        /// Executes an 'i64.rotl' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Rotl(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<long>();
            var lhs = Context.Pop<long>();
            Context.Push<long>(ValueHelpers.RotateLeft(lhs, rhs));
        }

        /// <summary>
        /// Executes an 'i64.rotr' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Rotr(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<long>();
            var lhs = Context.Pop<long>();
            Context.Push<long>(ValueHelpers.RotateRight(lhs, rhs));
        }

        /// <summary>
        /// Executes an 'i64.clz' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Clz(Instruction Value, InterpreterContext Context)
        {
            Context.Push<long>(ValueHelpers.CountLeadingZeros(Context.Pop<long>()));
        }

        /// <summary>
        /// Executes an 'i64.ctz' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Ctz(Instruction Value, InterpreterContext Context)
        {
            Context.Push<long>(ValueHelpers.CountTrailingZeros(Context.Pop<long>()));
        }

        /// <summary>
        /// Executes an 'i64.popcnt' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Popcnt(Instruction Value, InterpreterContext Context)
        {
            Context.Push<long>(ValueHelpers.PopCount(Context.Pop<long>()));
        }

        /// <summary>
        /// Executes an 'i64.eq' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Eq(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<long>();
            var lhs = Context.Pop<long>();
            Context.Push<long>(lhs == rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.ne' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Ne(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<long>();
            var lhs = Context.Pop<long>();
            Context.Push<long>(lhs != rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.lt_s' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64LtS(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<long>();
            var lhs = Context.Pop<long>();
            Context.Push<long>(lhs < rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.lt_u' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64LtU(Instruction Value, InterpreterContext Context)
        {
            var rhs = (ulong)Context.Pop<long>();
            var lhs = (ulong)Context.Pop<long>();
            Context.Push<long>(lhs < rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.le_s' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64LeS(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<long>();
            var lhs = Context.Pop<long>();
            Context.Push<long>(lhs <= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.le_u' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64LeU(Instruction Value, InterpreterContext Context)
        {
            var rhs = (ulong)Context.Pop<long>();
            var lhs = (ulong)Context.Pop<long>();
            Context.Push<long>(lhs <= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.gt_s' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64GtS(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<long>();
            var lhs = Context.Pop<long>();
            Context.Push<long>(lhs > rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.gt_u' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64GtU(Instruction Value, InterpreterContext Context)
        {
            var rhs = (ulong)Context.Pop<long>();
            var lhs = (ulong)Context.Pop<long>();
            Context.Push<long>(lhs > rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.ge_s' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64GeS(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<long>();
            var lhs = Context.Pop<long>();
            Context.Push<long>(lhs >= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.ge_u' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64GeU(Instruction Value, InterpreterContext Context)
        {
            var rhs = (ulong)Context.Pop<long>();
            var lhs = (ulong)Context.Pop<long>();
            Context.Push<long>(lhs >= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.eqz' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64Eqz(Instruction Value, InterpreterContext Context)
        {
            Context.Push<long>(Context.Pop<long>() == 0 ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.trunc_s/f32' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64TruncSFloat32(Instruction Value, InterpreterContext Context)
        {
            Context.Push<long>((long)Context.Pop<float>());
        }

        /// <summary>
        /// Executes an 'i64.trunc_u/f32' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64TruncUFloat32(Instruction Value, InterpreterContext Context)
        {
            Context.Push<long>((long)(ulong)Context.Pop<float>());
        }

        /// <summary>
        /// Executes an 'i64.trunc_s/f64' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64TruncSFloat64(Instruction Value, InterpreterContext Context)
        {
            Context.Push<long>((long)Context.Pop<double>());
        }

        /// <summary>
        /// Executes an 'i64.trunc_u/f64' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64TruncUFloat64(Instruction Value, InterpreterContext Context)
        {
            Context.Push<long>((long)(ulong)Context.Pop<double>());
        }

        /// <summary>
        /// Executes an 'i64.reinterpret/f32' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64ReinterpretFloat64(Instruction Value, InterpreterContext Context)
        {
            Context.Push<long>(ValueHelpers.ReinterpretAsInt64(Context.Pop<double>()));
        }

        /// <summary>
        /// Executes an 'i64.extend_s/i32' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64ExtendSInt32(Instruction Value, InterpreterContext Context)
        {
            Context.Push<long>(Context.Pop<int>());
        }

        /// <summary>
        /// Executes an 'i64.extend_u/i32' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Int64ExtendUInt32(Instruction Value, InterpreterContext Context)
        {
            Context.Push<long>((uint)Context.Pop<int>());
        }

        #endregion

        #region Float32 nullaries

        /// <summary>
        /// Executes an 'f32.abs' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32Abs(Instruction Value, InterpreterContext Context)
        {
            Context.Push<float>(Math.Abs(Context.Pop<float>()));
        }

        /// <summary>
        /// Executes an 'f32.add' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32Add(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<float>();
            var lhs = Context.Pop<float>();
            Context.Push<float>(lhs + rhs);
        }

        /// <summary>
        /// Executes an 'f32.ceil' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32Ceil(Instruction Value, InterpreterContext Context)
        {
            Context.Push<float>((float)Math.Ceiling(Context.Pop<float>()));
        }

        /// <summary>
        /// Executes an 'f32.copysign' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32Copysign(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<float>();
            var lhs = Context.Pop<float>();
            Context.Push<float>(ValueHelpers.Copysign(lhs, rhs));
        }

        /// <summary>
        /// Executes an 'f32.div' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32Div(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<float>();
            var lhs = Context.Pop<float>();
            Context.Push<float>(lhs / rhs);
        }

        /// <summary>
        /// Executes an 'f32.eq' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32Eq(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<float>();
            var lhs = Context.Pop<float>();
            Context.Push<int>(lhs == rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'f32.floor' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32Floor(Instruction Value, InterpreterContext Context)
        {
            Context.Push<float>((float)Math.Floor(Context.Pop<float>()));
        }

        /// <summary>
        /// Executes an 'f32.ge' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32Ge(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<float>();
            var lhs = Context.Pop<float>();
            Context.Push<int>(lhs >= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'f32.gt' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32Gt(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<float>();
            var lhs = Context.Pop<float>();
            Context.Push<int>(lhs > rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'f32.le' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32Le(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<float>();
            var lhs = Context.Pop<float>();
            Context.Push<int>(lhs <= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'f32.lt' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32Lt(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<float>();
            var lhs = Context.Pop<float>();
            Context.Push<int>(lhs < rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'f32.max' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32Max(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<float>();
            var lhs = Context.Pop<float>();
            Context.Push<float>(Math.Max(lhs, rhs));
        }

        /// <summary>
        /// Executes an 'f32.min' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32Min(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<float>();
            var lhs = Context.Pop<float>();
            Context.Push<float>(Math.Min(lhs, rhs));
        }

        /// <summary>
        /// Executes an 'f32.mul' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32Mul(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<float>();
            var lhs = Context.Pop<float>();
            Context.Push<float>(lhs * rhs);
        }

        /// <summary>
        /// Executes an 'f32.ne' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32Ne(Instruction Value, InterpreterContext Context)
        {
            var rhs = Context.Pop<float>();
            var lhs = Context.Pop<float>();
            Context.Push<int>(lhs != rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'f32.convert_s/i32' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32ConvertSInt32(Instruction Value, InterpreterContext Context)
        {
            Context.Push<float>(Context.Pop<int>());
        }

        /// <summary>
        /// Executes an 'f32.convert_u/i32' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32ConvertUInt32(Instruction Value, InterpreterContext Context)
        {
            Context.Push<float>((uint)Context.Pop<int>());
        }

        /// <summary>
        /// Executes an 'f32.convert_s/i64' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32ConvertSInt64(Instruction Value, InterpreterContext Context)
        {
            Context.Push<float>(Context.Pop<long>());
        }

        /// <summary>
        /// Executes an 'f32.convert_u/i64' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32ConvertUInt64(Instruction Value, InterpreterContext Context)
        {
            Context.Push<float>((ulong)Context.Pop<long>());
        }

        /// <summary>
        /// Executes an 'f32.demote/f64' instruction.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter's context.</param>
        public static void Float32DemoteFloat64(Instruction Value, InterpreterContext Context)
        {
            Context.Push<float>((float)Context.Pop<double>());
        }

        #endregion
    }
}