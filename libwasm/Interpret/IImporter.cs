using System;
using System.Collections.Generic;

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
        /// <param name="description">Describes the memory to import.</param>
        /// <returns>An imported memory.</returns>
        LinearMemory ImportMemory(ImportedMemory description);

        /// <summary>
        /// Imports the global variable with the given description.
        /// </summary>
        /// <param name="description">Describes the global variable to import.</param>
        /// <returns>An imported global variable.</returns>
        Variable ImportGlobal(ImportedGlobal description);

        /// <summary>
        /// Imports the function with the given description.
        /// </summary>
        /// <param name="description">Describes the function to import.</param>
        /// <param name="signature">The signature of the function to import.</param>
        /// <returns>An imported function.</returns>
        FunctionDefinition ImportFunction(ImportedFunction description, FunctionType signature);

        /// <summary>
        /// Imports the table with the given description.
        /// </summary>
        /// <param name="description">Describes the table to import.</param>
        /// <returns>An imported table.</returns>
        FunctionTable ImportTable(ImportedTable description);
    }
}
