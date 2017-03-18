using System;
using System.IO;

namespace Wasm.Binary
{
    /// <summary>
    /// A reader that reads the binary WebAssembly format.
    /// </summary>
    public sealed class BinaryWasmReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Wasm.Binary.BinaryWasmReader"/> class.
        /// </summary>
        /// <param name="InputStream">The binary reader for a WebAssembly file.</param>
        public BinaryWasmReader(BinaryReader Reader)
        {
            this.Reader = Reader;
        }

        /// <summary>
        /// The binary reader for a WebAssembly file.
        /// </summary>
        public BinaryReader Reader { get; private set; }

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
                byte b = Reader.ReadByte();
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
        /// Parses a signed LEB128 variable-length integer of variable size,
        /// which is at most 64 bits.
        /// </summary>
        /// <param name="Size">The size of the variable-length integer, in bits.</param>
        /// <returns>The parsed signed 64-bit integer.</returns>
        public long ReadVarInt(int Size)
        {
            // C# translation of code borrowed from Wikipedia article:
            // https://en.wikipedia.org/wiki/LEB128

            long result = 0;
            int shift = 0;
            byte b;
            do
            {
                b = Reader.ReadByte();
                result |= ((long)(b & 0x7F) << shift);
                shift += 7;
            } while ((b & 0x80) != 0);

            // Sign bit of byte is second high order bit. (0x40)
            if ((shift < Size) && ((b & 0x40) == 1))
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
            return (sbyte)ReadVarInt(7);
        }

        /// <summary>
        /// Parses a signed LEB128 variable-length integer, limited to 32 bits.
        /// </summary>
        /// <returns>The parsed signed 32-bit integer.</returns>
        public int ReadVarInt32()
        {
            return (int)ReadVarInt(32);
        }

        /// <summary>
        /// Parses a signed LEB128 variable-length integer, limited to 64 bits.
        /// </summary>
        /// <returns>The parsed signed 64-bit integer.</returns>
        public long ReadVarInt64()
        {
            return ReadVarInt(64);
        }

        /// <summary>
        /// Parses a length-prefixed bytestring.
        /// </summary>
        /// <returns>The parsed bytestring.</returns>
        public ByteString ReadByteString()
        {
            uint length = ReadVarUInt32();
            return new ByteString(Reader.ReadBytes((int)length));
        }

        /// <summary>
        /// Parses a version header.
        /// </summary>
        /// <returns>The parsed version header.</returns>
        public VersionHeader ReadVersionHeader()
        {
            return new VersionHeader(Reader.ReadUInt32(), Reader.ReadUInt32());
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
                var name = ReadByteString();
                return new SectionHeader(name, payloadLength);
            }
            else
            {
                return new SectionHeader(code, payloadLength);
            }
        }
    }
}

