using System.Collections.Generic;

namespace Wasm.Interpret
{
    /// <summary>
    /// An importer that delegates the issue of importing values to another
    /// importer based on the module name associated with the value. That is,
    /// module names serve as "namespaces" of sorts for other importers.
    /// </summary>
    public sealed class NamespacedImporter : IImporter
    {
        /// <summary>
        /// Creates a linking importer.
        /// </summary>
        public NamespacedImporter()
        {
            this.moduleImporters = new Dictionary<string, IImporter>();
        }

        private Dictionary<string, IImporter> moduleImporters;

        /// <summary>
        /// Registers an importer for a particular module name.
        /// </summary>
        /// <param name="moduleName">
        /// The module name to map to <paramref name="importer"/>.
        /// </param>
        /// <param name="importer">
        /// An importer to use for all imports that refer to module <paramref name="moduleName"/>.
        /// </param>
        public void RegisterImporter(string moduleName, IImporter importer)
        {
            moduleImporters[moduleName] = importer;
        }

        /// <inheritdoc/>
        public FunctionDefinition ImportFunction(ImportedFunction description, FunctionType signature)
        {
            if (moduleImporters.TryGetValue(description.ModuleName, out IImporter importer))
            {
                return importer.ImportFunction(description, signature);
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public Variable ImportGlobal(ImportedGlobal description)
        {
            if (moduleImporters.TryGetValue(description.ModuleName, out IImporter importer))
            {
                return importer.ImportGlobal(description);
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public LinearMemory ImportMemory(ImportedMemory description)
        {
            if (moduleImporters.TryGetValue(description.ModuleName, out IImporter importer))
            {
                return importer.ImportMemory(description);
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public FunctionTable ImportTable(ImportedTable description)
        {
            if (moduleImporters.TryGetValue(description.ModuleName, out IImporter importer))
            {
                return importer.ImportTable(description);
            }
            else
            {
                return null;
            }
        }
    }
}
