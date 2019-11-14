using System.IO;
using System.Text;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes a WebAssembly stack machine instruction that takes a
    /// variable-length 32-bit unsigned integer as immediate.
    /// </summary>
    public sealed class VarUInt32Instruction : Instruction
    {
        /// <summary>
        /// Creates an instruction that takes a variable-length 32-bit unsigned integer immediate.
        /// </summary>
        /// <param name="op">An appropriate operator.</param>
        /// <param name="immediate">A decoded immediate.</param>
        public VarUInt32Instruction(Operator op, uint immediate)
        {
            this.opValue = op;
            this.Immediate = immediate;
        }

        private Operator opValue;

        /// <summary>
        /// Gets the operator for this instruction.
        /// </summary>
        /// <returns>The instruction's operator.</returns>
        public override Operator Op { get { return opValue; } }

        /// <summary>
        /// Gets or sets this instruction's immediate.
        /// </summary>
        /// <returns>The immediate value.</returns>
        public uint Immediate { get; set; }

        /// <summary>
        /// Writes this instruction's immediates (but not its opcode)
        /// to the given WebAssembly file writer.
        /// </summary>
        /// <param name="writer">The writer to write this instruction's immediates to.</param>
        public override void WriteImmediatesTo(BinaryWasmWriter writer)
        {
            writer.WriteVarUInt32(Immediate);
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
