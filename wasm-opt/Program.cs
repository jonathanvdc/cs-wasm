using System;
using System.Collections.Generic;
using System.IO;
using Wasm.Binary;
using Wasm.Optimize;

namespace Wasm.Opt
{
    // wasm-opt takes WebAssembly takes a single WebAssembly file as input and optimizes it.
    public struct OptArgs
    {
        public string Output { get; set; }

        public string Input { get; set; }

        public static bool TryParse(string[] Args, out OptArgs Result)
        {
            Result = default(OptArgs);
            if (Args.Length == 0)
            {
                return false;
            }

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
                    if (Result.Input != null)
                    {
                        return false;
                    }
                    Result.Input = Args[i];
                }
            }
            return true;
        }
    }

    public static class Program
    {
        public static int Main(string[] args)
        {
            OptArgs parsedArgs;
            if (!OptArgs.TryParse(args, out parsedArgs))
            {
                Console.Error.WriteLine("usage: wasm-opt file.wasm [-o output.wasm]");
                return 1;
            }

            // Read the file.
            var file = WasmFile.ReadBinary(parsedArgs.Input);
            file.Optimize();

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
