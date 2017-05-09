using System.Collections.Generic;
using System.IO;
using System.Text;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes a WebAssembly stack machine instruction that runs one of two
    /// blocks of instructions, based on the value of its predicate.
    /// </summary>
    public sealed class IfElseInstruction : Instruction
    {
        public IfElseInstruction(
            WasmType Type,
            IEnumerable<Instruction> IfBranch,
            IEnumerable<Instruction> ElseBranch)
        {
            this.Type = Type;
            this.IfBranch = new List<Instruction>(IfBranch);
            this.ElseBranch = ElseBranch == null ? null : new List<Instruction>(ElseBranch);
        }

        /// <summary>
        /// Gets the operator for this instruction.
        /// </summary>
        /// <returns>The instruction's operator.</returns>
        public override Operator Op { get { return Operators.If; } }

        /// <summary>
        /// Gets the type of value returned by this block.
        /// </summary>
        /// <returns>The type of value returned by this block.</returns>
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
        /// <param name="Writer">The writer to write this instruction's immediates to.</param>
        public override void WriteImmediatesTo(BinaryWasmWriter Writer)
        {
            Writer.WriteWasmType(Type);
            WriteContentsTo(Writer);
        }

        /// <summary>
        /// Writes this instruction's child instructions to the given WebAssembly file writer,
        /// followed by an 'end' opcode.
        /// </summary>
        /// <param name="Writer">The writer to write this instruction's child instructions to.</param>
        public void WriteContentsTo(BinaryWasmWriter Writer)
        {
            foreach (var instr in IfBranch)
            {
                instr.WriteTo(Writer);
            }
            if (HasElseBranch)
            {
                Writer.Writer.Write(Operators.ElseOpCode);
                foreach (var instr in ElseBranch)
                {
                    instr.WriteTo(Writer);
                }
            }
            Writer.Writer.Write(Operators.EndOpCode);
        }

        /// <summary>
        /// Writes a string representation of this instruction to the given text writer.
        /// </summary>
        /// <param name="Writer">
        /// The writer to which a representation of this instruction is written.
        /// </param>
        public override void Dump(TextWriter Writer)
        {
            Op.Dump(Writer);
            Writer.Write(" (result: ");
            DumpHelpers.DumpWasmType(Type, Writer);
            Writer.Write(")");
            var indentedWriter = DumpHelpers.CreateIndentedTextWriter(Writer);
            foreach (var instr in IfBranch)
            {
                indentedWriter.WriteLine();
                instr.Dump(indentedWriter);
            }
            Writer.WriteLine();
            if (HasElseBranch)
            {
                Writer.Write("else");
                foreach (var instr in ElseBranch)
                {
                    indentedWriter.WriteLine();
                    instr.Dump(indentedWriter);
                }
                Writer.WriteLine();
            }
            Writer.Write("end");
        }
    }
}