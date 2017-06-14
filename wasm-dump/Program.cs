using System;
using System.IO;
using Wasm.Binary;

namespace Wasm.Dump
{
    public static class Program
    {
        private static MemoryStream ReadStdinToEnd()
        {
            // Based on Marc Gravell's answer to this StackOverflow question:
            // https://stackoverflow.com/questions/1562417/read-binary-data-from-console-in
            var memStream = new MemoryStream();
            using (var stdin = Console.OpenStandardInput())
            {
                byte[] buffer = new byte[2048];
                int bytes;
                while ((bytes = stdin.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memStream.Write(buffer, 0, bytes);
                }
            }
            memStream.Seek(0, SeekOrigin.Begin);
            return memStream;
        }

        public static int Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.Error.WriteLine("usage: wasm-dump [file.wasm]");
                return 1;
            }

            var memStream = new MemoryStream();

            WasmFile file;
            if (args.Length == 0)
            {
                using (var input = ReadStdinToEnd())
                {
                    file = WasmFile.ReadBinary(input);
                }
            }
            else
            {
                file = WasmFile.ReadBinary(args[0]);
            }
            file.Dump(Console.Out);
            Console.WriteLine();
            return 0;
        }
    }
}
