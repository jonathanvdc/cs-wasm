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
                        new HighlightedSource(new SourceRegion(expression.Head.Span))));
            }
            return file;
        }
    }
}
