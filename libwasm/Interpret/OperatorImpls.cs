using System;
using Wasm.Instructions;
using Wasm.Optimize;

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
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Unreachable(Instruction value, InterpreterContext context)
        {
            throw new TrapException("An 'unreachable' instruction was reached.", TrapException.SpecMessages.Unreachable);
        }

        /// <summary>
        /// Executes a 'nop' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Nop(Instruction value, InterpreterContext context)
        {
        }

        /// <summary>
        /// Executes a 'block' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Block(Instruction value, InterpreterContext context)
        {
            var instruction = Operators.Block.CastInstruction(value);
            var contents = instruction.Contents;
            var interpreter = context.Module.Interpreter;

            var outerStack = context.Stack;
            var innerStack = context.Stack = context.CreateStack();
            for (int i = 0; i < contents.Count; i++)
            {
                interpreter.Interpret(contents[i], context);
                if (context.BreakRequested)
                {
                    // Restore the outer stack.
                    context.Stack = outerStack;
                    if (context.BreakDepth == 0)
                    {
                        // The buck stops here. Push the topmost n items of the
                        // inner stack onto the outer stack, where n is the block
                        // instruction's arity.
                        context.Push(innerStack, instruction.Arity);
                    }
                    else
                    {
                        // Otherwise, push the entire inner stack onto the outer stack and
                        // make the issue of figuring out how many elements to pop the next
                        // block's problem.
                        context.Push(innerStack);
                    }

                    context.BreakDepth--;
                    return;
                }
            }

            // Restore the outer stack.
            context.Stack = outerStack;
            // Push the inner stack onto the outer stack.
            context.Push(innerStack);
        }

        /// <summary>
        /// Executes a 'loop' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Loop(Instruction value, InterpreterContext context)
        {
            var contents = Operators.Loop.CastInstruction(value).Contents;
            var interpreter = context.Module.Interpreter;
            for (int i = 0; i < contents.Count;)
            {
                interpreter.Interpret(contents[i], context);
                if (context.BreakRequested)
                {
                    if (context.BreakDepth == 0)
                    {
                        // This loop is the break's target. We should decrement the
                        // break depth to terminate the break request and then re-start
                        // the loop.
                        context.BreakDepth--;
                        i = 0;
                    }
                    else
                    {
                        // This loop is not the break's target. We should terminate the loop.
                        context.BreakDepth--;
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
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void If(Instruction value, InterpreterContext context)
        {
            // Determine which branch we should take.
            var instr = Operators.If.CastInstruction(value);
            var condVal = context.Pop<int>();
            var bodyToRun = condVal != 0
                ? instr.IfBranch
                : instr.ElseBranch;

            if (bodyToRun != null)
            {
                // Create a block and run it.
                var block = Operators.Block.Create(instr.Type, bodyToRun);
                Block(block, context);
            }
        }

        /// <summary>
        /// Executes a 'br' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Br(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Br.CastInstruction(value);
            context.BreakDepth = (int)instr.Immediate;
        }

        /// <summary>
        /// Executes a 'br_if' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void BrIf(Instruction value, InterpreterContext context)
        {
            var instr = Operators.BrIf.CastInstruction(value);
            if (context.Pop<int>() != 0)
            {
                context.BreakDepth = (int)instr.Immediate;
            }
        }

        /// <summary>
        /// Executes a 'br_table' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void BrTable(Instruction value, InterpreterContext context)
        {
            var instr = Operators.BrTable.CastInstruction(value);
            int index = context.Pop<int>();
            if (index < 0 || index >= instr.TargetTable.Count)
            {
                context.BreakDepth = (int)instr.DefaultTarget;
            }
            else
            {
                context.BreakDepth = (int)instr.TargetTable[index];
            }
        }

        /// <summary>
        /// Executes a 'return' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Return(Instruction value, InterpreterContext context)
        {
            Return(context);
        }

        /// <summary>
        /// Executes a 'return' instruction.
        /// </summary>
        /// <param name="context">The interpreter's context.</param>
        public static void Return(InterpreterContext context)
        {
            // Remove excess values from the evaluation stack.
            var oldStack = context.Stack;
            context.Stack = context.CreateStack();
            context.Push(oldStack, context.ReturnTypes.Count);

            context.Return();
        }

        /// <summary>
        /// Executes a 'drop' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Drop(Instruction value, InterpreterContext context)
        {
            context.Pop<object>();
        }

        /// <summary>
        /// Executes a 'select' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Select(Instruction value, InterpreterContext context)
        {
            // Stack layout:
            //
            //     lhs (any type)
            //     rhs (same type as `lhs`)
            //     condition (i32)
            //

            // Pop operands from the stack.
            int condVal = context.Pop<int>();
            var rhs = context.Pop<object>();
            var lhs = context.Pop<object>();

            // Push the lhs onto the stack if the condition
            // is truthy; otherwise, push the rhs onto the
            // stack.
            context.Push<object>(condVal != 0 ? lhs : rhs);
        }

        /// <summary>
        /// Executes a 'call' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Call(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Call.CastInstruction(value);
            var funcDef = context.Module.Functions[(int)instr.Immediate];

            var args = context.Pop<object>(funcDef.ParameterTypes.Count);
            CheckForStackOverflow(context);
            var results = funcDef.Invoke(args, context.CallStackDepth);
            context.Push<object>(results);
        }

        private static void CheckForStackOverflow(InterpreterContext context)
        {
            if (context.CallStackDepth >= context.Policy.MaxCallStackDepth)
            {
                throw new TrapException(
                    "A stack overflow occurred: the max call stack depth was exceeded.",
                    TrapException.SpecMessages.StackOverflow);
            }
        }

        /// <summary>
        /// Executes a 'call_indirect' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void CallIndirect(Instruction value, InterpreterContext context)
        {
            var funcDefIndex = context.Pop<int>();
            var funcDef = context.Module.Tables[0][(uint)funcDefIndex];

            if (!(funcDef is ThrowFunctionDefinition))
            {
                var funcType = new FunctionType(funcDef.ParameterTypes, funcDef.ReturnTypes);
                var instruction = Operators.CallIndirect.CastInstruction(value);
                var expectedFuncType = context.Module.Types[(int)instruction.TypeIndex];
                if (!ConstFunctionTypeComparer.Instance.Equals(funcType, expectedFuncType))
                {
                    throw new TrapException(
                        $"Indirect function call expected to refer to a function with signature '{funcType}' but " +
                        $"instead found a function with signature '{expectedFuncType}'",
                        TrapException.SpecMessages.IndirectCallTypeMismatch);
                }
            }

            var args = context.Pop<object>(funcDef.ParameterTypes.Count);
            CheckForStackOverflow(context);
            var results = funcDef.Invoke(args, context.CallStackDepth);
            context.Push<object>(results);
        }

        /// <summary>
        /// Executes a 'get_local' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void GetLocal(Instruction value, InterpreterContext context)
        {
            var instr = Operators.GetLocal.CastInstruction(value);
            context.Push<object>(context.Locals[(int)instr.Immediate].Get<object>());
        }

        /// <summary>
        /// Executes a 'set_local' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void SetLocal(Instruction value, InterpreterContext context)
        {
            var instr = Operators.SetLocal.CastInstruction(value);
            context.Locals[(int)instr.Immediate].Set<object>(context.Pop<object>());
        }

        /// <summary>
        /// Executes a 'tee_local' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void TeeLocal(Instruction value, InterpreterContext context)
        {
            var instr = Operators.TeeLocal.CastInstruction(value);
            context.Locals[(int)instr.Immediate].Set<object>(context.Peek<object>());
        }

        /// <summary>
        /// Executes a 'get_global' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void GetGlobal(Instruction value, InterpreterContext context)
        {
            var instr = Operators.GetGlobal.CastInstruction(value);
            context.Push<object>(context.Module.Globals[(int)instr.Immediate].Get<object>());
        }

        /// <summary>
        /// Executes a 'set_global' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void SetGlobal(Instruction value, InterpreterContext context)
        {
            var instr = Operators.SetGlobal.CastInstruction(value);
            context.Module.Globals[(int)instr.Immediate].Set<object>(context.Pop<object>());
        }

        /// <summary>
        /// Executes an 'i32.load' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Load(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int32Load.CastInstruction(value);
            var pointer = PopAlignedPointer(instr, context);
            var result = context.Module.Memories[0].Int32[pointer];
            context.Push<int>(result);
        }

        /// <summary>
        /// Executes an 'i64.load' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Load(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int64Load.CastInstruction(value);
            var pointer = PopAlignedPointer(instr, context);
            var result = context.Module.Memories[0].Int64[pointer];
            context.Push<long>(result);
        }

        /// <summary>
        /// Executes an 'i32.load8_s' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Load8S(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int32Load8S.CastInstruction(value);
            var pointer = PopAlignedPointer(instr, context);
            var result = context.Module.Memories[0].Int8[pointer];
            context.Push<int>(result);
        }

        /// <summary>
        /// Executes an 'i32.load8_u' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Load8U(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int32Load8U.CastInstruction(value);
            var pointer = PopAlignedPointer(instr, context);
            var result = (byte)context.Module.Memories[0].Int8[pointer];
            context.Push<int>(result);
        }

        /// <summary>
        /// Executes an 'i32.load16_s' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Load16S(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int32Load16S.CastInstruction(value);
            var pointer = PopAlignedPointer(instr, context);
            var result = context.Module.Memories[0].Int16[pointer];
            context.Push<int>(result);
        }

        /// <summary>
        /// Executes an 'i32.load16_u' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Load16U(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int32Load16U.CastInstruction(value);
            var pointer = PopAlignedPointer(instr, context);
            var result = (ushort)context.Module.Memories[0].Int16[pointer];
            context.Push<int>(result);
        }

        /// <summary>
        /// Executes an 'i64.load8_s' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Load8S(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int64Load8S.CastInstruction(value);
            var pointer = PopAlignedPointer(instr, context);
            var result = context.Module.Memories[0].Int8[pointer];
            context.Push<long>(result);
        }

        /// <summary>
        /// Executes an 'i64.load8_u' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Load8U(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int64Load8U.CastInstruction(value);
            var pointer = PopAlignedPointer(instr, context);
            var result = (byte)context.Module.Memories[0].Int8[pointer];
            context.Push<long>(result);
        }

        /// <summary>
        /// Executes an 'i64.load16_s' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Load16S(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int64Load16S.CastInstruction(value);
            var pointer = PopAlignedPointer(instr, context);
            var result = context.Module.Memories[0].Int16[pointer];
            context.Push<long>(result);
        }

        /// <summary>
        /// Executes an 'i64.load16_u' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Load16U(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int64Load16U.CastInstruction(value);
            var pointer = PopAlignedPointer(instr, context);
            var result = (ushort)context.Module.Memories[0].Int16[pointer];
            context.Push<long>(result);
        }

        /// <summary>
        /// Executes an 'i64.load32_s' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Load32S(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int64Load32S.CastInstruction(value);
            var pointer = PopAlignedPointer(instr, context);
            var result = context.Module.Memories[0].Int32[pointer];
            context.Push<long>(result);
        }

        /// <summary>
        /// Executes an 'i64.load32_u' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Load32U(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int64Load32U.CastInstruction(value);
            var pointer = PopAlignedPointer(instr, context);
            var result = (uint)context.Module.Memories[0].Int32[pointer];
            context.Push<long>(result);
        }

        /// <summary>
        /// Executes an 'f32.load' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Load(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Float32Load.CastInstruction(value);
            var pointer = PopAlignedPointer(instr, context);
            var result = context.Module.Memories[0].Float32[pointer];
            context.Push<float>(result);
        }

        /// <summary>
        /// Executes an 'f64.load' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Load(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Float64Load.CastInstruction(value);
            var pointer = PopAlignedPointer(instr, context);
            var result = context.Module.Memories[0].Float64[pointer];
            context.Push<double>(result);
        }

        /// <summary>
        /// Executes an 'i32.store8' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Store8(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int32Store8.CastInstruction(value);
            var result = context.Pop<int>();
            var pointer = PopAlignedPointer(instr, context);
            var memView = context.Module.Memories[0].Int8;
            memView[pointer] = (sbyte)result;
        }

        /// <summary>
        /// Executes an 'i32.store16' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Store16(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int32Store16.CastInstruction(value);
            var result = context.Pop<int>();
            var pointer = PopAlignedPointer(instr, context);
            var memView = context.Module.Memories[0].Int16;
            memView[pointer] = (short)result;
        }

        /// <summary>
        /// Executes an 'i32.store' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Store(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int32Store.CastInstruction(value);
            var result = context.Pop<int>();
            var pointer = PopAlignedPointer(instr, context);
            var memView = context.Module.Memories[0].Int32;
            memView[pointer] = result;
        }

        /// <summary>
        /// Executes an 'i64.store8' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Store8(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int64Store8.CastInstruction(value);
            var result = context.Pop<long>();
            var pointer = PopAlignedPointer(instr, context);
            var memView = context.Module.Memories[0].Int8;
            memView[pointer] = (sbyte)result;
        }

        /// <summary>
        /// Executes an 'i32.store16' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Store16(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int64Store16.CastInstruction(value);
            var result = context.Pop<long>();
            var pointer = PopAlignedPointer(instr, context);
            var memView = context.Module.Memories[0].Int16;
            memView[pointer] = (short)result;
        }

        /// <summary>
        /// Executes an 'i64.store32' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Store32(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int64Store32.CastInstruction(value);
            var result = context.Pop<long>();
            var pointer = PopAlignedPointer(instr, context);
            var memView = context.Module.Memories[0].Int32;
            memView[pointer] = (int)result;
        }

        /// <summary>
        /// Executes an 'i64.store' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Store(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int64Store.CastInstruction(value);
            var result = context.Pop<long>();
            var pointer = PopAlignedPointer(instr, context);
            var memView = context.Module.Memories[0].Int64;
            memView[pointer] = result;
        }

        /// <summary>
        /// Executes an 'f32.store' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Store(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Float32Store.CastInstruction(value);
            var result = context.Pop<float>();
            var pointer = PopAlignedPointer(instr, context);
            var memView = context.Module.Memories[0].Float32;
            memView[pointer] = result;
        }

        /// <summary>
        /// Executes an 'f64.store' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Store(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Float64Store.CastInstruction(value);
            var result = context.Pop<double>();
            var pointer = PopAlignedPointer(instr, context);
            var memView = context.Module.Memories[0].Float64;
            memView[pointer] = result;
        }

        private static uint PopAlignedPointer(MemoryInstruction Instruction, InterpreterContext context)
        {
            var longPtr = (ulong)(uint)context.Pop<int>() + Instruction.Offset;
            if (longPtr > uint.MaxValue)
            {
                throw new TrapException(
                    "Memory address overflow.",
                    TrapException.SpecMessages.OutOfBoundsMemoryAccess);
            }
            var pointer = (uint)longPtr;
            if (context.Policy.EnforceAlignment)
            {
                CheckAlignment(pointer, Instruction);
            }
            return pointer;
        }

        private static void CheckAlignment(uint Pointer, MemoryInstruction Instruction)
        {
            if (Pointer % Instruction.Alignment != 0)
            {
                throw new TrapException(
                    string.Format(
                        "Misaligned memory access at {0}. (alignment: {1})",
                        DumpHelpers.FormatHex(Pointer),
                        Instruction.Alignment),
                    TrapException.SpecMessages.MisalignedMemoryAccess);
            }
        }

        /// <summary>
        /// Executes a 'current_memory' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void CurrentMemory(Instruction value, InterpreterContext context)
        {
            var instr = Operators.CurrentMemory.CastInstruction(value);
            var result = context.Module.Memories[(int)instr.Immediate].Size;
            context.Push<int>((int)result);
        }

        /// <summary>
        /// Executes a 'grow_memory' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void GrowMemory(Instruction value, InterpreterContext context)
        {
            var instr = Operators.CurrentMemory.CastInstruction(value);
            var amount = (uint)context.Pop<int>();
            var result = context.Module.Memories[(int)instr.Immediate].Grow(amount);
            context.Push<int>(result);
        }

        /// <summary>
        /// Executes an 'i32.const' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Const(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int32Const.CastInstruction(value);
            context.Push<int>(instr.Immediate);
        }

        /// <summary>
        /// Executes an 'i64.const' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Const(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Int64Const.CastInstruction(value);
            context.Push<long>(instr.Immediate);
        }

        /// <summary>
        /// Executes an 'f32.const' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Const(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Float32Const.CastInstruction(value);
            context.Push<float>(instr.Immediate);
        }

        /// <summary>
        /// Executes an 'f64.const' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Const(Instruction value, InterpreterContext context)
        {
            var instr = Operators.Float64Const.CastInstruction(value);
            context.Push<double>(instr.Immediate);
        }

        #region Int32 nullaries

        /// <summary>
        /// Executes an 'i32.add' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Add(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<int>();
            var lhs = context.Pop<int>();
            context.Push<int>(lhs + rhs);
        }

        /// <summary>
        /// Executes an 'i32.sub' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Sub(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<int>();
            var lhs = context.Pop<int>();
            context.Push<int>(lhs - rhs);
        }

        /// <summary>
        /// Executes an 'i32.mul' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Mul(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<int>();
            var lhs = context.Pop<int>();
            context.Push<int>(lhs * rhs);
        }

        /// <summary>
        /// Executes an 'i32.div_s' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32DivS(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<int>();
            var lhs = context.Pop<int>();
            context.Push<int>(lhs / rhs);
        }

        /// <summary>
        /// Executes an 'i32.div_u' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32DivU(Instruction value, InterpreterContext context)
        {
            var rhs = (uint)context.Pop<int>();
            var lhs = (uint)context.Pop<int>();
            context.Push<int>((int)(lhs / rhs));
        }

        /// <summary>
        /// Executes an 'i32.rem_s' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32RemS(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<int>();
            var lhs = context.Pop<int>();
            if (lhs == int.MinValue && rhs == -1)
            {
                // We need to check for this corner case. As per the OpCodes.Rem docs:
                //
                //     Note that on the Intel-based platforms an OverflowException is thrown when computing (minint rem -1).
                //
                context.Push<int>(0);
            }
            else
            {
                context.Push<int>(lhs % rhs);
            }
        }

        /// <summary>
        /// Executes an 'i32.rem_u' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32RemU(Instruction value, InterpreterContext context)
        {
            var rhs = (uint)context.Pop<int>();
            var lhs = (uint)context.Pop<int>();
            context.Push<int>((int)(lhs % rhs));
        }

        /// <summary>
        /// Executes an 'i32.and' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32And(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<int>();
            var lhs = context.Pop<int>();
            context.Push<int>(lhs & rhs);
        }

        /// <summary>
        /// Executes an 'i32.or' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Or(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<int>();
            var lhs = context.Pop<int>();
            context.Push<int>(lhs | rhs);
        }

        /// <summary>
        /// Executes an 'i32.xor' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Xor(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<int>();
            var lhs = context.Pop<int>();
            context.Push<int>(lhs ^ rhs);
        }

        /// <summary>
        /// Executes an 'i32.shr_s' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32ShrS(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<int>();
            var lhs = context.Pop<int>();
            context.Push<int>(lhs >> rhs);
        }

        /// <summary>
        /// Executes an 'i32.shr_u' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32ShrU(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<int>();
            var lhs = (uint)context.Pop<int>();
            context.Push<int>((int)(lhs >> rhs));
        }

        /// <summary>
        /// Executes an 'i32.shl' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Shl(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<int>();
            var lhs = context.Pop<int>();
            context.Push<int>(lhs << rhs);
        }


        /// <summary>
        /// Executes an 'i32.rotl' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Rotl(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<int>();
            var lhs = context.Pop<int>();
            context.Push<int>(ValueHelpers.RotateLeft(lhs, rhs));
        }

        /// <summary>
        /// Executes an 'i32.rotr' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Rotr(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<int>();
            var lhs = context.Pop<int>();
            context.Push<int>(ValueHelpers.RotateRight(lhs, rhs));
        }

        /// <summary>
        /// Executes an 'i32.clz' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Clz(Instruction value, InterpreterContext context)
        {
            context.Push<int>(ValueHelpers.CountLeadingZeros(context.Pop<int>()));
        }

        /// <summary>
        /// Executes an 'i32.ctz' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Ctz(Instruction value, InterpreterContext context)
        {
            context.Push<int>(ValueHelpers.CountTrailingZeros(context.Pop<int>()));
        }

        /// <summary>
        /// Executes an 'i32.popcnt' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Popcnt(Instruction value, InterpreterContext context)
        {
            context.Push<int>(ValueHelpers.PopCount(context.Pop<int>()));
        }

        /// <summary>
        /// Executes an 'i32.eq' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Eq(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<int>();
            var lhs = context.Pop<int>();
            context.Push<int>(lhs == rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.ne' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Ne(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<int>();
            var lhs = context.Pop<int>();
            context.Push<int>(lhs != rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.lt_s' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32LtS(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<int>();
            var lhs = context.Pop<int>();
            context.Push<int>(lhs < rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.lt_u' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32LtU(Instruction value, InterpreterContext context)
        {
            var rhs = (uint)context.Pop<int>();
            var lhs = (uint)context.Pop<int>();
            context.Push<int>(lhs < rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.le_s' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32LeS(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<int>();
            var lhs = context.Pop<int>();
            context.Push<int>(lhs <= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.le_u' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32LeU(Instruction value, InterpreterContext context)
        {
            var rhs = (uint)context.Pop<int>();
            var lhs = (uint)context.Pop<int>();
            context.Push<int>(lhs <= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.gt_s' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32GtS(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<int>();
            var lhs = context.Pop<int>();
            context.Push<int>(lhs > rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.gt_u' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32GtU(Instruction value, InterpreterContext context)
        {
            var rhs = (uint)context.Pop<int>();
            var lhs = (uint)context.Pop<int>();
            context.Push<int>(lhs > rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.ge_s' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32GeS(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<int>();
            var lhs = context.Pop<int>();
            context.Push<int>(lhs >= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.ge_u' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32GeU(Instruction value, InterpreterContext context)
        {
            var rhs = (uint)context.Pop<int>();
            var lhs = (uint)context.Pop<int>();
            context.Push<int>(lhs >= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.eqz' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32Eqz(Instruction value, InterpreterContext context)
        {
            context.Push<int>(context.Pop<int>() == 0 ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i32.trunc_s/f32' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32TruncSFloat32(Instruction value, InterpreterContext context)
        {
            context.Push<int>(ValueHelpers.TruncateToInt32(context.Pop<float>()));
        }

        /// <summary>
        /// Executes an 'i32.trunc_u/f32' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32TruncUFloat32(Instruction value, InterpreterContext context)
        {
            context.Push<int>((int)ValueHelpers.TruncateToUInt32(context.Pop<float>()));
        }

        /// <summary>
        /// Executes an 'i32.trunc_s/f64' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32TruncSFloat64(Instruction value, InterpreterContext context)
        {
            context.Push<int>(ValueHelpers.TruncateToInt32(context.Pop<double>()));
        }

        /// <summary>
        /// Executes an 'i32.trunc_u/f64' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32TruncUFloat64(Instruction value, InterpreterContext context)
        {
            context.Push<int>((int)ValueHelpers.TruncateToUInt32(context.Pop<double>()));
        }

        /// <summary>
        /// Executes an 'i32.wrap/i64' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32WrapInt64(Instruction value, InterpreterContext context)
        {
            context.Push<int>((int)context.Pop<long>());
        }

        /// <summary>
        /// Executes an 'i32.reinterpret/f32' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int32ReinterpretFloat32(Instruction value, InterpreterContext context)
        {
            context.Push<int>(ValueHelpers.ReinterpretAsInt32(context.Pop<float>()));
        }

        #endregion

        #region Int64 nullaries

        /// <summary>
        /// Executes an 'i64.add' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Add(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<long>();
            var lhs = context.Pop<long>();
            context.Push<long>(lhs + rhs);
        }

        /// <summary>
        /// Executes an 'i64.sub' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Sub(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<long>();
            var lhs = context.Pop<long>();
            context.Push<long>(lhs - rhs);
        }

        /// <summary>
        /// Executes an 'i64.mul' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Mul(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<long>();
            var lhs = context.Pop<long>();
            context.Push<long>(lhs * rhs);
        }

        /// <summary>
        /// Executes an 'i64.div_s' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64DivS(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<long>();
            var lhs = context.Pop<long>();
            context.Push<long>(lhs / rhs);
        }

        /// <summary>
        /// Executes an 'i64.div_u' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64DivU(Instruction value, InterpreterContext context)
        {
            var rhs = (ulong)context.Pop<long>();
            var lhs = (ulong)context.Pop<long>();
            context.Push<long>((long)(lhs / rhs));
        }

        /// <summary>
        /// Executes an 'i64.rem_s' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64RemS(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<long>();
            var lhs = context.Pop<long>();
            if (lhs == long.MinValue && rhs == -1)
            {
                // We need to check for this corner case. As per the OpCodes.Rem docs:
                //
                //     Note that on the Intel-based platforms an OverflowException is thrown when computing (minint rem -1).
                //
                context.Push<long>(0);
            }
            else
            {
                context.Push<long>(lhs % rhs);
            }
        }

        /// <summary>
        /// Executes an 'i64.rem_u' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64RemU(Instruction value, InterpreterContext context)
        {
            var rhs = (ulong)context.Pop<long>();
            var lhs = (ulong)context.Pop<long>();
            context.Push<long>((long)(lhs % rhs));
        }

        /// <summary>
        /// Executes an 'i64.and' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64And(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<long>();
            var lhs = context.Pop<long>();
            context.Push<long>(lhs & rhs);
        }

        /// <summary>
        /// Executes an 'i64.or' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Or(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<long>();
            var lhs = context.Pop<long>();
            context.Push<long>(lhs | rhs);
        }

        /// <summary>
        /// Executes an 'i64.xor' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Xor(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<long>();
            var lhs = context.Pop<long>();
            context.Push<long>(lhs ^ rhs);
        }

        /// <summary>
        /// Executes an 'i64.shr_s' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64ShrS(Instruction value, InterpreterContext context)
        {
            var rhs = (int)context.Pop<long>();
            var lhs = context.Pop<long>();
            context.Push<long>(lhs >> rhs);
        }

        /// <summary>
        /// Executes an 'i64.shr_u' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64ShrU(Instruction value, InterpreterContext context)
        {
            var rhs = (int)context.Pop<long>();
            var lhs = (ulong)context.Pop<long>();
            context.Push<long>((long)(lhs >> rhs));
        }

        /// <summary>
        /// Executes an 'i64.shl' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Shl(Instruction value, InterpreterContext context)
        {
            var rhs = (int)context.Pop<long>();
            var lhs = context.Pop<long>();
            context.Push<long>(lhs << rhs);
        }


        /// <summary>
        /// Executes an 'i64.rotl' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Rotl(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<long>();
            var lhs = context.Pop<long>();
            context.Push<long>(ValueHelpers.RotateLeft(lhs, rhs));
        }

        /// <summary>
        /// Executes an 'i64.rotr' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Rotr(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<long>();
            var lhs = context.Pop<long>();
            context.Push<long>(ValueHelpers.RotateRight(lhs, rhs));
        }

        /// <summary>
        /// Executes an 'i64.clz' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Clz(Instruction value, InterpreterContext context)
        {
            context.Push<long>(ValueHelpers.CountLeadingZeros(context.Pop<long>()));
        }

        /// <summary>
        /// Executes an 'i64.ctz' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Ctz(Instruction value, InterpreterContext context)
        {
            context.Push<long>(ValueHelpers.CountTrailingZeros(context.Pop<long>()));
        }

        /// <summary>
        /// Executes an 'i64.popcnt' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Popcnt(Instruction value, InterpreterContext context)
        {
            context.Push<long>(ValueHelpers.PopCount(context.Pop<long>()));
        }

        /// <summary>
        /// Executes an 'i64.eq' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Eq(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<long>();
            var lhs = context.Pop<long>();
            context.Push<int>(lhs == rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.ne' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Ne(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<long>();
            var lhs = context.Pop<long>();
            context.Push<int>(lhs != rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.lt_s' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64LtS(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<long>();
            var lhs = context.Pop<long>();
            context.Push<int>(lhs < rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.lt_u' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64LtU(Instruction value, InterpreterContext context)
        {
            var rhs = (ulong)context.Pop<long>();
            var lhs = (ulong)context.Pop<long>();
            context.Push<int>(lhs < rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.le_s' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64LeS(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<long>();
            var lhs = context.Pop<long>();
            context.Push<int>(lhs <= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.le_u' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64LeU(Instruction value, InterpreterContext context)
        {
            var rhs = (ulong)context.Pop<long>();
            var lhs = (ulong)context.Pop<long>();
            context.Push<int>(lhs <= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.gt_s' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64GtS(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<long>();
            var lhs = context.Pop<long>();
            context.Push<int>(lhs > rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.gt_u' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64GtU(Instruction value, InterpreterContext context)
        {
            var rhs = (ulong)context.Pop<long>();
            var lhs = (ulong)context.Pop<long>();
            context.Push<int>(lhs > rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.ge_s' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64GeS(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<long>();
            var lhs = context.Pop<long>();
            context.Push<int>(lhs >= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.ge_u' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64GeU(Instruction value, InterpreterContext context)
        {
            var rhs = (ulong)context.Pop<long>();
            var lhs = (ulong)context.Pop<long>();
            context.Push<int>(lhs >= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.eqz' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64Eqz(Instruction value, InterpreterContext context)
        {
            context.Push<int>(context.Pop<long>() == 0 ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'i64.trunc_s/f32' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64TruncSFloat32(Instruction value, InterpreterContext context)
        {
            context.Push<long>(ValueHelpers.TruncateToInt64(context.Pop<float>()));
        }

        /// <summary>
        /// Executes an 'i64.trunc_u/f32' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64TruncUFloat32(Instruction value, InterpreterContext context)
        {
            context.Push<long>((long)ValueHelpers.TruncateToUInt64(context.Pop<float>()));
        }

        /// <summary>
        /// Executes an 'i64.trunc_s/f64' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64TruncSFloat64(Instruction value, InterpreterContext context)
        {
            context.Push<long>(ValueHelpers.TruncateToInt64(context.Pop<double>()));
        }

        /// <summary>
        /// Executes an 'i64.trunc_u/f64' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64TruncUFloat64(Instruction value, InterpreterContext context)
        {
            context.Push<long>((long)ValueHelpers.TruncateToUInt64(context.Pop<double>()));
        }

        /// <summary>
        /// Executes an 'i64.reinterpret/f32' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64ReinterpretFloat64(Instruction value, InterpreterContext context)
        {
            context.Push<long>(ValueHelpers.ReinterpretAsInt64(context.Pop<double>()));
        }

        /// <summary>
        /// Executes an 'i64.extend_s/i32' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64ExtendSInt32(Instruction value, InterpreterContext context)
        {
            context.Push<long>(context.Pop<int>());
        }

        /// <summary>
        /// Executes an 'i64.extend_u/i32' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Int64ExtendUInt32(Instruction value, InterpreterContext context)
        {
            context.Push<long>((uint)context.Pop<int>());
        }

        #endregion

        #region Float32 nullaries

        /// <summary>
        /// Executes an 'f32.abs' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Abs(Instruction value, InterpreterContext context)
        {
            context.Push<float>(Math.Abs(context.Pop<float>()));
        }

        /// <summary>
        /// Executes an 'f32.add' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Add(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<float>();
            var lhs = context.Pop<float>();
            context.Push<float>(lhs + rhs);
        }

        /// <summary>
        /// Executes an 'f32.ceil' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Ceil(Instruction value, InterpreterContext context)
        {
            context.Push<float>((float)Math.Ceiling(context.Pop<float>()));
        }

        /// <summary>
        /// Executes an 'f32.copysign' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Copysign(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<float>();
            var lhs = context.Pop<float>();
            context.Push<float>(ValueHelpers.Copysign(lhs, rhs));
        }

        /// <summary>
        /// Executes an 'f32.div' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Div(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<float>();
            var lhs = context.Pop<float>();
            context.Push<float>(lhs / rhs);
        }

        /// <summary>
        /// Executes an 'f32.eq' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Eq(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<float>();
            var lhs = context.Pop<float>();
            context.Push<int>(lhs == rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'f32.floor' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Floor(Instruction value, InterpreterContext context)
        {
            context.Push<float>((float)Math.Floor(context.Pop<float>()));
        }

        /// <summary>
        /// Executes an 'f32.ge' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Ge(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<float>();
            var lhs = context.Pop<float>();
            context.Push<int>(lhs >= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'f32.gt' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Gt(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<float>();
            var lhs = context.Pop<float>();
            context.Push<int>(lhs > rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'f32.le' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Le(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<float>();
            var lhs = context.Pop<float>();
            context.Push<int>(lhs <= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'f32.lt' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Lt(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<float>();
            var lhs = context.Pop<float>();
            context.Push<int>(lhs < rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'f32.max' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Max(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<float>();
            var lhs = context.Pop<float>();
            context.Push<float>(Math.Max(lhs, rhs));
        }

        /// <summary>
        /// Executes an 'f32.min' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Min(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<float>();
            var lhs = context.Pop<float>();
            context.Push<float>(Math.Min(lhs, rhs));
        }

        /// <summary>
        /// Executes an 'f32.mul' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Mul(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<float>();
            var lhs = context.Pop<float>();
            context.Push<float>(lhs * rhs);
        }

        /// <summary>
        /// Executes an 'f32.ne' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Ne(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<float>();
            var lhs = context.Pop<float>();
            context.Push<int>(lhs != rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'f32.nearest' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Nearest(Instruction value, InterpreterContext context)
        {
            context.Push<float>((float)Math.Round(context.Pop<float>(), MidpointRounding.ToEven));
        }

        /// <summary>
        /// Executes an 'f32.neg' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Neg(Instruction value, InterpreterContext context)
        {
            context.Push<float>(-context.Pop<float>());
        }

        /// <summary>
        /// Executes an 'f32.sub' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Sub(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<float>();
            var lhs = context.Pop<float>();
            context.Push<float>(lhs - rhs);
        }

        /// <summary>
        /// Executes an 'f32.sqrt' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Sqrt(Instruction value, InterpreterContext context)
        {
            context.Push<float>((float)Math.Sqrt(context.Pop<float>()));
        }

        /// <summary>
        /// Executes an 'f32.trunc' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32Trunc(Instruction value, InterpreterContext context)
        {
            context.Push<float>((float)Math.Truncate(context.Pop<float>()));
        }

        /// <summary>
        /// Executes an 'f32.convert_s/i32' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32ConvertSInt32(Instruction value, InterpreterContext context)
        {
            context.Push<float>(context.Pop<int>());
        }

        /// <summary>
        /// Executes an 'f32.convert_u/i32' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32ConvertUInt32(Instruction value, InterpreterContext context)
        {
            context.Push<float>((uint)context.Pop<int>());
        }

        /// <summary>
        /// Executes an 'f32.convert_s/i64' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32ConvertSInt64(Instruction value, InterpreterContext context)
        {
            context.Push<float>(context.Pop<long>());
        }

        /// <summary>
        /// Executes an 'f32.convert_u/i64' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32ConvertUInt64(Instruction value, InterpreterContext context)
        {
            context.Push<float>((ulong)context.Pop<long>());
        }

        /// <summary>
        /// Executes an 'f32.demote/f64' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32DemoteFloat64(Instruction value, InterpreterContext context)
        {
            context.Push<float>((float)context.Pop<double>());
        }

        /// <summary>
        /// Executes an 'f32.reinterpret/i32' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float32ReinterpretInt32(Instruction value, InterpreterContext context)
        {
            context.Push<float>(ValueHelpers.ReinterpretAsFloat32(context.Pop<int>()));
        }

        #endregion

        #region Float64 nullaries

        /// <summary>
        /// Executes an 'f64.abs' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Abs(Instruction value, InterpreterContext context)
        {
            context.Push<double>(Math.Abs(context.Pop<double>()));
        }

        /// <summary>
        /// Executes an 'f64.add' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Add(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<double>();
            var lhs = context.Pop<double>();
            context.Push<double>(lhs + rhs);
        }

        /// <summary>
        /// Executes an 'f64.ceil' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Ceil(Instruction value, InterpreterContext context)
        {
            context.Push<double>(Math.Ceiling(context.Pop<double>()));
        }

        /// <summary>
        /// Executes an 'f64.copysign' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Copysign(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<double>();
            var lhs = context.Pop<double>();
            context.Push<double>(ValueHelpers.Copysign(lhs, rhs));
        }

        /// <summary>
        /// Executes an 'f64.div' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Div(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<double>();
            var lhs = context.Pop<double>();
            context.Push<double>(lhs / rhs);
        }

        /// <summary>
        /// Executes an 'f64.eq' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Eq(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<double>();
            var lhs = context.Pop<double>();
            context.Push<int>(lhs == rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'f64.floor' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Floor(Instruction value, InterpreterContext context)
        {
            context.Push<double>(Math.Floor(context.Pop<double>()));
        }

        /// <summary>
        /// Executes an 'f64.ge' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Ge(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<double>();
            var lhs = context.Pop<double>();
            context.Push<int>(lhs >= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'f64.gt' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Gt(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<double>();
            var lhs = context.Pop<double>();
            context.Push<int>(lhs > rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'f64.le' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Le(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<double>();
            var lhs = context.Pop<double>();
            context.Push<int>(lhs <= rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'f64.lt' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Lt(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<double>();
            var lhs = context.Pop<double>();
            context.Push<int>(lhs < rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'f64.max' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Max(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<double>();
            var lhs = context.Pop<double>();
            context.Push<double>(Math.Max(lhs, rhs));
        }

        /// <summary>
        /// Executes an 'f64.min' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Min(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<double>();
            var lhs = context.Pop<double>();
            context.Push<double>(Math.Min(lhs, rhs));
        }

        /// <summary>
        /// Executes an 'f64.mul' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Mul(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<double>();
            var lhs = context.Pop<double>();
            context.Push<double>(lhs * rhs);
        }

        /// <summary>
        /// Executes an 'f64.ne' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Ne(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<double>();
            var lhs = context.Pop<double>();
            context.Push<int>(lhs != rhs ? 1 : 0);
        }

        /// <summary>
        /// Executes an 'f64.nearest' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Nearest(Instruction value, InterpreterContext context)
        {
            context.Push<double>(Math.Round(context.Pop<double>(), MidpointRounding.ToEven));
        }

        /// <summary>
        /// Executes an 'f64.neg' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Neg(Instruction value, InterpreterContext context)
        {
            context.Push<double>(-context.Pop<double>());
        }

        /// <summary>
        /// Executes an 'f64.sub' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Sub(Instruction value, InterpreterContext context)
        {
            var rhs = context.Pop<double>();
            var lhs = context.Pop<double>();
            context.Push<double>(lhs - rhs);
        }

        /// <summary>
        /// Executes an 'f64.sqrt' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Sqrt(Instruction value, InterpreterContext context)
        {
            context.Push<double>(Math.Sqrt(context.Pop<double>()));
        }

        /// <summary>
        /// Executes an 'f64.trunc' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64Trunc(Instruction value, InterpreterContext context)
        {
            context.Push<double>(Math.Truncate(context.Pop<double>()));
        }

        /// <summary>
        /// Executes an 'f64.convert_s/i32' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64ConvertSInt32(Instruction value, InterpreterContext context)
        {
            context.Push<double>(context.Pop<int>());
        }

        /// <summary>
        /// Executes an 'f64.convert_u/i32' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64ConvertUInt32(Instruction value, InterpreterContext context)
        {
            context.Push<double>((uint)context.Pop<int>());
        }

        /// <summary>
        /// Executes an 'f64.convert_s/i64' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64ConvertSInt64(Instruction value, InterpreterContext context)
        {
            context.Push<double>(context.Pop<long>());
        }

        /// <summary>
        /// Executes an 'f64.convert_u/i64' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64ConvertUInt64(Instruction value, InterpreterContext context)
        {
            context.Push<double>((ulong)context.Pop<long>());
        }

        /// <summary>
        /// Executes an 'f64.promote/f32' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64PromoteFloat32(Instruction value, InterpreterContext context)
        {
            context.Push<double>(context.Pop<float>());
        }

        /// <summary>
        /// Executes an 'f64.reinterpret/i64' instruction.
        /// </summary>
        /// <param name="value">The instruction to interpret.</param>
        /// <param name="context">The interpreter's context.</param>
        public static void Float64ReinterpretInt64(Instruction value, InterpreterContext context)
        {
            context.Push<double>(ValueHelpers.ReinterpretAsFloat64(context.Pop<long>()));
        }

        #endregion
    }
}