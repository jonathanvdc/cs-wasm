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
        /// <param name="Writer">The binary writer for a WebAssembly file.</param>
        public BinaryWasmWriter(BinaryWriter Writer)
            : this(Writer, UTF8Encoding.UTF8)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Wasm.Binary.BinaryWasmWriter"/> class.
        /// </summary>
        /// <param name="Writer">The binary writer for a WebAssembly file.</param>
        /// <param name="StringEncoding">The encoding for strings in the WebAssembly file.</param>
        public BinaryWasmWriter(BinaryWriter Writer, Encoding StringEncoding)
        {
            this.Writer = Writer;
            this.StringEncoding = StringEncoding;
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
        public int WriteVarUInt64(ulong Value)
        {
            // C# translation of code borrowed from Wikipedia article:
            // https://en.wikipedia.org/wiki/LEB128

            int count = 0;
            do {
                byte b = (byte)(Value & 0x7F);
                Value >>= 7;
                if (Value != 0)
                    b |= 0x80;

                Writer.Write(b);
                count++;
            } while (Value != 0);
            return count;
        }

        /// <summary>
        /// Writes an unsigned LEB128 variable-length integer, limited to 32 bits.
        /// </summary>
        /// <returns>The number of bytes used to encode the integer.</returns>
        public int WriteVarUInt32(uint Value)
        {
            return WriteVarUInt64(Value);
        }

        /// <summary>
        /// Writes a length-prefixed string to the WebAssembly file.
        /// </summary>
        /// <param name="Value">The string to write to the file.</param>
        public void WriteString(string Value)
        {
            byte[] buffer = StringEncoding.GetBytes(Value);
            WriteVarUInt32((uint)buffer.Length);
            Writer.Write(buffer);
        }
    }
}

