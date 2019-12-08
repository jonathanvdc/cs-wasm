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
        /// <param name="parameterTypes">The list of parameter types.</param>
        /// <param name="returnTypes">The list of return types.</param>
        /// <param name="implementation">The delegate that implements the function definition.</param>
        public DelegateFunctionDefinition(
            IReadOnlyList<WasmValueType> parameterTypes,
            IReadOnlyList<WasmValueType> returnTypes,
            Func<IReadOnlyList<object>, IReadOnlyList<object>> implementation)
        {
            this.paramTypes = parameterTypes;
            this.retTypes = returnTypes;
            this.Implementation = implementation;
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
        public override IReadOnlyList<object> Invoke(IReadOnlyList<object> arguments, uint callStackDepth = 0)
        {
            return Implementation(arguments);
        }
    }
}
