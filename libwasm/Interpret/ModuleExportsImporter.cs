using Wasm.Optimize;

namespace Wasm.Interpret
{
    /// <summary>
    /// An importer that imports a module instance's exported values.
    /// </summary>
    public sealed class ModuleExportsImporter : IImporter
    {
        /// <summary>
        /// Creates an importer for a module's exports.
        /// </summary>
        /// <param name="module">A module whose exports are imported by the resulting importer.</param>
        public ModuleExportsImporter(ModuleInstance module)
        {
            this.Module = module;
        }

        /// <summary>
        /// Gets the module instance whose exported values are imported by this importer.
        /// </summary>
        /// <value>A module instance.</value>
        public ModuleInstance Module { get; private set; }

        /// <inheritdoc/>
        public FunctionDefinition ImportFunction(ImportedFunction description, FunctionType signature)
        {
            if (Module.ExportedFunctions.TryGetValue(description.FieldName, out FunctionDefinition result)
                && ConstFunctionTypeComparer.Instance.Equals(signature, new FunctionType(result.ParameterTypes, result.ReturnTypes)))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public Variable ImportGlobal(ImportedGlobal description)
        {
            if (Module.ExportedGlobals.TryGetValue(description.FieldName, out Variable result)
                && description.Global.ContentType == result.Type
                && description.Global.IsMutable == result.IsMutable)
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public LinearMemory ImportMemory(ImportedMemory description)
        {
            if (Module.ExportedMemories.TryGetValue(description.FieldName, out LinearMemory result)
                && result.Limits.Initial >= description.Memory.Limits.Initial)
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public FunctionTable ImportTable(ImportedTable description)
        {
            if (Module.ExportedTables.TryGetValue(description.FieldName, out FunctionTable result)
                && result.Limits.Initial >= description.Table.Limits.Initial)
            {
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}
