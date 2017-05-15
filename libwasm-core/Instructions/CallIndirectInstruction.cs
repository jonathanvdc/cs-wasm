using System.IO;
using System.Text;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes a WebAssembly stack machine instruction that calls a function pointer.
    /// </summary>
    public sealed class CallIndirectInstruction : Instruction
    {
        public CallIndirectInstruction(Operator Op, uint TypeIndex)
            : this(Op, TypeIndex, 0)
        { }

        public CallIndirectInstruction(Operator Op, uint TypeIndex, uint Reserved)
        {
            this.opValue = Op;
            this.TypeIndex = TypeIndex;
            this.Reserved = Reserved;
        }

        private Operator opValue;

        /// <summary>
        /// Gets the operator for this instruction.
        /// </summary>
        /// <returns>The instruction's operator.</returns>
        public override Operator Op { get { return opValue; } }

        /// <summary>
        /// Gets the index in the type table of the callee's signature.
        /// </summary>
        /// <returns>The callee's signature, as an index in the type table.</returns>
        public uint TypeIndex { get; private set; }

        /// <summary>
        /// Gets a reserved value. This should always be zero.
        /// </summary>
        /// <returns>A reserved value.</returns>
        public uint Reserved { get; private set; }

        /// <summary>
        /// Writes this instruction's immediates (but not its opcode)
        /// to the given WebAssembly file writer.
        /// </summary>
        /// <param name="Writer">The writer to write this instruction's immediates to.</param>
        public override void WriteImmediatesTo(BinaryWasmWriter Writer)
        {
            Writer.WriteVarUInt32(TypeIndex);
            Writer.WriteVarUInt32(Reserved);
        }

        /// <summary>
        /// Writes a string representation of this instruction to the given text writer.
        /// </summary>
        /// <param name="Writer">
        /// The writer to which a representation of this instruction is written.
        /// </param>
        public override void Dump(TextWriter Writer)
        {
            Op.Dump(Writer);
            Writer.Write(" ");
            Writer.Write(TypeIndex);
            if (Reserved != 0)
            {
                Writer.Write(" (reserved=");
                Writer.Write(Reserved);
                Writer.Write(")");
            }
        }
    }
}