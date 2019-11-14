using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wasm.Binary;

namespace Wasm
{
    /// <summary>
    /// Represents a WebAssembly file.
    /// </summary>
    public sealed class WasmFile
    {
        /// <summary>
        /// Creates an empty WebAssembly file.
        /// </summary>
        public WasmFile()
            : this(VersionHeader.MvpHeader)
        { }

        /// <summary>
        /// Creates an empty WebAssembly file with the given header.
        /// </summary>
        /// <param name="header">The WebAssembly version header.</param>
        public WasmFile(VersionHeader header)
            : this(header, Enumerable.Empty<Section>())
        { }

        /// <summary>
        /// Creates a WebAssembly file from the given list of sections.
        /// </summary>
        /// <param name="header">The WebAssembly version header.</param>
        /// <param name="sections">The list of all sections in the WebAssembly file.</param>
        public WasmFile(VersionHeader header, IEnumerable<Section> sections)
        {
            this.Header = header;
            this.Sections = new List<Section>(sections);
        }

        /// <summary>
        /// Gets the WebAssembly version header for this file.
        /// </summary>
        /// <returns>The WebAssembly version header.</returns>
        public VersionHeader Header { get; set; }

        /// <summary>
        /// Gets a list of all sections in this file.
        /// </summary>
        /// <returns>All sections in this file.</returns>
        public List<Section> Sections { get; private set; }

        /// <summary>
        /// Gets or sets this module's name as defined in the names section.
        /// </summary>
        /// <value>
        /// The module's name if the names section defines a module name entry;
        /// otherwise, <c>null</c>.
        /// </value>
        public string ModuleName
        {
            get
            {
                return ModuleNameEntryOrNull?.ModuleName;
            }
            set
            {
                var entry = ModuleNameEntryOrNull;
                if (entry == null)
                {
                    AddNameEntry(new ModuleNameEntry(value));
                }
                else
                {
                    entry.ModuleName = value;
                }
            }
        }

        private ModuleNameEntry ModuleNameEntryOrNull
        {
            get
            {
                var nameSection = GetFirstSectionOrNull<NameSection>();
                if (nameSection == null)
                {
                    return null;
                }
                else
                {
                    var firstModuleNameEntry = nameSection.Names.OfType<ModuleNameEntry>().FirstOrDefault();
                    if (firstModuleNameEntry == null)
                    {
                        return null;
                    }
                    else
                    {
                        return firstModuleNameEntry;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the index of this module's entry point function, if any.
        /// </summary>
        /// <value>An entry point index.</value>
        public uint? StartFunctionIndex
        {
            get
            {
                var startSection = GetFirstSectionOrNull<StartSection>();
                return startSection?.StartFunctionIndex;
            }
            set
            {
                if (value.HasValue)
                {
                    var startSection = GetFirstSectionOrNull<StartSection>();
                    if (startSection == null)
                    {
                        InsertSection(new StartSection(value.Value));
                    }
                    else
                    {
                        startSection.StartFunctionIndex = value.Value;
                    }
                }
                else
                {
                    Sections.RemoveAll(s => s is StartSection);
                }
            }
        }

        /// <summary>
        /// Gets a list of all sections of the given type.
        /// </summary>
        /// <returns>A list of sections with the given type.</returns>
        public IReadOnlyList<T> GetSections<T>()
            where T : Section
        {
            var results = new List<T>();
            for (int i = 0; i < Sections.Count; i++)
            {
                var sec = Sections[i];
                if (sec is T)
                {
                    results.Add((T)sec);
                }
            }
            return results;
        }

        /// <summary>
        /// Gets a list of all sections with the given section name.
        /// </summary>
        /// <param name="name">The section name to look for.</param>
        /// <returns>A list of sections with the given section name.</returns>
        public IReadOnlyList<Section> GetSections(SectionName name)
        {
            var results = new List<Section>();
            for (int i = 0; i < Sections.Count; i++)
            {
                var sec = Sections[i];
                if (sec.Name == name)
                {
                    results.Add(sec);
                }
            }
            return results;
        }

        /// <summary>
        /// Gets the first section with the given name. If no such section exists,
        /// <c>null</c> is returned.
        /// </summary>
        /// <param name="name">The section name to look for.</param>
        /// <returns>The first section with the given name, if it exists; otherwise, <c>null</c>.</returns>
        public Section GetFirstSectionOrNull(SectionName name)
        {
            for (int i = 0; i < Sections.Count; i++)
            {
                var sec = Sections[i];
                if (sec.Name == name)
                {
                    return sec;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the first section of the given type. If no such section exists,
        /// <c>null</c> is returned.
        /// </summary>
        /// <returns>The first section of the given type, if it exists; otherwise, <c>null</c>.</returns>
        public T GetFirstSectionOrNull<T>()
            where T : Section
        {
            for (int i = 0; i < Sections.Count; i++)
            {
                var sec = Sections[i];
                if (sec is T)
                {
                    return (T)sec;
                }
            }
            return default(T);
        }

        /// <summary>
        /// Writes this WebAssembly file to the given stream using the binary WebAssembly file encoding.
        /// </summary>
        /// <param name="target">The stream to write to.</param>
        public void WriteBinaryTo(Stream target)
        {
            var writer = new BinaryWriter(target);
            var wasmWriter = new BinaryWasmWriter(writer);
            wasmWriter.WriteFile(this);
        }

        /// <summary>
        /// Writes this WebAssembly file to the given stream using the binary WebAssembly file encoding.
        /// </summary>
        /// <param name="path">A path to the file to write to.</param>
        public void WriteBinaryTo(string path)
        {
            using (var fileStream = File.OpenWrite(path))
            {
                WriteBinaryTo(fileStream);
            }
        }

        /// <summary>
        /// Writes a textual representation of this WebAssembly file to the given text writer.
        /// Note that this representation is intended as a human-readable debugging format that may
        /// change at any time, not as a first-class textual WebAssembly module encoding.
        /// </summary>
        /// <param name="writer">The text writer use.</param>
        public void Dump(TextWriter writer)
        {
            writer.Write(
                "WebAssembly module; magic number: {0}, version number: {1}",
                DumpHelpers.FormatHex(Header.Magic),
                Header.Version);

            foreach (var section in Sections)
            {
                writer.WriteLine();
                section.Dump(writer);
            }
        }

        /// <summary>
        /// Reads a binary WebAssembly from the given stream.
        /// </summary>
        /// <param name="source">The stream from which a WebAssembly file is to be read.</param>
        /// <returns>The WebAssembly file.</returns>
        public static WasmFile ReadBinary(Stream source)
        {
            // Create a WebAssembly reader and read the file.
            var reader = new BinaryReader(source);
            var wasmReader = new BinaryWasmReader(reader);
            return wasmReader.ReadFile();
        }

        /// <summary>
        /// Reads a binary WebAssembly from the given stream.
        /// </summary>
        /// <param name="source">The stream from which a WebAssembly file is to be read.</param>
        /// <param name="streamIsEmpty">Tests if the input stream is empty.</param>
        /// <returns>The WebAssembly file.</returns>
        public static WasmFile ReadBinary(Stream source, Func<bool> streamIsEmpty)
        {
            // Create a WebAssembly reader and read the file.
            var reader = new BinaryReader(source);
            var wasmReader = new BinaryWasmReader(reader, streamIsEmpty);
            return wasmReader.ReadFile();
        }

        /// <summary>
        /// Reads a binary WebAssembly from the file at the given path.
        /// </summary>
        /// <param name="path">A path to the file to read.</param>
        /// <returns>The WebAssembly file.</returns>
        public static WasmFile ReadBinary(string path)
        {
            WasmFile result;
            using (var fileStream = File.OpenRead(path))
            {
                result = ReadBinary(fileStream);
            }
            return result;
        }

        /// <summary>
        /// Inserts a new section into the WebAssembly file.
        /// The section is inserted in a way that preserves the ordering
        /// of sections as specified by the WebAssembly binary format.
        /// </summary>
        /// <param name="section">The section to insert.</param>
        /// <returns>The index in the section list at which <paramref name="section"/> is inserted.</returns>
        public int InsertSection(Section section)
        {
            if (!section.Name.IsCustom)
            {
                // The WebAssembly binary format requires that non-custom sections
                // are ordered by their codes.
                for (int i = 0; i < Sections.Count; i++)
                {
                    if (!Sections[i].Name.IsCustom && section.Name.Code < Sections[i].Name.Code)
                    {
                        Sections.Insert(i, section);
                        return i;
                    }
                }
            }
            Sections.Add(section);
            return Sections.Count - 1;
        }

        /// <summary>
        /// Adds a name entry to the names section, defining a new names section
        /// if one doesn't exist already.
        /// </summary>
        /// <param name="entry">A name entry to add.</param>
        /// <returns>The index in the name section of the newly added name entry.</returns>
        public uint AddNameEntry(NameEntry entry)
        {
            var names = GetFirstSectionOrNull<NameSection>();
            if (names == null)
            {
                InsertSection(names = new NameSection());
            }
            names.Names.Add(entry);
            return (uint)names.Names.Count - 1;
        }

        /// <summary>
        /// Adds a user-defined memory to this module's memory section, defining
        /// a new memory section if one doesn't exist already.
        /// </summary>
        /// <param name="memory">The memory to add.</param>
        /// <returns>The index in the memory section of the newly added memory.</returns>
        public uint AddMemory(MemoryType memory)
        {
            var memories = GetFirstSectionOrNull<MemorySection>();
            if (memories == null)
            {
                InsertSection(memories = new MemorySection());
            }
            memories.Memories.Add(memory);
            return (uint)memories.Memories.Count - 1;
        }

        /// <summary>
        /// Adds a data segment to this module's data section, defining
        /// a new data section if one doesn't exist already.
        /// </summary>
        /// <param name="segment">The data segment to add.</param>
        /// <returns>The index in the data section of the newly added data segment.</returns>
        public uint AddDataSegment(DataSegment segment)
        {
            var data = GetFirstSectionOrNull<DataSection>();
            if (data == null)
            {
                InsertSection(data = new DataSection());
            }
            data.Segments.Add(segment);
            return (uint)data.Segments.Count - 1;
        }

        /// <summary>
        /// Adds an import to this module's import section, defining
        /// a new import section if one doesn't exist already.
        /// </summary>
        /// <param name="import">The import to add.</param>
        /// <returns>The index in the import section of the newly added import.</returns>
        public uint AddImport(ImportedValue import)
        {
            var imports = GetFirstSectionOrNull<ImportSection>();
            if (imports == null)
            {
                InsertSection(imports = new ImportSection());
            }
            imports.Imports.Add(import);
            return (uint)imports.Imports.Count - 1;
        }

        /// <summary>
        /// Adds an export to this module's export section, defining
        /// a new export section if one doesn't exist already.
        /// </summary>
        /// <param name="export">The export to add.</param>
        /// <returns>The index in the export section of the newly added export.</returns>
        public uint AddExport(ExportedValue export)
        {
            var exports = GetFirstSectionOrNull<ExportSection>();
            if (exports == null)
            {
                InsertSection(exports = new ExportSection());
            }
            exports.Exports.Add(export);
            return (uint)exports.Exports.Count - 1;
        }

        /// <summary>
        /// Adds a function type to this module's type section, defining
        /// a new type section if one doesn't exist already.
        /// </summary>
        /// <param name="type">The type to add.</param>
        /// <returns>The index in the type section of the newly added function type.</returns>
        public uint AddFunctionType(FunctionType type)
        {
            var types = GetFirstSectionOrNull<TypeSection>();
            if (types == null)
            {
                InsertSection(types = new TypeSection());
            }
            types.FunctionTypes.Add(type);
            return (uint)types.FunctionTypes.Count - 1;
        }

        /// <summary>
        /// Adds a table to this module's type section, defining
        /// a new table section if one doesn't exist already.
        /// </summary>
        /// <param name="table">The table to add.</param>
        /// <returns>The index in the table section of the newly added table.</returns>
        public uint AddTable(TableType table)
        {
            var tables = GetFirstSectionOrNull<TableSection>();
            if (tables == null)
            {
                InsertSection(tables = new TableSection());
            }
            tables.Tables.Add(table);
            return (uint)tables.Tables.Count - 1;
        }

        /// <summary>
        /// Adds a element segment to this module's element section, defining
        /// a new element section if one doesn't exist already.
        /// </summary>
        /// <param name="segment">The element segment to add.</param>
        /// <returns>The index in the element section of the newly added element segment.</returns>
        public uint AddElementSegment(ElementSegment segment)
        {
            var elements = GetFirstSectionOrNull<ElementSection>();
            if (elements == null)
            {
                InsertSection(elements = new ElementSection());
            }
            elements.Segments.Add(segment);
            return (uint)elements.Segments.Count - 1;
        }

        /// <summary>
        /// Adds a function definition to this module.
        /// </summary>
        /// <param name="functionTypeIndex">The index in the type section of the function's type.</param>
        /// <param name="functionBody">The body of the function to define.</param>
        /// <returns>The index in the function section of the newly added function definition.</returns>
        public uint AddFunction(uint functionTypeIndex, FunctionBody functionBody)
        {
            var funs = GetFirstSectionOrNull<FunctionSection>();
            if (funs == null)
            {
                InsertSection(funs = new FunctionSection());
            }
            var code = GetFirstSectionOrNull<CodeSection>();
            if (code == null)
            {
                InsertSection(code = new CodeSection());
            }
            funs.FunctionTypes.Add(functionTypeIndex);
            code.Bodies.Add(functionBody);
            return (uint)funs.FunctionTypes.Count - 1;
        }

        /// <summary>
        /// Adds a global variable definition to this module.
        /// </summary>
        /// <param name="globalVariable">A global variable definition to introduce.</param>
        /// <returns>The index in the global section of the newly added global variable definition.</returns>
        public uint AddGlobal(GlobalVariable globalVariable)
        {
            var globals = GetFirstSectionOrNull<GlobalSection>();
            if (globals == null)
            {
                InsertSection(globals = new GlobalSection());
            }
            globals.GlobalVariables.Add(globalVariable);
            return (uint)globals.GlobalVariables.Count - 1;
        }
    }
}
