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
        public MemorySection(IEnumerable<MemoryType> memories)
            : this(memories, new byte[0])
        {
        }

        /// <summary>
        /// Creates a memory section from the given sequence of memory specifications
        /// and a trailing byte array.
        /// </summary>
        public MemorySection(IEnumerable<MemoryType> memories, byte[] extraPayload)
        {
            this.Memories = new List<MemoryType>(memories);
            this.ExtraPayload = extraPayload;
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
        public override void WritePayloadTo(BinaryWasmWriter writer)
        {
            writer.WriteVarUInt32((uint)Memories.Count);
            foreach (var limits in Memories)
            {
                limits.WriteTo(writer);
            }
            writer.Writer.Write(ExtraPayload);
        }


        /// <inheritdoc/>
        public override void Dump(TextWriter writer)
        {
            writer.Write(Name.ToString());
            writer.Write("; number of entries: ");
            writer.Write(Memories.Count);
            writer.WriteLine();
            for (int i = 0; i < Memories.Count; i++)
            {
                writer.Write("#");
                writer.Write(i);
                writer.Write(" -> ");
                Memories[i].Dump(writer);
                writer.WriteLine();
            }
            if (ExtraPayload.Length > 0)
            {
                writer.Write("Extra payload size: ");
                writer.Write(ExtraPayload.Length);
                writer.WriteLine();
                DumpHelpers.DumpBytes(ExtraPayload, writer);
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Reads the memory section with the given header.
        /// </summary>
        /// <param name="header">The section header.</param>
        /// <param name="reader">The WebAssembly file reader.</param>
        /// <returns>The parsed section.</returns>
        public static MemorySection ReadSectionPayload(SectionHeader header, BinaryWasmReader reader)
        {
            long startPos = reader.Position;
            // Read the resizable limits.
            uint count = reader.ReadVarUInt32();
            var limits = new List<MemoryType>();
            for (uint i = 0; i < count; i++)
            {
                limits.Add(MemoryType.ReadFrom(reader));
            }

            // Skip any remaining bytes.
            var extraPayload = reader.ReadRemainingPayload(startPos, header);
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
        /// <param name="limits">The linear memory's limits.</param>
        public MemoryType(ResizableLimits limits)
        {
            this.Limits = limits;
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
        /// <param name="writer">The writer for a binary WebAssembly file.</param>
        public void WriteTo(BinaryWasmWriter writer)
        {
            Limits.WriteTo(writer);
        }

        /// <summary>
        /// Writes a textual representation of this memory description to the given writer.
        /// </summary>
        /// <param name="writer">The writer to which text is written.</param>
        public void Dump(TextWriter writer)
        {
            Limits.Dump(writer);
        }

        /// <summary>
        /// Reads a single memory description from the given reader.
        /// </summary>
        /// <returns>The memory description.</returns>
        public static MemoryType ReadFrom(BinaryWasmReader reader)
        {
            return new MemoryType(reader.ReadResizableLimits());
        }
    }
}
