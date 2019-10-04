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
        /// <param name="ParameterTypes">The list of parameter types.</param>
        /// <param name="ReturnTypes">The list of return types.</param>
        /// <param name="Implementation">The delegate that implements the function definition.</param>
        public DelegateFunctionDefinition(
            IReadOnlyList<WasmValueType> ParameterTypes,
            IReadOnlyList<WasmValueType> ReturnTypes,
            Func<IReadOnlyList<object>, IReadOnlyList<object>> Implementation)
        {
            this.paramTypes = ParameterTypes;
            this.retTypes = ReturnTypes;
            this.Implementation = Implementation;
        }

        private IReadOnlyList<WasmValueType> paramTypes;
        private IReadOnlyList<WasmValueType> retTypes;

        /// <summary>
        /// Gets the delegate that implements this function definition.
        /// </summary>
        /// <returns>The delegate that implements this function definition.</returns>
        public Func<IReadOnlyList<object>, IReadOnlyList<object>> Implementation { get; private set; }

        /// <inheritdoc/>
        public override IReadOnlyList<WasmValueType> ParameterTypes => paramTypes;

        /// <inheritdoc/>
        public override IReadOnlyList<WasmValueType> ReturnTypes => retTypes;

        /// <inheritdoc/>
        public override IReadOnlyList<object> Invoke(IReadOnlyList<object> Arguments)
        {
            return Implementation(Arguments);
        }
    }
}