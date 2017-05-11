using System;
using System.IO;
using Wasm.Binary;

namespace Wasm.Cat
{
    // wasm-cat takes WebAssembly files as input and concatenates their sections.

    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("usage: wasm-cat file.wasm...");
                return 1;
            }

            var file = new WasmFile();
            foreach (var path in args)
            {
                using (var fileStream = File.OpenRead(path))
                {
                    using (var reader = new BinaryReader(fileStream))
                    {
                        // Create a WebAssembly reader, read the file and append its
                        // sections to the resulting file.
                        var wasmReader = new BinaryWasmReader(reader);
                        file.Sections.AddRange(wasmReader.ReadFile().Sections);
                    }
                }
            }

            // Now write the file to standard output.
            using (var outputStream = Console.OpenStandardOutput())
            {
                using (var writer = new BinaryWriter(outputStream))
                {
                    var wasmWriter = new BinaryWasmWriter(writer);
                    wasmWriter.WriteFile(file);
                }
            }
            return 0;
        }
    }
}
