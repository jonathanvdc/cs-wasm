using System.Collections.Generic;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes an operator that begins a sequence of expressions, yielding 0 or 1 values.
    /// </summary>
    public sealed class BlockOperator : Operator
    {
        /// <summary>
        /// Creates a block operator.
        /// </summary>
        /// <param name="opCode">The block operator's opcode.</param>
        /// <param name="declaringType">The type that declares the operator, if any.</param>
        /// <param name="mnemonic">The operator's mnemonic.</param>
        public BlockOperator(byte opCode, WasmType declaringType, string mnemonic)
            : base(opCode, declaringType, mnemonic)
        { }

        /// <summary>
        /// Creates a block instruction with this operator and the given operands.
        /// </summary>
        /// <param name="blockType">The resulting block instruction's type.</param>
        /// <param name="contents">
        /// The resulting block instruction's contents, as a sequence of instructions.
        /// </param>
        /// <returns>A block instruction.</returns>
        public BlockInstruction Create(WasmType blockType, IEnumerable<Instruction> contents)
        {
            return new BlockInstruction(this, blockType, contents);
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
            var type = reader.ReadWasmType();
            return ReadBlockContents(type, reader);
        }

        /// <summary>
        /// Reads the child instructions of a WebAssembly block from the given reader.
        /// </summary>
        /// <param name="blockType">The type of value returned by the resulting block.</param>
        /// <param name="reader">The WebAssembly file reader.</param>
        /// <returns>A WebAssembly block instruction.</returns>
        public BlockInstruction ReadBlockContents(WasmType blockType, BinaryWasmReader reader)
        {
            var contents = new List<Instruction>();
            while (true)
            {
                byte opCode = reader.ReadByte();
                if (opCode == Operators.EndOpCode)
                {
                    return Create(blockType, contents);
                }
                else
                {
                    var op = Operators.GetOperatorByOpCode(opCode);
                    contents.Add(op.ReadImmediates(reader));
                }
            }
        }

        /// <summary>
        /// Casts the given instruction to this operator's instruction type.
        /// </summary>
        /// <param name="value">The instruction to cast.</param>
        /// <returns>The given instruction as this operator's instruction type.</returns>
        public BlockInstruction CastInstruction(Instruction value)
        {
            return (BlockInstruction)value;
        }
    }
}