using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes an operator that calls a function pointer.
    /// </summary>
    public sealed class CallIndirectOperator : Operator
    {
        /// <summary>
        /// Creates an indirect call operator.
        /// </summary>
        /// <param name="opCode">The operator's opcode.</param>
        /// <param name="declaringType">A type that defines the operator, if any.</param>
        /// <param name="mnemonic">The operator's mnemonic.</param>
        public CallIndirectOperator(byte opCode, WasmType declaringType, string mnemonic)
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
            return new CallIndirectInstruction(this, reader.ReadVarUInt32(), reader.ReadVarUInt32());
        }

        /// <summary>
        /// Creates an indirect call instruction from this operator and
        /// an index into the 'type' table.
        /// </summary>
        /// <param name="typeIndex">The index of the callee's signature in the type table.</param>
        public CallIndirectInstruction Create(uint typeIndex)
        {
            return new CallIndirectInstruction(this, typeIndex);
        }

        /// <summary>
        /// Casts the given instruction to this operator's instruction type.
        /// </summary>
        /// <param name="value">The instruction to cast.</param>
        /// <returns>The given instruction as this operator's instruction type.</returns>
        public CallIndirectInstruction CastInstruction(Instruction value)
        {
            return (CallIndirectInstruction)value;
        }
    }
}