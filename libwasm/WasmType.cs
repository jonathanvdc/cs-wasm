namespace Wasm
{
    /// <summary>
    /// An enumeration of WebAssembly language types.
    /// </summary>
    public enum WasmType : sbyte
    {
        /// <summary>
        /// A 32-bit integer type.
        /// </summary>
        Int32 = -0x01,

        /// <summary>
        /// A 64-bit integer type.
        /// </summary>
        Int64 = -0x02,

        /// <summary>
        /// A 32-bit floating-point type.
        /// </summary>
        Float32 = -0x03,

        /// <summary>
        /// A 64-bit floating-point type.
        /// </summary>
        Float64 = -0x04,

        /// <summary>
        /// A pointer to a function of any type.
        /// </summary>
        AnyFunc = -0x10,

        /// <summary>
        /// The type of function declarations.
        /// </summary>
        Func = -0x20,

        /// <summary>
        /// A pseudo-type for representing an empty block type.
        /// </summary>
        Empty = -0x40
    }

    /// <summary>
    /// An enumeration of WebAssembly value types.
    /// </summary>
    public enum WasmValueType : sbyte
    {
        /// <summary>
        /// A 32-bit integer type.
        /// </summary>
        Int32 = -0x01,

        /// <summary>
        /// A 64-bit integer type.
        /// </summary>
        Int64 = -0x02,

        /// <summary>
        /// A 32-bit floating-point type.
        /// </summary>
        Float32 = -0x03,

        /// <summary>
        /// A 64-bit floating-point type.
        /// </summary>
        Float64 = -0x04
    }
}