using System.Collections.Generic;

namespace Wasm.Interpret
{
    /// <summary>
    /// A base class for objects that turn module definition functions into module instance functions.
    /// </summary>
    public abstract class ModuleCompiler
    {
        /// <summary>
        /// Declares all functions in a module.
        /// </summary>
        /// <param name="module">The module to declare functions for.</param>
        /// <param name="offset">The index of the first function to define.</param>
        /// <param name="types">A list of function types, one for each function declaration.</param>
        public abstract void Initialize(ModuleInstance module, int offset, IReadOnlyList<FunctionType> types);

        /// <summary>
        /// Compiles a single function by generating code that is equivalent to <paramref name="body"/>.
        /// </summary>
        /// <param name="index">The index of the function to compile.</param>
        /// <param name="body">The function body to compile.</param>
        /// <returns>A compiled function that runs <paramref name="body"/>.</returns>
        public abstract FunctionDefinition Compile(int index, FunctionBody body);

        /// <summary>
        /// Finalizes the module's code generation.
        /// </summary>
        public abstract void Finish();
    }

    /// <summary>
    /// A module compiler that "compiles" WebAssembly function definitions by wrapping them in
    /// interpreted function instances.
    /// </summary>
    public sealed class InterpreterCompiler : ModuleCompiler
    {
        private ModuleInstance module;
        private IReadOnlyList<FunctionType> types;

        /// <inheritdoc/>
        public override void Initialize(ModuleInstance module, int offset, IReadOnlyList<FunctionType> types)
        {
            this.module = module;
            this.types = types;
        }

        /// <inheritdoc/>
        public override FunctionDefinition Compile(int index, FunctionBody body)
        {
            return new WasmFunctionDefinition(types[index], body, module);
        }

        /// <inheritdoc/>
        public override void Finish()
        {
        }
    }
}
