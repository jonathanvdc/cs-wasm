using System.Collections.Generic;

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
                new TrapException("Indirect call target not initialized yet.", TrapException.SpecMessages.UninitializedElement));
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
            get
            {
                CheckBounds(index);
                return contents[(int)index];
            }
            set
            {
                CheckBounds(index);
                contents[(int)index] = value;
            }
        }

        private void CheckBounds(uint index)
        {
            if (index >= contents.Count)
            {
                throw new TrapException(
                    $"Cannot access element with index {index} in a function table of size {contents.Count}.",
                    TrapException.SpecMessages.UndefinedElement);
            }
        }

        /// <summary>
        /// Gets the number of elements in the table.
        /// </summary>
        /// <returns>An element count.</returns>
        public int Count => contents.Count;
    }
}
