using System;
using System.Collections.Generic;

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
        /// Creates an importer for the 'spectest' environment with
        /// the given print suffix.
        /// </summary>
        /// <param name="printSuffix">
        /// The string that is written to the console at the
        /// end of a print call.
        /// </param>
        public SpecTestImporter(string printSuffix)
        {
            this.PrintSuffix = printSuffix;
        }

        /// <summary>
        /// Gets the string that is written to the console at the
        /// end of a print call.
        /// </summary>
        /// <returns>The print suffix.</returns>
        public string PrintSuffix { get; private set; }

        /// <inheritdoc/>
        public FunctionDefinition ImportFunction(
            ImportedFunction description, FunctionType signature)
        {
            if (description.FieldName == "print")
            {
                return new SpecTestPrintFunctionDefinition(
                    signature.ParameterTypes,
                    signature.ReturnTypes,
                    PrintSuffix);
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public Variable ImportGlobal(ImportedGlobal description)
        {
            if (description.FieldName == "global")
            {
                switch (description.Global.ContentType)
                {
                    case WasmValueType.Int32:
                        return Variable.Create<int>(
                            WasmValueType.Int32,
                            description.Global.IsMutable,
                            666);

                    case WasmValueType.Int64:
                        return Variable.Create<long>(
                            WasmValueType.Int64,
                            description.Global.IsMutable,
                            666L);

                    case WasmValueType.Float32:
                        return Variable.Create<float>(
                            WasmValueType.Float32,
                            description.Global.IsMutable,
                            666.0f);

                    case WasmValueType.Float64:
                        return Variable.Create<double>(
                            WasmValueType.Float64,
                            description.Global.IsMutable,
                            666.0);

                    default:
                        return null;
                }
            }
            else
            {
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
            string PrintSuffix)
        {
            this.paramTypes = parameterTypes;
            this.retTypes = returnTypes;
            this.PrintSuffix = PrintSuffix;
        }

        private IReadOnlyList<WasmValueType> paramTypes;
        private IReadOnlyList<WasmValueType> retTypes;

        /// <summary>
        /// Gets the string that is written to the console at the
        /// end of a print call.
        /// </summary>
        /// <returns>The print suffix.</returns>
        public string PrintSuffix { get; private set; }

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
                    Console.Write(" ");
                }
                Console.Write(arguments[i]);
            }
            Console.Write(PrintSuffix);

            var results = new object[ReturnTypes.Count];
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = Variable.GetDefaultValue(ReturnTypes[i]);
            }
            return results;
        }
    }
}