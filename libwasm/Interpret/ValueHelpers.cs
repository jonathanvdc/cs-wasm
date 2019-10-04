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

        /// <summary>
        /// Rotates the first operand to the left by the number of
        /// bits given by the second operand.
        /// </summary>
        /// <param name="Left">The first operand.</param>
        /// <param name="Right">The second operand.</param>
        public static int RotateLeft(int Left, int Right)
        {
            var rhs = Right;
            var lhs = (uint)Left;
            uint result = (lhs << rhs) | (lhs >> (32 - rhs));
            return (int)result;
        }

        /// <summary>
        /// Rotates the first operand to the right by the number of
        /// bits given by the second operand.
        /// </summary>
        /// <param name="Left">The first operand.</param>
        /// <param name="Right">The second operand.</param>
        public static int RotateRight(int Left, int Right)
        {
            var rhs = Right;
            var lhs = (uint)Left;
            uint result = (lhs >> rhs) | (lhs << (32 - rhs));
            return (int)result;
        }

        /// <summary>
        /// Counts the number of leading zero bits in the given integer.
        /// </summary>
        /// <param name="Value">The operand.</param>
        public static int CountLeadingZeros(int Value)
        {
            var uintVal = (uint)Value;
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
        /// <param name="Value">The operand.</param>
        public static int CountTrailingZeros(int Value)
        {
            var uintVal = (uint)Value;
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
        /// <param name="Value">The operand.</param>
        public static int PopCount(int Value)
        {
            var uintVal = (uint)Value;
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
        /// <param name="Left">The first operand.</param>
        /// <param name="Right">The second operand.</param>
        public static long RotateLeft(long Left, long Right)
        {
            var rhs = (int)Right;
            var lhs = (ulong)Left;
            ulong result = (lhs << rhs) | (lhs >> (64 - rhs));
            return (long)result;
        }

        /// <summary>
        /// Rotates the first operand to the right by the number of
        /// bits given by the second operand.
        /// </summary>
        /// <param name="Left">The first operand.</param>
        /// <param name="Right">The second operand.</param>
        public static long RotateRight(long Left, long Right)
        {
            var rhs = (int)Right;
            var lhs = (ulong)Left;
            ulong result = (lhs >> rhs) | (lhs << (64 - rhs));
            return (long)result;
        }

        /// <summary>
        /// Counts the number of leading zero bits in the given integer.
        /// </summary>
        /// <param name="Value">The operand.</param>
        public static int CountLeadingZeros(long Value)
        {
            var uintVal = (ulong)Value;
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
        /// <param name="Value">The operand.</param>
        public static int CountTrailingZeros(long Value)
        {
            var uintVal = (ulong)Value;
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
        /// <param name="Value">The operand.</param>
        public static int PopCount(long Value)
        {
            var uintVal = (ulong)Value;
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
        /// <param name="Value">The value to test.</param>
        /// <returns><c>true</c> if the value's sign bit is set; otherwise, <c>false</c>.</returns>
        public static bool Signbit(float Value)
        {
            return (ReinterpretAsInt32(Value) & float32SignMask) == float32SignMask;
        }

        /// <summary>
        /// Composes a 32-bit floating point number with the magnitude of the first
        /// argument and the sign of the second.
        /// </summary>
        /// <param name="Left">The argument whose magnitude is used.</param>
        /// <param name="Right">The argument whose sign bit is used.</param>
        public static float Copysign(float Left, float Right)
        {
            int leftBits = ReinterpretAsInt32(Left);
            int rightBits = ReinterpretAsInt32(Right);
            int resultBits = (leftBits & ~float32SignMask) | (rightBits & float32SignMask);
            return ReinterpretAsFloat32(resultBits);
        }

        /// <summary>
        /// Tests if the sign bit of the given 64-bit floating point value is set,
        /// i.e., if the value is negative.
        /// </summary>
        /// <param name="Value">The value to test.</param>
        /// <returns><c>true</c> if the value's sign bit is set; otherwise, <c>false</c>.</returns>
        public static bool Signbit(double Value)
        {
            return (ReinterpretAsInt64(Value) & float64SignMask) == float64SignMask;
        }

        /// <summary>
        /// Composes a 64-bit floating point number with the magnitude of the first
        /// argument and the sign of the second.
        /// </summary>
        /// <param name="Left">The argument whose magnitude is used.</param>
        /// <param name="Right">The argument whose sign bit is used.</param>
        public static double Copysign(double Left, double Right)
        {
            long leftBits = ReinterpretAsInt64(Left);
            long rightBits = ReinterpretAsInt64(Right);
            long resultBits = (leftBits & ~float64SignMask) | (rightBits & float64SignMask);
            return ReinterpretAsFloat64(resultBits);
        }
    }
}