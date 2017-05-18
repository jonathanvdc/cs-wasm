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
        /// No additional operators should be implemented in this interpreter.
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
            Default.ImplementOperator(Operators.Drop, OperatorImpls.Drop);
            Default.ImplementOperator(Operators.Int32Const, OperatorImpls.Int32Const);
            Default.ImplementOperator(Operators.Int64Const, OperatorImpls.Int64Const);
            Default.ImplementOperator(Operators.Float32Const, OperatorImpls.Float32Const);
            Default.ImplementOperator(Operators.Float64Const, OperatorImpls.Float64Const);
        }
    }
}