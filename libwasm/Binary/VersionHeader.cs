using System;

namespace Wasm.Binary
{
    /// <summary>
    /// The header of a WebAssembly binary file, which specifies the magic number and
    /// file format version.
    /// </summary>
    public struct VersionHeader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Wasm.Binary.VersionHeader"/> struct.
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
        public uint Version { get;  private set; }

        /// <summary>
        /// Verifies that this version header is a WebAssembly version header for a known
        /// version.
        /// </summary>
        public void Verify()
        {
            if (Magic != WasmMagic)
            {
                throw new BadHeaderException(
                    this, "Invalid magic number. Got '" +
                    Magic + "', expected '" + WasmMagic + "'.");
            }

            if (Version != PreMvpVersion && Version != MvpVersion)
            {
                throw new BadHeaderException(this, "Invalid version number '" + Version + "'.");
            }
        }

        /// <summary>
        /// The WebAssembly magic number 0x6d736100 (i.e., '\0asm').
        /// </summary>
        public static readonly uint WasmMagic = 0x6d736100;

        /// <summary>
        /// The version number from the pre-MVP era.
        /// </summary>
        public static readonly uint PreMvpVersion = 0xd;

        /// <summary>
        /// The MVP version number.
        /// </summary>
        public static readonly uint MvpVersion = 1;
    }
}

