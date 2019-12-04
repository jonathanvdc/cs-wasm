using System;
using System.Collections.Generic;
using System.IO;

namespace Wasm.Interpret
{
    /// <summary>
    /// Imports values from the 'spectest' environment.
    /// </summary>
    public sealed class SpecTestImporter : IImporter
    {
        /// <summary>
        /// Creates an importer for the 'spectest' environment.
        /// </summary>
        public SpecTestImporter()
            : this(Environment.NewLine)
        { }

        /// <summary>
        /// Creates an importer for the 'spectest' environment.
        /// </summary>
        /// <param name="printWriter">
        /// A text writer to use for print calls.
        /// </param>
        public SpecTestImporter(TextWriter printWriter)
            : this(printWriter.NewLine, printWriter)
        { }

        /// <summary>
        /// Creates an importer for the 'spectest' environment.
        /// </summary>
        /// <param name="printSuffix">
        /// A string that is written to the console at the
        /// end of a print call.
        /// </param>
        public SpecTestImporter(string printSuffix)
            : this(printSuffix, Console.Out)
        { }

        /// <summary>
        /// Creates an importer for the 'spectest' environment.
        /// </summary>
        /// <param name="printSuffix">
        /// A string that is written to <paramref name="printWriter"/> at the
        /// end of a print call.
        /// </param>
        /// <param name="printWriter">
        /// A text writer to use for print calls.
        /// </param>
        public SpecTestImporter(string printSuffix, TextWriter printWriter)
        {
            this.PrintSuffix = printSuffix;
            this.PrintWriter = printWriter;
            this.globalI32 = Variable.Create<int>(
                WasmValueType.Int32,
                false,
                666);
            this.globalF32 = Variable.Create<float>(
                WasmValueType.Float32,
                false,
                666.0f);
            this.globalF64 = Variable.Create<double>(
                WasmValueType.Float64,
                false,
                666.0);
        }

        /// <summary>
        /// Gets the string that is written to the console at the
        /// end of a print call.
        /// </summary>
        /// <returns>The print suffix.</returns>
        public string PrintSuffix { get; private set; }

        /// <summary>
        /// Gets the text writer that is used for print calls.
        /// </summary>
        /// <value>A text writer.</value>
        public TextWriter PrintWriter { get; private set; }

        private Variable globalI32, globalF32, globalF64;

        /// <inheritdoc/>
        public FunctionDefinition ImportFunction(
            ImportedFunction description, FunctionType signature)
        {
            switch (description.FieldName)
            {
                case "print":
                case "print_i32":
                case "print_i32_f32":
                case "print_f64_f64":
                case "print_f32":
                case "print_f64":
                    return new SpecTestPrintFunctionDefinition(
                        signature.ParameterTypes,
                        signature.ReturnTypes,
                        PrintSuffix,
                        PrintWriter);
                default:
                    return null;
            }
        }

        /// <inheritdoc/>
        public Variable ImportGlobal(ImportedGlobal description)
        {
            switch (description.FieldName)
            {
                case "global_i32":
                    return globalI32;
                case "global_f32":
                    return globalF32;
                case "global_f64":
                    return globalF64;
                default:
                    return null;
            }
        }

        /// <inheritdoc/>
        public LinearMemory ImportMemory(ImportedMemory description)
        {
            if (description.FieldName == "memory")
            {
                return new LinearMemory(new ResizableLimits(1, 2));
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public FunctionTable ImportTable(ImportedTable description)
        {
            if (description.FieldName == "table")
            {
                return new FunctionTable(new ResizableLimits(10, 20));
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// An implementation of the 'spectest.print' function.
    /// </summary>
    internal sealed class SpecTestPrintFunctionDefinition : FunctionDefinition
    {
        public SpecTestPrintFunctionDefinition(
            IReadOnlyList<WasmValueType> parameterTypes,
            IReadOnlyList<WasmValueType> returnTypes,
            string printSuffix,
            TextWriter printWriter)
        {
            this.paramTypes = parameterTypes;
            this.retTypes = returnTypes;
            this.PrintSuffix = printSuffix;
            this.PrintWriter = printWriter;
        }

        private IReadOnlyList<WasmValueType> paramTypes;
        private IReadOnlyList<WasmValueType> retTypes;

        /// <summary>
        /// Gets the string that is written to the console at the
        /// end of a print call.
        /// </summary>
        /// <returns>The print suffix.</returns>
        public string PrintSuffix { get; private set; }

        public TextWriter PrintWriter { get; private set; }

        /// <inheritdoc/>
        public override IReadOnlyList<WasmValueType> ParameterTypes => paramTypes;

        /// <inheritdoc/>
        public override IReadOnlyList<WasmValueType> ReturnTypes => retTypes;

        /// <inheritdoc/>
        public override IReadOnlyList<object> Invoke(IReadOnlyList<object> arguments)
        {
            for (int i = 0; i < arguments.Count; i++)
            {
                if (i > 0)
                {
                    PrintWriter.Write(" ");
                }
                PrintWriter.Write(arguments[i]);
            }
            PrintWriter.Write(PrintSuffix);

            var results = new object[ReturnTypes.Count];
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = Variable.GetDefaultValue(ReturnTypes[i]);
            }
            return results;
        }
    }
}