using System;

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

            if (!IsInstanceOf<T>(Value, Type))
            {
                throw new WasmException(
                    "Cannot assign a value of type '" + Value.GetType().Name +
                    "' to a variable of type '" + ((object)Type).ToString() + "'.");
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
            if (!IsInstanceOf<T>(Value, Type))
            {
                throw new WasmException(
                    "Cannot create a variable of type '" + ((object)Type).ToString() +
                    "' with an initial value of type '" + Value.GetType().Name + "'.");
            }

            return new Variable(Value, Type, IsMutable);
        }

        /// <summary>
        /// Creates a new variable of the given type and mutability, and initializes
        /// it with the default value for the given type.
        /// </summary>
        /// <param name="Type">The variable's type.</param>
        /// <param name="IsMutable">The variable's mutability.</param>
        /// <returns>The newly-created variable.</returns>
        public static Variable CreateDefault(WasmValueType Type, bool IsMutable)
        {
            return Create<object>(Type, IsMutable, GetDefaultValue(Type));
        }

        /// <summary>
        /// Gets the default value for the given WebAssembly value tyoe.
        /// </summary>
        /// <param name="Type">A WebAssembly value type.</param>
        /// <returns>The default value.</returns>
        public static object GetDefaultValue(WasmValueType Type)
        {
            switch (Type)
            {
                case WasmValueType.Int32:
                    return default(int);
                case WasmValueType.Int64:
                    return default(long);
                case WasmValueType.Float32:
                    return default(float);
                case WasmValueType.Float64:
                    return default(double);
                default:
                    throw new WasmException("Unknown value type: " + Type);
            }
        }

        /// <summary>
        /// Checks if the given value is an instance of the given WebAssembly value type.
        /// </summary>
        /// <param name="Value">A value.</param>
        /// <param name="Type">A WebAssembly value type.</param>
        /// <returns>
        /// <c>true</c> if the given value is an instance of the given WebAssembly value type;
        /// otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInstanceOf<T>(T Value, WasmValueType Type)
        {
            switch (Type)
            {
                case WasmValueType.Int32:
                    return Value is int;
                case WasmValueType.Int64:
                    return Value is long;
                case WasmValueType.Float32:
                    return Value is float;
                case WasmValueType.Float64:
                    return Value is double;
                default:
                    throw new WasmException("Unknown value type: " + Type);
            }
        }
    }
}