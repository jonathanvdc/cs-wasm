using System;
using System.CodeDom.Compiler;
using System.IO;

namespace Wasm
{
    /// <summary>
    /// Contains functions which help convert raw data to a human-readable format.
    /// </summary>
    public static class DumpHelpers
    {
        /// <summary>
        /// Formats the given value as a hexadecimal number.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>A hexadecimal number, prefixed by '0x'.</returns>
        public static string FormatHex(byte value)
        {
            return string.Format("0x{0:x02}", value);
        }

        /// <summary>
        /// Formats the given value as a hexadecimal number.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>A hexadecimal number, prefixed by '0x'.</returns>
        public static string FormatHex(ushort value)
        {
            return string.Format("0x{0:x04}", value);
        }

        /// <summary>
        /// Formats the given value as a hexadecimal number.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>A hexadecimal number, prefixed by '0x'.</returns>
        public static string FormatHex(uint value)
        {
            return string.Format("0x{0:x08}", value);
        }

        /// <summary>
        /// Writes the contents of the given stream to the given text writer,
        /// as a space-delimited list of hex bytes.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="writer">The writer to which text is written.</param>
        public static void DumpStream(Stream stream, TextWriter writer)
        {
            bool isFirst = true;
            while (true)
            {
                int b = stream.ReadByte();
                if (b == -1)
                    return;

                if (isFirst)
                    isFirst = false;
                else
                    writer.Write(" ");

                writer.Write(FormatHex((byte)b));
            }
        }

        /// <summary>
        /// Writes the contents of the byte array to the given text writer,
        /// as a space-delimited list of hex bytes.
        /// </summary>
        /// <param name="bytes">The bytes to print.</param>
        /// <param name="writer">The writer to which text is written.</param>
        public static void DumpBytes(byte[] bytes, TextWriter writer)
        {
            using (var memStream = new MemoryStream(bytes))
            {
                DumpStream(memStream, writer);
            }
        }

        /// <summary>
        /// Creates a string representation for the given WebAssembly type.
        /// </summary>
        /// <param name="value">The WebAssembly type to convert to a string.</param>
        /// <returns>A string representation for a WebAssembly type.</returns>
        public static string WasmTypeToString(WasmType value)
        {
            switch (value)
            {
                case WasmType.AnyFunc:
                    return "anyfunc";
                case WasmType.Empty:
                    return "empty";
                case WasmType.Float32:
                    return "f32";
                case WasmType.Float64:
                    return "f64";
                case WasmType.Func:
                    return "funcdef";
                case WasmType.Int32:
                    return "i32";
                case WasmType.Int64:
                    return "i64";
                default:
                    return "unknown type (code: " + value + ")";
            }
        }

        /// <summary>
        /// Creates a string representation for the given WebAssembly value type.
        /// </summary>
        /// <param name="value">The WebAssembly value type to convert to a string.</param>
        /// <returns>A string representation for a WebAssembly value type.</returns>
        public static string WasmTypeToString(WasmValueType value)
        {
            return WasmTypeToString((WasmType)value);
        }

        /// <summary>
        /// Writes a textual representation of the given WebAssembly type to
        /// the given text writer.
        /// </summary>
        /// <param name="value">The type to print to the text writer.</param>
        /// <param name="writer">The writer to which the textual WebAssembly value type should be written.</param>
        public static void DumpWasmType(WasmType value, TextWriter writer)
        {
            writer.Write(WasmTypeToString(value));
        }

        /// <summary>
        /// Writes a textual representation of the given WebAssembly value type to
        /// the given text writer.
        /// </summary>
        /// <param name="value">The value type to print to the text writer.</param>
        /// <param name="writer">The writer to which the textual WebAssembly value type should be written.</param>
        public static void DumpWasmType(WasmValueType value, TextWriter writer)
        {
            DumpWasmType((WasmType)value, writer);
        }

        /// <summary>
        /// Creates a text writer that prepends the given indentation string to every line.
        /// </summary>
        /// <param name="writer">The text writer to which the indented writer should write.</param>
        /// <param name="indentation">The indentation string.</param>
        /// <returns>A text writer that prepends the given indentation string to every line.</returns>
        public static TextWriter CreateIndentedTextWriter(TextWriter writer, string indentation)
        {
            var result = new IndentedTextWriter(writer, indentation);
            result.Indent = 1;
            return result;
        }

        /// <summary>
        /// Creates a text writer that prepends indentation string to every line.
        /// </summary>
        /// <param name="writer">The text writer to which the indented writer should write.</param>
        /// <returns>A text writer that prepends indentation to every line.</returns>
        public static TextWriter CreateIndentedTextWriter(TextWriter writer)
        {
            return CreateIndentedTextWriter(writer, "    ");
        }
    }
}
