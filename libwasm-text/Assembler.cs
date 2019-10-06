using System.Collections.Generic;
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
        {
            this.Log = log;
        }

        /// <summary>
        /// Gets the log that is used for reporting diagnostics.
        /// </summary>
        /// <value>A log.</value>
        public ILog Log { get; private set; }

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
    }
}
