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
        /// <param name="Header">The WebAssembly version header.</param>
        public WasmFile(VersionHeader Header)
            : this(Header, Enumerable.Empty<Section>())
        { }

        /// <summary>
        /// Creates a WebAssembly file from the given list of sections.
        /// </summary>
        /// <param name="Header">The WebAssembly version header.</param>
        /// <param name="Sections">The list of all sections in the WebAssembly file.</param>
        public WasmFile(VersionHeader Header, IEnumerable<Section> Sections)
        {
            this.Header = Header;
            this.Sections = new List<Section>(Sections);
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
        /// <param name="Name">The section name to look for.</param>
        /// <returns>A list of sections with the given section name.</returns>
        public IReadOnlyList<Section> GetSections(SectionName Name)
        {
            var results = new List<Section>();
            for (int i = 0; i < Sections.Count; i++)
            {
                var sec = Sections[i];
                if (sec.Name == Name)
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
        /// <param name="Name">The section name to look for.</param>
        /// <returns>The first section with the given name, if it exists; otherwise, <c>null</c>.</returns>
        public Section GetFirstSectionOrNull(SectionName Name)
        {
            for (int i = 0; i < Sections.Count; i++)
            {
                var sec = Sections[i];
                if (sec.Name == Name)
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
        /// <param name="Target">The stream to write to.</param>
        public void WriteBinaryTo(Stream Target)
        {
            var writer = new BinaryWriter(Target);
            var wasmWriter = new BinaryWasmWriter(writer);
            wasmWriter.WriteFile(this);
        }

        /// <summary>
        /// Writes this WebAssembly file to the given stream using the binary WebAssembly file encoding.
        /// </summary>
        /// <param name="Path">A path to the file to write to.</param>
        public void WriteBinaryTo(string Path)
        {
            using (var fileStream = File.OpenWrite(Path))
            {
                WriteBinaryTo(fileStream);
            }
        }

        /// <summary>
        /// Writes a textual representation of this WebAssembly file to the given text writer.
        /// Note that this representation is intended as a human-readable debugging format that may
        /// change at any time, not as a first-class textual WebAssembly module encoding.
        /// </summary>
        /// <param name="Writer">The text writer use.</param>
        public void Dump(TextWriter Writer)
        {
            Writer.Write(
                "WebAssembly module; magic number: {0}, version number: {1}",
                DumpHelpers.FormatHex(Header.Magic),
                Header.Version);

            foreach (var section in Sections)
            {
                Writer.WriteLine();
                section.Dump(Writer);
            }
        }

        /// <summary>
        /// Reads a binary WebAssembly from the given stream.
        /// </summary>
        /// <param name="Source">The stream from which a WebAssembly file is to be read.</param>
        /// <returns>The WebAssembly file.</returns>
        public static WasmFile ReadBinary(Stream Source)
        {
            // Create a WebAssembly reader and read the file.
            var reader = new BinaryReader(Source);
            var wasmReader = new BinaryWasmReader(reader);
            return wasmReader.ReadFile();
        }

        /// <summary>
        /// Reads a binary WebAssembly from the given stream.
        /// </summary>
        /// <param name="Source">The stream from which a WebAssembly file is to be read.</param>
        /// <param name="StreamIsEmpty">Tests if the input stream is empty.</param>
        /// <returns>The WebAssembly file.</returns>
        public static WasmFile ReadBinary(Stream Source, Func<bool> StreamIsEmpty)
        {
            // Create a WebAssembly reader and read the file.
            var reader = new BinaryReader(Source);
            var wasmReader = new BinaryWasmReader(reader, StreamIsEmpty);
            return wasmReader.ReadFile();
        }

        /// <summary>
        /// Reads a binary WebAssembly from the file at the given path.
        /// </summary>
        /// <param name="Path">A path to the file to read.</param>
        /// <returns>The WebAssembly file.</returns>
        public static WasmFile ReadBinary(string Path)
        {
            WasmFile result;
            using (var fileStream = File.OpenRead(Path))
            {
                result = ReadBinary(fileStream);
            }
            return result;
        }
    }
}