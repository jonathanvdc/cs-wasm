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
        public FunctionDefinition ImportFunction(ImportedFunction Description)
        {
            if (Description.FieldName == "print")
            {
                return new DelegateFunctionDefinition(Print);
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

        private static IReadOnlyList<object> Print(IReadOnlyList<object> Args)
        {
            for (int i = 0; i < Args.Count; i++)
            {
                if (i > 0)
                {
                    Console.Write(" ");
                }
                Console.Write(Args[i]);
            }
            Console.WriteLine();
            return new object[0];
        }
    }
}