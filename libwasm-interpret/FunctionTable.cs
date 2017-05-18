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
        /// Creates an empty function table.
        /// </summary>
        public FunctionTable()
            : this(Enumerable.Empty<FunctionDefinition>())
        { }

        /// <summary>
        /// Creates a function table from the given list of function definitions.
        /// </summary>
        /// <param name="Contents">The function table's contents.</param>
        public FunctionTable(IEnumerable<FunctionDefinition> Contents)
        {
            this.contents = new List<FunctionDefinition>(Contents);
        }

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