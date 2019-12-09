using System;
using System.Runtime.Serialization;

namespace Wasm
{
    /// <summary>
    /// A type of exception that is thrown by the Wasm namespace and its sub-namespaces.
    /// </summary>
    [Serializable]
    public class WasmException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Wasm.WasmException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public WasmException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Wasm.WasmException"/> class.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">A streaming context.</param>
        protected WasmException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
