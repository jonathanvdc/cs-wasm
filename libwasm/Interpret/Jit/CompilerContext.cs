using System.Collections.Generic;
using System.Reflection.Emit;

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
        /// <param name="parameterTypes">The list of parameter types for the function being compiled.</param>
        /// <param name="locals">A mapping of indices to local variables.</param>
        public CompilerContext(
            JitCompiler compiler,
            IReadOnlyList<WasmValueType> parameterTypes,
            IReadOnlyDictionary<uint, LocalBuilder> locals)
        {
            this.Compiler = compiler;
            this.ParameterTypes = parameterTypes;
            this.Locals = locals;
        }

        /// <summary>
        /// Gets the JIT compiler that initiated the compilation.
        /// </summary>
        /// <value>A JIT compiler.</value>
        public JitCompiler Compiler { get; private set; }

        /// <summary>
        /// Gets the list of parameter types for the function being compiled.
        /// </summary>
        /// <value>A list of parameter types.</value>
        public IReadOnlyList<WasmValueType> ParameterTypes { get; internal set; }

        /// <summary>
        /// Gets a mapping of local variable indices to local variables. Note that
        /// only true local variables appear in this list; arguments do not.
        /// </summary>
        /// <value>A mapping of indices to local variables.</value>
        public IReadOnlyDictionary<uint, LocalBuilder> Locals { get; internal set; }
    }
}
