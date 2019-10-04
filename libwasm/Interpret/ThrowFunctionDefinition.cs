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
        /// <param name="parameterTypes">The list of parameter types.</param>
        /// <param name="returnTypes">The list of return types.</param>
        /// <param name="exceptionToThrow">The exception to throw.</param>
        public ThrowFunctionDefinition(
            IReadOnlyList<WasmValueType> parameterTypes,
            IReadOnlyList<WasmValueType> returnTypes,
            Exception exceptionToThrow)
        {
            this.paramTypes = parameterTypes;
            this.retTypes = returnTypes;
            this.ExceptionToThrow = exceptionToThrow;
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
        public override IReadOnlyList<object> Invoke(IReadOnlyList<object> arguments)
        {
            throw ExceptionToThrow;
        }
    }
}