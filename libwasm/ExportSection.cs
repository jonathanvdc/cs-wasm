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
        public ExportSection()
        {
            this.Exports = new List<ExportedValue>();
            this.ExtraPayload = new byte[0];
        }

        public ExportSection(IEnumerable<ExportedValue> Exports)
            : this(Exports, new byte[0])
        {
        }

        public ExportSection(IEnumerable<ExportedValue> Exports, byte[] ExtraPayload)
        {
            this.Exports = new List<ExportedValue>(Exports);
            this.ExtraPayload = ExtraPayload;
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
        public byte[] ExtraPayload { get; private set; }

        /// <inheritdoc/>
        public override void WritePayloadTo(BinaryWasmWriter Writer)
        {
            Writer.WriteVarUInt32((uint)Exports.Count);
            foreach (var export in Exports)
            {
                export.WriteTo(Writer);
            }
            Writer.Writer.Write(ExtraPayload);
        }


        /// <inheritdoc/>
        public override void Dump(TextWriter Writer)
        {
            Writer.Write(Name.ToString());
            Writer.Write("; number of entries: ");
            Writer.Write(Exports.Count);
            Writer.WriteLine();
            for (int i = 0; i < Exports.Count; i++)
            {
                Writer.Write("#");
                Writer.Write(i);
                Writer.Write(" -> ");
                Exports[i].Dump(Writer);
                Writer.WriteLine();
            }
            if (ExtraPayload.Length > 0)
            {
                Writer.Write("Extra payload size: ");
                Writer.Write(ExtraPayload.Length);
                Writer.WriteLine();
                DumpHelpers.DumpBytes(ExtraPayload, Writer);
                Writer.WriteLine();
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
        /// <param name="Name">The name of the exported value.</param>
        /// <param name="Kind">The kind of value that is exported.</param>
        /// <param name="Index">The index into the index space for the value's kind.</param>
        public ExportedValue(string Name, ExternalKind Kind, uint Index)
        {
            this.Name = Name;
            this.Kind = Kind;
            this.Index = Index;
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
        /// <param name="Writer">The WebAssembly file writer.</param>
        public void WriteTo(BinaryWasmWriter Writer)
        {
            Writer.WriteString(Name);
            Writer.Writer.Write((byte)Kind);
            Writer.WriteVarUInt32(Index);
        }

        public void Dump(TextWriter Writer)
        {
            Writer.Write("\"");
            Writer.Write(Name);
            Writer.Write("\", ");
            Writer.Write(((object)Kind).ToString().ToLower());
            Writer.Write(" #");
            Writer.Write(Index);
        }
    }
}