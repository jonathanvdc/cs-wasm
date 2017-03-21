using System.Collections.Generic;
using System.IO;
using Wasm.Binary;

namespace Wasm
{
    /// <summary>
    /// Represents a function section.
    /// </summary>
    public sealed class FunctionSection : Section
    {
        /// <summary>
        /// Creates an empty function section.
        /// </summary>
        public FunctionSection()
        {
            this.FunctionTypes = new List<uint>();
        }

        /// <summary>
        /// Creates a function from the given list of function types.
        /// </summary>
        /// <param name="FunctionTypes">The function section's list of types.</param>
        public FunctionSection(IEnumerable<uint> FunctionTypes)
            : this(FunctionTypes, new byte[0])
        {
        }

        /// <summary>
        /// Creates a function from the given list of function types and additional payload.
        /// </summary>
        /// <param name="FunctionTypes">The function section's list of types.</param>
        /// <param name="ExtraPayload">The function section's additional payload.</param>
        public FunctionSection(IEnumerable<uint> FunctionTypes, byte[] ExtraPayload)
        {
            this.FunctionTypes = new List<uint>(FunctionTypes);
            this.ExtraPayload = ExtraPayload;
        }

        /// <inheritdoc/>
        public override SectionName Name => new SectionName(SectionCode.Function);

        /// <summary>
        /// Gets this function section's function types, which are entries in the type
        /// section.
        /// </summary>
        /// <returns>A list of indices that refer to entries in the type section.</returns>
        public List<uint> FunctionTypes { get; private set; }

        /// <summary>
        /// This function section's additional payload.
        /// </summary>
        /// <returns>The additional payload, as an array of bytes.</returns>
        public byte[] ExtraPayload { get; private set; }

        /// <summary>
        /// Writes this WebAssembly section's payload to the given binary WebAssembly writer.
        /// </summary>
        /// <param name="Writer">The writer to which the payload is written.</param>
        public override void WritePayloadTo(BinaryWasmWriter Writer)
        {
            Writer.WriteVarUInt32((uint)FunctionTypes.Count);
            foreach (var index in FunctionTypes)
            {
                Writer.WriteVarUInt32(index);
            }
            Writer.Writer.Write(ExtraPayload);
        }

        /// <inheritdoc/>
        public override void Dump(TextWriter Writer)
        {
            Writer.Write(Name.ToString());
            Writer.Write("; number of entries: ");
            Writer.Write(FunctionTypes.Count);
            Writer.WriteLine();
            for (int i = 0; i < FunctionTypes.Count; i++)
            {
                Writer.Write("#");
                Writer.Write(i);
                Writer.Write(" -> type #");
                Writer.Write(FunctionTypes[i]);
                Writer.WriteLine();
            }
            if (ExtraPayload.Length > 0)
            {
                Writer.Write("Extra payload size: ");
                Writer.Write(ExtraPayload.Length);
                Writer.WriteLine();
                DumpHelpers.DumpBytes(ExtraPayload, Writer);
                Writer.WriteLine();
            }
        }
    }
}