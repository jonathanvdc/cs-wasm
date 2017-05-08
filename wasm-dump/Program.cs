using System;
using System.IO;
using Wasm.Binary;

namespace Wasm.Dump
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("usage: wasm-dump file.wasm");
                return 1;
            }

            WasmFile file;
            using (var fileStream = File.OpenRead(args[0]))
            {
                using (var reader = new BinaryReader(fileStream))
                {
                    // Create a WebAssembly reader and read the file.
                    var wasmReader = new BinaryWasmReader(reader);
                    file = wasmReader.ReadFile();
                }
            }
            DumpFile(file);
            return 0;
        }

        public static void DumpFile(WasmFile ParsedFile)
        {
            foreach (var section in ParsedFile.Sections)
            {
                section.Dump(Console.Out);
                Console.WriteLine();
            }
        }
    }
}
