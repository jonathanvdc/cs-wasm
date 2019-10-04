using System.Collections.Generic;
using System.IO;
using Wasm.Binary;

namespace Wasm
{
    /// <summary>
    /// Represents a function section.
    /// </summary>
    public sealed class FunctionSection : Section
    {
        /// <summary>
        /// Creates an empty function section.
        /// </summary>
        public FunctionSection()
        {
            this.FunctionTypes = new List<uint>();
            this.ExtraPayload = new byte[0];
        }

        /// <summary>
        /// Creates a function from the given list of function types.
        /// </summary>
        /// <param name="functionTypes">The function section's list of types.</param>
        public FunctionSection(IEnumerable<uint> functionTypes)
            : this(functionTypes, new byte[0])
        {
        }

        /// <summary>
        /// Creates a function from the given list of function types and additional payload.
        /// </summary>
        /// <param name="functionTypes">The function section's list of types.</param>
        /// <param name="extraPayload">The function section's additional payload.</param>
        public FunctionSection(IEnumerable<uint> functionTypes, byte[] extraPayload)
        {
            this.FunctionTypes = new List<uint>(functionTypes);
            this.ExtraPayload = extraPayload;
        }

        /// <inheritdoc/>
        public override SectionName Name => new SectionName(SectionCode.Function);

        /// <summary>
        /// Gets this function section's function types, which are entries in the type
        /// section.
        /// </summary>
        /// <returns>A list of indices that refer to entries in the type section.</returns>
        public List<uint> FunctionTypes { get; private set; }

        /// <summary>
        /// This function section's additional payload.
        /// </summary>
        /// <returns>The additional payload, as an array of bytes.</returns>
        public byte[] ExtraPayload { get; set; }

        /// <summary>
        /// Writes this WebAssembly section's payload to the given binary WebAssembly writer.
        /// </summary>
        /// <param name="writer">The writer to which the payload is written.</param>
        public override void WritePayloadTo(BinaryWasmWriter writer)
        {
            writer.WriteVarUInt32((uint)FunctionTypes.Count);
            foreach (var index in FunctionTypes)
            {
                writer.WriteVarUInt32(index);
            }
            writer.Writer.Write(ExtraPayload);
        }

        /// <summary>
        /// Reads the function section with the given header.
        /// </summary>
        /// <param name="header">The section header.</param>
        /// <param name="reader">The WebAssembly file reader.</param>
        /// <returns>The parsed section.</returns>
        public static FunctionSection ReadSectionPayload(SectionHeader header, BinaryWasmReader reader)
        {
            long startPos = reader.Position;
            // Read the function indices.
            uint count = reader.ReadVarUInt32();
            var funcTypes = new List<uint>();
            for (uint i = 0; i < count; i++)
            {
                funcTypes.Add(reader.ReadVarUInt32());
            }

            // Skip any remaining bytes.
            var extraPayload = reader.ReadRemainingPayload(startPos, header);
            return new FunctionSection(funcTypes, extraPayload);
        }

        /// <inheritdoc/>
        public override void Dump(TextWriter writer)
        {
            writer.Write(Name.ToString());
            writer.Write("; number of entries: ");
            writer.Write(FunctionTypes.Count);
            writer.WriteLine();
            for (int i = 0; i < FunctionTypes.Count; i++)
            {
                writer.Write("#");
                writer.Write(i);
                writer.Write(" -> type #");
                writer.Write(FunctionTypes[i]);
                writer.WriteLine();
            }
            if (ExtraPayload.Length > 0)
            {
                writer.Write("Extra payload size: ");
                writer.Write(ExtraPayload.Length);
                writer.WriteLine();
                DumpHelpers.DumpBytes(ExtraPayload, writer);
                writer.WriteLine();
            }
        }
    }
}
