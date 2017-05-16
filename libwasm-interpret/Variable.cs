namespace Wasm.Interpret
{
    /// <summary>
    /// Describes a WebAssembly variable.
    /// </summary>
    public sealed class Variable
    {
        /// <summary>
        /// Creates a variable with the given value.
        /// </summary>
        /// <param name="Value">The variable's value.</param>
        private Variable(object Value)
        {
            this.val = Value;
        }

        /// <summary>
        /// The variable's value.
        /// </summary>
        private object val;

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
            val = Value;
        }

        /// <summary>
        /// Creates a new variable from the given value.
        /// </summary>
        /// <param name="Value">The variable's initial value.</param>
        /// <returns>The newly-created variable.</returns>
        public static Variable Create<T>(T Value)
        {
            return new Variable(Value);
        }
    }
}