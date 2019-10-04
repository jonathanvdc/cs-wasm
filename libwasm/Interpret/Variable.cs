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
        /// <param name="value">The variable's value.</param>
        /// <param name="type">The variable's type.</param>
        /// <param name="isMutable">The variable's mutability.</param>
        private Variable(object value, WasmValueType type, bool isMutable)
        {
            this.val = value;
            this.Type = type;
            this.IsMutable = isMutable;
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
                    "Cannot assign a value of type '" + GetTypeName(Value) +
                    "' to a variable of type '" + ((object)Type).ToString() + "'.");
            }

            val = Value;
        }

        /// <summary>
        /// Creates a new variable from the given value.
        /// </summary>
        /// <param name="type">The variable's type.</param>
        /// <param name="isMutable">The variable's mutability.</param>
        /// <param name="value">The variable's initial value.</param>
        /// <returns>The newly-created variable.</returns>
        public static Variable Create<T>(WasmValueType type, bool isMutable, T value)
        {
            if (!IsInstanceOf<T>(value, type))
            {
                throw new WasmException(
                    "Cannot create a variable of type '" + ((object)type).ToString() +
                    "' with an initial value of type '" + GetTypeName(value) + "'.");
            }

            return new Variable(value, type, isMutable);
        }

        /// <summary>
        /// Creates a new variable of the given type and mutability, and initializes
        /// it with the default value for the given type.
        /// </summary>
        /// <param name="type">The variable's type.</param>
        /// <param name="isMutable">The variable's mutability.</param>
        /// <returns>The newly-created variable.</returns>
        public static Variable CreateDefault(WasmValueType type, bool isMutable)
        {
            return Create<object>(type, isMutable, GetDefaultValue(type));
        }

        /// <summary>
        /// Gets the default value for the given WebAssembly value tyoe.
        /// </summary>
        /// <param name="type">A WebAssembly value type.</param>
        /// <returns>The default value.</returns>
        public static object GetDefaultValue(WasmValueType type)
        {
            switch (type)
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
                    throw new WasmException("Unknown value type: " + type);
            }
        }

        /// <summary>
        /// Checks if the given value is an instance of the given WebAssembly value type.
        /// </summary>
        /// <param name="value">A value.</param>
        /// <param name="type">A WebAssembly value type.</param>
        /// <returns>
        /// <c>true</c> if the given value is an instance of the given WebAssembly value type;
        /// otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInstanceOf<T>(T value, WasmValueType type)
        {
            switch (type)
            {
                case WasmValueType.Int32:
                    return value is int;
                case WasmValueType.Int64:
                    return value is long;
                case WasmValueType.Float32:
                    return value is float;
                case WasmValueType.Float64:
                    return value is double;
                default:
                    throw new WasmException("Unknown value type: " + type);
            }
        }

        private static string GetTypeName(object value)
        {
            return value.GetType().Name;
        }
    }
}