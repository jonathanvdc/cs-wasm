using System.IO;
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
        /// <param name="op">The operator for this memory instruction.</param>
        /// <param name="log2Alignment">The log2 of the memory alignment for this instruction.</param>
        /// <param name="offset">
        /// The offset of the memory location relative to the pointer that is accessed.
        /// </param>
        public MemoryInstruction(MemoryOperator op, uint log2Alignment, uint offset)
        {
            this.opValue = op;
            this.Log2Alignment = log2Alignment;
            this.Offset = offset;
        }

        private MemoryOperator opValue;

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
        /// <param name="writer">The writer to write this instruction's immediates to.</param>
        public override void WriteImmediatesTo(BinaryWasmWriter writer)
        {
            writer.WriteVarUInt32(Log2Alignment);
            writer.WriteVarUInt32(Offset);
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
            writer.Write(" offset=");
            writer.Write(Offset);
            writer.Write(" align=");
            writer.Write(Alignment);
        }
    }
}
