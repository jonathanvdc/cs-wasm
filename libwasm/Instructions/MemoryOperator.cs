using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes an operator that accesses a memory location.
    /// </summary>
    public sealed class MemoryOperator : Operator
    {
        /// <summary>
        /// Creates a memory operator.
        /// </summary>
        /// <param name="opCode">The operator's opcode.</param>
        /// <param name="declaringType">A type that defines the operator, if any.</param>
        /// <param name="mnemonic">The operator's mnemonic.</param>
        public MemoryOperator(byte opCode, WasmType declaringType, string mnemonic)
            : base(opCode, declaringType, mnemonic)
        { }

        /// <summary>
        /// Reads the immediates (not the opcode) of a WebAssembly instruction
        /// for this operator from the given reader and returns the result as an
        /// instruction.
        /// </summary>
        /// <param name="reader">The WebAssembly file reader to read immediates from.</param>
        /// <returns>A WebAssembly instruction.</returns>
        public override Instruction ReadImmediates(BinaryWasmReader reader)
        {
            return Create(reader.ReadVarUInt32(), reader.ReadVarUInt32());
        }

        /// <summary>
        /// Creates a new instruction from this operator and the given
        /// immediates.
        /// </summary>
        /// <param name="log2Alignment">The log2 of the memory alignment for this instruction.</param>
        /// <param name="offset">
        /// The offset of the memory location relative to the pointer that is accessed.
        /// </param>
        /// <returns>A new instruction.</returns>
        public MemoryInstruction Create(uint log2Alignment, uint offset)
        {
            return new MemoryInstruction(this, log2Alignment, offset);
        }

        /// <summary>
        /// Casts the given instruction to this operator's instruction type.
        /// </summary>
        /// <param name="value">The instruction to cast.</param>
        /// <returns>The given instruction as this operator's instruction type.</returns>
        public MemoryInstruction CastInstruction(Instruction value)
        {
            return (MemoryInstruction)value;
        }
    }
}
