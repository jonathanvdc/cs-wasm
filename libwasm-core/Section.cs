using System.IO;
using System.Text;
using Wasm.Binary;

namespace Wasm
{
    /// <summary>
    /// A base class for sections.
    /// </summary>
    public abstract class Section
    {
        public Section()
        {

        }

        /// <summary>
        /// Gets this section's name.
        /// </summary>
        /// <returns>The section name.</returns>
        public abstract SectionName Name { get; }

        /// <summary>
        /// Writes this WebAssembly section's payload to the given binary WebAssembly writer.
        /// </summary>
        /// <param name="Writer">The writer to which the payload is written.</param>
        public abstract void WritePayloadTo(BinaryWasmWriter Writer);

        /// <summary>
        /// Writes this WebAssembly section's optional custom name and payload to the given
        /// WebAssembly writer.
        /// </summary>
        /// <param name="Writer">The writer to which the custom name and payload are written.</param>
        internal void WriteCustomNameAndPayloadTo(BinaryWasmWriter Writer)
        {
            if (Name.IsCustom)
            {
                Writer.WriteString(Name.CustomName);
            }
            WritePayloadTo(Writer);
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
        /// <param name="Writer">
        /// The writer to which a representation of this section is written.
        /// </param>
        public virtual void Dump(TextWriter Writer)
        {
            DumpNameAndPayload(Writer);
        }

        /// <summary>
        /// Writes a string representation of this section and its payload to the given text writer.
        /// </summary>
        /// <param name="Writer">
        /// The writer to which a representation of this section is written.
        /// </param>
        /// <remarks>This is the default 'Dump' implementation.</remarks>
        public void DumpNameAndPayload(TextWriter Writer)
        {
            Writer.Write(Name.ToString());
            Writer.Write("; payload length: ");
            using (var memStream = PayloadAsMemoryStream())
            {
                Writer.Write(memStream.Length);
                Writer.WriteLine();
                DumpHelpers.DumpStream(memStream, Writer);
                Writer.WriteLine();
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