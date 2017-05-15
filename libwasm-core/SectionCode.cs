namespace Wasm
{
    /// <summary>
    /// /// Enumerates possible section codes.
    /// </summary>
    public enum SectionCode
    {
        /// <summary>
        /// The section code for custom sections.
        /// </summary>
        Custom = 0,

        /// <summary>
        /// The section code for function signature declarations.
        /// </summary>
        Type = 1,

        /// <summary>
        /// The section code for import declarations.
        /// </summary>
        Import = 2,

        /// <summary>
        /// The section code for function declarations.
        /// </summary>
        Function = 3,

        /// <summary>
        /// The section code for tables, e.g., the indirect function table.
        /// </summary>
        Table = 4,

        /// <summary>
        /// The section code for memory attributes.
        /// </summary>
        Memory = 5,

        /// <summary>
        /// The section code for global declarations.
        /// </summary>
        Global = 6,

        /// <summary>
        /// The section code for exports.
        /// </summary>
        Export = 7,

        /// <summary>
        /// The section code for the start function declarations.
        /// </summary>
        Start = 8,

        /// <summary>
        /// The section code for an elements section.
        /// </summary>
        Element = 9,

        /// <summary>
        /// The section code for function bodies.
        /// </summary>
        Code = 10,

        /// <summary>
        /// The section code for data segments.
        /// </summary>
        Data = 11
    }
}