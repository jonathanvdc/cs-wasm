using System.IO;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes a WebAssembly stack machine instruction that takes a
    /// 32-bit floating-point number as immediate.
    /// </summary>
    public sealed class Float32Instruction : Instruction
    {
        /// <summary>
        /// Creates an instruction that takes a 32-bit floating-point number immediate.
        /// </summary>
        /// <param name="op">The instruction's opcode.</param>
        /// <param name="immediate">The instruction's immediate.</param>
        public Float32Instruction(Float32Operator op, float immediate)
        {
            this.opValue = op;
            this.Immediate = immediate;
        }

        private Float32Operator opValue;

        /// <summary>
        /// Gets the operator for this instruction.
        /// </summary>
        /// <returns>The instruction's operator.</returns>
        public override Operator Op { get { return opValue; } }

        /// <summary>
        /// Gets this instruction's immediate.
        /// </summary>
        /// <returns>The immediate value.</returns>
        public float Immediate { get; set; }

        /// <summary>
        /// Writes this instruction's immediates (but not its opcode)
        /// to the given WebAssembly file writer.
        /// </summary>
        /// <param name="writer">The writer to write this instruction's immediates to.</param>
        public override void WriteImmediatesTo(BinaryWasmWriter writer)
        {
            writer.WriteFloat32(Immediate);
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
            writer.Write(" ");
            writer.Write(Immediate);
        }
    }
}