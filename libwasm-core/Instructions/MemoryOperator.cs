using System.IO;
using System.Text;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes an operator that accesses a memory location.
    /// </summary>
    public sealed class MemoryOperator : Operator
    {
        public MemoryOperator(byte OpCode, WasmType DeclaringType, string Mnemonic)
            : base(OpCode, DeclaringType, Mnemonic)
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
            return Create(Reader.ReadVarUInt32(), Reader.ReadVarUInt32());
        }

        /// <summary>
        /// Creates a new instruction from this operator and the given
        /// immediates.
        /// </summary>
        /// <param name="Log2Alignment">The log2 of the memory alignment for this instruction.</param>
        /// <param name="Offset">
        /// The offset of the memory location relative to the pointer that is accessed.
        /// </param>
        /// <returns>A new instruction.</returns>
        public MemoryInstruction Create(uint Log2Alignment, uint Offset)
        {
            return new MemoryInstruction(this, Log2Alignment, Offset);
        }

        /// <summary>
        /// Casts the given instruction to this operator's instruction type.
        /// </summary>
        /// <param name="Value">The instruction to cast.</param>
        /// <returns>The given instruction as this operator's instruction type.</returns>
        public MemoryInstruction CastInstruction(Instruction Value)
        {
            return (MemoryInstruction)Value;
        }
    }
}