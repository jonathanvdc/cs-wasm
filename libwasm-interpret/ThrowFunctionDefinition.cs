using System;
using System.Collections.Generic;

namespace Wasm.Interpret
{
    /// <summary>
    /// Defines a type of function that throws an exception when invoked.
    /// </summary>
    public sealed class ThrowFunctionDefinition : FunctionDefinition
    {
        /// <summary>
        /// Creates a function definition from the given exception.
        /// </summary>
        /// <param name="ParameterTypes">The list of parameter types.</param>
        /// <param name="ReturnTypes">The list of return types.</param>
        /// <param name="ExceptionToThrow">The exception to throw.</param>
        public ThrowFunctionDefinition(
            IReadOnlyList<WasmValueType> ParameterTypes,
            IReadOnlyList<WasmValueType> ReturnTypes,
            Exception ExceptionToThrow)
        {
            this.paramTypes = ParameterTypes;
            this.retTypes = ReturnTypes;
            this.ExceptionToThrow = ExceptionToThrow;
        }

        private IReadOnlyList<WasmValueType> paramTypes;
        private IReadOnlyList<WasmValueType> retTypes;

        /// <inheritdoc/>
        public override IReadOnlyList<WasmValueType> ParameterTypes => paramTypes;

        /// <inheritdoc/>
        public override IReadOnlyList<WasmValueType> ReturnTypes => retTypes;

        /// <summary>
        /// Gets the exception to throw when this function is invoked.
        /// </summary>
        /// <returns>The exception to throw when this function is invoked.</returns>
        public Exception ExceptionToThrow { get; private set; }

        /// <inheritdoc/>
        public override IReadOnlyList<object> Invoke(IReadOnlyList<object> Arguments)
        {
            throw ExceptionToThrow;
        }
    }
}