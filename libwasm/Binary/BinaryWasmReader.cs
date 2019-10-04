using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Wasm.Binary
{
    /// <summary>
    /// A reader that reads the binary WebAssembly format.
    /// </summary>
    public class BinaryWasmReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Wasm.Binary.BinaryWasmReader"/> class.
        /// </summary>
        /// <param name="reader">The binary reader for a WebAssembly file.</param>
        public BinaryWasmReader(BinaryReader reader)
            : this(reader, UTF8Encoding.UTF8)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Wasm.Binary.BinaryWasmReader"/> class.
        /// </summary>
        /// <param name="reader">The binary reader for a WebAssembly file.</param>
        /// <param name="stringEncoding">The encoding for strings in the WebAssembly file.</param>
        public BinaryWasmReader(
            BinaryReader reader,
            Encoding stringEncoding)
        {
            this.reader = reader;
            this.StringEncoding = stringEncoding;
            this.streamIsEmpty = defaultStreamIsEmptyImpl;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Wasm.Binary.BinaryWasmReader"/> class.
        /// </summary>
        /// <param name="reader">The binary reader for a WebAssembly file.</param>
        /// <param name="stringEncoding">The encoding for strings in the WebAssembly file.</param>
        /// <param name="streamIsEmpty">Tests if the stream is empty.</param>
        public BinaryWasmReader(
            BinaryReader reader,
            Encoding stringEncoding,
            Func<bool> streamIsEmpty)
        {
            this.reader = reader;
            this.StringEncoding = stringEncoding;
            this.streamIsEmpty = streamIsEmpty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Wasm.Binary.BinaryWasmReader"/> class.
        /// </summary>
        /// <param name="reader">The binary reader for a WebAssembly file.</param>
        /// <param name="streamIsEmpty">Tests if the stream is empty.</param>
        public BinaryWasmReader(
            BinaryReader reader,
            Func<bool> streamIsEmpty)
        {
            this.reader = reader;
            this.StringEncoding = UTF8Encoding.UTF8;
            this.streamIsEmpty = streamIsEmpty;
        }

        /// <summary>
        /// The binary reader for a WebAssembly file.
        /// </summary>
        private BinaryReader reader;

        /// <summary>
        /// The encoding that is used to parse strings.
        /// </summary>
        /// <returns>The string encoding.</returns>
        public Encoding StringEncoding { get; private set; }

        /// <summary>
        /// Tests if the stream is empty.
        /// </summary>
        private Func<bool> streamIsEmpty;

        /// <summary>
        /// A default implementation that tests if the stream is empty.
        /// </summary>
        private bool defaultStreamIsEmptyImpl()
        {
            return Position >= reader.BaseStream.Length;
        }

        /// <summary>
        /// Gets the current position of the reader in the WebAssembly file.
        /// </summary>
        public long Position { get; private set; }

        /// <summary>
        /// Reads a single byte.
        /// </summary>
        /// <returns>The byte that was read.</returns>
        public byte ReadByte()
        {
            byte result = reader.ReadByte();
            Position++;
            return result;
        }

        /// <summary>
        /// Reads a range of bytes.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The array of bytes that were read.</returns>
        public byte[] ReadBytes(int count)
        {
            byte[] results = reader.ReadBytes(count);
            Position += count;
            return results;
        }

        /// <summary>
        /// Parses an unsigned LEB128 variable-length integer, limited to 64 bits.
        /// </summary>
        /// <returns>The parsed unsigned 64-bit integer.</returns>
        public ulong ReadVarUInt64()
        {
            // C# translation of code borrowed from Wikipedia article:
            // https://en.wikipedia.org/wiki/LEB128
            ulong result = 0;
            int shift = 0;
            while (true)
            {
                byte b = ReadByte();
                result |= ((ulong)(b & 0x7F) << shift);
                if ((b & 0x80) == 0)
                    break;
                shift += 7;
            }
            return result;
        }

        /// <summary>
        /// Parses an unsigned LEB128 variable-length integer, limited to one bit.
        /// </summary>
        /// <returns>The parsed unsigned 1-bit integer, as a Boolean.</returns>
        public bool ReadVarUInt1()
        {
            // Negate the integer twice to turn it into a Boolean.
            return !(ReadVarUInt64() == 0);
        }

        /// <summary>
        /// Parses an unsigned LEB128 variable-length integer, limited to 7 bits.
        /// </summary>
        /// <returns>The parsed unsigned 7-bit integer.</returns>
        public byte ReadVarUInt7()
        {
            return (byte)ReadVarUInt64();
        }

        /// <summary>
        /// Parses an unsigned LEB128 variable-length integer, limited to 32 bits.
        /// </summary>
        /// <returns>The parsed unsigned 32-bit integer.</returns>
        public uint ReadVarUInt32()
        {
            return (uint)ReadVarUInt64();
        }

        /// <summary>
        /// Parses a signed LEB128 variable-length integer, limited to 64 bits.
        /// </summary>
        /// <returns>The parsed signed 64-bit integer.</returns>
        public long ReadVarInt64()
        {
            // C# translation of code borrowed from Wikipedia article:
            // https://en.wikipedia.org/wiki/LEB128

            long result = 0;
            int shift = 0;
            byte b;
            do
            {
                b = ReadByte();
                result |= ((long)(b & 0x7F) << shift);
                shift += 7;
            } while ((b & 0x80) != 0);

            // Sign bit of byte is second high order bit. (0x40)
            if ((shift < 64) && ((b & 0x40) == 0x40))
            {
                // Sign extend.
                result |= -(1L << shift);
            }

            return result;
        }

        /// <summary>
        /// Parses a signed LEB128 variable-length integer, limited to 7 bits.
        /// </summary>
        /// <returns>The parsed signed 7-bit integer.</returns>
        public sbyte ReadVarInt7()
        {
            return (sbyte)ReadVarInt64();
        }

        /// <summary>
        /// Parses a signed LEB128 variable-length integer, limited to 32 bits.
        /// </summary>
        /// <returns>The parsed signed 32-bit integer.</returns>
        public int ReadVarInt32()
        {
            return (int)ReadVarInt64();
        }

        /// <summary>
        /// Parses a 32-bit floating-point number.
        /// </summary>
        /// <returns>The parsed 32-bit floating-point number.</returns>
        public float ReadFloat32()
        {
            var result = reader.ReadSingle();
            Position += sizeof(float);
            return result;
        }

        /// <summary>
        /// Parses a 64-bit floating-point number.
        /// </summary>
        /// <returns>The parsed 64-bit floating-point number.</returns>
        public double ReadFloat64()
        {
            var result = reader.ReadDouble();
            Position += sizeof(double);
            return result;
        }

        /// <summary>
        /// Reads a WebAssembly language type.
        /// </summary>
        /// <returns>The WebAssembly language type.</returns>
        public WasmType ReadWasmType()
        {
            return (WasmType)ReadVarInt7();
        }

        /// <summary>
        /// Reads a WebAssembly value type.
        /// </summary>
        /// <returns>The WebAssembly value type.</returns>
        public WasmValueType ReadWasmValueType()
        {
            return (WasmValueType)ReadVarInt7();
        }

        /// <summary>
        /// Parses a length-prefixed string.
        /// </summary>
        /// <returns>The parsed string.</returns>
        public string ReadString()
        {
            uint length = ReadVarUInt32();
            byte[] bytes = ReadBytes((int)length);
            return StringEncoding.GetString(bytes);
        }

        /// <summary>
        /// Reads resizable limits.
        /// </summary>
        /// <returns>The resizable limits.</returns>
        public ResizableLimits ReadResizableLimits()
        {
            bool hasMaximum = ReadVarUInt1();
            uint initial = ReadVarUInt32();
            Nullable<uint> max = hasMaximum
                ? new Nullable<uint>(ReadVarUInt32())
                : default(Nullable<uint>);
            return new ResizableLimits(initial, max);
        }

        /// <summary>
        /// Parses a version header.
        /// </summary>
        /// <returns>The parsed version header.</returns>
        public VersionHeader ReadVersionHeader()
        {
            var result = new VersionHeader(reader.ReadUInt32(), reader.ReadUInt32());
            Position += 2 * sizeof(uint);
            return result;
        }

        /// <summary>
        /// Parses a section header.
        /// </summary>
        /// <returns>The parsed section header.</returns>
        public SectionHeader ReadSectionHeader()
        {
            var code = (SectionCode)ReadVarUInt7();
            uint payloadLength = ReadVarUInt32();
            if (code == SectionCode.Custom)
            {
                uint startPos = (uint)Position;
                var name = ReadString();
                uint nameLength = (uint)Position - startPos;
                return new SectionHeader(new SectionName(name), payloadLength - nameLength);
            }
            else
            {
                return new SectionHeader(new SectionName(code), payloadLength);
            }
        }

        /// <summary>
        /// Reads a section.
        /// </summary>
        /// <returns>The section.</returns>
        public Section ReadSection()
        {
            var header = ReadSectionHeader();
            return ReadSectionPayload(header);
        }

        /// <summary>
        /// Reads the section with the given header.
        /// </summary>
        /// <param name="header">The section header.</param>
        /// <returns>The parsed section.</returns>
        public Section ReadSectionPayload(SectionHeader header)
        {
            if (header.Name.IsCustom)
                return ReadCustomSectionPayload(header);
            else
                return ReadKnownSectionPayload(header);
        }

        /// <summary>
        /// Reads the remaining payload of the section whose payload starts at the given position.
        /// </summary>
        /// <param name="startPosition">The start of the section's payload.</param>
        /// <param name="payloadLength">The length of the section's payload, in bytes.</param>
        /// <returns>The remaining payload of the section whose payload starts at the given position.</returns>
        public byte[] ReadRemainingPayload(long startPosition, uint payloadLength)
        {
            return ReadBytes((int)(Position - startPosition - payloadLength));
        }

        /// <summary>
        /// Reads the remaining payload of the section whose payload starts at the given position.
        /// </summary>
        /// <param name="startPosition">The start of the section's payload.</param>
        /// <param name="header">The section's header.</param>
        /// <returns>The remaining payload of the section whose payload starts at the given position.</returns>
        public byte[] ReadRemainingPayload(long startPosition, SectionHeader header)
        {
            return ReadRemainingPayload(startPosition, header.PayloadLength);
        }

        /// <summary>
        /// Reads the custom section with the given header.
        /// </summary>
        /// <param name="header">The section header.</param>
        /// <returns>The parsed section.</returns>
        protected virtual Section ReadCustomSectionPayload(SectionHeader header)
        {
            if (header.Name.CustomName == NameSection.CustomName)
            {
                return NameSection.ReadSectionPayload(header, this);
            }
            else
            {
                return new CustomSection(
                    header.Name.CustomName,
                    ReadBytes((int)header.PayloadLength));
            }
        }

        /// <summary>
        /// Reads the non-custom section with the given header.
        /// </summary>
        /// <param name="header">The section header.</param>
        /// <returns>The parsed section.</returns>
        protected Section ReadKnownSectionPayload(SectionHeader header)
        {
            switch (header.Name.Code)
            {
                case SectionCode.Type:
                    return TypeSection.ReadSectionPayload(header, this);
                case SectionCode.Import:
                    return ImportSection.ReadSectionPayload(header, this);
                case SectionCode.Function:
                    return FunctionSection.ReadSectionPayload(header, this);
                case SectionCode.Table:
                    return TableSection.ReadSectionPayload(header, this);
                case SectionCode.Memory:
                    return MemorySection.ReadSectionPayload(header, this);
                case SectionCode.Global:
                    return GlobalSection.ReadSectionPayload(header, this);
                case SectionCode.Export:
                    return ExportSection.ReadSectionPayload(header, this);
                case SectionCode.Start:
                    return StartSection.ReadSectionPayload(header, this);
                case SectionCode.Element:
                    return ElementSection.ReadSectionPayload(header, this);
                case SectionCode.Code:
                    return CodeSection.ReadSectionPayload(header, this);
                case SectionCode.Data:
                    return DataSection.ReadSectionPayload(header, this);
                default:
                    return ReadUnknownSectionPayload(header);
            }
        }

        /// <summary>
        /// Reads the unknown, non-custom section with the given header.
        /// </summary>
        /// <param name="header">The section header.</param>
        /// <returns>The parsed section.</returns>
        protected virtual Section ReadUnknownSectionPayload(SectionHeader header)
        {
            return new UnknownSection(
                header.Name.Code,
                ReadBytes((int)header.PayloadLength));
        }

        /// <summary>
        /// Reads an entire WebAssembly file.
        /// </summary>
        /// <returns>The WebAssembly file.</returns>
        public WasmFile ReadFile()
        {
            var version = ReadVersionHeader();
            version.Verify();
            var sections = new List<Section>();
            while (!streamIsEmpty())
            {
                sections.Add(ReadSection());
            }
            return new WasmFile(version, sections);
        }
    }
}

