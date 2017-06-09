using System;
using System.Collections.Generic;
using System.IO;
using Wasm.Instructions;

namespace Wasm.Interpret
{
    /// <summary>
    /// A type of interpreter that creates an execution trace as it runs instructions.
    /// </summary>
    public class TracingInstructionInterpreter : InstructionInterpreter
    {
        /// <summary>
        /// Creates a tracing instruction interpreter from the given inner interpreter
        /// and a trace writer.
        /// </summary>
        /// <param name="Interpreter">The inner interpreter that is used to run instructions.</param>
        /// <param name="TraceWriter">The text writer to which execution traces are written.</param>
        public TracingInstructionInterpreter(
            InstructionInterpreter Interpreter, TextWriter TraceWriter)
        {
            this.Interpreter = Interpreter;
            this.TraceWriter = TraceWriter;
        }

        /// <summary>
        /// Gets the inner interpreter that is used to run instructions.
        /// </summary>
        /// <returns>The instruction interpreter.</returns>
        public InstructionInterpreter Interpreter { get; private set; }

        /// <summary>
        /// Gets the text writer to which execution traces are written.
        /// </summary>
        /// <returns>The text writer.</returns>
        public TextWriter TraceWriter { get; private set; }

        /// <summary>
        /// Writes an instruction to the trace writer.
        /// </summary>
        /// <param name="Value">The instruction.</param>
        protected virtual void Trace(Instruction Value)
        {
            if (Value is BlockInstruction || Value is IfElseInstruction)
            {
                Value.Op.Dump(TraceWriter);
            }
            else
            {
                Value.Dump(TraceWriter);
            }
            TraceWriter.WriteLine();
        }

        /// <inheritdoc/>
        public override void Interpret(Instruction Value, InterpreterContext Context)
        {
            // Trace the instruction.
            if (!Context.HasReturned)
            {
                Trace(Value);
            }

            // Actually interpret the instruction.
            Interpreter.Interpret(Value, Context);
        }
    }
}