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
        public Variable(object Value)
        {
            this.Value = Value;
        }

        /// <summary>
        /// Gets or sets this variable's value.
        /// </summary>
        /// <returns>The variable's value.</returns>
        public object Value { get; set; }
    }
}