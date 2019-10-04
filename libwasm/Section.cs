using System.IO;
using System.Text;
using Wasm.Binary;

namespace Wasm
{
    /// <summary>
    /// A base class for WebAssembly module sections.
    /// </summary>
    public abstract class Section
    {
        /// <summary>
        /// Creates a new WebAssembly module section.
        /// </summary>
        public Section()
        { }

        /// <summary>
        /// Gets this section's name.
        /// </summary>
        /// <returns>The section name.</returns>
        public abstract SectionName Name { get; }

        /// <summary>
        /// Writes this WebAssembly section's payload to the given binary WebAssembly writer.
        /// </summary>
        /// <param name="writer">The writer to which the payload is written.</param>
        public abstract void WritePayloadTo(BinaryWasmWriter writer);

        /// <summary>
        /// Writes this WebAssembly section's optional custom name and payload to the given
        /// WebAssembly writer.
        /// </summary>
        /// <param name="writer">The writer to which the custom name and payload are written.</param>
        internal void WriteCustomNameAndPayloadTo(BinaryWasmWriter writer)
        {
            if (Name.IsCustom)
            {
                writer.WriteString(Name.CustomName);
            }
            WritePayloadTo(writer);
        }

        /// <summary>
        /// Creates a memory stream and fills it with this WebAssembly section's payload.
        /// </summary>
        /// <returns>The memory stream.</returns>
        public MemoryStream PayloadAsMemoryStream()
        {
            var memStream = new MemoryStream();
            WritePayloadTo(new BinaryWasmWriter(new BinaryWriter(memStream)));
            memStream.Seek(0, SeekOrigin.Begin);
            return memStream;
        }

        /// <summary>
        /// Writes a string representation of this section to the given text writer.
        /// </summary>
        /// <param name="writer">
        /// The writer to which a representation of this section is written.
        /// </param>
        public virtual void Dump(TextWriter writer)
        {
            DumpNameAndPayload(writer);
        }

        /// <summary>
        /// Writes a string representation of this section and its payload to the given text writer.
        /// </summary>
        /// <param name="writer">
        /// The writer to which a representation of this section is written.
        /// </param>
        /// <remarks>This is the default 'Dump' implementation.</remarks>
        public void DumpNameAndPayload(TextWriter writer)
        {
            writer.Write(Name.ToString());
            writer.Write("; payload length: ");
            using (var memStream = PayloadAsMemoryStream())
            {
                writer.Write(memStream.Length);
                writer.WriteLine();
                DumpHelpers.DumpStream(memStream, writer);
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Creates a string representation of this section.
        /// </summary>
        /// <returns>The section's string representation.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            Dump(new StringWriter(builder));
            return builder.ToString();
        }
    }
}
