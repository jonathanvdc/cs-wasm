using System;
using System.Collections.Generic;

namespace Wasm.Interpret
{
    /// <summary>
    /// An importer implementation that imports predefined values.
    /// </summary>
    public sealed class PredefinedImporter : IImporter
    {
        /// <summary>
        /// Creates a new importer.
        /// </summary>
        public PredefinedImporter()
        {
            this.funcDefDict = new Dictionary<string, FunctionDefinition>();
            this.varDefDict = new Dictionary<string, Variable>();
            this.memDefDict = new Dictionary<string, LinearMemory>();
            this.tableDefDict = new Dictionary<string, FunctionTable>();
        }

        private Dictionary<string, FunctionDefinition> funcDefDict;
        private Dictionary<string, Variable> varDefDict;
        private Dictionary<string, LinearMemory> memDefDict;
        private Dictionary<string, FunctionTable> tableDefDict;

        /// <summary>
        /// Gets a read-only dictionary view that contains all importable function definitions.
        /// </summary>
        public IReadOnlyDictionary<string, FunctionDefinition> FunctionDefinitions => funcDefDict;

        /// <summary>
        /// Gets a read-only dictionary view that contains all importable variable definitions.
        /// </summary>
        public IReadOnlyDictionary<string, Variable> VariableDefinitions => varDefDict;

        /// <summary>
        /// Gets a read-only dictionary view that contains all importable memory definitions.
        /// </summary>
        public IReadOnlyDictionary<string, LinearMemory> MemoryDefinitions => memDefDict;

        /// <summary>
        /// Gets a read-only dictionary view that contains all importable table definitions.
        /// </summary>
        public IReadOnlyDictionary<string, FunctionTable> TableDefinitions => tableDefDict;

        /// <summary>
        /// Maps the given name to the given function definition.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Definition"></param>
        public void DefineFunction(string Name, FunctionDefinition Definition)
        {
            funcDefDict[Name] = Definition;
        }

        private T ImportOrDefault<T>(ImportedValue Value, Dictionary<string, T> Definitions)
        {
            T result;
            if (Definitions.TryGetValue(Value.FieldName, out result))
            {
                return result;
            }
            else
            {
                return default(T);
            }
        }

        /// <inheritdoc/>
        public FunctionDefinition ImportFunction(ImportedFunction Description, FunctionType Signature)
        {
            return ImportOrDefault<FunctionDefinition>(Description, funcDefDict);
        }

        /// <inheritdoc/>
        public Variable ImportGlobal(ImportedGlobal Description)
        {
            return ImportOrDefault<Variable>(Description, varDefDict);
        }

        /// <inheritdoc/>
        public LinearMemory ImportMemory(ImportedMemory Description)
        {
            return ImportOrDefault<LinearMemory>(Description, memDefDict);
        }

        /// <inheritdoc/>
        public FunctionTable ImportTable(ImportedTable Description)
        {
            return ImportOrDefault<FunctionTable>(Description, tableDefDict);
        }
    }
}