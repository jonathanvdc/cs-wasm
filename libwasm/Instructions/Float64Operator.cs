using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes an operator that takes a single 64-bit floating-point number immediate.
    /// </summary>
    public sealed class Float64Operator : Operator
    {
        /// <summary>
        /// Creates an operator that takes a 64-bit floating-point number immediate.
        /// </summary>
        /// <param name="opCode">The operator's opcode.</param>
        /// <param name="declaringType">A type that defines the operator, if any.</param>
        /// <param name="mnemonic">The operator's mnemonic.</param>
        public Float64Operator(byte opCode, WasmType declaringType, string mnemonic)
            : base(opCode, declaringType, mnemonic)
        { }

        /// <summary>
        /// Reads the immediates (not the opcode) of a WebAssembly instruction
        /// for this operator from the given reader and returns the result as an
        /// instruction.
        /// </summary>
        /// <param name="Reader">The WebAssembly file reader to read immediates from.</param>
        /// <returns>A WebAssembly instruction.</returns>
        public override Instruction ReadImmediates(BinaryWasmReader Reader)
        {
            return Create(Reader.ReadFloat64());
        }

        /// <summary>
        /// Creates a new instruction from this operator and the given
        /// immediate.
        /// </summary>
        /// <param name="Immediate">The immediate.</param>
        /// <returns>A new instruction.</returns>
        public Float64Instruction Create(double Immediate)
        {
            return new Float64Instruction(this, Immediate);
        }

        /// <summary>
        /// Casts the given instruction to this operator's instruction type.
        /// </summary>
        /// <param name="Value">The instruction to cast.</param>
        /// <returns>The given instruction as this operator's instruction type.</returns>
        public Float64Instruction CastInstruction(Instruction Value)
        {
            return (Float64Instruction)Value;
        }
    }
}
