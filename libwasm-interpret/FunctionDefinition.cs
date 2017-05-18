using System.Collections.Generic;

namespace Wasm.Interpret
{
    /// <summary>
    /// Defines a base class for function definitions.
    /// </summary>
    public abstract class FunctionDefinition
    {
        /// <summary>
        /// Invokes this function with the given argument list.
        /// </summary>
        /// <param name="Arguments">The list of arguments for this function's parameters.</param>
        /// <returns>The list of return values.</returns>
        public abstract IReadOnlyList<object> Invoke(IReadOnlyList<object> Arguments);
    }
}