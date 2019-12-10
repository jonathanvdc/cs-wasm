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
        public InterpreterContext(
            ModuleInstance module,
            IReadOnlyList<WasmValueType> returnTypes,
            IReadOnlyList<Variable> locals)
            : this(module, returnTypes, locals, ExecutionPolicy.Create())
        { }

        /// <summary>
        /// Creates a new interpreter context from the given module, return types
        /// and list of local variables.
        /// </summary>
        /// <param name="module">The owning module.</param>
        /// <param name="returnTypes">The list of expected return types.</param>
        /// <param name="locals">The list of local variables in this context.</param>
        /// <param name="policy">The execution policy to use.</param>
        /// <param name="callStackDepth">
        /// The current depth of the call stack.
        /// </param>
        public InterpreterContext(
            ModuleInstance module,
            IReadOnlyList<WasmValueType> returnTypes,
            IReadOnlyList<Variable> locals,
            ExecutionPolicy policy,
            uint callStackDepth = 0)
        {
            this.Module = module;
            this.ReturnTypes = returnTypes;
            this.Locals = locals;
            this.Policy = policy;
            this.valStack = new Stack<object>();
            this.ReturnValues = null;
            this.BreakDepth = -1;
            this.CallStackDepth = callStackDepth;
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
        /// Gets the execution policy associated with this interpreter context.
        /// </summary>
        /// <value>An execution policy.</value>
        public ExecutionPolicy Policy { get; private set; }

        /// <summary>
        /// Gets the depth of the call stack at which the "frame" is placed for
        /// the instructions currently being executed.
        /// </summary>
        /// <value>A call stack depth.</value>
        public uint CallStackDepth { get; private set; }

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

    /// <summary>
    /// A description of an execution policy for WebAssembly modules.
    /// </summary>
    public sealed class ExecutionPolicy
    {
        private ExecutionPolicy()
        { }

        /// <summary>
        /// Creates a new execution policy.
        /// </summary>
        /// <param name="maxCallStackDepth">The maximal depth of the call stack.</param>
        /// <param name="maxMemorySize">
        /// The maximum size of any memory, in page units. A value of zero
        /// indicates that there is not maximum memory size.
        /// </param>
        /// <param name="enforceAlignment">
        /// Tells if memory access alignments should be taken to be normative instead
        /// of as hints.
        /// </param>
        /// <param name="translateExceptions">
        /// Tells if CLR exceptions should be translated to <see cref="TrapException"/> values.
        /// </param>
        public static ExecutionPolicy Create(
            uint maxCallStackDepth = 512,
            uint maxMemorySize = 0,
            bool enforceAlignment = false,
            bool translateExceptions = true)
        {
            return new ExecutionPolicy()
            {
                MaxCallStackDepth = maxCallStackDepth,
                EnforceAlignment = enforceAlignment,
                MaxMemorySize = maxMemorySize,
                TranslateExceptions = translateExceptions
            };
        }

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
        /// Gets the maximal depth of the call stack.
        /// </summary>
        /// <value>A maximal call stack depth.</value>
        public uint MaxCallStackDepth { get; private set; }

        /// <summary>
        /// Gets the maximum size of any memory, in page units. A value of zero
        /// indicates that there is not maximum memory size.
        /// </summary>
        /// <value>The maximum memory size.</value>
        public uint MaxMemorySize { get; private set; }

        /// <summary>
        /// Tells if CLR exceptions should be translated to <see cref="TrapException"/> values.
        /// </summary>
        /// <value>
        /// <c>true</c> if WebAssembly execution should throw only <see cref="TrapException"/> values;
        /// <c>false</c> if it may also throw other types of exceptions.
        /// </value>
        public bool TranslateExceptions { get; private set; }
    }
}
