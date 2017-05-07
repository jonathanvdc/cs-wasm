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
        public MemorySection()
        {
            this.MemoryLimits = new List<ResizableLimits>();
            this.ExtraPayload = new byte[0];
        }

        public MemorySection(IEnumerable<ResizableLimits> MemoryLimits)
            : this(MemoryLimits, new byte[0])
        {
        }

        public MemorySection(IEnumerable<ResizableLimits> MemoryLimits, byte[] ExtraPayload)
        {
            this.MemoryLimits = new List<ResizableLimits>(MemoryLimits);
            this.ExtraPayload = ExtraPayload;
        }

        /// <inheritdoc/>
        public override SectionName Name => new SectionName(SectionCode.Export);

        /// <summary>
        /// Gets a list that contains the limits of all memories defined by this section.
        /// </summary>
        /// <returns>The section's list of memory limits.</returns>
        public List<ResizableLimits> MemoryLimits { get; private set; }

        /// <summary>
        /// Gets this memory section's additional payload.
        /// </summary>
        /// <returns>The additional payload, as an array of bytes.</returns>
        public byte[] ExtraPayload { get; private set; }

        /// <inheritdoc/>
        public override void WritePayloadTo(BinaryWasmWriter Writer)
        {
            Writer.WriteVarUInt32((uint)MemoryLimits.Count);
            foreach (var limits in MemoryLimits)
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
            Writer.Write(MemoryLimits.Count);
            Writer.WriteLine();
            for (int i = 0; i < MemoryLimits.Count; i++)
            {
                Writer.Write("#");
                Writer.Write(i);
                Writer.Write(" -> ");
                MemoryLimits[i].Dump(Writer);
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
            var limits = new List<ResizableLimits>();
            for (uint i = 0; i < count; i++)
            {
                limits.Add(Reader.ReadResizableLimits());
            }

            // Skip any remaining bytes.
            var extraPayload = Reader.ReadRemainingPayload(startPos, Header);
            return new MemorySection(limits, extraPayload);
        }
    }
}