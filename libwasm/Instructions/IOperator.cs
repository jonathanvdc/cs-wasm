using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes an operator, which consists of an opcode and a mnemonic.
    /// </summary>
    public interface IOperator
    {
        /// <summary>
        /// Gets the opcode for this operator.
        /// </summary>
        /// <returns>The operator's opcode.</returns>
        byte OpCode { get; }

        /// <summary>
        /// Gets the mnemonic for this operator.
        /// </summary>
        /// <returns>The operator's mnemonic.</returns>
        string Mnemonic { get; }

        /// <summary>
        /// Reads the immediates (not the opcode) of a WebAssembly instruction
        /// for this operator from the given reader and returns the result as an
        /// instruction.
        /// </summary>
        /// <param name="Reader">The WebAssembly file reader to read immediates from.</param>
        /// <returns>A WebAssembly instruction.</returns>
        Instruction ReadImmediates(BinaryWasmReader Reader);
    }
}