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
        /// <param name="Limits">The table's limits.</param>
        public FunctionTable(ResizableLimits Limits)
        {
            this.Limits = Limits;
            this.contents = new List<FunctionDefinition>((int)Limits.Initial);
            var funcDef = new ThrowFunctionDefinition(new WasmException("Indirect call target not initialized yet."));
            for (int i = 0; i < Limits.Initial; i++)
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
        public FunctionDefinition this[uint Index]
        {
            get => contents[(int)Index];
            set => contents[(int)Index] = value;
        }
    }
}