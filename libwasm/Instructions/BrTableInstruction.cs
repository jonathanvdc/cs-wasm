using System.Collections.Generic;
using System.IO;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes a WebAssembly stack machine instruction that represents a break table.
    /// </summary>
    public sealed class BrTableInstruction : Instruction
    {
        /// <summary>
        /// Creates a break table instruction from the given operator, table of
        /// break targets and a default target.
        /// </summary>
        /// <param name="op">The operator for this instruction.</param>
        /// <param name="targetTable">
        /// A table of target entries that indicate an outer block or loop to which to break.
        /// </param>
        /// <param name="defaultTarget">
        /// The default target: an outer block or loop to which to break in the default case.
        /// </param>
        public BrTableInstruction(BrTableOperator op, IEnumerable<uint> targetTable, uint defaultTarget)
        {
            this.opValue = op;
            this.TargetTable = new List<uint>(targetTable);
            this.DefaultTarget = defaultTarget;
        }

        private BrTableOperator opValue;

        /// <summary>
        /// Gets the operator for this instruction.
        /// </summary>
        /// <returns>The instruction's operator.</returns>
        public override Operator Op { get { return opValue; } }

        /// <summary>
        /// Gets a table of target entries that indicate an outer block or loop to which to break.
        /// </summary>
        /// <returns>The target entry table.</returns>
        public List<uint> TargetTable { get; private set; }

        /// <summary>
        /// Gets the default target: an outer block or loop to which to break in the default case.
        /// </summary>
        /// <returns>The default target.</returns>
        public uint DefaultTarget { get; set; }

        /// <summary>
        /// Writes this instruction's immediates (but not its opcode)
        /// to the given WebAssembly file writer.
        /// </summary>
        /// <param name="writer">The writer to write this instruction's immediates to.</param>
        public override void WriteImmediatesTo(BinaryWasmWriter writer)
        {
            writer.WriteVarUInt32((uint)TargetTable.Count);
            foreach (var entry in TargetTable)
            {
                writer.WriteVarUInt32(entry);
            }
            writer.WriteVarUInt32(DefaultTarget);
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
            writer.Write(" default=");
            writer.Write(DefaultTarget);
            var indentedWriter = DumpHelpers.CreateIndentedTextWriter(writer);
            for (int i = 0; i < TargetTable.Count; i++)
            {
                indentedWriter.WriteLine();
                indentedWriter.Write(i);
                indentedWriter.Write(" -> ");
                indentedWriter.Write(TargetTable[i]);
            }
            writer.WriteLine();
        }
    }
}