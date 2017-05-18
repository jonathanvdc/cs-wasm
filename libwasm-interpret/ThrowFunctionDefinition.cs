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
        /// Creates a new throwing function definition from the given exception.
        /// </summary>
        /// <param name="ExceptionToThrow">The exception to throw.</param>
        public ThrowFunctionDefinition(Exception ExceptionToThrow)
        {
            this.ExceptionToThrow = ExceptionToThrow;
        }

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