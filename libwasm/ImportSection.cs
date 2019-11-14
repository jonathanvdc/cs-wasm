using System.Collections.Generic;
using System.IO;
using Wasm.Binary;

namespace Wasm
{
    /// <summary>
    /// A type of section that imports values.
    /// </summary>
    public sealed class ImportSection : Section
    {
        /// <summary>
        /// Creates an empty import section.
        /// </summary>
        public ImportSection()
        {
            this.Imports = new List<ImportedValue>();
            this.ExtraPayload = new byte[0];
        }

        /// <summary>
        /// Creates an import section from a sequence of imports.
        /// </summary>
        /// <param name="imports">A sequence of imports to put in the import section.</param>
        public ImportSection(IEnumerable<ImportedValue> imports)
            : this(imports, new byte[0])
        {
        }

        /// <summary>
        /// Creates an import section from a sequence of imports and a trailing payload.
        /// </summary>
        /// <param name="imports">A sequence of imports to put in the import section.</param>
        /// <param name="extraPayload">
        /// A sequence of bytes that have no intrinsic meaning; they are part
        /// of the import section but are placed after the import section's actual contents.
        /// </param>
        public ImportSection(IEnumerable<ImportedValue> imports, byte[] extraPayload)
        {
            this.Imports = new List<ImportedValue>(imports);
            this.ExtraPayload = extraPayload;
        }

        /// <inheritdoc/>
        public override SectionName Name => new SectionName(SectionCode.Import);

        /// <summary>
        /// Gets the list of all values that are exported by this section.
        /// </summary>
        /// <returns>A list of all values exported by this section.</returns>
        public List<ImportedValue> Imports { get; private set; }

        /// <summary>
        /// Gets this function section's additional payload.
        /// </summary>
        /// <returns>The additional payload, as an array of bytes.</returns>
        public byte[] ExtraPayload { get; set; }

        /// <inheritdoc/>
        public override void WritePayloadTo(BinaryWasmWriter writer)
        {
            writer.WriteVarUInt32((uint)Imports.Count);
            foreach (var import in Imports)
            {
                import.WriteTo(writer);
            }
            writer.Writer.Write(ExtraPayload);
        }

        /// <summary>
        /// Reads the import section with the given header.
        /// </summary>
        /// <param name="header">The section header.</param>
        /// <param name="reader">A reader for a binary WebAssembly file.</param>
        /// <returns>The parsed section.</returns>
        public static ImportSection ReadSectionPayload(
            SectionHeader header, BinaryWasmReader reader)
        {
            long startPos = reader.Position;
            // Read the imported values.
            uint count = reader.ReadVarUInt32();
            var importedVals = new List<ImportedValue>();
            for (uint i = 0; i < count; i++)
            {
                importedVals.Add(ImportedValue.ReadFrom(reader));
            }

            // Skip any remaining bytes.
            var extraPayload = reader.ReadRemainingPayload(startPos, header);
            return new ImportSection(importedVals, extraPayload);
        }

        /// <inheritdoc/>
        public override void Dump(TextWriter writer)
        {
            writer.Write(Name.ToString());
            writer.Write("; number of entries: ");
            writer.Write(Imports.Count);
            writer.WriteLine();
            for (int i = 0; i < Imports.Count; i++)
            {
                writer.Write("#");
                writer.Write(i);
                writer.Write(" -> ");
                Imports[i].Dump(writer);
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
    /// An entry in an import section.
    /// </summary>
    public abstract class ImportedValue
    {
        /// <summary>
        /// Creates an import value from the given pair of names.
        /// </summary>
        /// <param name="moduleName">The name of the module from which a value is imported.</param>
        /// <param name="fieldName">The name of the value that is imported.</param>
        public ImportedValue(string moduleName, string fieldName)
        {
            this.ModuleName = moduleName;
            this.FieldName = fieldName;
        }

        /// <summary>
        /// Gets or sets the name of the module from which a value is imported.
        /// </summary>
        /// <returns>The name of the module from which a value is imported.</returns>
        public string ModuleName { get; set; }

        /// <summary>
        /// Gets or sets the name of the value that is imported.
        /// </summary>
        /// <returns>The name of the value that is imported.</returns>
        public string FieldName { get; set; }

        /// <summary>
        /// Gets the kind of value that is exported.
        /// </summary>
        /// <returns>The kind of value that is exported.</returns>
        public abstract ExternalKind Kind { get; }

        /// <summary>
        /// Writes the contents of this imported value to the given binary WebAssembly writer.
        /// </summary>
        /// <param name="writer">A WebAssembly writer.</param>
        protected abstract void WriteContentsTo(BinaryWasmWriter writer);

        /// <summary>
        /// Dumps the contents of this imported value to the given text writer.
        /// </summary>
        /// <param name="writer">A text writer.</param>
        protected abstract void DumpContents(TextWriter writer);

        /// <summary>
        /// Writes this exported value to the given WebAssembly file writer.
        /// </summary>
        /// <param name="writer">The WebAssembly file writer.</param>
        public void WriteTo(BinaryWasmWriter writer)
        {
            writer.WriteString(ModuleName);
            writer.WriteString(FieldName);
            writer.Writer.Write((byte)Kind);
            WriteContentsTo(writer);
        }

        /// <summary>
        /// Writes a textual representation of this exported value to the given writer.
        /// </summary>
        /// <param name="writer">The writer to which text is written.</param>
        public void Dump(TextWriter writer)
        {
            writer.Write(
                "from \"{0}\" import {1} \"{2}\": ",
                ModuleName,
                ((object)Kind).ToString().ToLower(),
                FieldName);
            DumpContents(writer);
        }

        /// <summary>
        /// Reads an imported value from the given binary WebAssembly reader.
        /// </summary>
        /// <param name="reader">The WebAssembly reader.</param>
        /// <returns>The imported value that was read.</returns>
        public static ImportedValue ReadFrom(BinaryWasmReader reader)
        {
            string moduleName = reader.ReadString();
            string fieldName = reader.ReadString();
            var kind = (ExternalKind)reader.ReadByte();
            switch (kind)
            {
                case ExternalKind.Function:
                    return new ImportedFunction(moduleName, fieldName, reader.ReadVarUInt32());
                case ExternalKind.Global:
                    return new ImportedGlobal(moduleName, fieldName, GlobalType.ReadFrom(reader));
                case ExternalKind.Memory:
                    return new ImportedMemory(moduleName, fieldName, MemoryType.ReadFrom(reader));
                case ExternalKind.Table:
                    return new ImportedTable(moduleName, fieldName, TableType.ReadFrom(reader));
                default:
                    throw new WasmException("Unknown imported value kind: " + kind);
            }
        }
    }

    /// <summary>
    /// Describes an entry in the import section that imports a function.
    /// </summary>
    public sealed class ImportedFunction : ImportedValue
    {
        /// <summary>
        /// Creates a function import from the given module name, field and function index.
        /// </summary>
        /// <param name="moduleName">The name of the module from which a value is imported.</param>
        /// <param name="fieldName">The name of the value that is imported.</param>
        /// <param name="typeIndex">The type index of the function signature.</param>
        public ImportedFunction(string moduleName, string fieldName, uint typeIndex)
            : base(moduleName, fieldName)
        {
            this.TypeIndex = typeIndex;
        }

        /// <summary>
        /// Gets or sets the type index of the function signature.
        /// </summary>
        /// <returns>The type index of the function signature.</returns>
        public uint TypeIndex { get; set; }

        /// <inheritdoc/>
        public override ExternalKind Kind => ExternalKind.Function;

        /// <inheritdoc/>
        protected override void DumpContents(TextWriter writer)
        {
            writer.Write("type #{0}", TypeIndex);
        }

        /// <inheritdoc/>
        protected override void WriteContentsTo(BinaryWasmWriter writer)
        {
            writer.WriteVarUInt32(TypeIndex);
        }
    }

    /// <summary>
    /// Describes an entry in the import section that imports a table.
    /// </summary>
    public sealed class ImportedTable : ImportedValue
    {
        /// <summary>
        /// Creates a table import from the given module name, field and table type.
        /// </summary>
        /// <param name="moduleName">The name of the module from which a value is imported.</param>
        /// <param name="fieldName">The name of the value that is imported.</param>
        /// <param name="table">A description of the imported table.</param>
        public ImportedTable(string moduleName, string fieldName, TableType table)
            : base(moduleName, fieldName)
        {
            this.Table = table;
        }

        /// <summary>
        /// Gets or sets a description of the table that is imported.
        /// </summary>
        /// <returns>A description of the table that is imported.</returns>
        public TableType Table { get; set; }

        /// <inheritdoc/>
        public override ExternalKind Kind => ExternalKind.Table;

        /// <inheritdoc/>
        protected override void DumpContents(TextWriter writer)
        {
            Table.Dump(writer);
        }

        /// <inheritdoc/>
        protected override void WriteContentsTo(BinaryWasmWriter writer)
        {
            Table.WriteTo(writer);
        }
    }

    /// <summary>
    /// Describes an entry in the import section that imports a linear memory.
    /// </summary>
    public sealed class ImportedMemory : ImportedValue
    {
        /// <summary>
        /// Creates a memory import from the given module name, field and memory type.
        /// </summary>
        /// <param name="moduleName">The name of the module from which a value is imported.</param>
        /// <param name="fieldName">The name of the value that is imported.</param>
        /// <param name="memory">A description of the imported memory.</param>
        public ImportedMemory(string moduleName, string fieldName, MemoryType memory)
            : base(moduleName, fieldName)
        {
            this.Memory = memory;
        }

        /// <summary>
        /// Gets or sets a description of the table that is imported.
        /// </summary>
        /// <returns>A description of the table that is imported.</returns>
        public MemoryType Memory { get; set; }

        /// <inheritdoc/>
        public override ExternalKind Kind => ExternalKind.Memory;

        /// <inheritdoc/>
        protected override void DumpContents(TextWriter writer)
        {
            Memory.Dump(writer);
        }

        /// <inheritdoc/>
        protected override void WriteContentsTo(BinaryWasmWriter writer)
        {
            Memory.WriteTo(writer);
        }
    }

    /// <summary>
    /// Describes an entry in the import section that imports a global variable.
    /// </summary>
    public sealed class ImportedGlobal : ImportedValue
    {
        /// <summary>
        /// Creates a global import from the given module name, field and global type.
        /// </summary>
        /// <param name="moduleName">The name of the module from which a value is imported.</param>
        /// <param name="fieldName">The name of the value that is imported.</param>
        /// <param name="global">A description of the imported global.</param>
        public ImportedGlobal(string moduleName, string fieldName, GlobalType global)
            : base(moduleName, fieldName)
        {
            this.Global = global;
        }

        /// <summary>
        /// Gets or sets a description of the global variable that is imported.
        /// </summary>
        /// <returns>A description of the global variable that is imported.</returns>
        public GlobalType Global { get; set; }

        /// <inheritdoc/>
        public override ExternalKind Kind => ExternalKind.Global;

        /// <inheritdoc/>
        protected override void DumpContents(TextWriter writer)
        {
            Global.Dump(writer);
        }

        /// <inheritdoc/>
        protected override void WriteContentsTo(BinaryWasmWriter writer)
        {
            Global.WriteTo(writer);
        }
    }
}
