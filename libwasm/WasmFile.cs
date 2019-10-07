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
        public void AddNameEntry(NameEntry entry)
        {
            var names = GetFirstSectionOrNull<NameSection>();
            if (names == null)
            {
                InsertSection(names = new NameSection());
            }
            names.Names.Add(entry);
        }

        /// <summary>
        /// Adds a user-defined memory to this module.
        /// </summary>
        /// <param name="memory">The memory to add.</param>
        public void AddMemory(MemoryType memory)
        {
            var memories = GetFirstSectionOrNull<MemorySection>();
            if (memories == null)
            {
                InsertSection(memories = new MemorySection());
            }
            memories.Memories.Add(memory);
        }
    }
}
