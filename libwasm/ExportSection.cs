using System;
using System.Collections.Generic;
using System.IO;
using Wasm.Binary;

namespace Wasm
{
    /// <summary>
    /// A type of section that exports values.
    /// </summary>
    public sealed class ExportSection : Section
    {
        /// <summary>
        /// Creates an empty export section.
        /// </summary>
        public ExportSection()
        {
            this.Exports = new List<ExportedValue>();
            this.ExtraPayload = new byte[0];
        }

        /// <summary>
        /// Creates an export section from a sequence of exports.
        /// </summary>
        /// <param name="exports">The exports to put in the export section.</param>
        public ExportSection(IEnumerable<ExportedValue> exports)
            : this(exports, new byte[0])
        {
        }

        /// <summary>
        /// Creates an export section from a sequence of exports and a trailing payload.
        /// </summary>
        /// <param name="exports">The exports to put in the export section.</param>
        /// <param name="extraPayload">
        /// A sequence of bytes that have no intrinsic meaning; they are part
        /// of the element section but are placed after the element section's actual contents.
        /// </param>
        public ExportSection(IEnumerable<ExportedValue> exports, byte[] extraPayload)
        {
            this.Exports = new List<ExportedValue>(exports);
            this.ExtraPayload = extraPayload;
        }

        /// <inheritdoc/>
        public override SectionName Name => new SectionName(SectionCode.Export);

        /// <summary>
        /// Gets the list of all values that are exported by this section.
        /// </summary>
        /// <returns>A list of all values exported by this section.</returns>
        public List<ExportedValue> Exports { get; private set; }

        /// <summary>
        /// Gets this function section's additional payload.
        /// </summary>
        /// <returns>The additional payload, as an array of bytes.</returns>
        public byte[] ExtraPayload { get; set; }

        /// <inheritdoc/>
        public override void WritePayloadTo(BinaryWasmWriter writer)
        {
            writer.WriteVarUInt32((uint)Exports.Count);
            foreach (var export in Exports)
            {
                export.WriteTo(writer);
            }
            writer.Writer.Write(ExtraPayload);
        }

        /// <summary>
        /// Reads the export section with the given header.
        /// </summary>
        /// <param name="header">The section header.</param>
        /// <param name="reader">A reader for a binary WebAssembly file.</param>
        /// <returns>The parsed section.</returns>
        public static ExportSection ReadSectionPayload(
            SectionHeader header, BinaryWasmReader reader)
        {
            long startPos = reader.Position;
            // Read the function indices.
            uint count = reader.ReadVarUInt32();
            var exportedVals = new List<ExportedValue>();
            for (uint i = 0; i < count; i++)
            {
                exportedVals.Add(
                    new ExportedValue(
                        reader.ReadString(),
                        (ExternalKind)reader.ReadByte(),
                        reader.ReadVarUInt32()));
            }

            // Skip any remaining bytes.
            var extraPayload = reader.ReadRemainingPayload(startPos, header);
            return new ExportSection(exportedVals, extraPayload);
        }

        /// <inheritdoc/>
        public override void Dump(TextWriter writer)
        {
            writer.Write(Name.ToString());
            writer.Write("; number of entries: ");
            writer.Write(Exports.Count);
            writer.WriteLine();
            for (int i = 0; i < Exports.Count; i++)
            {
                writer.Write("#");
                writer.Write(i);
                writer.Write(" -> ");
                Exports[i].Dump(writer);
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

    /// <summary>
    /// An entry in an export section.
    /// </summary>
    public struct ExportedValue
    {
        /// <summary>
        /// Creates an exported value from the given name, kind and index.
        /// </summary>
        /// <param name="name">The name of the exported value.</param>
        /// <param name="kind">The kind of value that is exported.</param>
        /// <param name="index">The index into the index space for the value's kind.</param>
        public ExportedValue(string name, ExternalKind kind, uint index)
        {
            this.Name = name;
            this.Kind = kind;
            this.Index = index;
        }

        /// <summary>
        /// Gets the name of the exported value.
        /// </summary>
        /// <returns>The name of the exported value.</returns>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the kind of value that is exported.
        /// </summary>
        /// <returns>The kind of value that is exported.</returns>
        public ExternalKind Kind { get; private set; }

        /// <summary>
        /// Gets the index into the index space for this value's kind.
        /// </summary>
        /// <returns>The index into the appropriate index space.</returns>
        public uint Index { get; private set; }

        /// <summary>
        /// Writes this exported value to the given WebAssembly file writer.
        /// </summary>
        /// <param name="writer">The WebAssembly file writer.</param>
        public void WriteTo(BinaryWasmWriter writer)
        {
            writer.WriteString(Name);
            writer.Writer.Write((byte)Kind);
            writer.WriteVarUInt32(Index);
        }

        /// <summary>
        /// Writes a textual representation of this exported value to the given writer.
        /// </summary>
        /// <param name="writer">The writer to which text is written.</param>
        public void Dump(TextWriter writer)
        {
            writer.Write("\"");
            writer.Write(Name);
            writer.Write("\", ");
            writer.Write(((object)Kind).ToString().ToLower());
            writer.Write(" #");
            writer.Write(Index);
        }
    }
}
