using System;

namespace Wasm.Interpret
{
    /// <summary>
    /// Defines helper functions that operate on WebAssembly values.
    /// </summary>
    public static class ValueHelpers
    {
        /// <summary>
        /// Reinterprets the given 32-bit integer's bits as a 32-bit floating-point
        /// number.
        /// </summary>
        /// <param name="Value">The value to reinterpret.</param>
        /// <returns>A 32-bit floating-point number.</returns>
        public static float ReinterpretAsFloat32(int Value)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(Value), 0);
        }

        /// <summary>
        /// Reinterprets the given 32-bit floating-point number's bits as a 32-bit
        /// integer.
        /// </summary>
        /// <param name="Value">The value to reinterpret.</param>
        /// <returns>A 32-bit integer.</returns>
        public static int ReinterpretAsInt32(float Value)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(Value), 0);
        }

        /// <summary>
        /// Reinterprets the given 64-bit integer's bits as a 64-bit floating-point
        /// number.
        /// </summary>
        /// <param name="Value">The value to reinterpret.</param>
        /// <returns>A 64-bit floating-point number.</returns>
        public static double ReinterpretAsFloat64(long Value)
        {
            return BitConverter.Int64BitsToDouble(Value);
        }

        /// <summary>
        /// Reinterprets the given 64-bit floating-point number's bits as a 64-bit
        /// integer.
        /// </summary>
        /// <param name="Value">The value to reinterpret.</param>
        /// <returns>A 64-bit integer.</returns>
        public static long ReinterpretAsInt64(double Value)
        {
            return BitConverter.DoubleToInt64Bits(Value);
        }
    }
}