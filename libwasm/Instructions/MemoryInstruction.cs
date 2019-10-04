using System.IO;
using System.Text;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes a WebAssembly stack machine instruction that takes a memory-immediate.
    /// </summary>
    public sealed class MemoryInstruction : Instruction
    {
        /// <summary>
        /// Creates a memory instruction from the given operator, alignment
        /// and offset.
        /// </summary>
        /// <param name="Op">The operator for this memory instruction.</param>
        /// <param name="Log2Alignment">The log2 of the memory alignment for this instruction.</param>
        /// <param name="Offset">
        /// The offset of the memory location relative to the pointer that is accessed.
        /// </param>
        public MemoryInstruction(Operator Op, uint Log2Alignment, uint Offset)
        {
            this.opValue = Op;
            this.Log2Alignment = Log2Alignment;
            this.Offset = Offset;
        }

        private Operator opValue;

        /// <summary>
        /// Gets the operator for this instruction.
        /// </summary>
        /// <returns>The instruction's operator.</returns>
        public override Operator Op { get { return opValue; } }

        /// <summary>
        /// Gets log2(alignment), where the alignment is the memory location's
        /// alignment. As implied by the log2(alignment) encoding, the alignment
        /// must be a power of 2. As an additional validation criteria, the
        /// alignment must be less or equal to natural alignment.
        /// </summary>
        /// <returns>log2(alignment)</returns>
        public uint Log2Alignment { get; private set; }

        /// <summary>
        /// Gets the memory location's alignment.
        /// </summary>
        /// <returns>The memory location's alignment.</returns>
        public uint Alignment
        {
            get
            {
                return 1u << (int)Log2Alignment;
            }
        }

        /// <summary>
        /// Gets the offset of the memory location relative to the pointer
        /// that is accessed.
        /// </summary>
        /// <returns>The offset of the memory location relative to the pointer
        /// that is accessed.</returns>
        public uint Offset { get; private set; }

        /// <summary>
        /// Writes this instruction's immediates (but not its opcode)
        /// to the given WebAssembly file writer.
        /// </summary>
        /// <param name="Writer">The writer to write this instruction's immediates to.</param>
        public override void WriteImmediatesTo(BinaryWasmWriter Writer)
        {
            Writer.WriteVarUInt32(Log2Alignment);
            Writer.WriteVarUInt32(Offset);
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
            Writer.Write(" offset=");
            Writer.Write(Offset);
            Writer.Write(" align=");
            Writer.Write(Alignment);
        }
    }
}