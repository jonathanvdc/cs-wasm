using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wasm.Binary;
using Wasm.Instructions;

namespace Wasm
{
    /// <summary>
    /// A type of section that declares the initialized data that is loaded into the linear memory.
    /// </summary>
    public sealed class DataSection : Section
    {
        /// <summary>
        /// Creates an empty data section.
        /// </summary>
        public DataSection()
        {
            this.Segments = new List<DataSegment>();
            this.ExtraPayload = new byte[0];
        }

        /// <summary>
        /// Creates a data section from a sequence of data segments.
        /// </summary>
        /// <param name="segments">A sequence of data segments.</param>
        public DataSection(IEnumerable<DataSegment> segments)
            : this(segments, new byte[0])
        {
        }

        /// <summary>
        /// Creates a data section from a sequence of data segments and a trailing payload.
        /// </summary>
        /// <param name="segments">A sequence of data segments.</param>
        /// <param name="extraPayload">
        /// A sequence of bytes that have no intrinsic meaning; they are part
        /// of the data section but are placed after the data section's actual contents.
        /// </param>
        public DataSection(IEnumerable<DataSegment> segments, byte[] extraPayload)
        {
            this.Segments = new List<DataSegment>(segments);
            this.ExtraPayload = extraPayload;
        }

        /// <inheritdoc/>
        public override SectionName Name => new SectionName(SectionCode.Data);

        /// <summary>
        /// Gets the list of the data segments that are defined by this section.
        /// </summary>
        /// <returns>The data segments defined by this section.</returns>
        public List<DataSegment> Segments { get; private set; }

        /// <summary>
        /// Gets this function section's additional payload.
        /// </summary>
        /// <returns>The additional payload, as an array of bytes.</returns>
        public byte[] ExtraPayload { get; set; }

        /// <inheritdoc/>
        public override void WritePayloadTo(BinaryWasmWriter writer)
        {
            writer.WriteVarUInt32((uint)Segments.Count);
            foreach (var export in Segments)
            {
                export.WriteTo(writer);
            }
            writer.Writer.Write(ExtraPayload);
        }

        /// <summary>
        /// Reads the export section with the given header.
        /// </summary>
        /// <param name="header">The section header.</param>
        /// <param name="reader">A reader for a binary WebAssembly file.</param>
        /// <returns>The parsed section.</returns>
        public static DataSection ReadSectionPayload(
            SectionHeader header, BinaryWasmReader reader)
        {
            long startPos = reader.Position;
            // Read the data segments.
            uint count = reader.ReadVarUInt32();
            var exportedVals = new List<DataSegment>();
            for (uint i = 0; i < count; i++)
            {
                exportedVals.Add(DataSegment.ReadFrom(reader));
            }

            // Skip any remaining bytes.
            var extraPayload = reader.ReadRemainingPayload(startPos, header);
            return new DataSection(exportedVals, extraPayload);
        }

        /// <inheritdoc/>
        public override void Dump(TextWriter writer)
        {
            writer.Write(Name.ToString());
            writer.Write("; number of entries: ");
            writer.Write(Segments.Count);
            writer.WriteLine();
            var indentedWriter = DumpHelpers.CreateIndentedTextWriter(writer);
            for (int i = 0; i < Segments.Count; i++)
            {
                writer.Write("#{0}:", i);
                indentedWriter.WriteLine();
                Segments[i].Dump(indentedWriter);
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
    }

    /// <summary>
    /// Defines an initializer expression.
    /// </summary>
    public sealed class InitializerExpression
    {
        /// <summary>
        /// Creates an initializer expression from the given list of instructions.
        /// </summary>
        /// <param name="body">The list of instructions for this expression.</param>
        public InitializerExpression(IEnumerable<Instruction> body)
        {
            this.BodyInstructions = new List<Instruction>(body);
        }

        /// <summary>
        /// Creates an initializer expression from the given list of instructions.
        /// </summary>
        /// <param name="body">The list of instructions for this expression.</param>
        public InitializerExpression(params Instruction[] body)
            : this((IEnumerable<Instruction>)body)
        { }

        /// <summary>
        /// Gets the body of this initializer expression as a list instruction.
        /// </summary>
        /// <returns>The initializer expression's body.</returns>
        public List<Instruction> BodyInstructions { get; private set; }

        /// <summary>
        /// Reads an initializer expression from the given WebAssembly reader.
        /// </summary>
        /// <param name="reader">The WebAssembly reader.</param>
        /// <returns>The parsed initializer expression.</returns>
        public static InitializerExpression ReadFrom(BinaryWasmReader reader)
        {
            return new InitializerExpression(
                Operators.Block.ReadBlockContents(WasmType.Empty, reader).Contents);
        }

        /// <summary>
        /// Writes the initializer expression to the given WebAssembly writer.
        /// </summary>
        /// <param name="writer">The WebAssembly writer.</param>
        public void WriteTo(BinaryWasmWriter writer)
        {
            Operators.Block.Create(WasmType.Empty, BodyInstructions).WriteContentsTo(writer);
        }
    }

    /// <summary>
    /// An entry in the data section.
    /// </summary>
    public sealed class DataSegment
    {
        /// <summary>
        /// Creates a data segment from the given memory index, offset and data.
        /// </summary>
        /// <param name="memoryIndex">The memory index.</param>
        /// <param name="offset">An i32 initializer expression that computes the offset at which to place the data.</param>
        /// <param name="data">The data to which a segment of the linear memory is initialized.</param>
        public DataSegment(uint memoryIndex, InitializerExpression offset, byte[] data)
        {
            this.MemoryIndex = memoryIndex;
            this.Offset = offset;
            this.Data = data;
        }

        /// <summary>
        /// Gets or sets the linear memory index.
        /// </summary>
        /// <returns>The linear memory index.</returns>
        public uint MemoryIndex { get; set; }

        /// <summary>
        /// Gets or sets an i32 initializer expression that computes the offset at which to place the data.
        /// </summary>
        /// <returns>An i32 initializer expression.</returns>
        public InitializerExpression Offset { get; set; }

        /// <summary>
        /// Gets or sets the data to which a segment of the linear memory is initialized.
        /// </summary>
        /// <returns>Initial data for a segment of the linear memory.</returns>
        public byte[] Data { get; set; }

        /// <summary>
        /// Writes this exported value to the given WebAssembly file writer.
        /// </summary>
        /// <param name="writer">The WebAssembly file writer.</param>
        public void WriteTo(BinaryWasmWriter writer)
        {
            writer.WriteVarUInt32(MemoryIndex);
            Offset.WriteTo(writer);
            writer.WriteVarUInt32((uint)Data.Length);
            writer.Writer.Write(Data);
        }

        /// <summary>
        /// Reads a data segment from the given WebAssembly reader.
        /// </summary>
        /// <param name="reader">The WebAssembly reader.</param>
        /// <returns>The data segment that was read from the reader.</returns>
        public static DataSegment ReadFrom(BinaryWasmReader reader)
        {
            var index = reader.ReadVarUInt32();
            var offset = InitializerExpression.ReadFrom(reader);
            var dataLength = reader.ReadVarUInt32();
            var data = reader.ReadBytes((int)dataLength);
            return new DataSegment(index, offset, data);
        }

        /// <summary>
        /// Writes a textual representation of this exported value to the given writer.
        /// </summary>
        /// <param name="writer">The writer to which text is written.</param>
        public void Dump(TextWriter writer)
        {
            writer.Write("- Memory index: ");
            writer.Write(MemoryIndex);
            writer.WriteLine();
            writer.Write("- Offset:");
            var indentedWriter = DumpHelpers.CreateIndentedTextWriter(writer);
            foreach (var instruction in Offset.BodyInstructions)
            {
                indentedWriter.WriteLine();
                instruction.Dump(indentedWriter);
            }
            writer.WriteLine();
            writer.Write("- Data:");
            indentedWriter.WriteLine();
            DumpHelpers.DumpBytes(Data, indentedWriter);
        }
    }
}
