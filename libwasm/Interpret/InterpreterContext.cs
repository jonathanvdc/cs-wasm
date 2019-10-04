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
        /// <param name="module">The owning module.</param>
        public InterpreterContext(ModuleInstance module)
            : this(module, new Variable[0])
        { }

        /// <summary>
        /// Creates a new interpreter context from the given module and list of
        /// local variables.
        /// </summary>
        /// <param name="module">The owning module.</param>
        /// <param name="locals">The list of local variables in this context.</param>
        public InterpreterContext(ModuleInstance module, IReadOnlyList<Variable> locals)
        {
            this.Module = module;
            this.Locals = locals;
            this.valStack = new Stack<object>();
            this.ReturnValues = null;
            this.BreakDepth = -1;
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
        /// The evaluation stack.
        /// </summary>
        private Stack<object> valStack;

        /// <summary>
        /// Gets the list of values that have been returned, or <c>null</c> if nothing
        /// has been returned yet.
        /// </summary>
        /// <returns>The list of values that have been returned, or <c>null</c> if nothing
        /// has been returned yet.</returns>
        public IReadOnlyList<object> ReturnValues { get; private set; }

        /// <summary>
        /// Gets the number of items that are currently on the evaluation stack.
        /// </summary>
        public int StackDepth => valStack.Count;

        /// <summary>
        /// Tests if this interpreter context has returned.
        /// </summary>
        public bool HasReturned => ReturnValues != null;

        /// <summary>
        /// Gets or sets the depth of the break that is currently being handled.
        /// </summary>
        /// <returns>The depth of the break that is currently being handled.
        /// A negative value means that no break is currently being handled.</returns>
        public int BreakDepth { get; set; }

        /// <summary>
        /// Gets a flag that tells if a break has been requested.
        /// </summary>
        /// <returns>A flag that tells if a break has been requested.</returns>
        public bool BreakRequested => BreakDepth >= 0;

        /// <summary>
        /// Pops a value of the given type from the value stack.
        /// </summary>
        /// <returns>The popped value.</returns>
        public T Pop<T>()
        {
            if (StackDepth == 0)
            {
                throw new WasmException("Cannot pop an element from an empty stack.");
            }

            return (T)valStack.Pop();
        }

        /// <summary>
        /// Pops an array of values of the given type from the value stack.
        /// </summary>
        /// <returns>The popped values.</returns>
        public T[] Pop<T>(int count)
        {
            var results = new T[count];
            for (int i = count - 1; i >= 0; i--)
            {
                results[i] = Pop<T>();
            }
            return results;
        }

        /// <summary>
        /// Sets the list of return values to the contents of the value stack,
        /// if nothing has been returned already.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the contents of the value stack have been promoted
        /// to return values; otherwise, <c>false</c>.
        /// </returns>
        public bool Return()
        {
            if (HasReturned)
            {
                return false;
            }
            else
            {
                ReturnValues = valStack.ToArray();
                return true;
            }
        }

        /// <summary>
        /// Peeks a value of the given type from the value stack.
        /// </summary>
        /// <returns>The peeked value.</returns>
        public T Peek<T>()
        {
            if (StackDepth == 0)
            {
                throw new WasmException("Cannot peek an element from an empty stack.");
            }

            return (T)valStack.Peek();
        }

        /// <summary>
        /// Pushes the given value onto the value stack.
        /// </summary>
        /// <param name="value">The value to push onto the stack.</param>
        public void Push<T>(T value)
        {
            valStack.Push(value);
        }

        /// <summary>
        /// Pushes the given list of values onto the value stack.
        /// </summary>
        /// <param name="values">The list of values to push onto the stack.</param>
        public void Push<T>(IReadOnlyList<T> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                Push<T>(values[i]);
            }
        }
    } 
}