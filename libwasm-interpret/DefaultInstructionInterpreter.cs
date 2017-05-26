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
            Default.ImplementOperator(Operators.Float32Load, OperatorImpls.Float32Load);
            Default.ImplementOperator(Operators.Float64Load, OperatorImpls.Float64Load);
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
            Default.ImplementOperator(Operators.Int32ReinterpretFloat32, OperatorImpls.Int32ReinterpretFloat32);
            Default.ImplementOperator(Operators.Int32RemS, OperatorImpls.Int32RemS);
            Default.ImplementOperator(Operators.Int32RemU, OperatorImpls.Int32RemU);
            Default.ImplementOperator(Operators.Int32Rotl, OperatorImpls.Int32Rotl);
            Default.ImplementOperator(Operators.Int32Rotr, OperatorImpls.Int32Rotr);
            Default.ImplementOperator(Operators.Int32Shl, OperatorImpls.Int32Shl);
            Default.ImplementOperator(Operators.Int32ShrS, OperatorImpls.Int32ShrS);
            Default.ImplementOperator(Operators.Int32ShrU, OperatorImpls.Int32ShrU);
            Default.ImplementOperator(Operators.Int32Sub, OperatorImpls.Int32Sub);
            Default.ImplementOperator(Operators.Int32TruncSFloat32, OperatorImpls.Int32TruncSFloat32);
            Default.ImplementOperator(Operators.Int32TruncSFloat64, OperatorImpls.Int32TruncSFloat64);
            Default.ImplementOperator(Operators.Int32TruncUFloat32, OperatorImpls.Int32TruncUFloat32);
            Default.ImplementOperator(Operators.Int32TruncUFloat64, OperatorImpls.Int32TruncUFloat64);
            Default.ImplementOperator(Operators.Int32WrapInt64, OperatorImpls.Int32WrapInt64);
            Default.ImplementOperator(Operators.Int32Xor, OperatorImpls.Int32Xor);

            Default.ImplementOperator(Operators.Int64Add, OperatorImpls.Int64Add);
            Default.ImplementOperator(Operators.Int64And, OperatorImpls.Int64And);
            Default.ImplementOperator(Operators.Int64Clz, OperatorImpls.Int64Clz);
            Default.ImplementOperator(Operators.Int64Ctz, OperatorImpls.Int64Ctz);
            Default.ImplementOperator(Operators.Int64DivS, OperatorImpls.Int64DivS);
            Default.ImplementOperator(Operators.Int64DivU, OperatorImpls.Int64DivU);
            Default.ImplementOperator(Operators.Int64Eq, OperatorImpls.Int64Eq);
            Default.ImplementOperator(Operators.Int64Eqz, OperatorImpls.Int64Eqz);
            Default.ImplementOperator(Operators.Int64ExtendSInt32, OperatorImpls.Int64ExtendSInt32);
            Default.ImplementOperator(Operators.Int64ExtendUInt32, OperatorImpls.Int64ExtendUInt32);
            Default.ImplementOperator(Operators.Int64GeS, OperatorImpls.Int64GeS);
            Default.ImplementOperator(Operators.Int64GeU, OperatorImpls.Int64GeU);
            Default.ImplementOperator(Operators.Int64GtS, OperatorImpls.Int64GtS);
            Default.ImplementOperator(Operators.Int64GtU, OperatorImpls.Int64GtU);
            Default.ImplementOperator(Operators.Int64LeS, OperatorImpls.Int64LeS);
            Default.ImplementOperator(Operators.Int64LeU, OperatorImpls.Int64LeU);
            Default.ImplementOperator(Operators.Int64LtS, OperatorImpls.Int64LtS);
            Default.ImplementOperator(Operators.Int64LtU, OperatorImpls.Int64LtU);
            Default.ImplementOperator(Operators.Int64Mul, OperatorImpls.Int64Mul);
            Default.ImplementOperator(Operators.Int64Ne, OperatorImpls.Int64Ne);
            Default.ImplementOperator(Operators.Int64Or, OperatorImpls.Int64Or);
            Default.ImplementOperator(Operators.Int64Popcnt, OperatorImpls.Int64Popcnt);
            Default.ImplementOperator(Operators.Int64ReinterpretFloat64, OperatorImpls.Int64ReinterpretFloat64);
            Default.ImplementOperator(Operators.Int64RemS, OperatorImpls.Int64RemS);
            Default.ImplementOperator(Operators.Int64RemU, OperatorImpls.Int64RemU);
            Default.ImplementOperator(Operators.Int64Rotl, OperatorImpls.Int64Rotl);
            Default.ImplementOperator(Operators.Int64Rotr, OperatorImpls.Int64Rotr);
            Default.ImplementOperator(Operators.Int64Shl, OperatorImpls.Int64Shl);
            Default.ImplementOperator(Operators.Int64ShrS, OperatorImpls.Int64ShrS);
            Default.ImplementOperator(Operators.Int64ShrU, OperatorImpls.Int64ShrU);
            Default.ImplementOperator(Operators.Int64Sub, OperatorImpls.Int64Sub);
            Default.ImplementOperator(Operators.Int64TruncSFloat32, OperatorImpls.Int64TruncSFloat32);
            Default.ImplementOperator(Operators.Int64TruncSFloat64, OperatorImpls.Int64TruncSFloat64);
            Default.ImplementOperator(Operators.Int64TruncUFloat32, OperatorImpls.Int64TruncUFloat32);
            Default.ImplementOperator(Operators.Int64TruncUFloat64, OperatorImpls.Int64TruncUFloat64);
            Default.ImplementOperator(Operators.Int64Xor, OperatorImpls.Int64Xor);

            Default.ImplementOperator(Operators.Float32Abs, OperatorImpls.Float32Abs);
            Default.ImplementOperator(Operators.Float32Add, OperatorImpls.Float32Add);
            Default.ImplementOperator(Operators.Float32Ceil, OperatorImpls.Float32Ceil);
            Default.ImplementOperator(Operators.Float32ConvertSInt32, OperatorImpls.Float32ConvertSInt32);
            Default.ImplementOperator(Operators.Float32ConvertSInt64, OperatorImpls.Float32ConvertSInt64);
            Default.ImplementOperator(Operators.Float32ConvertUInt32, OperatorImpls.Float32ConvertUInt32);
            Default.ImplementOperator(Operators.Float32ConvertUInt64, OperatorImpls.Float32ConvertUInt64);
            Default.ImplementOperator(Operators.Float32Copysign, OperatorImpls.Float32Copysign);
            Default.ImplementOperator(Operators.Float32DemoteFloat64, OperatorImpls.Float32DemoteFloat64);
            Default.ImplementOperator(Operators.Float32Div, OperatorImpls.Float32Div);
            Default.ImplementOperator(Operators.Float32Eq, OperatorImpls.Float32Eq);
            Default.ImplementOperator(Operators.Float32Floor, OperatorImpls.Float32Floor);
            Default.ImplementOperator(Operators.Float32Ge, OperatorImpls.Float32Ge);
            Default.ImplementOperator(Operators.Float32Gt, OperatorImpls.Float32Gt);
            Default.ImplementOperator(Operators.Float32Le, OperatorImpls.Float32Le);
            Default.ImplementOperator(Operators.Float32Lt, OperatorImpls.Float32Lt);
            Default.ImplementOperator(Operators.Float32Max, OperatorImpls.Float32Max);
            Default.ImplementOperator(Operators.Float32Min, OperatorImpls.Float32Min);
            Default.ImplementOperator(Operators.Float32Mul, OperatorImpls.Float32Mul);
            Default.ImplementOperator(Operators.Float32Ne, OperatorImpls.Float32Ne);
            Default.ImplementOperator(Operators.Float32Nearest, OperatorImpls.Float32Nearest);
            Default.ImplementOperator(Operators.Float32Neg, OperatorImpls.Float32Neg);
            Default.ImplementOperator(Operators.Float32ReinterpretInt32, OperatorImpls.Float32ReinterpretInt32);
            Default.ImplementOperator(Operators.Float32Sqrt, OperatorImpls.Float32Sqrt);
            Default.ImplementOperator(Operators.Float32Sub, OperatorImpls.Float32Sub);
            Default.ImplementOperator(Operators.Float32Trunc, OperatorImpls.Float32Trunc);

            Default.ImplementOperator(Operators.Float64Abs, OperatorImpls.Float64Abs);
            Default.ImplementOperator(Operators.Float64Add, OperatorImpls.Float64Add);
            Default.ImplementOperator(Operators.Float64Ceil, OperatorImpls.Float64Ceil);
            Default.ImplementOperator(Operators.Float64ConvertSInt32, OperatorImpls.Float64ConvertSInt32);
            Default.ImplementOperator(Operators.Float64ConvertSInt64, OperatorImpls.Float64ConvertSInt64);
            Default.ImplementOperator(Operators.Float64ConvertUInt32, OperatorImpls.Float64ConvertUInt32);
            Default.ImplementOperator(Operators.Float64ConvertUInt64, OperatorImpls.Float64ConvertUInt64);
            Default.ImplementOperator(Operators.Float64Copysign, OperatorImpls.Float64Copysign);
            Default.ImplementOperator(Operators.Float64Div, OperatorImpls.Float64Div);
            Default.ImplementOperator(Operators.Float64Eq, OperatorImpls.Float64Eq);
            Default.ImplementOperator(Operators.Float64Floor, OperatorImpls.Float64Floor);
            Default.ImplementOperator(Operators.Float64Ge, OperatorImpls.Float64Ge);
            Default.ImplementOperator(Operators.Float64Gt, OperatorImpls.Float64Gt);
            Default.ImplementOperator(Operators.Float64Le, OperatorImpls.Float64Le);
            Default.ImplementOperator(Operators.Float64Lt, OperatorImpls.Float64Lt);
            Default.ImplementOperator(Operators.Float64Max, OperatorImpls.Float64Max);
            Default.ImplementOperator(Operators.Float64Min, OperatorImpls.Float64Min);
            Default.ImplementOperator(Operators.Float64Mul, OperatorImpls.Float64Mul);
            Default.ImplementOperator(Operators.Float64Ne, OperatorImpls.Float64Ne);
            Default.ImplementOperator(Operators.Float64Nearest, OperatorImpls.Float64Nearest);
            Default.ImplementOperator(Operators.Float64Neg, OperatorImpls.Float64Neg);
            Default.ImplementOperator(Operators.Float64PromoteFloat32, OperatorImpls.Float64PromoteFloat32);
            Default.ImplementOperator(Operators.Float64ReinterpretInt64, OperatorImpls.Float64ReinterpretInt64);
            Default.ImplementOperator(Operators.Float64Sqrt, OperatorImpls.Float64Sqrt);
            Default.ImplementOperator(Operators.Float64Sub, OperatorImpls.Float64Sub);
            Default.ImplementOperator(Operators.Float64Trunc, OperatorImpls.Float64Trunc);
        }
    }
}