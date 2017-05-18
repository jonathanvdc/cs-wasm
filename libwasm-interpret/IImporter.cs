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
        /// <param name="Description">Describes the memory to import.</param>
        /// <returns>An imported memory.</returns>
        LinearMemory ImportMemory(ImportedMemory Description);

        /// <summary>
        /// Imports the global variable with the given description.
        /// </summary>
        /// <param name="Description">Describes the global variable to import.</param>
        /// <returns>An imported global variable.</returns>
        Variable ImportGlobal(ImportedGlobal Description);

        /// <summary>
        /// Imports the function with the given description.
        /// </summary>
        /// <param name="Description">Describes the function to import.</param>
        /// <returns>An imported function.</returns>
        FunctionDefinition ImportFunction(ImportedFunction Description);

        /// <summary>
        /// Imports the table with the given description.
        /// </summary>
        /// <param name="Description">Describes the table to import.</param>
        /// <returns>An imported table.</returns>
        FunctionTable ImportTable(ImportedTable Description);
    }
}