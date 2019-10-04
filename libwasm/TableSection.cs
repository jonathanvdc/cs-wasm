using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wasm.Binary;

namespace Wasm
{
    /// <summary>
    /// Represents a table section in a WebAssembly file.
    /// </summary>
    public sealed class TableSection : Section
    {
        /// <summary>
        /// Creates an empty table section.
        /// </summary>
        public TableSection()
            : this(Enumerable.Empty<TableType>())
        {
        }

        /// <summary>
        /// Creates a table section from the given list of table descriptions.
        /// </summary>
        /// <param name="tables">The list of table descriptions in this type section.</param>
        public TableSection(IEnumerable<TableType> tables)
            : this(tables, new byte[0])
        {
        }

        /// <summary>
        /// Creates a type section from the given list of table descriptions and an additional payload.
        /// </summary>
        /// <param name="tables">The list of table descriptions in this type section.</param>
        /// <param name="extraPayload">The additional payload for this section, as an array of bytes.</param>
        public TableSection(IEnumerable<TableType> tables, byte[] extraPayload)
        {
            this.Tables = new List<TableType>(tables);
            this.ExtraPayload = extraPayload;
        }

        /// <summary>
        /// Gets this table section's list of tables.
        /// </summary>
        /// <returns>The list of tables in this table section.</returns>
        public List<TableType> Tables { get; private set; }

        /// <summary>
        /// This type section's additional payload.
        /// </summary>
        /// <returns>The additional payload, as an array of bytes.</returns>
        public byte[] ExtraPayload { get; set; }

        /// <inheritdoc/>
        public override SectionName Name => new SectionName(SectionCode.Table);

        /// <inheritdoc/>
        public override void WritePayloadTo(BinaryWasmWriter writer)
        {
            writer.WriteVarUInt32((uint)Tables.Count);
            foreach (var type in Tables)
                type.WriteTo(writer);

            writer.Writer.Write(ExtraPayload);
        }

        /// <inheritdoc/>
        public override void Dump(TextWriter writer)
        {
            writer.Write(Name.ToString());
            writer.Write("; number of entries: ");
            writer.Write(Tables.Count);
            writer.WriteLine();
            for (int i = 0; i < Tables.Count; i++)
            {
                writer.Write("#");
                writer.Write(i);
                writer.Write(" -> ");
                Tables[i].Dump(writer);
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

        /// <summary>
        /// Reads a table section's payload from the given binary WebAssembly reader.
        /// </summary>
        /// <param name="header">The type section's header.</param>
        /// <param name="reader">A reader for a binary WebAssembly file.</param>
        /// <returns>A parsed type section.</returns>
        public static TableSection ReadSectionPayload(SectionHeader header, BinaryWasmReader reader)
        {
            long initPos = reader.Position;
            uint typeCount = reader.ReadVarUInt32();
            var tables = new List<TableType>((int)typeCount);
            for (uint i = 0; i < typeCount; i++)
            {
                tables.Add(TableType.ReadFrom(reader));
            }
            var extraBytes = reader.ReadRemainingPayload(initPos, header);
            return new TableSection(tables, extraBytes);
        }
    }

    /// <summary>
    /// Describes a table in a table section.
    /// </summary>
    public struct TableType
    {
        /// <summary>
        /// Creates a table description from the given element type and limits.
        /// </summary>
        /// <param name="elementType">The table's element type.</param>
        /// <param name="limits">The table's limits.</param>
        public TableType(WasmType elementType, ResizableLimits limits)
        {
            this.ElementType = elementType;
            this.Limits = limits;
        }

        /// <summary>
        /// Gets the type of element in the table.
        /// </summary>
        /// <returns>The type of element in the table.</returns>
        public WasmType ElementType { get; private set; }

        /// <summary>
        /// Gets the table's limits.
        /// </summary>
        /// <returns>The table's limits.</returns>
        public ResizableLimits Limits { get; private set; }

        /// <summary>
        /// Writes this table description to the given binary WebAssembly file.
        /// </summary>
        /// <param name="writer">The writer for a binary WebAssembly file.</param>
        public void WriteTo(BinaryWasmWriter writer)
        {
            writer.WriteWasmType(ElementType);
            Limits.WriteTo(writer);
        }

        /// <summary>
        /// Writes a textual representation of this table description to the given writer.
        /// </summary>
        /// <param name="writer">The writer to which text is written.</param>
        public void Dump(TextWriter writer)
        {
            writer.Write("(elem_type: ");
            DumpHelpers.DumpWasmType(ElementType, writer);
            writer.Write(", limits: ");
            Limits.Dump(writer);
            writer.Write(")");
        }

        /// <summary>
        /// Reads a single table description from the given reader.
        /// </summary>
        /// <returns>The table description.</returns>
        public static TableType ReadFrom(BinaryWasmReader reader)
        {
            var elemType = (WasmType)reader.ReadWasmType();
            var limits = reader.ReadResizableLimits();
            return new TableType(elemType, limits);
        }
    }
}
