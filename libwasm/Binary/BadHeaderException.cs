using System;

namespace Wasm.Binary
{
    /// <summary>
    /// The type of exception that is thrown when an invalid header is detected.
    /// </summary>
    public sealed class BadHeaderException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Wasm.Binary.BadHeaderException"/> class.
        /// </summary>
        /// <param name="Header">The version header.</param>
        /// <param name="Message">The error message.</param>
        public BadHeaderException(VersionHeader Header, string Message)
            : base(Message)
        {
            this.Header = Header;
        }

        /// <summary>
        /// Gets the erroneous version header.
        /// </summary>
        /// <value>The version header.</value>
        public VersionHeader Header { get; private set; }
    }
}

