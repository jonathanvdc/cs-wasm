using System.Collections.Generic;

namespace Wasm.Interpret
{
    /// <summary>
    /// Represents an instance of a WebAssembly module.
    /// </summary>
    public sealed class ModuleInstance
    {

        private ModuleInstance(InstructionInterpreter Interpreter)
        {
            this.Interpreter = Interpreter;
            this.definedMemories = new List<LinearMemory>();
            this.definedGlobals = new List<Variable>();
            this.definedFuncs = new List<FunctionDefinition>();
            this.definedTables = new List<FunctionTable>();
            this.expMemories = new Dictionary<string, LinearMemory>();
            this.expGlobals = new Dictionary<string, Variable>();
            this.expFuncs = new Dictionary<string, FunctionDefinition>();
            this.expTables = new Dictionary<string, FunctionTable>();
        }

        /// <summary>
        /// Gets the interpreter for this module instance.
        /// </summary>
        public InstructionInterpreter Interpreter { get; private set; }

        private List<LinearMemory> definedMemories;
        private List<Variable> definedGlobals;
        private List<FunctionDefinition> definedFuncs;
        private List<FunctionTable> definedTables;
        private Dictionary<string, LinearMemory> expMemories;
        private Dictionary<string, Variable> expGlobals;
        private Dictionary<string, FunctionDefinition> expFuncs;
        private Dictionary<string, FunctionTable> expTables;

        /// <summary>
        /// Gets a read-only list of the memories in this module.
        /// </summary>
        public IReadOnlyList<LinearMemory> Memories => definedMemories;

        /// <summary>
        /// Gets a read-only list of the memories in this module.
        /// </summary>
        public IReadOnlyList<FunctionDefinition> Functions => definedFuncs;

        /// <summary>
        /// Gets a read-only list of global variables in this module.
        /// </summary>
        public IReadOnlyList<Variable> Globals => definedGlobals;

        /// <summary>
        /// Gets a read-only list of tables defined in this module.
        /// </summary>
        public IReadOnlyList<FunctionTable> Tables => definedTables;

        /// <summary>
        /// Gets a read-only mapping of names to functions exported by this module.
        /// </summary>
        public IReadOnlyDictionary<string, LinearMemory> ExportedMemories => expMemories;

        /// <summary>
        /// Gets a read-only mapping of names to functions exported by this module.
        /// </summary>
        public IReadOnlyDictionary<string, FunctionDefinition> ExportedFunctions => expFuncs;

        /// <summary>
        /// Gets a read-only mapping of names to global variables exported by this module.
        /// </summary>
        public IReadOnlyDictionary<string, Variable> ExportedGlobals => expGlobals;

        /// <summary>
        /// Gets a read-only mapping of names to tables exported by this module.
        /// </summary>
        public IReadOnlyDictionary<string, FunctionTable> ExportedTables => expTables;

        /// <summary>
        /// Evaluates the given initializer expression.
        /// </summary>
        /// <param name="Expression">The expression to evaluate.</param>
        /// <returns>The value obtained by evaluating the initializer expression.</returns>
        public T Evaluate<T>(InitializerExpression Expression)
        {
            var context = new InterpreterContext(this);
            foreach (var instruction in Expression.BodyInstructions)
            {
                Interpreter.Interpret(instruction, context);
            }
            var result = context.Pop<T>();
            if (context.StackDepth > 0)
            {
                throw new WasmException(
                    "The stack must contain exactly one value after " +
                    "evaluating an initializer expression. Actual stack depth: " +
                    context.StackDepth + ".");
            }
            return result;
        }

        /// <summary>
        /// Runs the function at the given index with the given sequence of arguments.
        /// </summary>
        /// <param name="Index">The index of the function to run.</param>
        /// <param name="Arguments">The function's argument list.</param>
        /// <returns>The function's return value.</returns>
        public IReadOnlyList<object> RunFunction(uint Index, IReadOnlyList<object> Arguments)
        {
            return definedFuncs[(int)Index].Invoke(Arguments);
        }

        /// <summary>
        /// Instantiates the given WebAssembly file. An importer is used to
        /// resolve module imports.
        /// </summary>
        /// <param name="File">The file to instantiate.</param>
        /// <param name="Importer">Resolves module imports.</param>
        /// <returns>A module instance.</returns>
        public static ModuleInstance Instantiate(WasmFile File, IImporter Importer)
        {
            return Instantiate(File, Importer, DefaultInstructionInterpreter.Default);
        }

        /// <summary>
        /// Instantiates the given WebAssembly file. An importer is used to
        /// resolve module imports and an interpreter is used to interpret
        /// instructions.
        /// </summary>
        /// <param name="File">The file to instantiate.</param>
        /// <param name="Importer">Resolves module imports.</param>
        /// <param name="Interpreter">Interprets instructions.</param>
        /// <returns>A module instance.</returns>
        public static ModuleInstance Instantiate(
            WasmFile File,
            IImporter Importer,
            InstructionInterpreter Interpreter)
        {
            var instance = new ModuleInstance(Interpreter);

            // Extract the function types.
            var allFuncTypes = GetFunctionTypes(File);

            // Resolve all imports.
            instance.ResolveImports(File, Importer, allFuncTypes);

            // Instantiate global variables.
            instance.InstantiateGlobals(File);

            // Instantiate memories.
            instance.InstantiateMemories(File);

            // Instantiate function definitions.
            instance.InstantiateFunctionDefs(File, allFuncTypes);

            // Instantiate function tables.
            instance.InstantiateTables(File);

            // Export values.
            instance.RegisterExports(File);

            return instance;
        }

        /// <summary>
        /// Uses the given importer to resolve all imported values.
        /// </summary>
        /// <param name="Importer">The importer.</param>
        private void ResolveImports(
            WasmFile File,
            IImporter Importer,
            List<FunctionType> FunctionTypes)
        {
            var allImportSections = File.GetSections<ImportSection>();
            for (int i = 0; i < allImportSections.Count; i++)
            {
                var importSection = allImportSections[i];
                foreach (var import in importSection.Imports)
                {
                    if (import is ImportedMemory)
                    {
                        var memory = Importer.ImportMemory((ImportedMemory)import);
                        if (memory == null)
                        {
                            ThrowCannotResolveImport(import, "linear memory");
                        }
                        definedMemories.Add(memory);
                    }
                    else if (import is ImportedGlobal)
                    {
                        var globalVar = Importer.ImportGlobal((ImportedGlobal)import);
                        if (globalVar == null)
                        {
                            ThrowCannotResolveImport(import, "global variable");
                        }
                        definedGlobals.Add(globalVar);
                    }
                    else if (import is ImportedFunction)
                    {
                        var funcImport = (ImportedFunction)import;
                        var funcDef = Importer.ImportFunction(funcImport, FunctionTypes[(int)funcImport.TypeIndex]);
                        if (funcDef == null)
                        {
                            ThrowCannotResolveImport(import, "function");
                        }
                        definedFuncs.Add(funcDef);
                    }
                    else if (import is ImportedTable)
                    {
                        var table = Importer.ImportTable((ImportedTable)import);
                        if (table == null)
                        {
                            ThrowCannotResolveImport(import, "table");
                        }
                        definedTables.Add(table);
                    }
                    else
                    {
                        throw new WasmException("Unknown import type: " + import.ToString());
                    }
                }
            }
        }

        private static void ThrowCannotResolveImport(ImportedValue Import, string ImportType)
        {
            throw new WasmException(
                string.Format(
                    "Importer cannot resolve {0} definition '{1}.{2}'.",
                    ImportType, Import.ModuleName, Import.FieldName));
        }

        private void InstantiateMemories(WasmFile File)
        {
            // Create module-defined memories.
            var allMemorySections = File.GetSections<MemorySection>();
            for (int i = 0; i < allMemorySections.Count; i++)
            {
                var memorySection = allMemorySections[i];
                foreach (var memorySpec in memorySection.Memories)
                {
                    definedMemories.Add(new LinearMemory(memorySpec.Limits));
                }
            }

            // Initialize memories by applying the segments defined by data sections.
            var allDataSections = File.GetSections<DataSection>();
            for (int i = 0; i < allDataSections.Count; i++)
            {
                var dataSection = allDataSections[i];
                foreach (var segment in dataSection.Segments)
                {
                    var memoryView = Memories[(int)segment.MemoryIndex].Int8;
                    var evalOffset = Evaluate<int>(segment.Offset);
                    for (int j = 0; j < segment.Data.Length; j++)
                    {
                        memoryView[(uint)(evalOffset + j)] = (sbyte)segment.Data[j];
                    }
                }
            }
        }

        private void InstantiateGlobals(WasmFile File)
        {
            // Create module-defined globals.
            var allGlobalSections = File.GetSections<GlobalSection>();
            for (int i = 0; i < allGlobalSections.Count; i++)
            {
                var globalSection = allGlobalSections[i];
                foreach (var globalSpec in globalSection.GlobalVariables)
                {
                    definedGlobals.Add(
                        Variable.Create<object>(
                            globalSpec.Type.ContentType,
                            globalSpec.Type.IsMutable,
                            Evaluate<object>(globalSpec.InitialValue)));
                }
            }
        }

        /// <summary>
        /// Gets a list of all function types declared by the given WebAssembly file.
        /// </summary>
        /// <param name="File">The WebAssembly file to examine.</param>
        /// <returns>The list of function types.</returns>
        private static List<FunctionType> GetFunctionTypes(WasmFile File)
        {
            var allFuncTypes = new List<FunctionType>();
            var allTypeSections = File.GetSections<TypeSection>();
            for (int i = 0; i < allTypeSections.Count; i++)
            {
                allFuncTypes.AddRange(allTypeSections[i].FunctionTypes);
            }
            return allFuncTypes;
        }

        /// <summary>
        /// Instantiates all function definitions from the given WebAssembly file.
        /// </summary>
        /// <param name="File">A WebAssembly file.</param>
        /// <param name="FunctionTypes">The list of all function types declared by the WebAssembly file.</param>
        private void InstantiateFunctionDefs(WasmFile File, List<FunctionType> FunctionTypes)
        {
            var funcSignatures = new List<FunctionType>();
            var funcBodies = new List<FunctionBody>();

            var allFuncSections = File.GetSections<FunctionSection>();
            for (int i = 0; i < allFuncSections.Count; i++)
            {
                foreach (var funcSpec in allFuncSections[i].FunctionTypes)
                {
                    funcSignatures.Add(FunctionTypes[(int)funcSpec]);
                }
            }

            var allCodeSections = File.GetSections<CodeSection>();
            for (int i = 0; i < allCodeSections.Count; i++)
            {
                funcBodies.AddRange(allCodeSections[i].Bodies);
            }

            if (funcSignatures.Count != funcBodies.Count)
            {
                throw new WasmException(
                    "Function declaration/definition count mismatch: module declares " +
                    funcSignatures.Count + " functions and defines " + funcBodies.Count + ".");
            }

            for (int i = 0; i < funcSignatures.Count; i++)
            {
                DefineFunction(funcSignatures[i], funcBodies[i]);
            }
        }

        /// <summary>
        /// Defines a function with the given signature and function body.
        /// </summary>
        /// <param name="Signature">The function's signature.</param>
        /// <param name="Body">The function's body.</param>
        private void DefineFunction(FunctionType Signature, FunctionBody Body)
        {
            definedFuncs.Add(new WasmFunctionDefinition(Signature, Body, this));
        }

        /// <summary>
        /// Instantiates the tables in the given WebAssembly file.
        /// </summary>
        /// <param name="File">The file whose tables are to be instantiated.</param>
        private void InstantiateTables(WasmFile File)
        {
            // Create module-defined tables.
            var allTableSections = File.GetSections<TableSection>();
            for (int i = 0; i < allTableSections.Count; i++)
            {
                foreach (var tableSpec in allTableSections[i].Tables)
                {
                    definedTables.Add(new FunctionTable(tableSpec.Limits));
                }
            }

            // Initialize tables by applying the segments defined by element sections.
            var allElementSections = File.GetSections<ElementSection>();
            for (int i = 0; i < allElementSections.Count; i++)
            {
                foreach (var segment in allElementSections[i].Segments)
                {
                    var table = Tables[(int)segment.TableIndex];
                    var evalOffset = Evaluate<int>(segment.Offset);
                    for (int j = 0; j < segment.Elements.Count; j++)
                    {
                        table[(uint)(evalOffset + j)] = definedFuncs[(int)segment.Elements[j]];
                    }
                }
            }
        }

        /// <summary>
        /// Exports values specified by the given WebAssembly file.
        /// </summary>
        /// <param name="File">The file that specifies which values are to be exported and how.</param>
        private void RegisterExports(WasmFile File)
        {
            var allExportSections = File.GetSections<ExportSection>();
            for (int i = 0; i < allExportSections.Count; i++)
            {
                foreach (var export in allExportSections[i].Exports)
                {
                    switch (export.Kind)
                    {
                        case ExternalKind.Memory:
                            expMemories[export.Name] = Memories[(int)export.Index];
                            break;
                        case ExternalKind.Global:
                            expGlobals[export.Name] = Globals[(int)export.Index];
                            break;
                        case ExternalKind.Function:
                            expFuncs[export.Name] = Functions[(int)export.Index];
                            break;
                        case ExternalKind.Table:
                            expTables[export.Name] = Tables[(int)export.Index];
                            break;
                        default:
                            throw new WasmException("Unknown export kind: " + export.Kind);
                    }
                }
            }
        }
    }
}