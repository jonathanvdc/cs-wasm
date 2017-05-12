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
        public DataSection()
        {
            this.Segments = new List<DataSegment>();
            this.ExtraPayload = new byte[0];
        }

        public DataSection(IEnumerable<DataSegment> Segments)
            : this(Segments, new byte[0])
        {
        }

        public DataSection(IEnumerable<DataSegment> Segments, byte[] ExtraPayload)
        {
            this.Segments = new List<DataSegment>(Segments);
            this.ExtraPayload = ExtraPayload;
        }

        /// <inheritdoc/>
        public override SectionName Name => new SectionName(SectionCode.Data);

        /// <summary>
        /// Gets the list of all values that are exported by this section.
        /// </summary>
        /// <returns>A list of all values exported by this section.</returns>
        public List<DataSegment> Segments { get; private set; }

        /// <summary>
        /// Gets this function section's additional payload.
        /// </summary>
        /// <returns>The additional payload, as an array of bytes.</returns>
        public byte[] ExtraPayload { get; private set; }

        /// <inheritdoc/>
        public override void WritePayloadTo(BinaryWasmWriter Writer)
        {
            Writer.WriteVarUInt32((uint)Segments.Count);
            foreach (var export in Segments)
            {
                export.WriteTo(Writer);
            }
            Writer.Writer.Write(ExtraPayload);
        }

        /// <summary>
        /// Reads the export section with the given header.
        /// </summary>
        /// <param name="Header">The section header.</param>
        /// <param name="Reader">A reader for a binary WebAssembly file.</param>
        /// <returns>The parsed section.</returns>
        public static DataSection ReadSectionPayload(
            SectionHeader Header, BinaryWasmReader Reader)
        {
            long startPos = Reader.Position;
            // Read the data segments.
            uint count = Reader.ReadVarUInt32();
            var exportedVals = new List<DataSegment>();
            for (uint i = 0; i < count; i++)
            {
                exportedVals.Add(DataSegment.Read(Reader));
            }

            // Skip any remaining bytes.
            var extraPayload = Reader.ReadRemainingPayload(startPos, Header);
            return new DataSection(exportedVals, extraPayload);
        }

        /// <inheritdoc/>
        public override void Dump(TextWriter Writer)
        {
            Writer.Write(Name.ToString());
            Writer.Write("; number of entries: ");
            Writer.Write(Segments.Count);
            Writer.WriteLine();
            var indentedWriter = DumpHelpers.CreateIndentedTextWriter(Writer);
            for (int i = 0; i < Segments.Count; i++)
            {
                Writer.Write("#{0}:", i);
                indentedWriter.WriteLine();
                Segments[i].Dump(indentedWriter);
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

    /// <summary>
    /// Defines an initializer expression.
    /// </summary>
    public class InitializerExpression
    {
        /// <summary>
        /// Creates an initializer expression from the given list of instructions.
        /// </summary>
        /// <param name="Body">The list of instructions for this expression.</param>
        public InitializerExpression(IEnumerable<Instruction> Body)
        {
            this.Body = new List<Instruction>(Body);
        }

        /// <summary>
        /// Gets the body of this initializer expression as a list instruction.
        /// </summary>
        /// <returns>The initializer expression's body.</returns>
        public List<Instruction> Body { get; private set; }

        /// <summary>
        /// Reads an initializer expression from the given WebAssembly reader.
        /// </summary>
        /// <param name="Reader">The WebAssembly reader.</param>
        /// <returns>The parsed initializer expression.</returns>
        public static InitializerExpression Read(BinaryWasmReader Reader)
        {
            return new InitializerExpression(
                Operators.Block.ReadBlockContents(WasmType.Empty, Reader).Contents);
        }

        /// <summary>
        /// Writes the initializer expression to the given WebAssembly writer.
        /// </summary>
        /// <param name="Writer">The WebAssembly writer.</param>
        public void WriteTo(BinaryWasmWriter Writer)
        {
            Operators.Block.Create(WasmType.Empty, Body).WriteContentsTo(Writer);
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
        /// <param name="MemoryIndex">The memory index.</param>
        /// <param name="Offset">An i32 initializer expression that computes the offset at which to place the data.</param>
        /// <param name="Data">The data to which a segment of the linear memory is initialized.</param>
        public DataSegment(uint MemoryIndex, InitializerExpression Offset, byte[] Data)
        {
            this.MemoryIndex = MemoryIndex;
            this.Offset = Offset;
            this.Data = Data;
        }

        /// <summary>
        /// Gets the linear memory index.
        /// </summary>
        /// <returns>The linear memory index.</returns>
        public uint MemoryIndex { get; private set; }

        /// <summary>
        /// Gets an i32 initializer expression that computes the offset at which to place the data.
        /// </summary>
        /// <returns>An i32 initializer expression.</returns>
        public InitializerExpression Offset { get; private set; }

        /// <summary>
        /// Gets the data to which a segment of the linear memory is initialized.
        /// </summary>
        /// <returns>Inital data for a segment of the linear memory.</returns>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Writes this exported value to the given WebAssembly file writer.
        /// </summary>
        /// <param name="Writer">The WebAssembly file writer.</param>
        public void WriteTo(BinaryWasmWriter Writer)
        {
            Writer.WriteVarUInt32(MemoryIndex);
            Offset.WriteTo(Writer);
            Writer.WriteVarUInt32((uint)Data.Length);
            Writer.Writer.Write(Data);
        }

        /// <summary>
        /// Reads a data segment from the given WebAssembly reader.
        /// </summary>
        /// <param name="Reader">The WebAssembly reader.</param>
        /// <returns>The data segment that was read from the reader.</returns>
        public static DataSegment Read(BinaryWasmReader Reader)
        {
            var index = Reader.ReadVarUInt32();
            var offset = InitializerExpression.Read(Reader);
            var dataLength = Reader.ReadVarUInt32();
            var data = Reader.Reader.ReadBytes((int)dataLength);
            return new DataSegment(index, offset, data);
        }

        /// <summary>
        /// Writes a textual representation of this exported value to the given writer.
        /// </summary>
        /// <param name="Writer">The writer to which text is written.</param>
        public void Dump(TextWriter Writer)
        {
            Writer.Write("- Memory index: ");
            Writer.Write(MemoryIndex);
            Writer.WriteLine();
            Writer.Write("- Offset:");
            var indentedWriter = DumpHelpers.CreateIndentedTextWriter(Writer);
            foreach (var instruction in Offset.Body)
            {
                indentedWriter.WriteLine();
                instruction.Dump(indentedWriter);
            }
            Writer.WriteLine();
            Writer.Write("- Data:");
            indentedWriter.WriteLine();
            DumpHelpers.DumpBytes(Data, indentedWriter);
        }
    }
}