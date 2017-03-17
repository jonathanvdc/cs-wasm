using System;
using System.IO;

namespace Wasm.Binary
{
    /// <summary>
    /// A reader that reads the binary WebAssembly format.
    /// </summary>
    public sealed class BinaryWasmReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Wasm.Binary.BinaryWasmReader"/> class.
        /// </summary>
        /// <param name="InputStream">The binary reader for a WebAssembly file.</param>
        public BinaryWasmReader(BinaryReader Reader)
        {
            this.Reader = Reader;
        }

        /// <summary>
        /// The binary reader for a WebAssembly file.
        /// </summary>
        private BinaryReader Reader;

        /// <summary>
        /// Parses a version header.
        /// </summary>
        /// <returns>The parsed version header.</returns>
        public VersionHeader ParseVersionHeader()
        {
            return new VersionHeader(Reader.ReadUInt32(), Reader.ReadUInt32());
        }
    }
}

