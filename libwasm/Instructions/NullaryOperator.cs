using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes an operator that does not have any immediates.
    /// </summary>
    public sealed class NullaryOperator : Operator
    {
        /// <summary>
        /// Creates a nullary operator.
        /// </summary>
        /// <param name="opCode">The operator's opcode.</param>
        /// <param name="declaringType">A type that defines the operator, if any.</param>
        /// <param name="mnemonic">The operator's mnemonic.</param>
        public NullaryOperator(byte opCode, WasmType declaringType, string mnemonic)
            : base(opCode, declaringType, mnemonic)
        {
            this.instruction = new NullaryInstruction(this);
        }

        /// <summary>
        /// A nullary instruction for this operator. Since nullary operators don't take
        /// any values, their instruction instances can be shared.
        /// </summary>
        private NullaryInstruction instruction;

        /// <summary>
        /// Gets an instruction that applies this operator.
        /// </summary>
        /// <returns>An instruction.</returns>
        public NullaryInstruction Create()
        {
            return instruction;
        }

        /// <summary>
        /// Reads the immediates (not the opcode) of a WebAssembly instruction
        /// for this operator from the given reader and returns the result as an
        /// instruction.
        /// </summary>
        /// <param name="reader">The WebAssembly file reader to read immediates from.</param>
        /// <returns>A WebAssembly instruction.</returns>
        public override Instruction ReadImmediates(BinaryWasmReader reader)
        {
            // Return the shared nullary instruction.
            return instruction;
        }
    }
}
