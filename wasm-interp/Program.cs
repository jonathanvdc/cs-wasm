using System;
using System.Collections.Generic;
using Wasm.Interpret.BaseRuntime;

namespace Wasm.Interpret
{
    /// <summary>
    /// Represents interpreter command-line options.
    /// </summary>
    public struct InterpreterArguments
    {
        /// <summary>
        /// Gets a path to the WebAssembly file to load.
        /// </summary>
        public string WasmFilePath { get; private set; }

        /// <summary>
        /// Gets the name of the function to run, if any.
        /// </summary>
        public string FunctionToRun { get; private set; }

        /// <summary>
        /// Gets the name of the importer to use.
        /// </summary>
        /// <returns>The name of the importer.</returns>
        public string ImporterName { get; private set; }

        /// <summary>
        /// Gets the arguments for the function to run, if any.
        /// </summary>
        public object[] FunctionArgs { get; private set; }

        /// <summary>
        /// Tries to create an importer for these options.
        /// </summary>
        /// <param name="Result">The importer.</param>
        /// <returns><c>true</c> if <c>ImporterName</c> identifies an importer; otherwise, <c>false</c>.</returns>
        public bool TryGetImporter(out IImporter Result)
        {
            if (ImporterName == null
                || ImporterName.Equals("spectest", StringComparison.OrdinalIgnoreCase))
            {
                Result = new SpecTestImporter();
                return true;
            }
            else if (ImporterName.Equals("base-runtime", StringComparison.OrdinalIgnoreCase))
            {
                var importer = new PredefinedImporter();
                TerminalRuntime.IncludeDefinitionsIn(
                    Console.OpenStandardInput(),
                    Console.OpenStandardOutput(),
                    Console.OpenStandardError(),
                    importer);
                Result = importer;
                return true;
            }
            else
            {
                Result = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to read command-line options.
        /// </summary>
        public static bool TryRead(string[] Args, out InterpreterArguments ParsedArgs)
        {
            ParsedArgs = default(InterpreterArguments);
            bool expectingRunFuncName = false;
            bool expectingImporterName = false;
            bool expectingArgs = false;
            var funcArgs = new List<object>();
            for (int i = 0; i < Args.Length; i++)
            {
                if (expectingArgs)
                {
                    string argStr = Args[i];
                    if (argStr.EndsWith("l", StringComparison.OrdinalIgnoreCase))
                    {
                        long fArg;
                        if (!long.TryParse(argStr.Substring(0, argStr.Length - 1), out fArg))
                        {
                            return false;
                        }
                        funcArgs.Add(fArg);
                    }
                    else if (argStr.EndsWith("f", StringComparison.OrdinalIgnoreCase))
                    {
                        float fArg;
                        if (!float.TryParse(argStr.Substring(0, argStr.Length - 1), out fArg))
                        {
                            return false;
                        }
                        funcArgs.Add(fArg);
                    }
                    else
                    {
                        int intFArg;
                        double doubleFArg;
                        uint uintFArg;
                        if (int.TryParse(argStr, out intFArg))
                        {
                            funcArgs.Add(intFArg);
                        }
                        else if (uint.TryParse(argStr, out uintFArg))
                        {
                            funcArgs.Add((int)uintFArg);
                        }
                        else
                        {
                            if (!double.TryParse(argStr, out doubleFArg))
                            {
                                return false;
                            }
                            funcArgs.Add(doubleFArg);
                        }
                    }
                }
                else if (expectingRunFuncName)
                {
                    if (ParsedArgs.FunctionToRun != null)
                    {
                        return false;
                    }

                    ParsedArgs.FunctionToRun = Args[i];
                    expectingRunFuncName = false;
                    expectingArgs = true;
                }
                else if (expectingImporterName)
                {
                    if (ParsedArgs.ImporterName != null)
                    {
                        return false;
                    }

                    ParsedArgs.ImporterName = Args[i];
                    expectingImporterName = false;
                }
                else if (Args[i] == "--run")
                {
                    expectingRunFuncName = true;
                }
                else if (Args[i] == "--importer")
                {
                    expectingImporterName = true;
                }
                else
                {
                    if (ParsedArgs.WasmFilePath != null)
                    {
                        return false;
                    }

                    ParsedArgs.WasmFilePath = Args[i];
                }
            }

            ParsedArgs.FunctionArgs = funcArgs.ToArray();

            return ParsedArgs.WasmFilePath != null
                && !expectingRunFuncName
                && !expectingImporterName;
        }
    }

    public static class Program
    {
        private static int PrintUsage()
        {
            Console.Error.WriteLine("usage: wasm-interp file.wasm [--importer spectest|base-runtime] [--run exported_func_name [args...]]");
            return 1;
        }

        public static int Main(string[] args)
        {
            // Read command-line arguments.
            InterpreterArguments parsedArgs;
            if (!InterpreterArguments.TryRead(args, out parsedArgs))
            {
                return PrintUsage();
            }

            IImporter importer;
            if (!parsedArgs.TryGetImporter(out importer))
            {
                Console.Error.WriteLine("error: there is no importer named '" + parsedArgs.ImporterName + "'");
                return 1;
            }

            // Read and instantiate the module.
            var wasmFile = WasmFile.ReadBinary(parsedArgs.WasmFilePath);
            var module = ModuleInstance.Instantiate(wasmFile, importer);

            // Figure out which function to run.
            FunctionDefinition funcToRun = null;
            if (parsedArgs.FunctionToRun != null)
            {
                if (!module.ExportedFunctions.TryGetValue(parsedArgs.FunctionToRun, out funcToRun))
                {
                    Console.Error.WriteLine(
                        "error: module does not export a function named '" +
                        parsedArgs.FunctionToRun + "'");
                    return 1;
                }
            }
            else
            {
                var startSec = wasmFile.GetFirstSectionOrNull<StartSection>();
                if (startSec == null)
                {
                    Console.Error.WriteLine(
                        "error: module does not define a 'start' section " +
                        " and '--run exported_func_name' was not specified.");
                    return 1;
                }
                else
                {
                    IReadOnlyList<FunctionDefinition> funcs = module.Functions;
                    funcToRun = funcs[(int)startSec.StartFunctionIndex];
                }
            }

            // Run that function.
            int exitCode = 0;
            try
            {
                IReadOnlyList<object> output = funcToRun.Invoke(parsedArgs.FunctionArgs);
                if (output.Count > 0)
                {
                    for (int i = 0; i < output.Count; i++)
                    {
                        if (i > 0)
                        {
                            Console.Write(" ");
                        }
                        Console.Write(output[i]);
                    }
                    Console.WriteLine();
                }
            }
            catch (WasmException ex)
            {
                Console.Error.WriteLine("error: " + ex.Message);
                exitCode = 1;
            }
            return exitCode;
        }
    }
}
