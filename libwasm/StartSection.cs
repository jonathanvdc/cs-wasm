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
        /// <summary>
        /// Creates a section that identifies a particular function as the entry point.
        /// </summary>
        /// <param name="startFunctionIndex">The index of a function to define as the entry point.</param>
        public StartSection(uint startFunctionIndex)
            : this(startFunctionIndex, new byte[0])
        {
        }

        /// <summary>
        /// Creates a section that identifies a particular function as the entry point.
        /// </summary>
        /// <param name="startFunctionIndex">The index of a function to define as the entry point.</param>
        /// <param name="extraPayload">
        /// A sequence of bytes that have no intrinsic meaning; they are part
        /// of the start section but are placed after the start section's actual contents.
        /// </param>
        public StartSection(uint startFunctionIndex, byte[] extraPayload)
        {
            this.StartFunctionIndex = startFunctionIndex;
            this.ExtraPayload = extraPayload;
        }

        /// <inheritdoc/>
        public override SectionName Name => new SectionName(SectionCode.Start);

        /// <summary>
        /// Gets the index of the WebAssembly module's entry point.
        /// </summary>
        /// <returns></returns>
        public uint StartFunctionIndex { get; set; }

        /// <summary>
        /// Gets this start section's additional payload.
        /// </summary>
        /// <returns>The additional payload, as an array of bytes.</returns>
        public byte[] ExtraPayload { get; set; }

        /// <inheritdoc/>
        public override void WritePayloadTo(BinaryWasmWriter writer)
        {
            writer.WriteVarUInt32(StartFunctionIndex);
            writer.Writer.Write(ExtraPayload);
        }


        /// <inheritdoc/>
        public override void Dump(TextWriter writer)
        {
            writer.Write(Name.ToString());
            writer.Write("; entry point: function #");
            writer.Write(StartFunctionIndex);
            writer.WriteLine();
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
        /// Reads the start section with the given header.
        /// </summary>
        /// <param name="header">The section header.</param>
        /// <param name="reader">The WebAssembly file reader.</param>
        /// <returns>The parsed section.</returns>
        public static StartSection ReadSectionPayload(SectionHeader header, BinaryWasmReader reader)
        {
            long startPos = reader.Position;
            // Read the start function index.
            uint startIndex = reader.ReadVarUInt32();
            // Skip any remaining bytes.
            var extraPayload = reader.ReadRemainingPayload(startPos, header);
            return new StartSection(startIndex, extraPayload);
        }
    }
}
