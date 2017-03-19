using System;
using System.IO;
using Wasm.Binary;

namespace Wasm.Dump
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
                Console.WriteLine("usage: wasm-dump file.wasm");

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
        }

        public static void DumpFile(WasmFile ParsedFile)
        {
            foreach (var section in ParsedFile.Sections)
            {
                Console.WriteLine(section.Name.ToString());
            }
        }
    }
}
