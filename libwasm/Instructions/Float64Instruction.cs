using System.IO;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes a WebAssembly stack machine instruction that takes a
    /// 64-bit floating-point number as immediate.
    /// </summary>
    public sealed class Float64Instruction : Instruction
    {
        /// <summary>
        /// Creates an instruction that takes a 32-bit floating-point number immediate.
        /// </summary>
        /// <param name="op">The instruction's opcode.</param>
        /// <param name="immediate">The instruction's immediate.</param>
        public Float64Instruction(Float64Operator op, double immediate)
        {
            this.opValue = op;
            this.Immediate = immediate;
        }

        private Float64Operator opValue;

        /// <summary>
        /// Gets the operator for this instruction.
        /// </summary>
        /// <returns>The instruction's operator.</returns>
        public override Operator Op { get { return opValue; } }

        /// <summary>
        /// Gets this instruction's immediate.
        /// </summary>
        /// <returns>The immediate value.</returns>
        public double Immediate { get; set; }

        /// <summary>
        /// Writes this instruction's immediates (but not its opcode)
        /// to the given WebAssembly file writer.
        /// </summary>
        /// <param name="writer">The writer to write this instruction's immediates to.</param>
        public override void WriteImmediatesTo(BinaryWasmWriter writer)
        {
            writer.WriteFloat64(Immediate);
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
