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

            var file = WasmFile.ReadBinary(args[0]);
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
