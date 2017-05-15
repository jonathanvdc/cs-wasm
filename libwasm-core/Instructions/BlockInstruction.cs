using System.Collections.Generic;
using System.IO;
using System.Text;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes a WebAssembly stack machine instruction that takes a
    /// block of instructions as an immediate.
    /// </summary>
    public sealed class BlockInstruction : Instruction
    {
        public BlockInstruction(Operator Op, WasmType Type, IEnumerable<Instruction> Contents)
        {
            this.opValue = Op;
            this.Type = Type;
            this.Contents = new List<Instruction>(Contents);
        }

        private Operator opValue;

        /// <summary>
        /// Gets the operator for this instruction.
        /// </summary>
        /// <returns>The instruction's operator.</returns>
        public override Operator Op { get { return opValue; } }

        /// <summary>
        /// Gets the type of value returned by this block.
        /// </summary>
        /// <returns>The type of value returned by this block.</returns>
        public WasmType Type { get; set; }

        /// <summary>
        /// Gets this block instruction's contents.
        /// </summary>
        /// <returns>The instruction's contents.</returns>
        public List<Instruction> Contents { get; private set; }

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
            foreach (var instr in Contents)
            {
                instr.WriteTo(Writer);
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
            foreach (var instr in Contents)
            {
                indentedWriter.WriteLine();
                instr.Dump(indentedWriter);
            }
            Writer.WriteLine();
            Writer.Write("end");
        }
    }
}