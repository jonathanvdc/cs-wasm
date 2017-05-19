using System;
using System.Collections.Generic;
using Wasm.Instructions;

namespace Wasm.Interpret
{
    /// <summary>
    /// The default instruction interpreter implementation.
    /// </summary>
    public sealed class DefaultInstructionInterpreter : InstructionInterpreter
    {
        /// <summary>
        /// Creates an instruction interpreter with no operator implementations.
        /// </summary>
        public DefaultInstructionInterpreter()
        {
            this.operatorImpls =
                new Dictionary<Operator, Action<Instruction, InterpreterContext>>();
        }

        /// <summary>
        /// Creates an instruction interpreter that clones the given interpreter's
        /// operator implementations.
        /// </summary>
        public DefaultInstructionInterpreter(DefaultInstructionInterpreter Other)
        {
            this.operatorImpls =
                new Dictionary<Operator, Action<Instruction, InterpreterContext>>(
                    Other.operatorImpls);
        }

        /// <summary>
        /// A mapping of operators to their implementations.
        /// </summary>
        private Dictionary<Operator, Action<Instruction, InterpreterContext>> operatorImpls;

        /// <summary>
        /// Implements the given operator as the specified action.
        /// </summary>
        /// <param name="Op">The operator to implement.</param>
        /// <param name="Implementation">The action that implements the operator.</param>
        public void ImplementOperator(
            Operator Op,
            Action<Instruction, InterpreterContext> Implementation)
        {
            operatorImpls[Op] = Implementation;
        }

        /// <summary>
        /// Interprets the given instruction within the specified context.
        /// </summary>
        /// <param name="Value">The instruction to interpret.</param>
        /// <param name="Context">The interpreter context.</param>
        public override void Interpret(Instruction Value, InterpreterContext Context)
        {
            if (Context.HasReturned)
            {
                return;
            }

            Action<Instruction, InterpreterContext> impl;
            if (operatorImpls.TryGetValue(Value.Op, out impl))
            {
                impl(Value, Context);
            }
            else
            {
                throw new WasmException("Operator not implemented by interpreter: " + Value.Op.ToString());
            }
        }

        /// <summary>
        /// The default instruction interpreter with the default list of operator implementations.
        /// Please don't implement any additional operators in this interpreter instance.
        /// </summary>
        public static readonly DefaultInstructionInterpreter Default;

        static DefaultInstructionInterpreter()
        {
            Default = new DefaultInstructionInterpreter();
            Default.ImplementOperator(Operators.Unreachable, OperatorImpls.Unreachable);
            Default.ImplementOperator(Operators.Nop, OperatorImpls.Nop);
            Default.ImplementOperator(Operators.Block, OperatorImpls.Block);
            Default.ImplementOperator(Operators.Loop, OperatorImpls.Loop);
            Default.ImplementOperator(Operators.If, OperatorImpls.If);
            Default.ImplementOperator(Operators.Br, OperatorImpls.Br);
            Default.ImplementOperator(Operators.BrIf, OperatorImpls.BrIf);
            Default.ImplementOperator(Operators.BrTable, OperatorImpls.BrTable);
            Default.ImplementOperator(Operators.Return, OperatorImpls.Return);
            Default.ImplementOperator(Operators.Drop, OperatorImpls.Drop);
            Default.ImplementOperator(Operators.Select, OperatorImpls.Select);
            Default.ImplementOperator(Operators.Call, OperatorImpls.Call);
            Default.ImplementOperator(Operators.CallIndirect, OperatorImpls.CallIndirect);
            Default.ImplementOperator(Operators.GetLocal, OperatorImpls.GetLocal);
            Default.ImplementOperator(Operators.SetLocal, OperatorImpls.SetLocal);
            Default.ImplementOperator(Operators.TeeLocal, OperatorImpls.TeeLocal);
            Default.ImplementOperator(Operators.GetGlobal, OperatorImpls.GetGlobal);
            Default.ImplementOperator(Operators.SetGlobal, OperatorImpls.SetGlobal);
            Default.ImplementOperator(Operators.Int32Load, OperatorImpls.Int32Load);
            Default.ImplementOperator(Operators.Int64Load, OperatorImpls.Int64Load);
            Default.ImplementOperator(Operators.Int32Load8S, OperatorImpls.Int32Load8S);
            Default.ImplementOperator(Operators.Int32Load8U, OperatorImpls.Int32Load8U);
            Default.ImplementOperator(Operators.Int32Load16S, OperatorImpls.Int32Load16S);
            Default.ImplementOperator(Operators.Int32Load16U, OperatorImpls.Int32Load16U);
            Default.ImplementOperator(Operators.Int64Load8S, OperatorImpls.Int64Load8S);
            Default.ImplementOperator(Operators.Int64Load8U, OperatorImpls.Int64Load8U);
            Default.ImplementOperator(Operators.Int64Load16S, OperatorImpls.Int64Load16S);
            Default.ImplementOperator(Operators.Int64Load16U, OperatorImpls.Int64Load16U);
            Default.ImplementOperator(Operators.Int64Load32S, OperatorImpls.Int64Load32S);
            Default.ImplementOperator(Operators.Int64Load32U, OperatorImpls.Int64Load32U);
            Default.ImplementOperator(Operators.Int32Const, OperatorImpls.Int32Const);
            Default.ImplementOperator(Operators.Int64Const, OperatorImpls.Int64Const);
            Default.ImplementOperator(Operators.Float32Const, OperatorImpls.Float32Const);
            Default.ImplementOperator(Operators.Float64Const, OperatorImpls.Float64Const);

            Default.ImplementOperator(Operators.Int32Add, OperatorImpls.Int32Add);
            Default.ImplementOperator(Operators.Int32And, OperatorImpls.Int32And);
            Default.ImplementOperator(Operators.Int32Clz, OperatorImpls.Int32Clz);
            Default.ImplementOperator(Operators.Int32Ctz, OperatorImpls.Int32Ctz);
            Default.ImplementOperator(Operators.Int32DivS, OperatorImpls.Int32DivS);
            Default.ImplementOperator(Operators.Int32DivU, OperatorImpls.Int32DivU);
            Default.ImplementOperator(Operators.Int32Eq, OperatorImpls.Int32Eq);
            Default.ImplementOperator(Operators.Int32Eqz, OperatorImpls.Int32Eqz);
            Default.ImplementOperator(Operators.Int32GeS, OperatorImpls.Int32GeS);
            Default.ImplementOperator(Operators.Int32GeU, OperatorImpls.Int32GeU);
            Default.ImplementOperator(Operators.Int32GtS, OperatorImpls.Int32GtS);
            Default.ImplementOperator(Operators.Int32GtU, OperatorImpls.Int32GtU);
            Default.ImplementOperator(Operators.Int32LeS, OperatorImpls.Int32LeS);
            Default.ImplementOperator(Operators.Int32LeU, OperatorImpls.Int32LeU);
            Default.ImplementOperator(Operators.Int32LtS, OperatorImpls.Int32LtS);
            Default.ImplementOperator(Operators.Int32LtU, OperatorImpls.Int32LtU);
            Default.ImplementOperator(Operators.Int32Mul, OperatorImpls.Int32Mul);
            Default.ImplementOperator(Operators.Int32Ne, OperatorImpls.Int32Ne);
            Default.ImplementOperator(Operators.Int32Or, OperatorImpls.Int32Or);
            Default.ImplementOperator(Operators.Int32Popcnt, OperatorImpls.Int32Popcnt);
            // TODO: Default.ImplementOperator(Operators.Int32ReinterpretFloat32, OperatorImpls.Int32ReinterpretFloat32);
            Default.ImplementOperator(Operators.Int32RemS, OperatorImpls.Int32RemS);
            Default.ImplementOperator(Operators.Int32RemU, OperatorImpls.Int32RemU);
            Default.ImplementOperator(Operators.Int32Rotl, OperatorImpls.Int32Rotl);
            Default.ImplementOperator(Operators.Int32Rotr, OperatorImpls.Int32Rotr);
            Default.ImplementOperator(Operators.Int32Shl, OperatorImpls.Int32Shl);
            Default.ImplementOperator(Operators.Int32ShrS, OperatorImpls.Int32ShrS);
            Default.ImplementOperator(Operators.Int32ShrU, OperatorImpls.Int32ShrU);
            Default.ImplementOperator(Operators.Int32Sub, OperatorImpls.Int32Sub);
            // TODO: Default.ImplementOperator(Operators.Int32TruncSFloat32, OperatorImpls.Int32TruncSFloat32);
            // TODO: Default.ImplementOperator(Operators.Int32TruncSFloat64, OperatorImpls.Int32TruncSFloat64);
            // TODO: Default.ImplementOperator(Operators.Int32TruncUFloat32, OperatorImpls.Int32TruncUFloat32);
            // TODO: Default.ImplementOperator(Operators.Int32TruncUFloat64, OperatorImpls.Int32TruncUFloat64);
            Default.ImplementOperator(Operators.Int32Xor, OperatorImpls.Int32Xor);
        }
    }
}