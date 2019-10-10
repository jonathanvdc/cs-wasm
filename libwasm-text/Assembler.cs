using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Pixie;
using Pixie.Code;
using Pixie.Markup;
using Wasm.Instructions;

namespace Wasm.Text
{
    /// <summary>
    /// An assembler for the WebAssembly text format. Converts parsed WebAssembly text format
    /// modules to in-memory WebAssembly binary format modules.
    /// </summary>
    public sealed class Assembler
    {
        /// <summary>
        /// Creates a WebAssembly assembler.
        /// </summary>
        /// <param name="log">A log to send diagnostics to.</param>
        public Assembler(ILog log)
            : this(log, DefaultModuleFieldAssemblers)
        { }

        /// <summary>
        /// Creates a WebAssembly assembler.
        /// </summary>
        /// <param name="log">A log to send diagnostics to.</param>
        /// <param name="moduleFieldAssemblers">
        /// A mapping of module field keywords to module field assemblers.
        /// </param>
        public Assembler(
            ILog log,
            IReadOnlyDictionary<string, ModuleFieldAssembler> moduleFieldAssemblers)
        {
            this.Log = log;
            this.ModuleFieldAssemblers = moduleFieldAssemblers;
        }

        /// <summary>
        /// Gets the log that is used for reporting diagnostics.
        /// </summary>
        /// <value>A log.</value>
        public ILog Log { get; private set; }

        /// <summary>
        /// Gets the module field assemblers this assembler uses to process
        /// module fields.
        /// </summary>
        /// <value>A mapping of module field keywords to module field assemblers.</value>
        public IReadOnlyDictionary<string, ModuleFieldAssembler> ModuleFieldAssemblers { get; private set; }

        /// <summary>
        /// Assembles an S-expression representing a module into a WebAssembly module.
        /// </summary>
        /// <param name="expression">The expression to assemble.</param>
        /// <returns>An assembled module.</returns>
        public WasmFile AssembleModule(SExpression expression)
        {
            var file = new WasmFile();
            if (!expression.IsCallTo("module"))
            {
                Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "syntax error",
                        Quotation.QuoteEvenInBold(
                            "top-level modules must be encoded as S-expressions that call ",
                            "module",
                            "."),
                        Highlight(expression)));
            }

            IReadOnlyList<SExpression> fields;
            if (expression.Tail.Count > 0 && expression.Tail[0].IsIdentifier)
            {
                // We encountered a module name. Turn it into a name entry and then skip it
                // for the purpose of module field analysis.
                file.ModuleName = (string)expression.Tail[0].Head.Value;
                fields = expression.Tail.Skip(1).ToArray();
            }
            else
            {
                fields = expression.Tail;
            }

            // Now assemble the module's fields.
            var context = new ModuleContext(this);
            foreach (var field in fields)
            {
                ModuleFieldAssembler fieldAssembler;
                if (!field.IsCall)
                {
                    Log.Log(
                        new LogEntry(
                            Severity.Error,
                            "syntax error",
                            "unexpected token; expected a module field.",
                            Highlight(expression)));
                }
                else if (DefaultModuleFieldAssemblers.TryGetValue((string)field.Head.Value, out fieldAssembler))
                {
                    fieldAssembler(field, file, context);
                }
                else
                {
                    Log.Log(
                        new LogEntry(
                            Severity.Error,
                            "syntax error",
                            Quotation.QuoteEvenInBold(
                                "unexpected module field type ",
                                (string)field.Head.Value,
                                "."),
                            Highlight(expression)));
                }
            }
            context.ResolveIdentifiers(file);
            return file;
        }

        /// <summary>
        /// Assembles an S-expression representing a module into a WebAssembly module.
        /// </summary>
        /// <param name="tokens">A stream of tokens to parse and assemble.</param>
        /// <returns>An assembled module.</returns>
        public WasmFile AssembleModule(IEnumerable<Lexer.Token> tokens)
        {
            var exprs = Parser.ParseAsSExpressions(tokens, Log);
            if (exprs.Count == 0)
            {
                Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "nothing to assemble",
                        "input stream contains no S-expression that can be assembled into a module."));
                return new WasmFile();
            }
            else if (exprs.Count != 1)
            {
                Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "multiple modules",
                        "input stream contains more than one S-expression to assemble into a module; expected just one.",
                        Highlight(exprs[1])));
            }
            return AssembleModule(exprs[0]);
        }

        /// <summary>
        /// Assembles an S-expression representing a module into a WebAssembly module.
        /// </summary>
        /// <param name="document">A document to parse and assemble.</param>
        /// <param name="fileName">The name of the file in which <paramref name="document"/> is saved.</param>
        /// <returns>An assembled module.</returns>
        public WasmFile AssembleModule(string document, string fileName = "<string>")
        {
            return AssembleModule(Lexer.Tokenize(document, fileName));
        }

        private static HighlightedSource Highlight(Lexer.Token expression)
        {
            return new HighlightedSource(new SourceRegion(expression.Span));
        }

        private static HighlightedSource Highlight(SExpression expression)
        {
            return Highlight(expression.Head);
        }

        /// <summary>
        /// A type for module field assemblers.
        /// </summary>
        /// <param name="moduleField">A module field to assemble.</param>
        /// <param name="module">The module that is being assembled.</param>
        /// <param name="context">The module's assembly context.</param>
        public delegate void ModuleFieldAssembler(
            SExpression moduleField,
            WasmFile module,
            ModuleContext context);

        /// <summary>
        /// Context that is used when assembling a module.
        /// </summary>
        public sealed class ModuleContext
        {
            /// <summary>
            /// Creates a module context.
            /// </summary>
            /// <param name="assembler">The assembler that gives rise to this conetxt.</param>
            public ModuleContext(Assembler assembler)
            {
                this.Assembler = assembler;
                this.MemoryContext = IdentifierContext<MemoryType>.Create();
                this.FunctionContext = IdentifierContext<LocalOrImportRef>.Create();
                this.GlobalContext = IdentifierContext<LocalOrImportRef>.Create();
                this.TableContext = IdentifierContext<LocalOrImportRef>.Create();
            }

            /// <summary>
            /// Gets the identifier context for the module's memories.
            /// </summary>
            /// <value>An identifier context.</value>
            public IdentifierContext<MemoryType> MemoryContext { get; private set; }

            /// <summary>
            /// Gets the identifier context for the module's functions.
            /// </summary>
            /// <value>An identifier context.</value>
            public IdentifierContext<LocalOrImportRef> FunctionContext { get; private set; }

            /// <summary>
            /// Gets the identifier context for the module's globals.
            /// </summary>
            /// <value>An identifier context.</value>
            public IdentifierContext<LocalOrImportRef> GlobalContext { get; private set; }

            /// <summary>
            /// Gets the identifier context for the module's tables.
            /// </summary>
            /// <value>An identifier context.</value>
            public IdentifierContext<LocalOrImportRef> TableContext { get; private set; }

            /// <summary>
            /// Gets the assembler that gives rise to this context.
            /// </summary>
            /// <value>An assembler.</value>
            public Assembler Assembler { get; private set; }

            /// <summary>
            /// Gets the log used by the assembler and, by extension, this context.
            /// </summary>
            public ILog Log => Assembler.Log;

            /// <summary>
            /// Resolves any pending references in the module.
            /// </summary>
            /// <param name="module">The module for which this context was created.</param>
            public void ResolveIdentifiers(WasmFile module)
            {
                var importSection = module.GetFirstSectionOrNull<ImportSection>() ?? new ImportSection();
                var memorySection = module.GetFirstSectionOrNull<MemorySection>() ?? new MemorySection();
                var functionSection = module.GetFirstSectionOrNull<FunctionSection>() ?? new FunctionSection();
                var globalSection = module.GetFirstSectionOrNull<GlobalSection>() ?? new GlobalSection();
                var tableSection = module.GetFirstSectionOrNull<TableSection>() ?? new TableSection();

                var memoryIndices = new Dictionary<MemoryType, uint>();
                var functionIndices = new Dictionary<LocalOrImportRef, uint>();
                var globalIndices = new Dictionary<LocalOrImportRef, uint>();
                var tableIndices = new Dictionary<LocalOrImportRef, uint>();
                for (int i = 0; i < importSection.Imports.Count; i++)
                {
                    var import = importSection.Imports[i];
                    if (import is ImportedMemory importedMemory)
                    {
                        memoryIndices[importedMemory.Memory] = (uint)memoryIndices.Count;
                    }
                    else if (import is ImportedFunction importedFunction)
                    {
                        functionIndices[new LocalOrImportRef(true, (uint)i)] = (uint)functionIndices.Count;
                    }
                    else if (import is ImportedGlobal importedGlobal)
                    {
                        globalIndices[new LocalOrImportRef(true, (uint)i)] = (uint)globalIndices.Count;
                    }
                    else if (import is ImportedTable importedTable)
                    {
                        tableIndices[new LocalOrImportRef(true, (uint)i)] = (uint)tableIndices.Count;
                    }
                }
                foreach (var memory in memorySection.Memories)
                {
                    memoryIndices[memory] = (uint)memoryIndices.Count;
                }
                for (int i = 0; i < functionSection.FunctionTypes.Count; i++)
                {
                    functionIndices[new LocalOrImportRef(false, (uint)i)] = (uint)functionIndices.Count;
                }
                for (int i = 0; i < globalSection.GlobalVariables.Count; i++)
                {
                    globalIndices[new LocalOrImportRef(false, (uint)i)] = (uint)globalIndices.Count;
                }
                for (int i = 0; i < tableSection.Tables.Count; i++)
                {
                    tableIndices[new LocalOrImportRef(false, (uint)i)] = (uint)tableIndices.Count;
                }

                // Resolve memory identifiers.
                MemoryContext.ResolveAll(
                    Assembler.Log,
                    mem => memoryIndices[mem]);

                // Resolve function identifiers.
                FunctionContext.ResolveAll(
                    Assembler.Log,
                    func => functionIndices[func]);

                // Resolve global identifiers.
                GlobalContext.ResolveAll(
                    Assembler.Log,
                    global => globalIndices[global]);

                // Resolve table identifiers.
                TableContext.ResolveAll(
                    Assembler.Log,
                    table => tableIndices[table]);
            }
        }

        /// <summary>
        /// A reference to a function, global, table or memory that is either defined
        /// locally or imported.
        /// </summary>
        public struct LocalOrImportRef
        {
            /// <summary>
            /// Creates a reference to a function, global, table or memory that is either defined
            /// locally or imported.
            /// </summary>
            /// <param name="isImport">
            /// Tells if the value referred to by this reference is an import.
            /// </param>
            /// <param name="indexInSection">
            /// The intra-section index of the value being referred to.
            /// </param>
            public LocalOrImportRef(bool isImport, uint indexInSection)
            {
                this.IsImport = isImport;
                this.IndexInSection = indexInSection;
            }

            /// <summary>
            /// Tells if the value referred to by this reference is an import.
            /// </summary>
            /// <value><c>true</c> if the value is an import; otherwise, <c>false</c>.</value>
            public bool IsImport { get; private set; }

            /// <summary>
            /// Gets the intra-section index of the value being referred to.
            /// </summary>
            /// <value>An intra-section index.</value>
            public uint IndexInSection { get; private set; }
        }

        /// <summary>
        /// An identifier context, which maps identifiers to indices.
        /// </summary>
        public struct IdentifierContext<T>
        {
            /// <summary>
            /// Creates an empty identifier context.
            /// </summary>
            /// <returns>An identifier context.</returns>
            public static IdentifierContext<T> Create()
            {
                return new IdentifierContext<T>()
                {
                    identifierDefinitions = new Dictionary<string, T>(),
                    pendingIdentifierReferences = new List<KeyValuePair<Lexer.Token, Action<uint>>>()
                };
            }

            private Dictionary<string, T> identifierDefinitions;
            private List<KeyValuePair<Lexer.Token, Action<uint>>> pendingIdentifierReferences;

            /// <summary>
            /// Defines a new identifier.
            /// </summary>
            /// <param name="identifier">The identifier to define.</param>
            /// <param name="value">The value identified by the identifier.</param>
            /// <returns>
            /// <c>true</c> if <paramref name="identifier"/> is non-null and there is no
            /// previous definition of the identifier; otherwise, <c>false</c>.
            /// </returns>
            public bool Define(string identifier, T value)
            {
                if (identifier == null || identifierDefinitions.ContainsKey(identifier))
                {
                    return false;
                }
                else
                {
                    identifierDefinitions[identifier] = value;
                    return true;
                }
            }

            /// <summary>
            /// Introduces a new identifier use.
            /// </summary>
            /// <param name="token">A token that refers to an identifier or an index.</param>
            /// <param name="patch">
            /// An action that patches a user based on the index assigned to the token.
            /// Will be executed once the module is fully assembled.
            /// </param>
            public void Use(Lexer.Token token, Action<uint> patch)
            {
                pendingIdentifierReferences.Add(
                    new KeyValuePair<Lexer.Token, Action<uint>>(token, patch));
            }

            /// <summary>
            /// Introduces a new identifier use.
            /// </summary>
            /// <param name="value">A value that will eventually be assigned an index.</param>
            /// <param name="patch">
            /// An action that patches a user based on the index assigned to the value.
            /// Will be executed once the module is fully assembled.
            /// </param>
            public void Use(T value, Action<uint> patch)
            {
                Use(Lexer.Token.Synthesize(value), patch);
            }

            /// <summary>
            /// Resolves all pending references.
            /// </summary>
            /// <param name="log">A log to send diagnostics to.</param>
            /// <param name="getIndex">A function that maps defined values to indices.</param>
            public void ResolveAll(ILog log, Func<T, uint> getIndex)
            {
                foreach (var pair in pendingIdentifierReferences)
                {
                    uint index;
                    if (TryResolve(pair.Key, getIndex, out index))
                    {
                        pair.Value(index);
                    }
                    else
                    {
                        var id = (string)pair.Key.Value;
                        var suggested = NameSuggestion.SuggestName(id, identifierDefinitions.Keys);
                        log.Log(
                            new LogEntry(
                                Severity.Error,
                                "syntax error",
                                Quotation.QuoteEvenInBold("identifier ", id, " does is undefined"),
                                suggested == null
                                    ? (MarkupNode)"."
                                    : Quotation.QuoteEvenInBold("; did you mean ", suggested, "?"),
                                Highlight(pair.Key)));
                    }
                }
                pendingIdentifierReferences.Clear();
            }

            /// <summary>
            /// Tries to map an identifier to its associated index.
            /// </summary>
            /// <param name="identifier">An identifier to inspect.</param>
            /// <param name="getIndex">A function that maps defined values to indices.</param>
            /// <param name="index">The associated index.</param>
            /// <returns>
            /// <c>true</c> if an index was found for <paramref name="identifier"/>; otherwise, <c>false</c>.
            /// </returns>
            private bool TryResolve(string identifier, Func<T, uint> getIndex, out uint index)
            {
                T val;
                if (identifierDefinitions.TryGetValue(identifier, out val))
                {
                    index = getIndex(val);
                    return true;
                }
                else
                {
                    index = 0;
                    return false;
                }
            }

            /// <summary>
            /// Tries to map an identifier or index to its associated index.
            /// </summary>
            /// <param name="identifierOrIndex">An identifier or index to inspect.</param>
            /// <param name="getIndex">A function that maps defined values to indices.</param>
            /// <param name="index">The associated index.</param>
            /// <returns>
            /// <c>true</c> if an index was found for <paramref name="identifierOrIndex"/>; otherwise, <c>false</c>.
            /// </returns>
            private bool TryResolve(Lexer.Token identifierOrIndex, Func<T, uint> getIndex, out uint index)
            {
                if (identifierOrIndex.Kind == Lexer.TokenKind.UnsignedInteger)
                {
                    index = (uint)(BigInteger)identifierOrIndex.Value;
                    return true;
                }
                else if (identifierOrIndex.Kind == Lexer.TokenKind.Identifier)
                {
                    return TryResolve((string)identifierOrIndex.Value, getIndex, out index);
                }
                else if (identifierOrIndex.Kind == Lexer.TokenKind.Synthetic
                    && identifierOrIndex.Value is T)
                {
                    index = getIndex((T)identifierOrIndex.Value);
                    return true;
                }
                else
                {
                    index = 0;
                    return false;
                }
            }
        }

        /// <summary>
        /// The default set of module field assemblers.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, ModuleFieldAssembler> DefaultModuleFieldAssemblers =
            new Dictionary<string, ModuleFieldAssembler>()
        {
            ["export"] = AssembleExport,
            ["import"] = AssembleImport,
            ["memory"] = AssembleMemory,
            ["table"] = AssembleTable
        };

        private static void AssembleMemory(
            SExpression moduleField,
            WasmFile module,
            ModuleContext context)
        {
            const string kind = "memory definition";

            // Process the optional memory identifier.
            var memory = new MemoryType(new ResizableLimits(0));
            var tail = moduleField.Tail;
            if (tail.Count > 0 && tail[0].IsIdentifier)
            {
                context.MemoryContext.Define((string)tail[0].Head.Value, memory);
                tail = tail.Skip(1).ToArray();
            }

            if (!AssertNonEmpty(moduleField, tail, kind, context))
            {
                return;
            }

            // Parse inline exports.
            while (tail[0].IsCallTo("export"))
            {
                var exportExpr = tail[0];
                tail = tail.Skip(1).ToArray();
                if (!AssertNonEmpty(moduleField, tail, kind, context))
                {
                    return;
                }
                if (!AssertElementCount(exportExpr, exportExpr.Tail, 1, context))
                {
                    continue;
                }

                var exportName = AssembleString(exportExpr.Tail[0], context);
                AddExport(module, context.MemoryContext, memory, ExternalKind.Memory, exportName);
            }

            if (tail[0].IsCallTo("data"))
            {
                var data = AssembleDataString(tail[0].Tail, context);
                var pageCount = (uint)Math.Ceiling((double)data.Length / MemoryType.PageSize);
                memory.Limits = new ResizableLimits(pageCount, pageCount);
                module.AddMemory(memory);
                var dataSegment = new DataSegment(0, new InitializerExpression(Operators.Int32Const.Create(0)), data);
                context.MemoryContext.Use(memory, index => { dataSegment.MemoryIndex = index; });
                module.AddDataSegment(dataSegment);
                AssertEmpty(context, kind, tail.Skip(1));
            }
            else if (tail[0].IsCallTo("import"))
            {
                var (moduleName, memoryName) = AssembleInlineImport(tail[0], context);
                tail = tail.Skip(1).ToArray();
                memory.Limits = AssembleLimits(moduleField, tail, context);
                var import = new ImportedMemory(moduleName, memoryName, memory);
                module.AddImport(import);
            }
            else
            {
                memory.Limits = AssembleLimits(moduleField, tail, context);
                module.AddMemory(memory);
            }
        }

        private static void AssembleExport(
            SExpression moduleField,
            WasmFile module,
            ModuleContext context)
        {
            var tail = moduleField.Tail;

            if (!AssertElementCount(moduleField, tail, 2, context)
                || !AssertElementCount(tail[1], tail[1].Tail, 1, context))
            {
                return;
            }

            var exportName = AssembleString(tail[0], context);
            var index = AssembleIdentifierOrIndex(tail[1].Tail[0], context);

            if (tail[1].IsCallTo("memory"))
            {
                AddExport(module, context.MemoryContext, index, ExternalKind.Memory, exportName);
            }
            else if (tail[1].IsCallTo("func"))
            {
                AddExport(module, context.FunctionContext, index, ExternalKind.Function, exportName);
            }
            else if (tail[1].IsCallTo("table"))
            {
                AddExport(module, context.TableContext, index, ExternalKind.Table, exportName);
            }
            else if (tail[1].IsCallTo("global"))
            {
                AddExport(module, context.GlobalContext, index, ExternalKind.Global, exportName);
            }
            else
            {
                context.Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "syntax error",
                        Quotation.QuoteEvenInBold(
                            "unexpected expression in export definition; expected ",
                            "func", ",", "table", ",", "memory", " or ", "global", "."),
                        Highlight(tail[1])));
            }
        }

        private static void AssembleImport(SExpression moduleField, WasmFile module, ModuleContext context)
        {
            if (!AssertElementCount(moduleField, moduleField.Tail, 3, context))
            {
                return;
            }

            var moduleName = AssembleString(moduleField.Tail[0], context);
            var importName = AssembleString(moduleField.Tail[1], context);
            var importDesc = moduleField.Tail[2];

            string importId = null;
            var importTail = importDesc.Tail;
            if (importDesc.Tail.Count > 0 && importDesc.Tail[0].IsIdentifier)
            {
                importId = (string)importDesc.Tail[0].Head.Value;
                importTail = importTail.Skip(1).ToArray();
            }

            if (importDesc.IsCallTo("memory"))
            {
                if (!AssertNonEmpty(importDesc, importTail, "import", context))
                {
                    return;
                }
                var memory = new MemoryType(AssembleLimits(importDesc, importTail, context));
                module.AddImport(new ImportedMemory(moduleName, importName, memory));
                context.MemoryContext.Define(importId, memory);
            }
            else if (importDesc.IsCallTo("func"))
            {
                var type = AssembleTypeUse(importDesc, ref importTail, context);
                var typeIndex = module.AddFunctionType(type);
                var importIndex = module.AddImport(new ImportedFunction(moduleName, importName, typeIndex));
                context.FunctionContext.Define(importId, new LocalOrImportRef(true, importIndex));
                AssertEmpty(context, "import", importTail);
            }
            else if (importDesc.IsCallTo("global"))
            {
                var type = AssembleGlobalType(importTail[0], context);
                var importIndex = module.AddImport(new ImportedGlobal(moduleName, importName, type));
                context.GlobalContext.Define(importId, new LocalOrImportRef(true, importIndex));
                AssertEmpty(context, "global", importTail.Skip(1).ToArray());
            }
            else if (importDesc.IsCallTo("table"))
            {
                var type = AssembleTableType(importDesc, importTail, context);
                var importIndex = module.AddImport(new ImportedTable(moduleName, importName, type));
                context.TableContext.Define(importId, new LocalOrImportRef(true, importIndex));
            }
            else
            {
                context.Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "syntax error",
                        Quotation.QuoteEvenInBold(
                            "unexpected expression in import; expected ",
                            "func", ",", "table", ",", "memory", " or ", "global", "."),
                        Highlight(importDesc)));
            }
        }

        private static void AssembleTable(
            SExpression moduleField,
            WasmFile module,
            ModuleContext context)
        {
            string tableId = null;
            var tail = moduleField.Tail;
            if (moduleField.Tail.Count > 0 && moduleField.Tail[0].IsIdentifier)
            {
                tableId = (string)moduleField.Tail[0].Head.Value;
                tail = tail.Skip(1).ToArray();
            }

            var exportNames = new List<string>();
            while (tail.Count > 0 && tail[0].IsCallTo("export"))
            {
                var exportExpr = tail[0];
                if (!AssertElementCount(exportExpr, exportExpr.Tail, 1, context))
                {
                    continue;
                }

                exportNames.Add(AssembleString(exportExpr.Tail[0], context));
                tail = tail.Skip(1).ToArray();
            }

            if (!AssertNonEmpty(moduleField, tail, "table definition", context))
            {
                return;
            }

            LocalOrImportRef tableRef;
            if (tail[0].IsCallTo("import"))
            {
                var (moduleName, importName) = AssembleInlineImport(tail[0], context);
                var table = AssembleTableType(moduleField, tail.Skip(1).ToArray(), context);
                var tableIndex = module.AddImport(new ImportedTable(moduleName, importName, table));
                tableRef = new LocalOrImportRef(true, tableIndex);
                context.TableContext.Define(tableId, tableRef);
            }
            else if (tail[0].Head.Kind == Lexer.TokenKind.Keyword)
            {
                var elemType = AssembleElemType(tail[0], context);
                var table = new TableType(elemType, new ResizableLimits(0));
                var tableIndex = module.AddTable(table);
                tableRef = new LocalOrImportRef(false, tableIndex);
                context.TableContext.Define(tableId, tableRef);

                if (!AssertElementCount(moduleField, tail, 2, context))
                {
                    return;
                }

                var elems = tail[1];
                if (!elems.IsCallTo("elem"))
                {
                    context.Log.Log(
                        new LogEntry(
                            Severity.Error,
                            "syntax error",
                            Quotation.QuoteEvenInBold(
                                "unexpected expression in initialized table; expected an ",
                                "elem", " expression."),
                            Highlight(elems)));
                }

                var elemSegment = new ElementSegment(
                    0,
                    new InitializerExpression(Operators.Int32Const.Create(0)),
                    Enumerable.Empty<uint>());
                module.AddElementSegment(elemSegment);

                context.TableContext.Use(tableRef, index => { elemSegment.TableIndex = index; });

                for (int i = 0; i < elems.Tail.Count; i++)
                {
                    elemSegment.Elements.Add(0);
                    var functionId = AssembleIdentifierOrIndex(elems.Tail[i], context);
                    var j = i;
                    context.FunctionContext.Use(functionId, index => { elemSegment.Elements[j] = index; });
                }
            }
            else
            {
                var table = AssembleTableType(moduleField, tail, context);
                var tableIndex = module.AddTable(table);
                tableRef = new LocalOrImportRef(false, tableIndex);
                context.TableContext.Define(tableId, tableRef);
            }

            foreach (var exportName in exportNames)
            {
                AddExport(module, context.TableContext, tableRef, ExternalKind.Table, exportName);
            }
        }

        private static TableType AssembleTableType(SExpression parent, IReadOnlyList<SExpression> tail, ModuleContext context)
        {
            if (tail.Count < 2 || tail.Count > 3)
            {
                context.Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "syntax error",
                        Quotation.QuoteEvenInBold(
                            "expected a table type, that is, resizable limits followed by ",
                            "funcref",
                            ", the table element type."),
                        Highlight(parent)));
                return new TableType(WasmType.AnyFunc, new ResizableLimits(0));
            }

            var limits = AssembleLimits(parent, tail.Take(tail.Count - 1).ToArray(), context);
            var elemType = AssembleElemType(tail[tail.Count - 1], context);
            return new TableType(WasmType.AnyFunc, limits);
        }

        private static WasmType AssembleElemType(SExpression expression, ModuleContext context)
        {
            if (!expression.IsSpecificKeyword("funcref"))
            {
                context.Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "syntax error",
                        Quotation.QuoteEvenInBold(
                            "unexpected table type expression; expected ",
                            "funcref", "."),
                        Highlight(expression)));
            }
            return WasmType.AnyFunc;
        }

        private static GlobalType AssembleGlobalType(SExpression expression, ModuleContext context)
        {
            if (expression.IsCallTo("mut"))
            {
                if (!AssertElementCount(expression, expression.Tail, 1, context))
                {
                    return new GlobalType(WasmValueType.Int32, true);
                }

                return new GlobalType(AssembleValueType(expression.Tail[0], context), true);
            }
            else
            {
                return new GlobalType(AssembleValueType(expression, context), false);
            }
        }

        private static FunctionType AssembleTypeUse(
            SExpression parent,
            ref IReadOnlyList<SExpression> tail,
            ModuleContext context)
        {
            var result = new FunctionType();

            // TODO: parse optional leading 'type' expression.

            // Parse parameters.
            while (tail.Count > 0 && tail[0].IsCallTo("param"))
            {
                var paramSpec = tail[0];
                var paramTail = paramSpec.Tail;
                if (paramTail.Count > 0 && paramTail[0].IsIdentifier)
                {
                    // TODO: actually parse these identifiers.
                    paramTail = paramTail.Skip(1).ToArray();
                    if (!AssertNonEmpty(paramSpec, paramTail, "param", context))
                    {
                        continue;
                    }

                    var valType = AssembleValueType(paramTail[0], context);
                    result.ParameterTypes.Add(valType);

                    paramTail = paramTail.Skip(1).ToArray();
                    AssertEmpty(context, "param", paramTail);
                }
                else
                {
                    result.ParameterTypes.AddRange(paramTail.Select(x => AssembleValueType(x, context)));
                }
                tail = tail.Skip(1).ToArray();
            }

            // Parse results.
            while (tail.Count > 0 && tail[0].IsCallTo("result"))
            {
                var resultSpec = tail[0];
                var resultTail = resultSpec.Tail;
                result.ReturnTypes.AddRange(resultTail.Select(x => AssembleValueType(x, context)));
                tail = tail.Skip(1).ToArray();
            }

            return result;
        }

        private static WasmValueType AssembleValueType(SExpression expression, ModuleContext context)
        {
            if (expression.IsSpecificKeyword("i32"))
            {
                return WasmValueType.Int32;
            }
            else if (expression.IsSpecificKeyword("i64"))
            {
                return WasmValueType.Int64;
            }
            else if (expression.IsSpecificKeyword("f32"))
            {
                return WasmValueType.Float32;
            }
            else if (expression.IsSpecificKeyword("f64"))
            {
                return WasmValueType.Float64;
            }
            else
            {
                context.Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "unexpected token",
                        Quotation.QuoteEvenInBold(
                            "unexpected a value type, that is, ",
                            "i32", ",", "i64", ",", "f32", " or ", "f64", "."),
                        Highlight(expression)));
                return WasmValueType.Int32;
            }
        }

        private static void AddExport<T>(
            WasmFile module,
            IdentifierContext<T> context,
            T value,
            ExternalKind kind,
            string exportName)
        {
            var export = new ExportedValue(exportName, kind, 0);
            module.AddExport(export);
            var exportSection = module.GetFirstSectionOrNull<ExportSection>();
            int index = exportSection.Exports.Count - 1;
            context.Use(
                value,
                i => { exportSection.Exports[index] = new ExportedValue(exportName, kind, i); });
        }

        private static void AddExport<T>(
            WasmFile module,
            IdentifierContext<T> context,
            Lexer.Token identifier,
            ExternalKind kind,
            string exportName)
        {
            var export = new ExportedValue(exportName, kind, 0);
            module.AddExport(export);
            var exportSection = module.GetFirstSectionOrNull<ExportSection>();
            int index = exportSection.Exports.Count - 1;
            context.Use(
                identifier,
                i => { exportSection.Exports[index] = new ExportedValue(exportName, kind, i); });
        }

        private static Lexer.Token AssembleIdentifierOrIndex(SExpression expression, ModuleContext context)
        {
            if (expression.Head.Kind != Lexer.TokenKind.UnsignedInteger
                && expression.Head.Kind != Lexer.TokenKind.Identifier)
            {
                context.Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "syntax error",
                        "expected an identifier or unsigned integer.",
                        Highlight(expression)));
            }
            return expression.Head;
        }

        private static (string, string) AssembleInlineImport(SExpression import, ModuleContext context)
        {
            if (import.Tail.Count != 2)
            {
                context.Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "syntax error",
                        Quotation.QuoteEvenInBold(
                            "encountered ",
                            import.Tail.Count.ToString(),
                            " elements; expected exactly two names."),
                        Highlight(import)));
                return ("", "");
            }
            else
            {
                return (AssembleString(import.Tail[0], context), AssembleString(import.Tail[1], context));
            }
        }

        private static string AssembleString(SExpression expression, ModuleContext context)
        {
            if (expression.IsCall || expression.Head.Kind != Lexer.TokenKind.String)
            {
                context.Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "syntax error",
                        Quotation.QuoteEvenInBold(
                            "expected a string literal."),
                        Highlight(expression)));
                return "";
            }
            else
            {
                return (string)expression.Head.Value;
            }
        }

        private static bool AssertElementCount(
            SExpression expression,
            IReadOnlyList<SExpression> tail,
            int count,
            ModuleContext context)
        {
            if (tail.Count == count)
            {
                return true;
            }
            else
            {
                context.Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "syntax error",
                        Quotation.QuoteEvenInBold(
                            "encountered ",
                            tail.Count.ToString(),
                            " elements; expected exactly ", count.ToString(), "."),
                        Highlight(expression)));
                return false;
            }
        }

        private static bool AssertNonEmpty(
            SExpression expression,
            IReadOnlyList<SExpression> tail,
            string kind,
            ModuleContext context)
        {
            if (tail.Count == 0)
            {
                context.Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "syntax error",
                        Quotation.QuoteEvenInBold(kind + " is unexpectedly empty."),
                        Highlight(expression)));
                return false;
            }
            else
            {
                return true;
            }
        }

        private static byte[] AssembleDataString(
            IReadOnlyList<SExpression> tail,
            ModuleContext context)
        {
            var results = new List<byte>();
            foreach (var item in tail)
            {
                results.AddRange(Encoding.UTF8.GetBytes(AssembleString(item, context)));
            }
            return results.ToArray();
        }

        private static void AssertEmpty(
            ModuleContext context,
            string kind,
            IEnumerable<SExpression> tail)
        {
            var tailArray = tail.ToArray();
            if (tailArray.Length > 0)
            {
                context.Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "syntax error",
                        Quotation.QuoteEvenInBold("", kind, " has an unexpected trailing expression."),
                        Highlight(tailArray[0])));
            }
        }

        private static ResizableLimits AssembleLimits(SExpression parent, IReadOnlyList<SExpression> tail, ModuleContext context)
        {
            if (tail.Count == 0)
            {
                context.Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "syntax error",
                        "limits expression is empty.",
                        Highlight(parent)));
                return new ResizableLimits(0);
            }

            var init = AssembleUInt32(tail[0], context);

            if (tail.Count == 1)
            {
                return new ResizableLimits(init);
            }
            if (tail.Count == 2)
            {
                var max = AssembleUInt32(tail[1], context);
                return new ResizableLimits(init, max);
            }
            else
            {
                context.Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "syntax error",
                        "limits expression contains more than two elements.",
                        Highlight(tail[2])));
                return new ResizableLimits(0);
            }
        }

        private static uint AssembleUInt32(
            SExpression expression,
            ModuleContext context)
        {
            if (expression.IsCall)
            {
                context.Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "syntax error",
                        Quotation.QuoteEvenInBold("expected a 32-bit unsigned integer; got a call."),
                        Highlight(expression)));
                return 0;
            }
            else if (expression.Head.Kind != Lexer.TokenKind.UnsignedInteger)
            {
                context.Log.Log(
                    new LogEntry(
                        Severity.Error,
                        "syntax error",
                        Quotation.QuoteEvenInBold("expected a 32-bit unsigned integer; got other token."),
                        Highlight(expression)));
                return 0;
            }
            else
            {
                var data = (BigInteger)expression.Head.Value;
                if (data <= uint.MaxValue)
                {
                    return (uint)data;
                }
                else
                {
                    context.Log.Log(
                        new LogEntry(
                            Severity.Error,
                            "syntax error",
                            Quotation.QuoteEvenInBold("expected a 32-bit unsigned integer; got an unsigned integer that is out of range."),
                            Highlight(expression)));
                    return 0;
                }
            }
        }
    }
}
