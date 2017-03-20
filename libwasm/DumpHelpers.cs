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
    }
}