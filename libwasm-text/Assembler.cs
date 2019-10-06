using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Pixie;
using Pixie.Code;
using Pixie.Markup;

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
                        "top-level modules must be encoded as S-expressions that call 'module'.",
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

        private static HighlightedSource Highlight(SExpression expression)
        {
            return new HighlightedSource(new SourceRegion(expression.Head.Span));
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
                this.MemoryContext = IdentifierContext.Create();
            }

            /// <summary>
            /// Gets the memory context for the module.
            /// </summary>
            /// <value>A memory context.</value>
            public IdentifierContext MemoryContext { get; private set; }

            /// <summary>
            /// Gets the assembler that gives rise to this context.
            /// </summary>
            /// <value>An assembler.</value>
            public Assembler Assembler { get; private set; }
        }

        /// <summary>
        /// An identifier context, which maps identifiers to indices.
        /// </summary>
        public struct IdentifierContext
        {
            /// <summary>
            /// Creates an empty identifier context.
            /// </summary>
            /// <returns>An identifier context.</returns>
            public static IdentifierContext Create()
            {
                return new IdentifierContext()
                {
                    identifiers = new Dictionary<string, uint>()
                };
            }

            private Dictionary<string, uint> identifiers;

            /// <summary>
            /// Defines a new identifier.
            /// </summary>
            /// <param name="identifier">The identifier to define.</param>
            /// <param name="index">The index to associate with the identifier.</param>
            /// <returns>
            /// <c>true</c> if there was no previous definition of the identifier; otherwise, <c>false</c>.
            /// </returns>
            public bool Define(string identifier, uint index)
            {
                if (identifiers.ContainsKey(identifier))
                {
                    return false;
                }
                else
                {
                    identifiers[identifier] = index;
                    return true;
                }
            }

            /// <summary>
            /// Tries to map an identifier to its associated index.
            /// </summary>
            /// <param name="identifier">An identifier to inspect.</param>
            /// <param name="index">The associated index.</param>
            /// <returns>
            /// <c>true</c> if an index was found for <paramref name="identifier"/>; otherwise, <c>false</c>.
            /// </returns>
            public bool TryUse(string identifier, out uint index)
            {
                return identifiers.TryGetValue(identifier, out index);
            }

            /// <summary>
            /// Tries to map an identifier or index to its associated index.
            /// </summary>
            /// <param name="identifierOrIndex">An identifier or index to inspect.</param>
            /// <param name="index">The associated index.</param>
            /// <returns>
            /// <c>true</c> if an index was found for <paramref name="identifierOrIndex"/>; otherwise, <c>false</c>.
            /// </returns>
            public bool TryUse(Lexer.Token identifierOrIndex, out uint index)
            {
                if (identifierOrIndex.Kind == Lexer.TokenKind.UnsignedInteger)
                {
                    index = (uint)(BigInteger)identifierOrIndex.Value;
                    return true;
                }
                else if (identifierOrIndex.Kind == Lexer.TokenKind.Identifier)
                {
                    return TryUse((string)identifierOrIndex.Value, out index);
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
        };
    }
}
