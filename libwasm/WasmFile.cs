using System.Collections.Generic;

namespace Wasm
{
    /// <summary>
    /// Represents a WebAssembly file.
    /// </summary>
    public sealed class WasmFile
    {
        /// <summary>
        /// Creates a WebAssembly file from the given list of sections.
        /// </summary>
        /// <param name="Sections">The list of all sections in the WebAssembly file.</param>
        public WasmFile(IReadOnlyList<Section> Sections)
        {
            this.Sections = Sections;
        }

        /// <summary>
        /// Gets a list of all sections in this file.
        /// </summary>
        /// <returns>All sections in this file.</returns>
        public IReadOnlyList<Section> Sections { get; private set; }
    }
}