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
        /// <param name="Name">The name to define.</param>
        /// <param name="Definition">The function definition.</param>
        public void DefineFunction(string Name, FunctionDefinition Definition)
        {
            funcDefDict[Name] = Definition;
        }

        /// <summary>
        /// Maps the given name to the given variable.
        /// </summary>
        /// <param name="Name">The name to define.</param>
        /// <param name="Definition">The variable definition.</param>
        public void DefineVariable(string Name, Variable Definition)
        {
            varDefDict[Name] = Definition;
        }

        /// <summary>
        /// Maps the given name to the given memory.
        /// </summary>
        /// <param name="Name">The name to define.</param>
        /// <param name="Definition">The memory definition.</param>
        public void DefineMemory(string Name, LinearMemory Definition)
        {
            memDefDict[Name] = Definition;
        }

        /// <summary>
        /// Maps the given name to the given table.
        /// </summary>
        /// <param name="Name">The name to define.</param>
        /// <param name="Definition">The table definition.</param>
        public void DefineTable(string Name, FunctionTable Definition)
        {
            tableDefDict[Name] = Definition;
        }

        /// <summary>
        /// Includes the definitions from the given importer in this importer.
        /// </summary>
        /// <param name="Importer">The importer to include.</param>
        public void IncludeDefinitions(PredefinedImporter Importer)
        {
            CopyDefinitions<FunctionDefinition>(Importer.funcDefDict, this.funcDefDict);
            CopyDefinitions<Variable>(Importer.varDefDict, this.varDefDict);
            CopyDefinitions<LinearMemory>(Importer.memDefDict, this.memDefDict);
            CopyDefinitions<FunctionTable>(Importer.tableDefDict, this.tableDefDict);
        }

        private static void CopyDefinitions<T>(
            Dictionary<string, T> SourceDefinitions,
            Dictionary<string, T> TargetDefinitions)
        {
            foreach (var pair in SourceDefinitions)
            {
                TargetDefinitions[pair.Key] = pair.Value;
            }
        }

        private static T ImportOrDefault<T>(ImportedValue Value, Dictionary<string, T> Definitions)
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