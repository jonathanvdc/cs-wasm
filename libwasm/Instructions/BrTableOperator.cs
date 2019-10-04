using System.Collections.Generic;
using System.IO;
using System.Text;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes an operator that begins a break table.
    /// </summary>
    public sealed class BrTableOperator : Operator
    {
        public BrTableOperator(byte OpCode, WasmType DeclaringType, string Mnemonic)
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
            uint tableSize = Reader.ReadVarUInt32();
            var tableEntries = new List<uint>((int)tableSize);
            for (uint i = 0; i < tableSize; i++)
            {
                tableEntries.Add(Reader.ReadVarUInt32());
            }
            uint defaultEntry = Reader.ReadVarUInt32();
            return Create(tableEntries, defaultEntry);
        }

        /// <summary>
        /// Creates a break table instruction from this operator, a table of
        /// break targets and a default target.
        /// </summary>
        /// <param name="TargetTable">
        /// A table of target entries that indicate an outer block or loop to which to break.
        /// </param>
        /// <param name="DefaultTarget">
        /// The default target: an outer block or loop to which to break in the default case.
        /// </param>
        public BrTableInstruction Create(IEnumerable<uint> TargetTable, uint DefaultTarget)
        {
            return new BrTableInstruction(this, TargetTable, DefaultTarget);
        }

        /// <summary>
        /// Casts the given instruction to this operator's instruction type.
        /// </summary>
        /// <param name="Value">The instruction to cast.</param>
        /// <returns>The given instruction as this operator's instruction type.</returns>
        public BrTableInstruction CastInstruction(Instruction Value)
        {
            return (BrTableInstruction)Value;
        }
    }
}