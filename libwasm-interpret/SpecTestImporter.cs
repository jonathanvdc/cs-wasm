using System;
using System.Collections.Generic;

namespace Wasm.Interpret
{
    /// <summary>
    /// Imports values from the 'spectest' environment.
    /// </summary>
    public sealed class SpecTestImporter : IImporter
    {
        /// <inheritdoc/>
        public FunctionDefinition ImportFunction(
            ImportedFunction Description, FunctionType Signature)
        {
            if (Description.FieldName == "print")
            {
                return new SpecTestPrintFunctionDefinition(
                    Signature.ParameterTypes,
                    Signature.ReturnTypes);
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public Variable ImportGlobal(ImportedGlobal Description)
        {
            if (Description.FieldName == "global")
            {
                switch (Description.Global.ContentType)
                {
                    case WasmValueType.Int32:
                        return Variable.Create<int>(
                            WasmValueType.Int32,
                            Description.Global.IsMutable,
                            666);

                    case WasmValueType.Int64:
                        return Variable.Create<long>(
                            WasmValueType.Int64,
                            Description.Global.IsMutable,
                            666L);

                    case WasmValueType.Float32:
                        return Variable.Create<float>(
                            WasmValueType.Float32,
                            Description.Global.IsMutable,
                            666.0f);

                    case WasmValueType.Float64:
                        return Variable.Create<double>(
                            WasmValueType.Float64,
                            Description.Global.IsMutable,
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
        public LinearMemory ImportMemory(ImportedMemory Description)
        {
            if (Description.FieldName == "memory")
            {
                return new LinearMemory(new ResizableLimits(1, 2));
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public FunctionTable ImportTable(ImportedTable Description)
        {
            if (Description.FieldName == "table")
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
            IReadOnlyList<WasmValueType> ParameterTypes,
            IReadOnlyList<WasmValueType> ReturnTypes)
        {
            this.paramTypes = ParameterTypes;
            this.retTypes = ReturnTypes;
        }

        private IReadOnlyList<WasmValueType> paramTypes;
        private IReadOnlyList<WasmValueType> retTypes;

        /// <inheritdoc/>
        public override IReadOnlyList<WasmValueType> ParameterTypes => paramTypes;

        /// <inheritdoc/>
        public override IReadOnlyList<WasmValueType> ReturnTypes => retTypes;

        /// <inheritdoc/>
        public override IReadOnlyList<object> Invoke(IReadOnlyList<object> Arguments)
        {
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (i > 0)
                {
                    Console.Write(" ");
                }
                Console.Write(Arguments[i]);
            }
            Console.WriteLine();

            var results = new object[ReturnTypes.Count];
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = Variable.GetDefaultValue(ReturnTypes[i]);
            }
            return results;
        }
    }
}