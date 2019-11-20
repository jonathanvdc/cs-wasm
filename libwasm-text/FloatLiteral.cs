using System;
using System.Numerics;

namespace Wasm.Text
{
    /// <summary>
    /// Represents a parsed floating-point number literal.
    /// </summary>
    public struct FloatLiteral
    {
        private FloatLiteral(
            FloatLiteralKind kind,
            bool isNegative,
            BigInteger significand,
            int @base,
            BigInteger exponent)
        {
            this.Kind = kind;
            this.IsNegative = isNegative;
            this.Significand = significand;
            this.Base = @base;
            this.Exponent = exponent;
        }

        /// <summary>
        /// Gets the float literal's kind.
        /// </summary>
        /// <value>A float literal kind.</value>
        public FloatLiteralKind Kind { get; private set; }

        /// <summary>
        /// Tells if the float literal's sign is negative.
        /// </summary>
        /// <value><c>true</c> if the float literal's sign is negative; otherwise, <c>false</c>.</value>
        public bool IsNegative { get; private set; }

        /// <summary>
        /// Tells if the float literal's sign is positive.
        /// </summary>
        /// <value><c>true</c> if the float literal's sign is positive; otherwise, <c>false</c>.</value>
        public bool IsPositive => !IsNegative;

        /// <summary>
        /// Gets the float literal's significand as a positive integer.
        /// </summary>
        /// <value>A significand.</value>
        public BigInteger Significand { get; private set; }

        /// <summary>
        /// Gets the base for the float literal's exponent.
        /// </summary>
        /// <value>A base.</value>
        public int Base { get; private set; }

        /// <summary>
        /// Gets the float literal's exponent as an integer.
        /// </summary>
        /// <value>The literal's exponent.</value>
        public BigInteger Exponent { get; private set; }

        /// <summary>
        /// Creates a Not-a-Number float literal.
        /// </summary>
        /// <param name="isNegative">Tells if the Not-a-Number float is negative.</param>
        /// <param name="payload">The NaN payload.</param>
        /// <returns>A NaN float literal.</returns>
        public static FloatLiteral NaN(bool isNegative, BigInteger payload)
        {
            return new FloatLiteral(FloatLiteralKind.NaN, isNegative, payload, 2, 0);
        }

        /// <summary>
        /// Creates a numeric float literal.
        /// </summary>
        /// <param name="isNegative">Tells if the float is negative.</param>
        /// <param name="significand">The float's significand.</param>
        /// <param name="baseNum">The float's base.</param>
        /// <param name="exponent">The exponent to which <paramref name="baseNum"/> is raised.</param>
        /// <returns>A numeric float literal.</returns>
        public static FloatLiteral Number(bool isNegative, BigInteger significand, int baseNum, BigInteger exponent)
        {
            return new FloatLiteral(FloatLiteralKind.Number, isNegative, significand, baseNum, exponent);
        }

        /// <summary>
        /// A float literal representing positive infinity.
        /// </summary>
        public static readonly FloatLiteral PositiveInfinity = new FloatLiteral(FloatLiteralKind.Infinity, false, 0, 2, 0);

        /// <summary>
        /// A float literal representing negative infinity.
        /// </summary>
        public static readonly FloatLiteral NegativeInfinity = new FloatLiteral(FloatLiteralKind.Infinity, true, 0, 2, 0);

        /// <summary>
        /// Negates a float literal.
        /// </summary>
        /// <param name="value">The float literal to negate.</param>
        /// <returns>The additive inverse of a float literal.</returns>
        public static FloatLiteral operator-(FloatLiteral value)
        {
            return new FloatLiteral(value.Kind, !value.IsNegative, value.Significand, value.Base, value.Exponent);
        }

        /// <summary>
        /// Transforms a float literal to a double-precision floating point number.
        /// </summary>
        /// <param name="value">A float literal.</param>
        public static explicit operator double(FloatLiteral value)
        {
            double result;
            switch (value.Kind)
            {
                case FloatLiteralKind.Infinity:
                    result = double.PositiveInfinity;
                    break;
                case FloatLiteralKind.NaN:
                    result = CreateFloat64NaNWithSignificand((long)value.Significand);
                    break;
                case FloatLiteralKind.Number:
                default:
                    var exp = value.Exponent;
                    if (exp == 0)
                    {
                        result = 1.0;
                    }
                    else
                    {
                        result = (double)value.Significand;

                        var expVal = 1.0;
                        bool negExp = exp < 0;
                        if (negExp)
                        {
                            exp = -exp;
                        }

                        for (int i = 0; i < exp; i++)
                        {
                            expVal *= value.Base;
                        }

                        if (negExp)
                        {
                            result /= expVal;
                        }
                        else
                        {
                            result *= expVal;
                        }
                    }
                    break;
            }
            if (value.IsNegative)
            {
                result = -result;
            }
            return result;
        }

        /// <summary>
        /// Transforms a float literal to a single-precision floating point number.
        /// </summary>
        /// <param name="value">A float literal.</param>
        public static explicit operator float(FloatLiteral value)
        {
            float result;
            switch (value.Kind)
            {
                case FloatLiteralKind.Infinity:
                    result = float.PositiveInfinity;
                    break;
                case FloatLiteralKind.NaN:
                    result = CreateFloat32NaNWithSignificand((int)value.Significand);
                    break;
                case FloatLiteralKind.Number:
                default:
                    var exp = value.Exponent;
                    if (exp == 0)
                    {
                        result = 1.0f;
                    }
                    else
                    {
                        result = (float)value.Significand;

                        var expVal = 1.0f;
                        bool negExp = exp < 0;
                        if (negExp)
                        {
                            exp = -exp;
                        }

                        for (int i = 0; i < exp; i++)
                        {
                            expVal *= value.Base;
                        }

                        if (negExp)
                        {
                            result /= expVal;
                        }
                        else
                        {
                            result *= expVal;
                        }
                    }
                    break;
            }
            if (value.IsNegative)
            {
                result = -result;
            }
            return result;
        }

        private static double CreateFloat64NaNWithSignificand(long significand)
        {
            // We're going to create a NaN with a special significand.
            long bits = BitConverter.DoubleToInt64Bits(double.NaN);
            long oldSignificand = bits & 0xfffffffffffffL;
            // Wipe out the bits originally in the significand.
            bits ^= oldSignificand;
            // Put in our bits.
            bits |= significand;
            return BitConverter.Int64BitsToDouble(bits);
        }

        private static float CreateFloat32NaNWithSignificand(int significand)
        {
            // We're going to create a NaN with a special significand.
            int bits = Interpret.ValueHelpers.ReinterpretAsInt32(float.NaN);
            int oldSignificand = bits & 0x3fffff;
            // Wipe out the bits originally in the significand.
            bits ^= oldSignificand;
            // Put in our bits.
            bits |= significand;
            return Interpret.ValueHelpers.ReinterpretAsFloat32(bits);
        }
    }

    /// <summary>
    /// An enumeration of different kinds of float literals.
    /// </summary>
    public enum FloatLiteralKind
    {
        /// <summary>
        /// Indicates that a float literal represents a concrete number.
        /// </summary>
        Number,

        /// <summary>
        /// Indicates that a float literal represents a Not-a-Number (NaN) value.
        /// </summary>
        NaN,

        /// <summary>
        /// Indicates that a float literal represents an infinite quantity.
        /// </summary>
        Infinity
    }
}
