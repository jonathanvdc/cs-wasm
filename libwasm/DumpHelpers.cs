using System;
using System.IO;

namespace Wasm
{
    /// <summary>
    /// Contains functions which help convert raw data to a human-readable format.
    /// </summary>
    public static class DumpHelpers
    {
        /// <summary>
        /// Writes the contents of the given stream to the given text writer,
        /// as a space-delimited list of hex bytes.
        /// </summary>
        /// <param name="Stream">The stream to read.</param>
        /// <param name="Writer">The writer to which text is written.</param>
        public static void DumpStream(Stream Stream, TextWriter Writer)
        {
            for (long i = 0; i < Stream.Length; i++)
            {
                if (i > 0)
                    Writer.Write(" ");

                Writer.Write("{0:X02}", Stream.ReadByte());
            }
        }

        /// <summary>
        /// Writes the contents of the byte array to the given text writer,
        /// as a space-delimited list of hex bytes.
        /// </summary>
        /// <param name="Bytes">The bytes to print.</param>
        /// <param name="Writer">The writer to which text is written.</param>
        public static void DumpBytes(byte[] Bytes, TextWriter Writer)
        {
            using (var memStream = new MemoryStream(Bytes))
            {
                DumpStream(memStream, Writer);
            }
        }

        /// <summary>
        /// Creates a string representation for the given WebAssembly type.
        /// </summary>
        /// <param name="Value">The WebAssembly type to convert to a string.</param>
        /// <returns>A string representation for a WebAssembly type.</returns>
        public static string WasmTypeToString(WasmType Value)
        {
            switch (Value)
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
                    return "unknown type (code: " + Value + ")";
            }
        }

        /// <summary>
        /// Creates a string representation for the given WebAssembly value type.
        /// </summary>
        /// <param name="Value">The WebAssembly value type to convert to a string.</param>
        /// <returns>A string representation for a WebAssembly value type.</returns>
        public static string WasmTypeToString(WasmValueType Value)
        {
            return WasmTypeToString((WasmType)Value);
        }

        /// <summary>
        /// Writes a textual representation of the given WebAssembly type to
        /// the given text writer.
        /// </summary>
        /// <param name="Value">The type to print to the text writer.</param>
        /// <param name="Writer">The writer to which the textual WebAssembly value type should be written.</param>
        public static void DumpWasmType(WasmType Value, TextWriter Writer)
        {
            Writer.Write(WasmTypeToString(Value));
        }

        /// <summary>
        /// Writes a textual representation of the given WebAssembly value type to
        /// the given text writer.
        /// </summary>
        /// <param name="Value">The value type to print to the text writer.</param>
        /// <param name="Writer">The writer to which the textual WebAssembly value type should be written.</param>
        public static void DumpWasmType(WasmValueType Value, TextWriter Writer)
        {
            DumpWasmType((WasmType)Value, Writer);
        }
    }
}