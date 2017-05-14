using System;
using System.Collections.Generic;
using System.IO;
using Wasm.Binary;

namespace Wasm
{
    /// <summary>
    /// A type of section that defines a WebAssembly module's entry point.
    /// </summary>
    public sealed class StartSection : Section
    {
        public StartSection(uint StartFunctionIndex)
            : this(StartFunctionIndex, new byte[0])
        {
        }

        public StartSection(uint StartFunctionIndex, byte[] ExtraPayload)
        {
            this.StartFunctionIndex = StartFunctionIndex;
            this.ExtraPayload = ExtraPayload;
        }

        /// <inheritdoc/>
        public override SectionName Name => new SectionName(SectionCode.Start);

        /// <summary>
        /// Gets the index of the WebAssembly module's entry point.
        /// </summary>
        /// <returns></returns>
        public uint StartFunctionIndex { get; private set; }

        /// <summary>
        /// Gets this start section's additional payload.
        /// </summary>
        /// <returns>The additional payload, as an array of bytes.</returns>
        public byte[] ExtraPayload { get; set; }

        /// <inheritdoc/>
        public override void WritePayloadTo(BinaryWasmWriter Writer)
        {
            Writer.WriteVarUInt32(StartFunctionIndex);
            Writer.Writer.Write(ExtraPayload);
        }


        /// <inheritdoc/>
        public override void Dump(TextWriter Writer)
        {
            Writer.Write(Name.ToString());
            Writer.Write("; entry point: function #");
            Writer.Write(StartFunctionIndex);
            Writer.WriteLine();
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
        /// Reads the start section with the given header.
        /// </summary>
        /// <param name="Header">The section header.</param>
        /// <param name="Reader">The WebAssembly file reader.</param>
        /// <returns>The parsed section.</returns>
        public static StartSection ReadSectionPayload(SectionHeader Header, BinaryWasmReader Reader)
        {
            long startPos = Reader.Position;
            // Read the start function index.
            uint startIndex = Reader.ReadVarUInt32();
            // Skip any remaining bytes.
            var extraPayload = Reader.ReadRemainingPayload(startPos, Header);
            return new StartSection(startIndex, extraPayload);
        }
    }
}