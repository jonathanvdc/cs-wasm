using System.IO;
using Wasm.Binary;

namespace Wasm
{
    /// <summary>
    /// Represents an unknown section: a non-custom section whose section code was not recognized.
    /// </summary>
    public sealed class UnknownSection : Section
    {
        /// <summary>
        /// Creates an unknown section from the given section name and payload.
        /// </summary>
        /// <param name="code">The unknown section's code.</param>
        /// <param name="payload">The unknown section's payload.</param>
        public UnknownSection(SectionCode code, byte[] payload)
        {
            this.Code = code;
            this.Payload = payload;
        }

        /// <summary>
        /// Gets this unknown section's code.
        /// </summary>
        /// <returns>The code of the unknown section.</returns>
        public SectionCode Code { get; private set; }

        /// <inheritdoc/>
        public override SectionName Name => new SectionName(Code);

        /// <summary>
        /// Gets this unknown section's payload, as an array of bytes.
        /// </summary>
        /// <returns>A byte array that defines the unknown section's payload.</returns>
        public byte[] Payload { get; private set; }
        
        /// <summary>
        /// Writes this WebAssembly section's payload to the given binary WebAssembly writer.
        /// </summary>
        /// <param name="writer">The writer to which the payload is written.</param>
        public override void WritePayloadTo(BinaryWasmWriter writer)
        {
            writer.Writer.Write(Payload);
        }
    }
}
