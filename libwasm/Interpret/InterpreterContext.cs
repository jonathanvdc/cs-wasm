using System.Collections.Generic;
using System.Linq;

namespace Wasm.Interpret
{
    /// <summary>
    /// Describes the context in which instructions are interpreted.
    /// </summary>
    public sealed class InterpreterContext
    {
        /// <summary>
        /// Creates a new interpreter context from the given module and
        /// expected return types.
        /// </summary>
        /// <param name="module">The owning module.</param>
        /// <param name="returnTypes">The list of expected return types.</param>
        public InterpreterContext(ModuleInstance module, IReadOnlyList<WasmValueType> returnTypes)
            : this(module, returnTypes, new Variable[0])
        { }

        /// <summary>
        /// Creates a new interpreter context from the given module, return types
        /// and list of local variables.
        /// </summary>
        /// <param name="module">The owning module.</param>
        /// <param name="returnTypes">The list of expected return types.</param>
        /// <param name="locals">The list of local variables in this context.</param>
        /// <param name="enforceAlignment">
        /// Tells if memory access alignments should be taken to be normative instead
        /// of as hints.
        /// </param>
        public InterpreterContext(
            ModuleInstance module,
            IReadOnlyList<WasmValueType> returnTypes,
            IReadOnlyList<Variable> locals,
            bool enforceAlignment = false)
        {
            this.Module = module;
            this.ReturnTypes = returnTypes;
            this.Locals = locals;
            this.EnforceAlignment = enforceAlignment;
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
        /// Gets the list of expected return types.
        /// </summary>
        /// <value>The expected return types.</value>
        public IReadOnlyList<WasmValueType> ReturnTypes { get; private set; }

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
        /// Gets or sets the evaluation stack.
        /// </summary>
        /// <value>The evaluation stack.</value>
        public EvaluationStack Stack
        {
            get
            {
                return new EvaluationStack { stack = valStack };
            }
            set
            {
                this.valStack = value.stack;
            }
        }

        /// <summary>
        /// Gets the list of values that have been returned, or <c>null</c> if nothing
        /// has been returned yet.
        /// </summary>
        /// <returns>The list of values that have been returned, or <c>null</c> if nothing
        /// has been returned yet.</returns>
        public IReadOnlyList<object> ReturnValues { get; private set; }

        /// <summary>
        /// Tells if the alignment specified by memory instructions is to be taken as
        /// a mandatory alignment to which memory accesses must adhere instead of a mere
        /// hint.
        /// </summary>
        /// <value><c>true</c> if unaligned accesses must throw exceptions; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// The WebAssembly specification states that memory instruction alignments do not
        /// affect execution semantics. In order to comply with the standard, this property
        /// should be set to <c>false</c> (the default).
        /// </remarks>
        public bool EnforceAlignment { get; private set; }

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
        /// Pushes the given sequence of values onto the value stack.
        /// </summary>
        /// <param name="values">The list of values to push onto the stack.</param>
        public void Push<T>(IEnumerable<T> values)
        {
            foreach (var item in values)
            {
                Push<T>(item);
            }
        }

        /// <summary>
        /// Pushes the contents of an evaluation stack onto this context's stack.
        /// </summary>
        /// <param name="stack">The stack to push onto this context's evaluation stack.</param>
        public void Push(EvaluationStack stack)
        {
            Push<object>(stack.stack.Reverse());
        }

        /// <summary>
        /// Pushes the topmost <paramref name="count"/> elements of <paramref name="stack"/> onto this context's
        /// evaluation stack.
        /// </summary>
        /// <param name="stack">The stack to push onto this context's evaluation stack.</param>
        /// <param name="count">
        /// The number of elements to take from <paramref name="stack"/> and push onto this
        /// context's evaluation stack.
        /// </param>
        public void Push(EvaluationStack stack, int count)
        {
            Push<object>(stack.stack.Take(count).Reverse());
        }

        /// <summary>
        /// Creates an empty evaluation stack.
        /// </summary>
        /// <returns>An empty evaluation stack.</returns>
        public EvaluationStack CreateStack()
        {
            return new EvaluationStack { stack = new Stack<object>() };
        }

        /// <summary>
        /// A data structure that represents the interpreter's value stack.
        /// </summary>
        public struct EvaluationStack
        {
            // Internal on purpose so we can keep the 'Stack<object>' an
            // implementation detail.
            internal Stack<object> stack;
        }
    }
}