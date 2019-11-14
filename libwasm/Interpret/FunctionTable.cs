using System.Collections.Generic;
using System.Linq;

namespace Wasm.Interpret
{
    /// <summary>
    /// Defines a table of function values.
    /// </summary>
    public sealed class FunctionTable
    {
        /// <summary>
        /// Creates a function table from the given resizable limits.
        /// The table's initial contents are trap values.
        /// </summary>
        /// <param name="limits">The table's limits.</param>
        public FunctionTable(ResizableLimits limits)
        {
            this.Limits = limits;
            this.contents = new List<FunctionDefinition>((int)limits.Initial);
            var funcDef = new ThrowFunctionDefinition(
                new WasmValueType[0],
                new WasmValueType[0],
                new WasmException("Indirect call target not initialized yet."));
            for (int i = 0; i < limits.Initial; i++)
            {
                contents.Add(funcDef);
            }
        }

        /// <summary>
        /// Gets this function table's limits.
        /// </summary>
        /// <returns>The function table's limits.</returns>
        public ResizableLimits Limits { get; private set; }

        private List<FunctionDefinition> contents;

        /// <summary>
        /// Gets or sets the function definition at the given index in the table.
        /// </summary>
        public FunctionDefinition this[uint index]
        {
            get { return contents[(int)index]; }
            set { contents[(int)index] = value; }
        }

        /// <summary>
        /// Gets the number of elements in the table.
        /// </summary>
        /// <returns>An element count.</returns>
        public int Count => contents.Count;
    }
}
