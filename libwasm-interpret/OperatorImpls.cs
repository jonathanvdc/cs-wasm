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