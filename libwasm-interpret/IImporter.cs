namespace Wasm.Interpret
{
    /// <summary>
    /// Defines a specification for objects that resolve WebAssembly imports.
    /// </summary>
    public interface IImporter
    {
        /// <summary>
        /// Imports the linear memory with the given description.
        /// </summary>
        /// <param name="Description">Describes the memory to import.</param>
        /// <returns>An imported memory.</returns>
        LinearMemory ImportMemory(ImportedMemory Description);
    }
}