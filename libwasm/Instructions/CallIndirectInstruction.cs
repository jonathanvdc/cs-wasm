using System.IO;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes a WebAssembly stack machine instruction that calls a function pointer.
    /// </summary>
    public sealed class CallIndirectInstruction : Instruction
    {
        /// <summary>
        /// Creates an indirect call instruction from the given operator and
        /// an index into the 'type' table.
        /// </summary>
        /// <param name="op">The operator for this instruction.</param>
        /// <param name="typeIndex">The index of the callee's signature in the type table.</param>
        public CallIndirectInstruction(CallIndirectOperator op, uint typeIndex)
            : this(op, typeIndex, 0)
        { }

        /// <summary>
        /// Creates an indirect call instruction from the given operator,
        /// an index into the 'type' table and a value for the reserved field.
        /// </summary>
        /// <param name="op">The operator for this instruction.</param>
        /// <param name="typeIndex">The index of the callee's signature in the type table.</param>
        /// <param name="reserved">A reserved value, which should always be zero.</param>
        public CallIndirectInstruction(CallIndirectOperator op, uint typeIndex, uint reserved)
        {
            this.opValue = op;
            this.TypeIndex = typeIndex;
            this.Reserved = reserved;
        }

        private CallIndirectOperator opValue;

        /// <summary>
        /// Gets the operator for this instruction.
        /// </summary>
        /// <returns>The instruction's operator.</returns>
        public override Operator Op { get { return opValue; } }

        /// <summary>
        /// Gets the index of the callee's signature in the type table.
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
        /// <param name="writer">The writer to write this instruction's immediates to.</param>
        public override void WriteImmediatesTo(BinaryWasmWriter writer)
        {
            writer.WriteVarUInt32(TypeIndex);
            writer.WriteVarUInt32(Reserved);
        }

        /// <summary>
        /// Writes a string representation of this instruction to the given text writer.
        /// </summary>
        /// <param name="writer">
        /// The writer to which a representation of this instruction is written.
        /// </param>
        public override void Dump(TextWriter writer)
        {
            Op.Dump(writer);
            writer.Write(" ");
            writer.Write(TypeIndex);
            if (Reserved != 0)
            {
                writer.Write(" (reserved=");
                writer.Write(Reserved);
                writer.Write(")");
            }
        }
    }
}
