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
        /// Reads a binary WebAssembly from the given stream.
        /// </summary>
        /// <param name="Source">The stream from which a WebAssembly file is to be read.</param>
        /// <returns>The WebAssembly file.</returns>
        public static WasmFile ReadBinary(Stream Source)
        {
            WasmFile result;
            using (var reader = new BinaryReader(Source))
            {
                // Create a WebAssembly reader and read the file.
                var wasmReader = new BinaryWasmReader(reader);
                result = wasmReader.ReadFile();
            }
            return result;
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