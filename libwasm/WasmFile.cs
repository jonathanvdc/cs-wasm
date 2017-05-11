using System.Collections.Generic;
using System.Linq;

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
    }
}