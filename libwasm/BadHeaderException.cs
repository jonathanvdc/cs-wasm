using System;

namespace Wasm
{
    /// <summary>
    /// The type of exception that is thrown when an invalid header is detected.
    /// </summary>
    public sealed class BadHeaderException : WasmException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BadHeaderException"/> class.
        /// </summary>
        /// <param name="header">The version header.</param>
        /// <param name="message">The error message.</param>
        public BadHeaderException(VersionHeader header, string message)
            : base(message)
        {
            this.Header = header;
        }

        /// <summary>
        /// Gets the erroneous version header.
        /// </summary>
        /// <value>The version header.</value>
        public VersionHeader Header { get; private set; }
    }
}

