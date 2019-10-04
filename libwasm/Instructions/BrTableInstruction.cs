using System.Collections.Generic;
using System.IO;
using System.Text;
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
        /// <param name="Op">The operator for this instruction.</param>
        /// <param name="TargetTable">
        /// A table of target entries that indicate an outer block or loop to which to break.
        /// </param>
        /// <param name="DefaultTarget">
        /// The default target: an outer block or loop to which to break in the default case.
        /// </param>
        public BrTableInstruction(Operator Op, IEnumerable<uint> TargetTable, uint DefaultTarget)
        {
            this.opValue = Op;
            this.TargetTable = new List<uint>(TargetTable);
            this.DefaultTarget = DefaultTarget;
        }

        private Operator opValue;

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
        /// <param name="Writer">The writer to write this instruction's immediates to.</param>
        public override void WriteImmediatesTo(BinaryWasmWriter Writer)
        {
            Writer.WriteVarUInt32((uint)TargetTable.Count);
            foreach (var entry in TargetTable)
            {
                Writer.WriteVarUInt32(entry);
            }
            Writer.WriteVarUInt32(DefaultTarget);
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
            Writer.Write(" default=");
            Writer.Write(DefaultTarget);
            var indentedWriter = DumpHelpers.CreateIndentedTextWriter(Writer);
            for (int i = 0; i < TargetTable.Count; i++)
            {
                indentedWriter.WriteLine();
                indentedWriter.Write(i);
                indentedWriter.Write(" -> ");
                indentedWriter.Write(TargetTable[i]);
            }
            Writer.WriteLine();
        }
    }
}