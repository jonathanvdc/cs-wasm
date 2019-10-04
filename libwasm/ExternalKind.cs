namespace Wasm
{
    /// <summary>
    /// A single-byte unsigned integer indicating the kind of definition being imported or defined.
    /// </summary>
    public enum ExternalKind : byte
    {
        /// <summary>
        /// Indicates a Function import or definition.
        /// </summary>
        Function = 0,

        /// <summary>
        /// Indicates a Table import or definition.
        /// </summary>
        Table = 1,

        /// <summary>
        /// Indicates a Memory import or definition.
        /// </summary>
        Memory = 2,

        /// <summary>
        /// Indicates a Global import or definition.
        /// </summary>
        Global = 3
    }
}