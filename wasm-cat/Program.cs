using System;
using System.Collections.Generic;
using System.IO;

namespace Wasm.Cat
{
    // wasm-cat takes WebAssembly files as input and concatenates their sections.

    public struct CatArgs
    {
        public string Output { get; set; }

        public IEnumerable<string> Inputs { get; set; }

        public static bool TryParse(string[] Args, out CatArgs Result)
        {
            Result = default(CatArgs);
            if (Args.Length == 0)
            {
                return false;
            }

            var inputs = new List<string>();
            for (int i = 0; i < Args.Length; i++)
            {
                if (Args[i] == "-o")
                {
                    if (i == Args.Length - 1)
                    {
                        return false;
                    }
                    i++;
                    Result.Output = Args[i];
                }
                else
                {
                    inputs.Add(Args[i]);
                }
            }
            Result.Inputs = inputs;
            return true;
        }
    }

    public static class Program
    {
        public static int Main(string[] args)
        {
            CatArgs parsedArgs;
            if (!CatArgs.TryParse(args, out parsedArgs))
            {
                Console.Error.WriteLine("usage: wasm-cat file.wasm... [-o output.wasm]");
                return 1;
            }

            var file = new WasmFile();
            foreach (var path in parsedArgs.Inputs)
            {
                // Read the file and append its sections to the resulting file.
                var inputFile = WasmFile.ReadBinary(path);
                file.Sections.AddRange(inputFile.Sections);

                // Also, set the WebAssembly version number to the max of the
                // input files.
                if (inputFile.Header.Version > file.Header.Version)
                {
                    file.Header = inputFile.Header;
                }
            }

            // Now write the file to standard output.
            using (var outputStream = string.IsNullOrEmpty(parsedArgs.Output)
                ? Console.OpenStandardOutput()
                : File.OpenWrite(parsedArgs.Output))
            {
                file.WriteBinaryTo(outputStream);
            }
            return 0;
        }
    }
}
