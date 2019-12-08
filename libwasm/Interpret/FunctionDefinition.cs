using System.Collections.Generic;

namespace Wasm.Interpret
{
    /// <summary>
    /// Defines a base class for function definitions.
    /// </summary>
    public abstract class FunctionDefinition
    {
        /// <summary>
        /// Gets this function definition's list of parameter types.
        /// </summary>
        /// <returns>The list of parameter types.</returns>
        public abstract IReadOnlyList<WasmValueType> ParameterTypes { get; }

        /// <summary>
        /// Gets this function definition's list of return types.
        /// </summary>
        /// <returns>The list of return types.</returns>
        public abstract IReadOnlyList<WasmValueType> ReturnTypes { get; }

        /// <summary>
        /// Invokes this function with the given argument list.
        /// </summary>
        /// <param name="arguments">The list of arguments for this function's parameters.</param>
        /// <param name="callStackDepth">
        /// The depth of the call stack when the function is invoked, excluding the function's stack frame.
        /// </param>
        /// <returns>The list of return values.</returns>
        public abstract IReadOnlyList<object> Invoke(IReadOnlyList<object> arguments, uint callStackDepth = 0);
    }
}
