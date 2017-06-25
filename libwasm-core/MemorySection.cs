using System;
using System.Collections.Generic;
using System.IO;
using Wasm.Binary;

namespace Wasm
{
    /// <summary>
    /// A type of section that defines zero or more memories.
    /// </summary>
    public sealed class MemorySection : Section
    {
        /// <summary>
        /// Creates an empty memory section.
        /// </summary>
        public MemorySection()
        {
            this.Memories = new List<MemoryType>();
            this.ExtraPayload = new byte[0];
        }

        /// <summary>
        /// Creates a memory section from the given sequence of memory specifications.
        /// </summary>
        public MemorySection(IEnumerable<MemoryType> Memories)
            : this(Memories, new byte[0])
        {
        }

        /// <summary>
        /// Creates a memory section from the given sequence of memory specifications
        /// and a trailing byte array.
        /// </summary>
        public MemorySection(IEnumerable<MemoryType> Memories, byte[] ExtraPayload)
        {
            this.Memories = new List<MemoryType>(Memories);
            this.ExtraPayload = ExtraPayload;
        }

        /// <inheritdoc/>
        public override SectionName Name => new SectionName(SectionCode.Memory);

        /// <summary>
        /// Gets a list that contains the limits of all memories defined by this section.
        /// </summary>
        /// <returns>The section's list of memory limits.</returns>
        public List<MemoryType> Memories { get; private set; }

        /// <summary>
        /// Gets this memory section's additional payload.
        /// </summary>
        /// <returns>The additional payload, as an array of bytes.</returns>
        public byte[] ExtraPayload { get; set; }

        /// <inheritdoc/>
        public override void WritePayloadTo(BinaryWasmWriter Writer)
        {
            Writer.WriteVarUInt32((uint)Memories.Count);
            foreach (var limits in Memories)
            {
                limits.WriteTo(Writer);
            }
            Writer.Writer.Write(ExtraPayload);
        }


        /// <inheritdoc/>
        public override void Dump(TextWriter Writer)
        {
            Writer.Write(Name.ToString());
            Writer.Write("; number of entries: ");
            Writer.Write(Memories.Count);
            Writer.WriteLine();
            for (int i = 0; i < Memories.Count; i++)
            {
                Writer.Write("#");
                Writer.Write(i);
                Writer.Write(" -> ");
                Memories[i].Dump(Writer);
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

        /// <summary>
        /// Reads the memory section with the given header.
        /// </summary>
        /// <param name="Header">The section header.</param>
        /// <param name="Reader">The WebAssembly file reader.</param>
        /// <returns>The parsed section.</returns>
        public static MemorySection ReadSectionPayload(SectionHeader Header, BinaryWasmReader Reader)
        {
            long startPos = Reader.Position;
            // Read the resizable limits.
            uint count = Reader.ReadVarUInt32();
            var limits = new List<MemoryType>();
            for (uint i = 0; i < count; i++)
            {
                limits.Add(MemoryType.ReadFrom(Reader));
            }

            // Skip any remaining bytes.
            var extraPayload = Reader.ReadRemainingPayload(startPos, Header);
            return new MemorySection(limits, extraPayload);
        }
    }

    /// <summary>
    /// Describes a linear memory.
    /// </summary>
    public sealed class MemoryType
    {
        /// <summary>
        /// Creates a new linear memory description from the given limits.
        /// </summary>
        /// <param name="Limits">The linear memory's limits.</param>
        public MemoryType(ResizableLimits Limits)
        {
            this.Limits = Limits;
        }

        /// <summary>
        /// Gets this memory's limits.
        /// </summary>
        /// <returns>This memory's limits.</returns>
        public ResizableLimits Limits { get; set; }

        /// <summary>
        /// Gets the size of a single page, in bytes.
        /// </summary>
        public const uint PageSize = 64 * 1024;

        /// <summary>
        /// Writes this memory description to the given binary WebAssembly file.
        /// </summary>
        /// <param name="Writer">The writer for a binary WebAssembly file.</param>
        public void WriteTo(BinaryWasmWriter Writer)
        {
            Limits.WriteTo(Writer);
        }

        /// <summary>
        /// Writes a textual representation of this memory description to the given writer.
        /// </summary>
        /// <param name="Writer">The writer to which text is written.</param>
        public void Dump(TextWriter Writer)
        {
            Limits.Dump(Writer);
        }

        /// <summary>
        /// Reads a single memory description from the given reader.
        /// </summary>
        /// <returns>The memory description.</returns>
        public static MemoryType ReadFrom(BinaryWasmReader Reader)
        {
            return new MemoryType(Reader.ReadResizableLimits());
        }
    }
}