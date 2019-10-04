using System.IO;
using Wasm.Binary;

namespace Wasm
{
    /// <summary>
    /// Represents a custom section.
    /// </summary>
    public sealed class CustomSection : Section
    {
        /// <summary>
        /// Creates a custom section from the given section name and payload.
        /// </summary>
        /// <param name="CustomName">The custom section's name.</param>
        /// <param name="Payload">The custom section's payload.</param>
        public CustomSection(string CustomName, byte[] Payload)
        {
            this.CustomName = CustomName;
            this.Payload = Payload;
        }

        /// <summary>
        /// Gets this custom section's custom name.
        /// </summary>
        /// <returns>The custom name of the custom section.</returns>
        public string CustomName { get; private set; }

        /// <inheritdoc/>
        public override SectionName Name => new SectionName(CustomName);

        /// <summary>
        /// Gets this custom section's payload, as an array of bytes.
        /// </summary>
        /// <returns>A byte array that defines the custom section's payload.</returns>
        public byte[] Payload { get; private set; }

        /// <summary>
        /// Writes this WebAssembly section's payload to the given binary writer.
        /// </summary>
        /// <param name="Writer">The writer to which the payload is written.</param>
        public override void WritePayloadTo(BinaryWasmWriter Writer)
        {
            Writer.Writer.Write(Payload);
        }
    }
}