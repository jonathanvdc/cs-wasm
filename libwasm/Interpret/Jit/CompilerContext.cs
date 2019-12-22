namespace Wasm.Interpret.Jit
{
    /// <summary>
    /// Context that is used when compiling a function body.
    /// </summary>
    public sealed class CompilerContext
    {
        /// <summary>
        /// Creates a compiler context.
        /// </summary>
        /// <param name="compiler">The JIT compiler itself.</param>
        public CompilerContext(JitCompiler compiler)
        {
            this.Compiler = compiler;
        }

        /// <summary>
        /// Gets the JIT compiler that initiated the compilation.
        /// </summary>
        /// <value>A JIT compiler.</value>
        public JitCompiler Compiler { get; private set; }
    }
}
