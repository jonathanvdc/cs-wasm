using System;

namespace Wasm.Interpret
{
    /// <summary>
    /// Defines helper functions that operate on WebAssembly values.
    /// </summary>
    public static class ValueHelpers
    {
        /// <summary>
        /// Takes a type and maps it to its corresponding WebAssembly value type.
        /// </summary>
        /// <param name="type">The type to map to a WebAssembly value type.</param>
        /// <returns>A WebAssembly value type.</returns>
        public static WasmValueType ToWasmValueType(Type type)
        {
            if (type == typeof(int))
            {
                return WasmValueType.Int32;
            }
            else if (type == typeof(long))
            {
                return WasmValueType.Int64;
            }
            else if (type == typeof(float))
            {
                return WasmValueType.Float32;
            }
            else if (type == typeof(double))
            {
                return WasmValueType.Float64;
            }
            else
            {
                throw new WasmException($"Type '{type}' does not map to a WebAssembly type.");
            }
        }

        /// <summary>
        /// Takes a type and maps it to its corresponding WebAssembly value type.
        /// </summary>
        /// <typeparam name="T">The type to map to a WebAssembly value type.</typeparam>
        /// <returns>A WebAssembly value type.</returns>
        public static WasmValueType ToWasmValueType<T>()
        {
            return ToWasmValueType(typeof(T));
        }

        /// <summary>
        /// Reinterprets the given 32-bit integer's bits as a 32-bit floating-point
        /// number.
        /// </summary>
        /// <param name="value">The value to reinterpret.</param>
        /// <returns>A 32-bit floating-point number.</returns>
        public static unsafe float ReinterpretAsFloat32(int value)
        {
            return *(float*)&value;
        }

        /// <summary>
        /// Reinterprets the given 32-bit floating-point number's bits as a 32-bit
        /// integer.
        /// </summary>
        /// <param name="value">The value to reinterpret.</param>
        /// <returns>A 32-bit integer.</returns>
        public static unsafe int ReinterpretAsInt32(float value)
        {
            return *(int*)&value;
        }

        /// <summary>
        /// Reinterprets the given 64-bit integer's bits as a 64-bit floating-point
        /// number.
        /// </summary>
        /// <param name="value">The value to reinterpret.</param>
        /// <returns>A 64-bit floating-point number.</returns>
        public static double ReinterpretAsFloat64(long value)
        {
            return BitConverter.Int64BitsToDouble(value);
        }

        /// <summary>
        /// Reinterprets the given 64-bit floating-point number's bits as a 64-bit
        /// integer.
        /// </summary>
        /// <param name="value">The value to reinterpret.</param>
        /// <returns>A 64-bit integer.</returns>
        public static long ReinterpretAsInt64(double value)
        {
            return BitConverter.DoubleToInt64Bits(value);
        }

        /// <summary>
        /// Rotates the first operand to the left by the number of
        /// bits given by the second operand.
        /// </summary>
        /// <param name="left">The first operand.</param>
        /// <param name="right">The second operand.</param>
        public static int RotateLeft(int left, int right)
        {
            var rhs = right;
            var lhs = (uint)left;
            uint result = (lhs << rhs) | (lhs >> (32 - rhs));
            return (int)result;
        }

        /// <summary>
        /// Rotates the first operand to the right by the number of
        /// bits given by the second operand.
        /// </summary>
        /// <param name="left">The first operand.</param>
        /// <param name="right">The second operand.</param>
        public static int RotateRight(int left, int right)
        {
            var rhs = right;
            var lhs = (uint)left;
            uint result = (lhs >> rhs) | (lhs << (32 - rhs));
            return (int)result;
        }

        /// <summary>
        /// Counts the number of leading zero bits in the given integer.
        /// </summary>
        /// <param name="value">The operand.</param>
        public static int CountLeadingZeros(int value)
        {
            var uintVal = (uint)value;
            int numOfLeadingZeros = 32;
            while (uintVal != 0)
            {
                numOfLeadingZeros--;
                uintVal >>= 1;
            }
            return numOfLeadingZeros;
        }

        /// <summary>
        /// Counts the number of trailing zero bits in the given integer.
        /// </summary>
        /// <param name="value">The operand.</param>
        public static int CountTrailingZeros(int value)
        {
            var uintVal = (uint)value;
            if (uintVal == 0u)
            {
                return 32;
            }
            int numOfTrailingZeros = 0;
            while ((uintVal & 0x1u) == 0u)
            {
                numOfTrailingZeros++;
                uintVal >>= 1;
            }
            return numOfTrailingZeros;
        }

        /// <summary>
        /// Counts the number of one bits in the given integer.
        /// </summary>
        /// <param name="value">The operand.</param>
        public static int PopCount(int value)
        {
            var uintVal = (uint)value;
            int numOfOnes = 0;
            while (uintVal != 0)
            {
                numOfOnes += (int)(uintVal & 0x1u);
                uintVal >>= 1;
            }
            return numOfOnes;
        }

        /// <summary>
        /// Rotates the first operand to the left by the number of
        /// bits given by the second operand.
        /// </summary>
        /// <param name="left">The first operand.</param>
        /// <param name="right">The second operand.</param>
        public static long RotateLeft(long left, long right)
        {
            var rhs = (int)right;
            var lhs = (ulong)left;
            ulong result = (lhs << rhs) | (lhs >> (64 - rhs));
            return (long)result;
        }

        /// <summary>
        /// Rotates the first operand to the right by the number of
        /// bits given by the second operand.
        /// </summary>
        /// <param name="left">The first operand.</param>
        /// <param name="right">The second operand.</param>
        public static long RotateRight(long left, long right)
        {
            var rhs = (int)right;
            var lhs = (ulong)left;
            ulong result = (lhs >> rhs) | (lhs << (64 - rhs));
            return (long)result;
        }

        /// <summary>
        /// Counts the number of leading zero bits in the given integer.
        /// </summary>
        /// <param name="value">The operand.</param>
        public static int CountLeadingZeros(long value)
        {
            var uintVal = (ulong)value;
            int numOfLeadingZeros = 64;
            while (uintVal != 0)
            {
                numOfLeadingZeros--;
                uintVal >>= 1;
            }
            return numOfLeadingZeros;
        }

        /// <summary>
        /// Counts the number of trailing zero bits in the given integer.
        /// </summary>
        /// <param name="value">The operand.</param>
        public static int CountTrailingZeros(long value)
        {
            var uintVal = (ulong)value;
            if (uintVal == 0ul)
            {
                return 64;
            }

            int numOfTrailingZeros = 0;
            while ((uintVal & 0x1u) == 0u)
            {
                numOfTrailingZeros++;
                uintVal >>= 1;
            }
            return numOfTrailingZeros;
        }

        /// <summary>
        /// Counts the number of one bits in the given integer.
        /// </summary>
        /// <param name="value">The operand.</param>
        public static int PopCount(long value)
        {
            var uintVal = (ulong)value;
            int numOfOnes = 0;
            while (uintVal != 0)
            {
                numOfOnes += (int)(uintVal & 0x1u);
                uintVal >>= 1;
            }
            return numOfOnes;
        }

        // Based on the StackOverflow answer by Deduplicator:
        // https://stackoverflow.com/questions/26576285/how-can-i-get-the-sign-bit-of-a-double
        private static readonly int float32SignMask =
            ReinterpretAsInt32(-0.0f) ^ ReinterpretAsInt32(+0.0f);

        private static readonly long float64SignMask =
            ReinterpretAsInt64(-0.0) ^ ReinterpretAsInt64(+0.0);

        /// <summary>
        /// Tests if the sign bit of the given 32-bit floating point value is set,
        /// i.e., if the value is negative.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns><c>true</c> if the value's sign bit is set; otherwise, <c>false</c>.</returns>
        public static bool Signbit(float value)
        {
            return (ReinterpretAsInt32(value) & float32SignMask) == float32SignMask;
        }

        /// <summary>
        /// Composes a 32-bit floating point number with the magnitude of the first
        /// argument and the sign of the second.
        /// </summary>
        /// <param name="left">The argument whose magnitude is used.</param>
        /// <param name="right">The argument whose sign bit is used.</param>
        public static float Copysign(float left, float right)
        {
            int leftBits = ReinterpretAsInt32(left);
            int rightBits = ReinterpretAsInt32(right);
            int resultBits = (leftBits & ~float32SignMask) | (rightBits & float32SignMask);
            return ReinterpretAsFloat32(resultBits);
        }

        /// <summary>
        /// Tests if the sign bit of the given 64-bit floating point value is set,
        /// i.e., if the value is negative.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns><c>true</c> if the value's sign bit is set; otherwise, <c>false</c>.</returns>
        public static bool Signbit(double value)
        {
            return (ReinterpretAsInt64(value) & float64SignMask) == float64SignMask;
        }

        /// <summary>
        /// Composes a 64-bit floating point number with the magnitude of the first
        /// argument and the sign of the second.
        /// </summary>
        /// <param name="left">The argument whose magnitude is used.</param>
        /// <param name="right">The argument whose sign bit is used.</param>
        public static double Copysign(double left, double right)
        {
            long leftBits = ReinterpretAsInt64(left);
            long rightBits = ReinterpretAsInt64(right);
            long resultBits = (leftBits & ~float64SignMask) | (rightBits & float64SignMask);
            return ReinterpretAsFloat64(resultBits);
        }
    }
}
