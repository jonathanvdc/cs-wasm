namespace Wasm.Interpret
{
    /// <summary>
    /// Describes a WebAssembly variable.
    /// </summary>
    public sealed class Variable
    {
        /// <summary>
        /// Creates a variable with the given value, type and mutability.
        /// </summary>
        /// <param name="Value">The variable's value.</param>
        /// <param name="Type">The variable's type.</param>
        /// <param name="IsMutable">The variable's mutability.</param>
        private Variable(object Value, WasmValueType Type, bool IsMutable)
        {
            this.val = Value;
            this.Type = Type;
            this.IsMutable = IsMutable;
        }

        /// <summary>
        /// The variable's value.
        /// </summary>
        private object val;

        /// <summary>
        /// Gets this variable's type.
        /// </summary>
        /// <returns>The variable's type.</returns>
        public WasmValueType Type { get; private set; }

        /// <summary>
        /// Gets this variable's mutability.
        /// </summary>
        /// <returns>The variable's mutability.</returns>
        public bool IsMutable { get; private set; }

        /// <summary>
        /// Gets this variable's value.
        /// </summary>
        /// <returns>The variable's value.</returns>
        public T Get<T>()
        {
            return (T)val;
        }

        /// <summary>
        /// Sets this variable's value.
        /// </summary>
        /// <param name="Value">The variable's new value.</param>
        public void Set<T>(T Value)
        {
            if (!IsMutable)
            {
                throw new WasmException("Cannot assign a value to an immutable variable.");
            }

            val = Value;
        }

        /// <summary>
        /// Creates a new variable from the given value.
        /// </summary>
        /// <param name="Type">The variable's type.</param>
        /// <param name="IsMutable">The variable's mutability.</param>
        /// <param name="Value">The variable's initial value.</param>
        /// <returns>The newly-created variable.</returns>
        public static Variable Create<T>(WasmValueType Type, bool IsMutable, T Value)
        {
            return new Variable(Value, Type, IsMutable);
        }
    }
}