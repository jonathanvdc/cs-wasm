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
        /// <param name="name">The name to define.</param>
        /// <param name="definition">The function definition.</param>
        public void DefineFunction(string name, FunctionDefinition definition)
        {
            funcDefDict[name] = definition;
        }

        /// <summary>
        /// Maps the given name to the given variable.
        /// </summary>
        /// <param name="name">The name to define.</param>
        /// <param name="definition">The variable definition.</param>
        public void DefineVariable(string name, Variable definition)
        {
            varDefDict[name] = definition;
        }

        /// <summary>
        /// Maps the given name to the given memory.
        /// </summary>
        /// <param name="name">The name to define.</param>
        /// <param name="definition">The memory definition.</param>
        public void DefineMemory(string name, LinearMemory definition)
        {
            memDefDict[name] = definition;
        }

        /// <summary>
        /// Maps the given name to the given table.
        /// </summary>
        /// <param name="name">The name to define.</param>
        /// <param name="definition">The table definition.</param>
        public void DefineTable(string name, FunctionTable definition)
        {
            tableDefDict[name] = definition;
        }

        /// <summary>
        /// Includes the definitions from the given importer in this importer.
        /// </summary>
        /// <param name="importer">The importer to include.</param>
        public void IncludeDefinitions(PredefinedImporter importer)
        {
            CopyDefinitions<FunctionDefinition>(importer.funcDefDict, this.funcDefDict);
            CopyDefinitions<Variable>(importer.varDefDict, this.varDefDict);
            CopyDefinitions<LinearMemory>(importer.memDefDict, this.memDefDict);
            CopyDefinitions<FunctionTable>(importer.tableDefDict, this.tableDefDict);
        }

        private static void CopyDefinitions<T>(
            Dictionary<string, T> sourceDefinitions,
            Dictionary<string, T> targetDefinitions)
        {
            foreach (var pair in sourceDefinitions)
            {
                targetDefinitions[pair.Key] = pair.Value;
            }
        }

        private static T ImportOrDefault<T>(ImportedValue value, Dictionary<string, T> definitions)
        {
            T result;
            if (definitions.TryGetValue(value.FieldName, out result))
            {
                return result;
            }
            else
            {
                return default(T);
            }
        }

        /// <inheritdoc/>
        public FunctionDefinition ImportFunction(ImportedFunction description, FunctionType signature)
        {
            return ImportOrDefault<FunctionDefinition>(description, funcDefDict);
        }

        /// <inheritdoc/>
        public Variable ImportGlobal(ImportedGlobal description)
        {
            return ImportOrDefault<Variable>(description, varDefDict);
        }

        /// <inheritdoc/>
        public LinearMemory ImportMemory(ImportedMemory description)
        {
            return ImportOrDefault<LinearMemory>(description, memDefDict);
        }

        /// <inheritdoc/>
        public FunctionTable ImportTable(ImportedTable description)
        {
            return ImportOrDefault<FunctionTable>(description, tableDefDict);
        }
    }
}
