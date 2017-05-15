using System.IO;
using System.Text;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes an operator that takes a single 64-bit signed integer immediate.
    /// </summary>
    public sealed class VarInt64Operator : Operator
    {
        public VarInt64Operator(byte OpCode, WasmType DeclaringType, string Mnemonic)
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
            return new VarInt64Instruction(this, Reader.ReadVarInt64());
        }
    }
}