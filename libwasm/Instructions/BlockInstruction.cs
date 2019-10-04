using System.Collections.Generic;
using System.IO;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes a WebAssembly stack machine instruction that takes a
    /// block of instructions as an immediate.
    /// </summary>
    public sealed class BlockInstruction : Instruction
    {
        /// <summary>
        /// Creates a block instruction.
        /// </summary>
        /// <param name="op">The operator performed by the block instruction.</param>
        /// <param name="type">The block instruction's result type.</param>
        /// <param name="contents">The block instruction's contents, as a sequence of instructions.</param>
        public BlockInstruction(BlockOperator op, WasmType type, IEnumerable<Instruction> contents)
        {
            this.opValue = op;
            this.Type = type;
            this.Contents = new List<Instruction>(contents);
        }

        private BlockOperator opValue;

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
            foreach (var instr in Contents)
            {
                instr.WriteTo(writer);
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
            foreach (var instr in Contents)
            {
                indentedWriter.WriteLine();
                instr.Dump(indentedWriter);
            }
            writer.WriteLine();
            writer.Write("end");
        }
    }
}