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
        /// <param name="localTypes">A list of all local variable types including parameters.</param>
        /// <param name="parameterCount">The number of parameters defined for the function being compiled.</param>
        /// <param name="locals">A mapping of indices to local variables.</param>
        public CompilerContext(
            JitCompiler compiler,
            IReadOnlyList<WasmValueType> localTypes,
            int parameterCount,
            IReadOnlyDictionary<uint, LocalBuilder> locals)
        {
            this.Compiler = compiler;
            this.LocalTypes = localTypes;
            this.ParameterCount = parameterCount;
            this.Locals = locals;
            this.StackContents = new Stack<WasmValueType>();
        }

        /// <summary>
        /// Gets the JIT compiler that initiated the compilation.
        /// </summary>
        /// <value>A JIT compiler.</value>
        public JitCompiler Compiler { get; private set; }

        /// <summary>
        /// Gets the number of parameters defined for the function being compiled.
        /// </summary>
        /// <value>A parameter count.</value>
        public int ParameterCount { get; private set; }

        /// <summary>
        /// Gets a list of local variables types. All local variables including
        /// parameters appear in this list.
        /// </summary>
        /// <value>A list of all local variable types.</value>
        public IReadOnlyList<WasmValueType> LocalTypes { get; private set; }

        /// <summary>
        /// Gets a mapping of local variable indices to local variables. Note that
        /// only true local variables appear in this list; arguments do not.
        /// </summary>
        /// <value>A mapping of indices to local variables.</value>
        public IReadOnlyDictionary<uint, LocalBuilder> Locals { get; internal set; }

        /// <summary>
        /// Gets the types of the values on the evaluation stack.
        /// </summary>
        /// <value>A stack of types.</value>
        public Stack<WasmValueType> StackContents { get; private set; }

        /// <summary>
        /// Informs that compiler context that a value is pushed onto the stack at the
        /// current point in the code generation process.
        /// </summary>
        /// <param name="type">The type of the value that is pushed on the stack.</param>
        public void Push(WasmValueType type)
        {
            StackContents.Push(type);
        }

        /// <summary>
        /// Informs the compiler context that a number of values are popped from the stack
        /// at this point. Returns theit types.
        /// </summary>
        /// <param name="count">The number of values to pop from the stack.</param>
        /// <returns>
        /// The types of the <paramref name="count"/> topmost values on the stack, in the
        /// order those types were pushed onto the stack.
        /// </returns>
        public IReadOnlyList<WasmValueType> Pop(int count)
        {
            var result = new Stack<WasmValueType>();
            for (int i = 0; i < count; i++)
            {
                result.Push(StackContents.Pop());
            }
            return result.ToArray();
        }

        /// <summary>
        /// Informs the compiler context that a value is popped from the stack
        /// at this point. Returns its type.
        /// </summary>
        /// <returns>
        /// The types of thetopmost value on the stack.
        /// </returns>
        public WasmValueType Pop()
        {
            return StackContents.Pop();
        }
    }
}
