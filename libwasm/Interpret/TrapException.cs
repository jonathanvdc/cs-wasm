using System;
using System.Runtime.Serialization;

namespace Wasm.Interpret
{
    /// <summary>
    /// A WebAssembly exception that is thrown when WebAssembly execution traps.
    /// </summary>
    [Serializable]
    public class TrapException : WasmException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrapException"/> class.
        /// </summary>
        /// <param name="message">A user-friendly error message.</param>
        /// <param name="specMessage">A spec-mandated generic error message.</param>
        public TrapException(string message, string specMessage) : base(message)
        {
            this.SpecMessage = specMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrapException"/> class.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">A streaming context.</param>
        protected TrapException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Gets the generic error message mandated by the spec, as opposed to the possibly
        /// more helpful message encapsulated in the exception itself.
        /// </summary>
        /// <value>A spec error message.</value>
        public string SpecMessage { get; private set; }

        /// <summary>
        /// A collection of generic spec error messages for traps.
        /// </summary>
        public static class SpecMessages
        {
            /// <summary>
            /// The error message for out of bounds memory accesses.
            /// </summary>
            public const string OutOfBoundsMemoryAccess = "out of bounds memory access";

            /// <summary>
            /// The error message for when an unreachable instruction is reached.
            /// </summary>
            public const string Unreachable = "unreachable";

            /// <summary>
            /// The error message for when the max execution stack depth is exceeded.
            /// </summary>
            public const string CallStackExhausted = "call stack exhausted";

            /// <summary>
            /// The error message for when integer overflow occurs.
            /// </summary>
            public const string IntegerOverflow = "integer overflow";

            /// <summary>
            /// The error message for when NaN is converted to an integer.
            /// </summary>
            public const string InvalidConversionToInteger = "invalid conversion to integer";

            /// <summary>
            /// The error message for misaligned memory accesses.
            /// </summary>
            public const string MisalignedMemoryAccess = "misaligned memory access";

            /// <summary>
            /// The error message for when an indirect call's expected type does not match
            /// the actual type of the function being called.
            /// </summary>
            public const string IndirectCallTypeMismatch = "indirect call type mismatch";

            /// <summary>
            /// The error message for when an integer is divided by zero.
            /// </summary>
            public const string IntegerDivideByZero = "integer divide by zero";

            /// <summary>
            /// The error message for when an undefined element of a table is accessed.
            /// </summary>
            public const string UndefinedElement = "undefined element";

            /// <summary>
            /// The error message for when an uninitialized element of a table is accessed.
            /// </summary>
            public const string UninitializedElement = "uninitialized element";
        }
    }
}
