using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes a WebAssembly stack machine instruction that does not have any immediates.
    /// </summary>
    public sealed class NullaryInstruction : Instruction
    {
        /// <summary>
        /// Creates a nullary instruction: an instruction that does not take any immediates.
        /// </summary>
        /// <param name="op">The nullary instruction's operator.</param>
        public NullaryInstruction(NullaryOperator op)
        {
            this.opValue = op;
        }

        private NullaryOperator opValue;

        /// <summary>
        /// Gets the operator for this instruction.
        /// </summary>
        /// <returns>The instruction's operator.</returns>
        public override Operator Op { get { return opValue; } }

        /// <summary>
        /// Writes this instruction's immediates (but not its opcode)
        /// to the given WebAssembly file writer.
        /// </summary>
        /// <param name="writer">The writer to write this instruction's immediates to.</param>
        public override void WriteImmediatesTo(BinaryWasmWriter writer)
        {
            // Do nothing. This instruction doesn't have any immediates.
        }
    }
}
