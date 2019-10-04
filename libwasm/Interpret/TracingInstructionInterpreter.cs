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
        /// <param name="interpreter">The inner interpreter that is used to run instructions.</param>
        /// <param name="traceWriter">The text writer to which execution traces are written.</param>
        public TracingInstructionInterpreter(
            InstructionInterpreter interpreter, TextWriter traceWriter)
        {
            this.Interpreter = interpreter;
            this.TraceWriter = traceWriter;
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
        /// <param name="value">The instruction.</param>
        protected virtual void Trace(Instruction value)
        {
            if (value is BlockInstruction || value is IfElseInstruction)
            {
                value.Op.Dump(TraceWriter);
            }
            else
            {
                value.Dump(TraceWriter);
            }
            TraceWriter.WriteLine();
        }

        /// <inheritdoc/>
        public override void Interpret(Instruction value, InterpreterContext context)
        {
            // Trace the instruction.
            if (!context.HasReturned)
            {
                Trace(value);
            }

            // Actually interpret the instruction.
            Interpreter.Interpret(value, context);
        }
    }
}