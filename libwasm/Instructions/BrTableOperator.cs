using System.Collections.Generic;
using Wasm.Binary;

namespace Wasm.Instructions
{
    /// <summary>
    /// Describes an operator that begins a break table.
    /// </summary>
    public sealed class BrTableOperator : Operator
    {
        /// <summary>
        /// Creates a break table operator.
        /// </summary>
        /// <param name="opCode">The operator's opcode.</param>
        /// <param name="declaringType">A type that defines the operator, if any.</param>
        /// <param name="mnemonic">The operator's mnemonic.</param>
        public BrTableOperator(byte opCode, WasmType declaringType, string mnemonic)
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
            uint tableSize = reader.ReadVarUInt32();
            var tableEntries = new List<uint>((int)tableSize);
            for (uint i = 0; i < tableSize; i++)
            {
                tableEntries.Add(reader.ReadVarUInt32());
            }
            uint defaultEntry = reader.ReadVarUInt32();
            return Create(tableEntries, defaultEntry);
        }

        /// <summary>
        /// Creates a break table instruction from this operator, a table of
        /// break targets and a default target.
        /// </summary>
        /// <param name="targetTable">
        /// A table of target entries that indicate an outer block or loop to which to break.
        /// </param>
        /// <param name="defaultTarget">
        /// The default target: an outer block or loop to which to break in the default case.
        /// </param>
        public BrTableInstruction Create(IEnumerable<uint> targetTable, uint defaultTarget)
        {
            return new BrTableInstruction(this, targetTable, defaultTarget);
        }

        /// <summary>
        /// Casts the given instruction to this operator's instruction type.
        /// </summary>
        /// <param name="value">The instruction to cast.</param>
        /// <returns>The given instruction as this operator's instruction type.</returns>
        public BrTableInstruction CastInstruction(Instruction value)
        {
            return (BrTableInstruction)value;
        }
    }
}
