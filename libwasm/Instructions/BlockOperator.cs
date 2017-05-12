using System.Collections.Generic;
using System.IO;
using System.Text;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes an operator that begins a sequence of expressions, yielding 0 or 1 values.
    /// </summary>
    public sealed class BlockOperator : Operator
    {
        public BlockOperator(byte OpCode, WasmType DeclaringType, string Mnemonic)
            : base(OpCode, DeclaringType, Mnemonic)
        { }

        /// <summary>
        /// Creates a block instruction with this operator and the given operands.
        /// </summary>
        /// <param name="BlockType">The resulting block instruction's type.</param>
        /// <param name="Contents">
        /// The resulting block instruction's contents, as a sequence of instructions.
        /// </param>
        /// <returns>A block instruction.</returns>
        public BlockInstruction Create(WasmType BlockType, IEnumerable<Instruction> Contents)
        {
            return new BlockInstruction(this, BlockType, Contents);
        }

        /// <summary>
        /// Reads the immediates (not the opcode) of a WebAssembly instruction
        /// for this operator from the given reader and returns the result as an
        /// instruction.
        /// </summary>
        /// <param name="Reader">The WebAssembly file reader to read immediates from.</param>
        /// <returns>A WebAssembly instruction.</returns>
        public override Instruction ReadImmediates(BinaryWasmReader Reader)
        {
            var type = Reader.ReadWasmType();
            return ReadBlockContents(type, Reader);
        }

        /// <summary>
        /// Reads the child instructions of a WebAssembly block from the given reader.
        /// </summary>
        /// <param name="BlockType">The type of value returned by the resulting block.</param>
        /// <param name="Reader">The WebAssembly file reader.</param>
        /// <returns>A WebAssembly block instruction.</returns>
        public BlockInstruction ReadBlockContents(WasmType BlockType, BinaryWasmReader Reader)
        {
            var contents = new List<Instruction>();
            while (true)
            {
                byte opCode = Reader.Reader.ReadByte();
                if (opCode == Operators.EndOpCode)
                {
                    return Create(BlockType, contents);
                }
                else
                {
                    var op = Operators.GetOperatorByOpCode(opCode);
                    contents.Add(op.ReadImmediates(Reader));
                }
            }
        }
    }
}