using System;
using System.Collections.Generic;

namespace Wasm.Interpret
{
    /// <summary>
    /// Represents an instance of a WebAssembly module.
    /// </summary>
    public sealed class ModuleInstance
    {

        private ModuleInstance(InstructionInterpreter interpreter)
        {
            this.Interpreter = interpreter;
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
        /// Gets a read-only list of the functions in this module.
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
        /// Gets a read-only mapping of names to memories exported by this module.
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
        /// Evaluates an initializer expression.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <param name="resultType">The result type expected from the expression.</param>
        /// <returns>The value obtained by evaluating the initializer expression.</returns>
        public object Evaluate(InitializerExpression expression, WasmValueType resultType)
        {
            var context = new InterpreterContext(this, new[] { resultType });
            foreach (var instruction in expression.BodyInstructions)
            {
                Interpreter.Interpret(instruction, context);
            }
            var result = context.Pop<object>();
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
        /// Evaluates an initializer expression.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <param name="resultType">The result type expected from the expression.</param>
        /// <returns>The value obtained by evaluating the initializer expression.</returns>
        public object Evaluate(InitializerExpression expression, Type resultType)
        {
            return Evaluate(expression, ValueHelpers.ToWasmValueType(resultType));
        }

        /// <summary>
        /// Evaluates an initializer expression.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <returns>The value obtained by evaluating the initializer expression.</returns>
        public T Evaluate<T>(InitializerExpression expression)
        {
            return (T)Evaluate(expression, ValueHelpers.ToWasmValueType<T>());
        }

        /// <summary>
        /// Runs the function at the given index with the given sequence of arguments.
        /// </summary>
        /// <param name="index">The index of the function to run.</param>
        /// <param name="arguments">The function's argument list.</param>
        /// <returns>The function's return value.</returns>
        public IReadOnlyList<object> RunFunction(uint index, IReadOnlyList<object> arguments)
        {
            return definedFuncs[(int)index].Invoke(arguments);
        }

        /// <summary>
        /// Instantiates the given WebAssembly file.
        /// </summary>
        /// <param name="file">The file to instantiate.</param>
        /// <param name="importer">The importer to use to resolve module imports.</param>
        /// <param name="interpreter">
        /// Interprets instructions. A <c>null</c> interpreter indicates that the default interpreter should be used.
        /// </param>
        /// <param name="maxMemorySize">
        /// The maximum size of any memory, in page units. A value of zero
        /// indicates that there is not maximum memory size.
        /// </param>
        /// <returns>A module instance.</returns>
        public static ModuleInstance Instantiate(
            WasmFile file,
            IImporter importer,
            InstructionInterpreter interpreter = null,
            uint maxMemorySize = 0)
        {
            if (interpreter == null)
            {
                interpreter = DefaultInstructionInterpreter.Default;
            }

            var instance = new ModuleInstance(interpreter);

            // Extract the function types.
            var allFuncTypes = GetFunctionTypes(file);

            // Resolve all imports.
            instance.ResolveImports(file, importer, allFuncTypes);

            // Instantiate global variables.
            instance.InstantiateGlobals(file);

            // Instantiate memories.
            instance.InstantiateMemories(file, maxMemorySize);

            // Instantiate function definitions.
            instance.InstantiateFunctionDefs(file, allFuncTypes);

            // Instantiate function tables.
            instance.InstantiateTables(file);

            // Export values.
            instance.RegisterExports(file);

            return instance;
        }

        /// <summary>
        /// Uses the given importer to resolve all imported values.
        /// </summary>
        /// <param name="file">A file whose imports are to be resolved.</param>
        /// <param name="importer">The importer.</param>
        /// <param name="functionTypes">A list of <paramref name="file"/>'s function types.</param>
        private void ResolveImports(
            WasmFile file,
            IImporter importer,
            List<FunctionType> functionTypes)
        {
            var allImportSections = file.GetSections<ImportSection>();
            for (int i = 0; i < allImportSections.Count; i++)
            {
                var importSection = allImportSections[i];
                foreach (var import in importSection.Imports)
                {
                    if (import is ImportedMemory)
                    {
                        var memory = importer.ImportMemory((ImportedMemory)import);
                        if (memory == null)
                        {
                            ThrowCannotResolveImport(import, "linear memory");
                        }
                        definedMemories.Add(memory);
                    }
                    else if (import is ImportedGlobal)
                    {
                        var globalVar = importer.ImportGlobal((ImportedGlobal)import);
                        if (globalVar == null)
                        {
                            ThrowCannotResolveImport(import, "global variable");
                        }
                        definedGlobals.Add(globalVar);
                    }
                    else if (import is ImportedFunction)
                    {
                        var funcImport = (ImportedFunction)import;
                        var funcDef = importer.ImportFunction(funcImport, functionTypes[(int)funcImport.TypeIndex]);
                        if (funcDef == null)
                        {
                            ThrowCannotResolveImport(import, "function");
                        }
                        definedFuncs.Add(funcDef);
                    }
                    else if (import is ImportedTable)
                    {
                        var table = importer.ImportTable((ImportedTable)import);
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

        private static void ThrowCannotResolveImport(ImportedValue import, string importType)
        {
            throw new WasmException(
                string.Format(
                    "Importer cannot resolve {0} definition '{1}.{2}'.",
                    importType, import.ModuleName, import.FieldName));
        }

        private void InstantiateMemories(WasmFile file, uint maxMemorySize)
        {
            // Create module-defined memories.
            var allMemorySections = file.GetSections<MemorySection>();
            for (int i = 0; i < allMemorySections.Count; i++)
            {
                var memorySection = allMemorySections[i];
                foreach (var memorySpec in memorySection.Memories)
                {
                    if (maxMemorySize == 0)
                    {
                        definedMemories.Add(new LinearMemory(memorySpec.Limits));
                    }
                    else
                    {
                        definedMemories.Add(
                            new LinearMemory(
                                new ResizableLimits(
                                    memorySpec.Limits.Initial,
                                    memorySpec.Limits.HasMaximum
                                        ? Math.Min(memorySpec.Limits.Maximum.Value, maxMemorySize)
                                        : maxMemorySize)));
                    }
                }
            }

            // Initialize memories by applying the segments defined by data sections.
            var allDataSections = file.GetSections<DataSection>();
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

        private void InstantiateGlobals(WasmFile file)
        {
            // Create module-defined globals.
            var allGlobalSections = file.GetSections<GlobalSection>();
            for (int i = 0; i < allGlobalSections.Count; i++)
            {
                var globalSection = allGlobalSections[i];
                foreach (var globalSpec in globalSection.GlobalVariables)
                {
                    definedGlobals.Add(
                        Variable.Create<object>(
                            globalSpec.Type.ContentType,
                            globalSpec.Type.IsMutable,
                            Evaluate(globalSpec.InitialValue, globalSpec.Type.ContentType)));
                }
            }
        }

        /// <summary>
        /// Gets a list of all function types declared by the given WebAssembly file.
        /// </summary>
        /// <param name="file">The WebAssembly file to examine.</param>
        /// <returns>The list of function types.</returns>
        private static List<FunctionType> GetFunctionTypes(WasmFile file)
        {
            var allFuncTypes = new List<FunctionType>();
            var allTypeSections = file.GetSections<TypeSection>();
            for (int i = 0; i < allTypeSections.Count; i++)
            {
                allFuncTypes.AddRange(allTypeSections[i].FunctionTypes);
            }
            return allFuncTypes;
        }

        /// <summary>
        /// Instantiates all function definitions from the given WebAssembly file.
        /// </summary>
        /// <param name="file">A WebAssembly file.</param>
        /// <param name="functionTypes">The list of all function types declared by the WebAssembly file.</param>
        private void InstantiateFunctionDefs(WasmFile file, List<FunctionType> functionTypes)
        {
            var funcSignatures = new List<FunctionType>();
            var funcBodies = new List<FunctionBody>();

            var allFuncSections = file.GetSections<FunctionSection>();
            for (int i = 0; i < allFuncSections.Count; i++)
            {
                foreach (var funcSpec in allFuncSections[i].FunctionTypes)
                {
                    funcSignatures.Add(functionTypes[(int)funcSpec]);
                }
            }

            var allCodeSections = file.GetSections<CodeSection>();
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
        /// <param name="signature">The function's signature.</param>
        /// <param name="body">The function's body.</param>
        private void DefineFunction(FunctionType signature, FunctionBody body)
        {
            definedFuncs.Add(new WasmFunctionDefinition(signature, body, this));
        }

        /// <summary>
        /// Instantiates the tables in the given WebAssembly file.
        /// </summary>
        /// <param name="file">The file whose tables are to be instantiated.</param>
        private void InstantiateTables(WasmFile file)
        {
            // Create module-defined tables.
            var allTableSections = file.GetSections<TableSection>();
            for (int i = 0; i < allTableSections.Count; i++)
            {
                foreach (var tableSpec in allTableSections[i].Tables)
                {
                    definedTables.Add(new FunctionTable(tableSpec.Limits));
                }
            }

            // Initialize tables by applying the segments defined by element sections.
            var allElementSections = file.GetSections<ElementSection>();
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
        /// <param name="file">The file that specifies which values are to be exported and how.</param>
        private void RegisterExports(WasmFile file)
        {
            var allExportSections = file.GetSections<ExportSection>();
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