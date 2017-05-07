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
        /// <param name="Tables">The list of table descriptions in this type section.</param>
        public TableSection(IEnumerable<TableType> Tables)
            : this(Tables, new byte[0])
        {
        }

        /// <summary>
        /// Creates a type section from the given list of table descriptions and an additional payload.
        /// </summary>
        /// <param name="Tables">The list of table descriptions in this type section.</param>
        /// <param name="ExtraPayload">The additional payload for this section, as an array of bytes.</param>
        public TableSection(IEnumerable<TableType> Tables, byte[] ExtraPayload)
        {
            this.Tables = new List<TableType>(Tables);
            this.ExtraPayload = ExtraPayload;
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
        public byte[] ExtraPayload { get; private set; }

        /// <inheritdoc/>
        public override SectionName Name => new SectionName(SectionCode.Table);

        /// <inheritdoc/>
        public override void WritePayloadTo(BinaryWasmWriter Writer)
        {
            Writer.WriteVarUInt32((uint)Tables.Count);
            foreach (var type in Tables)
                type.WriteTo(Writer);

            Writer.Writer.Write(ExtraPayload);
        }

        /// <inheritdoc/>
        public override void Dump(TextWriter Writer)
        {
            Writer.Write(Name.ToString());
            Writer.Write("; number of entries: ");
            Writer.Write(Tables.Count);
            Writer.WriteLine();
            for (int i = 0; i < Tables.Count; i++)
            {
                Writer.Write("#");
                Writer.Write(i);
                Writer.Write(" -> ");
                Tables[i].Dump(Writer);
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

        /// <summary>
        /// Reads a table section's payload from the given binary WebAssembly reader.
        /// </summary>
        /// <param name="Header">The type section's header.</param>
        /// <param name="Reader">A reader for a binary WebAssembly file.</param>
        /// <returns>A parsed type section.</returns>
        public static TableSection ReadSectionPayload(SectionHeader Header, BinaryWasmReader Reader)
        {
            long initPos = Reader.Position;
            uint typeCount = Reader.ReadVarUInt32();
            var tables = new List<TableType>((int)typeCount);
            for (uint i = 0; i < typeCount; i++)
            {
                tables.Add(TableType.ReadFrom(Reader));
            }
            var extraBytes = Reader.ReadRemainingPayload(initPos, Header);
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
        /// <param name="ElementType">The table's element type.</param>
        /// <param name="Limits">The table's limits.</param>
        public TableType(WasmType ElementType, ResizableLimits Limits)
        {
            this.ElementType = ElementType;
            this.Limits = Limits;
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
        /// Writes this function type to the given binary WebAssembly file.
        /// </summary>
        /// <param name="Writer">The writer for a binary WebAssembly file.</param>
        public void WriteTo(BinaryWasmWriter Writer)
        {
            Writer.WriteWasmType(ElementType);
            Limits.WriteTo(Writer);
        }

        /// <summary>
        /// Writes a textual representation of this exported value to the given writer.
        /// </summary>
        /// <param name="Writer">The writer to which text is written.</param>
        public void Dump(TextWriter Writer)
        {
            Writer.Write("(elem_type: ");
            DumpHelpers.DumpWasmType(ElementType, Writer);
            Writer.Write(", limits: ");
            Limits.Dump(Writer);
            Writer.Write(")");
        }

        /// <summary>
        /// Reads a single table description from the given reader.
        /// </summary>
        /// <returns>The table description.</returns>
        public static TableType ReadFrom(BinaryWasmReader Reader)
        {
            var elemType = (WasmType)Reader.ReadWasmType();
            var limits = Reader.ReadResizableLimits();
            return new TableType(elemType, limits);
        }
    }
}