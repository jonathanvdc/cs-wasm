using System.Collections.Generic;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes an operator that runs one of two blocks
    /// </summary>
    public sealed class IfElseOperator : Operator
    {
        internal IfElseOperator(byte opCode, WasmType declaringType, string mnemonic)
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
            var type = reader.ReadWasmType();
            return ReadBlockContents(type, reader);
        }

        /// <summary>
        /// Reads the child instructions of a WebAssembly block from the given reader.
        /// </summary>
        /// <param name="blockType">The type of value returned by the resulting block.</param>
        /// <param name="reader">The WebAssembly file reader.</param>
        /// <returns>A WebAssembly block instruction.</returns>
        public static IfElseInstruction ReadBlockContents(WasmType blockType, BinaryWasmReader reader)
        {
            var ifBranch = new List<Instruction>();
            List<Instruction> elseBranch = null;
            while (true)
            {
                byte opCode = reader.ReadByte();
                if (opCode == Operators.EndOpCode)
                {
                    return new IfElseInstruction(blockType, ifBranch, elseBranch);
                }
                else if (opCode == Operators.ElseOpCode)
                {
                    if (elseBranch != null)
                    {
                        throw new WasmException("More than one 'else' opcode in an 'if' instruction");
                    }

                    elseBranch = new List<Instruction>();
                }
                else
                {
                    var op = Operators.GetOperatorByOpCode(opCode);
                    (elseBranch == null ? ifBranch : elseBranch).Add(op.ReadImmediates(reader));
                }
            }
        }

        /// <summary>
        /// Creates an if-else instruction from the given type, if-branch and
        /// else-branch.
        /// </summary>
        /// <param name="type">The type of value returned by the if-else instruction.</param>
        /// <param name="ifBranch">The if-else instruction's 'if' branch.</param>
        /// <param name="elseBranch">The if-else instruction's 'else' branch.</param>
        public IfElseInstruction Create(
            WasmType type,
            IEnumerable<Instruction> ifBranch,
            IEnumerable<Instruction> elseBranch)
        {
            return new IfElseInstruction(type, ifBranch, elseBranch);
        }

        /// <summary>
        /// Casts the given instruction to this operator's instruction type.
        /// </summary>
        /// <param name="value">The instruction to cast.</param>
        /// <returns>The given instruction as this operator's instruction type.</returns>
        public IfElseInstruction CastInstruction(Instruction value)
        {
            return (IfElseInstruction)value;
        }
    }
}
