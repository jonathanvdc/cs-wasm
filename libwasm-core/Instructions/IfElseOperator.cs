using System.Collections.Generic;
using System.IO;
using System.Text;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes an operator that runs one of two blocks
    /// </summary>
    public sealed class IfElseOperator : Operator
    {
        public IfElseOperator(byte OpCode, WasmType DeclaringType, string Mnemonic)
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
            var type = Reader.ReadWasmType();
            return ReadBlockContents(type, Reader);
        }

        /// <summary>
        /// Reads the child instructions of a WebAssembly block from the given reader.
        /// </summary>
        /// <param name="BlockType">The type of value returned by the resulting block.</param>
        /// <param name="Reader">The WebAssembly file reader.</param>
        /// <returns>A WebAssembly block instruction.</returns>
        public static IfElseInstruction ReadBlockContents(WasmType BlockType, BinaryWasmReader Reader)
        {
            var ifBranch = new List<Instruction>();
            List<Instruction> elseBranch = null;
            while (true)
            {
                byte opCode = Reader.Reader.ReadByte();
                if (opCode == Operators.EndOpCode)
                {
                    return new IfElseInstruction(BlockType, ifBranch, elseBranch);
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
                    (elseBranch == null ? ifBranch : elseBranch).Add(op.ReadImmediates(Reader));
                }
            }
        }

        /// <summary>
        /// Casts the given instruction to this operator's instruction type.
        /// </summary>
        /// <param name="Value">The instruction to cast.</param>
        /// <returns>The given instruction as this operator's instruction type.</returns>
        public IfElseInstruction CastInstruction(Instruction Value)
        {
            return (IfElseInstruction)Value;
        }
    }
}