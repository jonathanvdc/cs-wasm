using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Wasm.Binary
{
    /// <summary>
    /// A writes that writes the binary WebAssembly format.
    /// </summary>
    public class BinaryWasmWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Wasm.Binary.BinaryWasmWriter"/> class.
        /// </summary>
        /// <param name="writer">The binary writer for a WebAssembly file.</param>
        public BinaryWasmWriter(BinaryWriter writer)
            : this(writer, UTF8Encoding.UTF8)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Wasm.Binary.BinaryWasmWriter"/> class.
        /// </summary>
        /// <param name="writer">The binary writer for a WebAssembly file.</param>
        /// <param name="stringEncoding">The encoding for strings in the WebAssembly file.</param>
        public BinaryWasmWriter(BinaryWriter writer, Encoding stringEncoding)
        {
            this.Writer = writer;
            this.StringEncoding = stringEncoding;
        }

        /// <summary>
        /// The binary writer for a WebAssembly file.
        /// </summary>
        public BinaryWriter Writer { get; private set; }

        /// <summary>
        /// The encoding that is used to write strings.
        /// </summary>
        /// <returns>The string encoding.</returns>
        public Encoding StringEncoding { get; private set; }

        /// <summary>
        /// Writes an unsigned LEB128 variable-length integer, limited to 64 bits.
        /// </summary>
        /// <returns>The number of bytes used to encode the integer.</returns>
        public int WriteVarUInt64(ulong value)
        {
            // C# translation of code borrowed from Wikipedia article:
            // https://en.wikipedia.org/wiki/LEB128

            int count = 0;
            do
            {
                byte b = (byte)(value & 0x7F);
                value >>= 7;
                if (value != 0)
                    b |= 0x80;

                Writer.Write(b);
                count++;
            } while (value != 0);
            return count;
        }

        /// <summary>
        /// Writes an unsigned LEB128 variable-length integer, limited to 32 bits.
        /// </summary>
        /// <returns>The number of bytes used to encode the integer.</returns>
        public int WriteVarUInt32(uint value)
        {
            return WriteVarUInt64(value);
        }

        /// <summary>
        /// Writes an unsigned LEB128 variable-length integer, limited to 7 bits.
        /// </summary>
        /// <returns>The number of bytes used to encode the integer.</returns>
        public int WriteVarUInt7(byte value)
        {
            return WriteVarUInt32(value);
        }

        /// <summary>
        /// Writes an unsigned LEB128 variable-length integer, limited to one bit.
        /// </summary>
        /// <returns>The number of bytes used to encode the integer.</returns>
        public int WriteVarUInt1(bool value)
        {
            return WriteVarUInt32(value ? 1u : 0u);
        }

        /// <summary>
        /// Writes a signed LEB128 variable-length integer, limited to 64 bits.
        /// </summary>
        /// <returns>The number of bytes used to encode the integer.</returns>
        public int WriteVarInt64(long value)
        {
            // C# translation of code borrowed from Wikipedia article:
            // https://en.wikipedia.org/wiki/LEB128

            int count = 0;
            bool more = true;
            while (more)
            {
                byte b = (byte)(value & 0x7F);
                value >>= 7;

                if ((value == 0 && ((b & 0x40) == 0)) || (value == -1 && ((b & 0x40) == 0x40)))
                    more = false;
                else
                    // set high order bit of byte
                    b |= 0x80;

                Writer.Write(b);
                count++;
            }
            return count;
        }

        /// <summary>
        /// Writes a signed LEB128 variable-length integer, limited to 32 bits.
        /// </summary>
        /// <returns>The number of bytes used to encode the integer.</returns>
        public int WriteVarInt32(int value)
        {
            return WriteVarInt64(value);
        }

        /// <summary>
        /// Writes a signed LEB128 variable-length integer, limited to 7 bits.
        /// </summary>
        /// <returns>The number of bytes used to encode the integer.</returns>
        public int WriteVarInt7(sbyte value)
        {
            return WriteVarInt64(value);
        }

        /// <summary>
        /// Writes a 32-bit floating-point number.
        /// </summary>
        /// <param name="value">The floating-point number to write.</param>
        /// <returns>The number of bytes used to encode the floating-point number.</returns>
        public int WriteFloat32(float value)
        {
            Writer.Write(value);
            return 4;
        }

        /// <summary>
        /// Writes a 64-bit floating-point number.
        /// </summary>
        /// <param name="value">The floating-point number to write.</param>
        /// <returns>The number of bytes used to encode the floating-point number.</returns>
        public int WriteFloat64(double value)
        {
            Writer.Write(value);
            return 8;
        }

        /// <summary>
        /// Writes a WebAssembly language type.
        /// </summary>
        /// <param name="value">The WebAssembly language type to write.</param>
        /// <returns>The number of bytes used to encode the type.</returns>
        public int WriteWasmType(WasmType value)
        {
            return WriteVarInt7((sbyte)value);
        }

        /// <summary>
        /// Writes a WebAssembly value type.
        /// </summary>
        /// <param name="value">The WebAssembly language value to write.</param>
        /// <returns>The number of bytes used to encode the type.</returns>
        public int WriteWasmValueType(WasmValueType value)
        {
            return WriteVarInt7((sbyte)value);
        }

        /// <summary>
        /// Writes a length-prefixed string to the WebAssembly file.
        /// </summary>
        /// <param name="value">The string to write to the file.</param>
        public void WriteString(string value)
        {
            byte[] buffer = StringEncoding.GetBytes(value);
            WriteVarUInt32((uint)buffer.Length);
            Writer.Write(buffer);
        }

        /// <summary>
        /// Writes data and prefixes it with a variable-length 32-bit unsigned integer
        /// that specifies the number of bytes written.
        /// </summary>
        /// <param name="writeData">Writes data to a WebAssembly file.</param>
        public void WriteLengthPrefixed(Action<BinaryWasmWriter> writeData)
        {
            using (var memStream = new MemoryStream())
            {
                var innerWriter = new BinaryWasmWriter(
                    new BinaryWriter(memStream),
                    StringEncoding);

                // Write the contents to the memory stream.
                writeData(innerWriter);

                // Save the number of bytes we've written.
                var numberOfBytes = memStream.Position;

                // Seek to the beginning of the memory stream.
                memStream.Seek(0, SeekOrigin.Begin);

                // Write the size of the contents to follow, in bytes.
                WriteVarUInt32((uint)numberOfBytes);

                // Write the memory stream's data to the writer's stream.
                Writer.Write(memStream.GetBuffer(), 0, (int)numberOfBytes);
            }
        }

        /// <summary>
        /// Writes a WebAssembly version header.
        /// </summary>
        /// <param name="header">The WebAssembly version header to write.</param>
        public void WriteVersionHeader(VersionHeader header)
        {
            Writer.Write(header.Magic);
            Writer.Write(header.Version);
        }

        /// <summary>
        /// Writes a WebAssembly section, including its header.
        /// </summary>
        /// <param name="value">The WebAssembly section to write.</param>
        public void WriteSection(Section value)
        {
            WriteVarInt7((sbyte)value.Name.Code);
            WriteLengthPrefixed(value.WriteCustomNameAndPayloadTo);
        }

        /// <summary>
        /// Writes a WebAssembly file.
        /// </summary>
        /// <param name="file">The WebAssembly file to write.</param>
        public void WriteFile(WasmFile file)
        {
            WriteVersionHeader(file.Header);
            foreach (var section in file.Sections)
            {
                WriteSection(section);
            }
        }
    }
}
