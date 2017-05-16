using System.Collections.Generic;

namespace Wasm.Interpret
{
    /// <summary>
    /// Describes the context in which instructions are interpreted.
    /// </summary>
    public sealed class InterpreterContext
    {
        /// <summary>
        /// Creates a new interpreter context from the given module.
        /// </summary>
        /// <param name="Module">The owning module.</param>
        public InterpreterContext(ModuleInstance Module)
            : this(Module, new Variable[0])
        { }

        /// <summary>
        /// Creates a new interpreter context from the given module and list of
        /// local variables.
        /// </summary>
        /// <param name="Module">The owning module.</param>
        /// <param name="Locals">The list of local variables in this context.</param>
        public InterpreterContext(ModuleInstance Module, IReadOnlyList<Variable> Locals)
        {
            this.Module = Module;
            this.Locals = Locals;
            this.valStack = new Stack<object>();
        }

        /// <summary>
        /// Gets the module instance that owns the instructions being interpreted.
        /// </summary>
        /// <returns>The module instance.</returns>
        public ModuleInstance Module { get; private set; }

        /// <summary>
        /// Gets a list of local variables for this interpreter context.
        /// </summary>
        /// <returns>A list of local variables.</returns>
        public IReadOnlyList<Variable> Locals { get; private set; }

        /// <summary>
        /// The evaluation stack stack.
        /// </summary>
        private Stack<object> valStack;

        /// <summary>
        /// Gets the number of items that are currently on the evaluation stack.
        /// </summary>
        public int StackDepth => valStack.Count;

        /// <summary>
        /// Pops a value of the given type from the value stack.
        /// </summary>
        /// <returns>The popped value.</returns>
        public T Pop<T>()
        {
            return (T)valStack.Pop();
        }

        /// <summary>
        /// Peeks a value of the given type from the value stack.
        /// </summary>
        /// <returns>The peeked value.</returns>
        public T Peek<T>()
        {
            return (T)valStack.Peek();
        }

        /// <summary>
        /// Pushes the given value onto the value stack.
        /// </summary>
        /// <param name="Value">The value to push onto the stack.</param>
        public void Push<T>(T Value)
        {
            valStack.Push(Value);
        }
    } 
}