using System;

namespace Wasm
{
    /// <summary>
    /// A type of exception that is thrown by the Wasm namespace and its sub-namespaces.
    /// </summary>
    public class WasmException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Wasm.WasmException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public WasmException(string message)
            : base(message)
        { }
    }
}
