using System;
using System.Collections.Generic;

namespace Wasm.Interpret
{
    /// <summary>
    /// Defines a function definition implementation that calls a delegate when invoked.
    /// </summary>
    public sealed class DelegateFunctionDefinition : FunctionDefinition
    {
        /// <summary>
        /// Creates a function definition from the given delegate.
        /// </summary>
        /// <param name="Implementation">The delegate that implements the function definition.</param>
        public DelegateFunctionDefinition(
            Func<IReadOnlyList<object>, IReadOnlyList<object>> Implementation)
        {
            this.Implementation = Implementation;
        }

        /// <summary>
        /// Gets the delegate that implements this function definition.
        /// </summary>
        /// <returns>The delegate that implements this function definition.</returns>
        public Func<IReadOnlyList<object>, IReadOnlyList<object>> Implementation { get; private set; }

        /// <inheritdoc/>
        public override IReadOnlyList<object> Invoke(IReadOnlyList<object> Arguments)
        {
            return Implementation(Arguments);
        }
    }
}