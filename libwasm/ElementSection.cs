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
        /// <summary>
        /// Creates an empty element section.
        /// </summary>
        public ElementSection()
        {
            this.Segments = new List<ElementSegment>();
            this.ExtraPayload = new byte[0];
        }

        /// <summary>
        /// Creates an element section from a sequence of segments.
        /// </summary>
        /// <param name="segments">The segments to put in the elements section.</param>
        public ElementSection(IEnumerable<ElementSegment> segments)
            : this(segments, new byte[0])
        {
        }

        /// <summary>
        /// Creates an element section from a sequence of segments and a trailing payload.
        /// </summary>
        /// <param name="segments">The segments to put in the elements section.</param>
        /// <param name="extraPayload">
        /// A sequence of bytes that have no intrinsic meaning; they are part
        /// of the element section but are placed after the element section's actual contents.
        /// </param>
        public ElementSection(IEnumerable<ElementSegment> segments, byte[] extraPayload)
        {
            this.Segments = new List<ElementSegment>(segments);
            this.ExtraPayload = extraPayload;
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
        public override void WritePayloadTo(BinaryWasmWriter writer)
        {
            writer.WriteVarUInt32((uint)Segments.Count);
            foreach (var segment in Segments)
            {
                segment.WriteTo(writer);
            }
            writer.Writer.Write(ExtraPayload);
        }

        /// <summary>
        /// Reads the element section with the given header.
        /// </summary>
        /// <param name="header">The section header.</param>
        /// <param name="reader">A reader for a binary WebAssembly file.</param>
        /// <returns>The parsed section.</returns>
        public static ElementSection ReadSectionPayload(
            SectionHeader header, BinaryWasmReader reader)
        {
            long startPos = reader.Position;
            // Read the element segments.
            uint count = reader.ReadVarUInt32();
            var segments = new List<ElementSegment>();
            for (uint i = 0; i < count; i++)
            {
                segments.Add(ElementSegment.ReadFrom(reader));
            }

            // Skip any remaining bytes.
            var extraPayload = reader.ReadRemainingPayload(startPos, header);
            return new ElementSection(segments, extraPayload);
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
    /// An entry in the element section.
    /// </summary>
    public sealed class ElementSegment
    {
        /// <summary>
        /// Creates an element segment from the given table index, offset and data.
        /// </summary>
        /// <param name="tableIndex">The table index.</param>
        /// <param name="offset">An i32 initializer expression that computes the offset at which to place the data.</param>
        /// <param name="elements">A sequence of function indices to which a segment of the table is initialized.</param>
        public ElementSegment(uint tableIndex, InitializerExpression offset, IEnumerable<uint> elements)
        {
            this.TableIndex = tableIndex;
            this.Offset = offset;
            this.Elements = new List<uint>(elements);
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
        /// <param name="writer">The WebAssembly file writer.</param>
        public void WriteTo(BinaryWasmWriter writer)
        {
            writer.WriteVarUInt32(TableIndex);
            Offset.WriteTo(writer);
            writer.WriteVarUInt32((uint)Elements.Count);
            foreach (var item in Elements)
            {
                writer.WriteVarUInt32(item);
            }
        }

        /// <summary>
        /// Reads an element segment from the given WebAssembly reader.
        /// </summary>
        /// <param name="reader">The WebAssembly reader.</param>
        /// <returns>The element segment that was read from the reader.</returns>
        public static ElementSegment ReadFrom(BinaryWasmReader reader)
        {
            var index = reader.ReadVarUInt32();
            var offset = InitializerExpression.ReadFrom(reader);
            var dataLength = reader.ReadVarUInt32();
            var elements = new List<uint>((int)dataLength);
            for (uint i = 0; i < dataLength; i++)
            {
                elements.Add(reader.ReadVarUInt32());
            }
            return new ElementSegment(index, offset, elements);
        }

        /// <summary>
        /// Writes a textual representation of this element segment to the given writer.
        /// </summary>
        /// <param name="writer">The writer to which text is written.</param>
        public void Dump(TextWriter writer)
        {
            writer.Write("- Table index: ");
            writer.Write(TableIndex);
            writer.WriteLine();
            writer.Write("- Offset:");
            var indentedWriter = DumpHelpers.CreateIndentedTextWriter(writer);
            foreach (var instruction in Offset.BodyInstructions)
            {
                indentedWriter.WriteLine();
                instruction.Dump(indentedWriter);
            }
            writer.WriteLine();
            writer.Write("- Elements:");
            for (int i = 0; i < Elements.Count; i++)
            {
                indentedWriter.WriteLine();
                indentedWriter.Write("#{0} -> func #{1}", i, Elements[i]);
            }
        }
    }
}
