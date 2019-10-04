using System.Collections.Generic;
using System.IO;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes a WebAssembly stack machine instruction that runs one of two
    /// blocks of instructions, based on the value of its predicate.
    /// </summary>
    public sealed class IfElseInstruction : Instruction
    {
        /// <summary>
        /// Creates an if-else instruction from the given type, if-branch and
        /// else-branch.
        /// </summary>
        /// <param name="type">The type of value returned by the if-else instruction.</param>
        /// <param name="ifBranch">The if-else instruction's 'if' branch.</param>
        /// <param name="elseBranch">The if-else instruction's 'else' branch.</param>
        public IfElseInstruction(
            WasmType type,
            IEnumerable<Instruction> ifBranch,
            IEnumerable<Instruction> elseBranch)
        {
            this.Type = type;
            this.IfBranch = new List<Instruction>(ifBranch);
            this.ElseBranch = elseBranch == null ? null : new List<Instruction>(elseBranch);
        }

        /// <summary>
        /// Gets the operator for this instruction.
        /// </summary>
        /// <returns>The instruction's operator.</returns>
        public override Operator Op { get { return Operators.If; } }

        /// <summary>
        /// Gets the type of value returned by this instruction.
        /// </summary>
        /// <returns>The type of value returned by this instruction.</returns>
        public WasmType Type { get; set; }

        /// <summary>
        /// Gets this if-else instruction's contents for the 'if' branch.
        /// </summary>
        /// <returns>The if-else instruction's contents for the 'if' branch.</returns>
        public List<Instruction> IfBranch { get; private set; }

        /// <summary>
        /// Gets this if-else instruction's contents for the 'else' branch.
        /// </summary>
        /// <returns>
        /// The if-else instruction's contents for the 'else' branch.
        /// <c>null</c> is returned if there is no 'else' branch.
        /// </returns>
        public List<Instruction> ElseBranch { get; private set; }

        /// <summary>
        /// Checks if this if-else instruction has an 'else' branch.
        /// </summary>
        public bool HasElseBranch => ElseBranch != null;

        /// <summary>
        /// Writes this instruction's immediates (but not its opcode)
        /// to the given WebAssembly file writer.
        /// </summary>
        /// <param name="writer">The writer to write this instruction's immediates to.</param>
        public override void WriteImmediatesTo(BinaryWasmWriter writer)
        {
            writer.WriteWasmType(Type);
            WriteContentsTo(writer);
        }

        /// <summary>
        /// Writes this instruction's child instructions to the given WebAssembly file writer,
        /// followed by an 'end' opcode.
        /// </summary>
        /// <param name="writer">The writer to write this instruction's child instructions to.</param>
        public void WriteContentsTo(BinaryWasmWriter writer)
        {
            foreach (var instr in IfBranch)
            {
                instr.WriteTo(writer);
            }
            if (HasElseBranch)
            {
                writer.Writer.Write(Operators.ElseOpCode);
                foreach (var instr in ElseBranch)
                {
                    instr.WriteTo(writer);
                }
            }
            writer.Writer.Write(Operators.EndOpCode);
        }

        /// <summary>
        /// Writes a string representation of this instruction to the given text writer.
        /// </summary>
        /// <param name="writer">
        /// The writer to which a representation of this instruction is written.
        /// </param>
        public override void Dump(TextWriter writer)
        {
            Op.Dump(writer);
            writer.Write(" (result: ");
            DumpHelpers.DumpWasmType(Type, writer);
            writer.Write(")");
            var indentedWriter = DumpHelpers.CreateIndentedTextWriter(writer);
            foreach (var instr in IfBranch)
            {
                indentedWriter.WriteLine();
                instr.Dump(indentedWriter);
            }
            writer.WriteLine();
            if (HasElseBranch)
            {
                writer.Write("else");
                foreach (var instr in ElseBranch)
                {
                    indentedWriter.WriteLine();
                    instr.Dump(indentedWriter);
                }
                writer.WriteLine();
            }
            writer.Write("end");
        }
    }
}