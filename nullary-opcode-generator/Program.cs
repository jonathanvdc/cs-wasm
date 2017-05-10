using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Wasm.Instructions;

namespace Wasm.NullaryOpCodeGenerator
{
    public static class Program
    {
        private static int PrintUsage()
        {
            Console.Error.WriteLine("usage: nullary-opcode-generator gen-init|gen-defs nullary-opcode-defs.txt nullary-opcode-docs.txt");
            Console.Error.WriteLine();
            Console.Error.WriteLine("    gen-init: generates initialization code for nullary opcodes.");
            Console.Error.WriteLine("    gen-defs: generates field definition code for nullary opcodes.");
            Console.Error.WriteLine("    nullary-opcode-defs.txt: a file that contains whitespace-separated (mnemonic, opcode) pairs.");
            Console.Error.WriteLine("    nullary-opcode-docs.txt: a file that contains colon-separated (mnemonic, documentation) pairs.");
            return 1;
        }

        public static int Main(string[] args)
        {
            // This program generates C# code from (potentially large) tables of
            // nullary opcode docs and definitions.
            // The resulting code is then to be included manually in libwasm/Operators.cs.

            if (args.Length != 3)
            {
                return PrintUsage();
            }

            bool genInit = false;
            if (args[0] == "gen-init")
            {
                genInit = true;
            }
            else if (args[0] != "gen-defs")
            {
                return PrintUsage();
            }

            var defLines = File.ReadAllLines(args[1]);
            var docLines = File.ReadAllLines(args[2]);

            var opCodes = new Dictionary<string, string>();
            var opDocs = new Dictionary<string, string>();

            foreach (var line in docLines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    string[] splitLine = line.Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    opDocs.Add(splitLine[0], splitLine[1].Trim());
                }
            }

            foreach (var line in defLines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    string[] splitLine = line.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    opCodes.Add(splitLine[0], splitLine[1].Trim());
                }
            }

            if (genInit)
            {
                foreach (var pair in opCodes)
                {
                    var op = ParseMnemonic(pair.Key);
                    Console.WriteLine(
                        "{0} = Register<NullaryOperator>(new NullaryOperator({1}, WasmType.{2}, \"{3}\"));",
                        GenerateFieldName(op),
                        pair.Value,
                        WasmTypeToIdentifier(op.DeclaringType),
                        op.Mnemonic);
                }
            }
            else
            {
                foreach (var pair in opCodes)
                {
                    var op = ParseMnemonic(pair.Key);
                    Console.WriteLine("/// <summary>");
                    Console.WriteLine(
                        "/// The '{0}' operator: {1}",
                        op.ToString(), opDocs.ContainsKey(pair.Key)
                            ? opDocs[pair.Key] + "."
                            : "");
                    Console.WriteLine("/// </summary>");
                    Console.WriteLine("public static readonly NullaryOperator {0};", GenerateFieldName(op));
                    Console.WriteLine();
                }
            }

            return 0;
        }

        private static NullaryOperator ParseMnemonic(string FullMnemonic)
        {
            string[] split = FullMnemonic.Split(new char[] { '.' }, 2);
            if (split.Length == 1)
            {
                return new NullaryOperator(0, WasmType.Empty, split[0]);
            }
            else
            {
                return new NullaryOperator(0, ParseWasmType(split[0]), split[1]);
            }
        }

        private static string GenerateFieldName(NullaryOperator Op)
        {
            if (Op.HasDeclaringType)
            {
                return WasmTypeToIdentifier(Op.DeclaringType) + MnemonicToIdentifier(Op.Mnemonic);
            }
            else
            {
                return MnemonicToIdentifier(Op.Mnemonic);
            }
        }

        private static string MnemonicToIdentifier(string Mnemonic)
        {
            var result = new StringBuilder();
            bool useCaps = true;
            int i = 0;
            foreach (var character in Mnemonic)
            {
                i++;
                if (character == '_')
                {
                    useCaps = true;
                    continue;
                }
                else if (character == '/')
                {
                    string suffixType = Mnemonic.Substring(i);
                    result.Append(WasmTypeToIdentifier(ParseWasmType(suffixType)));
                    break;
                }

                if (useCaps)
                {
                    result.Append(char.ToUpper(character));
                    useCaps = false;
                }
                else
                {
                    result.Append(character);
                }
            }
            return result.ToString();
        }

        private static WasmType ParseWasmType(string Type)
        {
            switch (Type)
            {
                case "i32":
                    return WasmType.Int32;
                case "i64":
                    return WasmType.Int64;
                case "f32":
                    return WasmType.Float32;
                case "f64":
                    return WasmType.Float64;
                default:
                    throw new InvalidOperationException("Unknown WasmType: '" + Type + "'");
            }
        }

        private static string WasmTypeToIdentifier(WasmType Type)
        {
            return ((object)Type).ToString();
        }
    }
}