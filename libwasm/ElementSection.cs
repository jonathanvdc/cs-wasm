using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wasm.Binary;
using Wasm.Instructions;

namespace Wasm
{
    /// <summary>
    /// A type of section that declares the initialized data that is loaded into a table.
    /// </summary>
    public sealed class ElementSection : Section
    {
        public ElementSection()
        {
            this.Segments = new List<ElementSegment>();
            this.ExtraPayload = new byte[0];
        }

        public ElementSection(IEnumerable<ElementSegment> Segments)
            : this(Segments, new byte[0])
        {
        }

        public ElementSection(IEnumerable<ElementSegment> Segments, byte[] ExtraPayload)
        {
            this.Segments = new List<ElementSegment>(Segments);
            this.ExtraPayload = ExtraPayload;
        }

        /// <inheritdoc/>
        public override SectionName Name => new SectionName(SectionCode.Element);

        /// <summary>
        /// Gets the list of the element segments defined by this section.
        /// </summary>
        /// <returns>The element segments defined by this section.</returns>
        public List<ElementSegment> Segments { get; private set; }

        /// <summary>
        /// Gets this function section's additional payload.
        /// </summary>
        /// <returns>The additional payload, as an array of bytes.</returns>
        public byte[] ExtraPayload { get; set; }

        /// <inheritdoc/>
        public override void WritePayloadTo(BinaryWasmWriter Writer)
        {
            Writer.WriteVarUInt32((uint)Segments.Count);
            foreach (var segment in Segments)
            {
                segment.WriteTo(Writer);
            }
            Writer.Writer.Write(ExtraPayload);
        }

        /// <summary>
        /// Reads the element section with the given header.
        /// </summary>
        /// <param name="Header">The section header.</param>
        /// <param name="Reader">A reader for a binary WebAssembly file.</param>
        /// <returns>The parsed section.</returns>
        public static ElementSection ReadSectionPayload(
            SectionHeader Header, BinaryWasmReader Reader)
        {
            long startPos = Reader.Position;
            // Read the element segments.
            uint count = Reader.ReadVarUInt32();
            var segments = new List<ElementSegment>();
            for (uint i = 0; i < count; i++)
            {
                segments.Add(ElementSegment.ReadFrom(Reader));
            }

            // Skip any remaining bytes.
            var extraPayload = Reader.ReadRemainingPayload(startPos, Header);
            return new ElementSection(segments, extraPayload);
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
    /// An entry in the element section.
    /// </summary>
    public sealed class ElementSegment
    {
        /// <summary>
        /// Creates an element segment from the given table index, offset and data.
        /// </summary>
        /// <param name="TableIndex">The table index.</param>
        /// <param name="Offset">An i32 initializer expression that computes the offset at which to place the data.</param>
        /// <param name="Elements">A sequence of function indices to which a segment of the table is initialized.</param>
        public ElementSegment(uint TableIndex, InitializerExpression Offset, IEnumerable<uint> Elements)
        {
            this.TableIndex = TableIndex;
            this.Offset = Offset;
            this.Elements = new List<uint>(Elements);
        }

        /// <summary>
        /// Gets the table index.
        /// </summary>
        /// <returns>The table index.</returns>
        public uint TableIndex { get; set; }

        /// <summary>
        /// Gets an i32 initializer expression that computes the offset at which to place the data.
        /// </summary>
        /// <returns>An i32 initializer expression.</returns>
        public InitializerExpression Offset { get; set; }

        /// <summary>
        /// Gets a list of function indices to which this segment of the table is initialized.
        /// </summary>
        /// <returns>The list of function indices to which this segment of the table is initialized.</returns>
        public List<uint> Elements { get; private set; }

        /// <summary>
        /// Writes this element segment to the given WebAssembly file writer.
        /// </summary>
        /// <param name="Writer">The WebAssembly file writer.</param>
        public void WriteTo(BinaryWasmWriter Writer)
        {
            Writer.WriteVarUInt32(TableIndex);
            Offset.WriteTo(Writer);
            Writer.WriteVarUInt32((uint)Elements.Count);
            foreach (var item in Elements)
            {
                Writer.WriteVarUInt32(item);
            }
        }

        /// <summary>
        /// Reads an element segment from the given WebAssembly reader.
        /// </summary>
        /// <param name="Reader">The WebAssembly reader.</param>
        /// <returns>The element segment that was read from the reader.</returns>
        public static ElementSegment ReadFrom(BinaryWasmReader Reader)
        {
            var index = Reader.ReadVarUInt32();
            var offset = InitializerExpression.ReadFrom(Reader);
            var dataLength = Reader.ReadVarUInt32();
            var elements = new List<uint>((int)dataLength);
            for (uint i = 0; i < dataLength; i++)
            {
                elements.Add(Reader.ReadVarUInt32());
            }
            return new ElementSegment(index, offset, elements);
        }

        /// <summary>
        /// Writes a textual representation of this element segment to the given writer.
        /// </summary>
        /// <param name="Writer">The writer to which text is written.</param>
        public void Dump(TextWriter Writer)
        {
            Writer.Write("- Table index: ");
            Writer.Write(TableIndex);
            Writer.WriteLine();
            Writer.Write("- Offset:");
            var indentedWriter = DumpHelpers.CreateIndentedTextWriter(Writer);
            foreach (var instruction in Offset.BodyInstructions)
            {
                indentedWriter.WriteLine();
                instruction.Dump(indentedWriter);
            }
            Writer.WriteLine();
            Writer.Write("- Elements:");
            for (int i = 0; i < Elements.Count; i++)
            {
                indentedWriter.WriteLine();
                indentedWriter.Write("#{0} -> func #{1}", i, Elements[i]);
            }
        }
    }
}