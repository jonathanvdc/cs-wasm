using System;

namespace Wasm
{
    /// <summary>
    /// The header of a WebAssembly binary file, which specifies the magic number and
    /// file format version.
    /// </summary>
    public struct VersionHeader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionHeader"/> struct.
        /// </summary>
        /// <param name="Magic">The magic number.</param>
        /// <param name="Version">The version number.</param>
        public VersionHeader(uint Magic, uint Version)
        {
            this.Magic = Magic;
            this.Version = Version;
        }

        /// <summary>
        /// Gets the magic number in this version header.
        /// </summary>
        /// <value>The magic number.</value>
        public uint Magic { get; private set; }

        /// <summary>
        /// Gets the version specified by this version header.
        /// </summary>
        /// <value>The version.</value>
        public uint Version { get; private set; }

        /// <summary>
        /// Verifies that this version header is a WebAssembly version header for a known
        /// version.
        /// </summary>
        public void Verify()
        {
            if (Magic != WasmMagic)
            {
                throw new BadHeaderException(
                    this,
                    string.Format(
                        "Invalid magic number. Got '{0}', expected '{1}'.",
                        DumpHelpers.FormatHex(Magic),
                        DumpHelpers.FormatHex(WasmMagic)));
            }

            if (Version != PreMvpVersion && Version != MvpVersion)
            {
                throw new BadHeaderException(this, "Invalid version number '" + Version + "'.");
            }
        }

        /// <summary>
        /// Gets the WebAssembly magic number 0x6d736100 (i.e., '\0asm').
        /// </summary>
        public static uint WasmMagic => 0x6d736100;

        /// <summary>
        /// Gets the version number from the pre-MVP era.
        /// </summary>
        public static uint PreMvpVersion => 0xd;

        /// <summary>
        /// Gets the MVP version number.
        /// </summary>
        public static uint MvpVersion => 1;

        /// <summary>
        /// Gets the MVP version header.
        /// </summary>
        public static VersionHeader MvpHeader => new VersionHeader(WasmMagic, MvpVersion);
    }
}

