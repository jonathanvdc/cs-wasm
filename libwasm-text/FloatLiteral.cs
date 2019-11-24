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
        /// Gets the float literal's significand and sign as a single integer whose
        /// absolute value equals the significand and whose sign equals the sign.
        /// </summary>
        public BigInteger SignedSignificand => IsNegative ? -Significand : Significand;

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
        /// Creates a Not-a-Number float literal with a custom payload.
        /// </summary>
        /// <param name="isNegative">Tells if the Not-a-Number float is negative.</param>
        /// <param name="payload">The NaN payload.</param>
        /// <returns>A NaN float literal.</returns>
        public static FloatLiteral NaN(bool isNegative, BigInteger payload)
        {
            return new FloatLiteral(FloatLiteralKind.NaNWithPayload, isNegative, payload, 2, 0);
        }

        /// <summary>
        /// Creates a canonical Not-a-Number float literal.
        /// </summary>
        /// <param name="isNegative">Tells if the Not-a-Number float is negative.</param>
        /// <returns>A NaN float literal.</returns>
        public static FloatLiteral NaN(bool isNegative)
        {
            return new FloatLiteral(FloatLiteralKind.CanonicalNaN, isNegative, 0, 2, 0);
        }

        /// <summary>
        /// Creates a numeric float literal.
        /// </summary>
        /// <param name="isNegative">Tells if the float is negative.</param>
        /// <param name="significand">The float's significand.</param>
        /// <param name="baseNum">The float's base.</param>
        /// <param name="exponent">The exponent to which <paramref name="baseNum"/> is raised.</param>
        /// <returns>A numeric float literal.</returns>
        private static FloatLiteral Number(bool isNegative, BigInteger significand, int baseNum, BigInteger exponent)
        {
            return new FloatLiteral(FloatLiteralKind.Number, isNegative, significand, baseNum, exponent);
        }

        /// <summary>
        /// Creates a numeric float literal that is equal to an integer multiplied by a base exponentiation.
        /// </summary>
        /// <param name="significand">The float's significand.</param>
        /// <param name="baseNum">The float's base.</param>
        /// <param name="exponent">The exponent to which <paramref name="baseNum"/> is raised.</param>
        /// <returns>A numeric float literal.</returns>
        public static FloatLiteral Number(BigInteger significand, int baseNum, BigInteger exponent)
        {
            bool isNeg = significand < 0;
            return Number(isNeg, isNeg ? -significand : significand, baseNum, exponent);
        }

        /// <summary>
        /// Creates a numeric float literal that is equal to an integer.
        /// </summary>
        /// <param name="significand">The float's significand.</param>
        /// <param name="baseNum">The float's base.</param>
        /// <returns>A numeric float literal.</returns>
        public static FloatLiteral Number(BigInteger significand, int baseNum)
        {
            return Number(significand, baseNum, 0);
        }

        /// <summary>
        /// Creates a zero float literal constant.
        /// </summary>
        /// <param name="baseNum">The base for the zero literal.</param>
        /// <returns>A zero float literal.</returns>
        public static FloatLiteral Zero(int baseNum) => Number(false, 0, baseNum, 0);

        /// <summary>
        /// A float literal representing positive infinity.
        /// </summary>
        public static readonly FloatLiteral PositiveInfinity = new FloatLiteral(FloatLiteralKind.Infinity, false, 0, 2, 0);

        /// <summary>
        /// A float literal representing negative infinity.
        /// </summary>
        public static readonly FloatLiteral NegativeInfinity = new FloatLiteral(FloatLiteralKind.Infinity, true, 0, 2, 0);

        /// <summary>
        /// Adds a value to this float literal's exponent.
        /// </summary>
        /// <param name="exponentDelta">The value to add to the exponent.</param>
        /// <returns>A new float literal.</returns>
        public FloatLiteral AddToExponent(BigInteger exponentDelta)
        {
            if (Kind == FloatLiteralKind.Number)
            {
                return Number(IsNegative, Significand, Base, Exponent + exponentDelta);
            }
            else
            {
                return this;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var sign = IsNegative ? "-" : "";
            switch (Kind)
            {
                case FloatLiteralKind.Number:
                default:
                    return $"{sign}{Significand} * {Base} ^ {Exponent}";
                case FloatLiteralKind.NaNWithPayload:
                    return $"{sign}nan:0x{Significand.ToString("x")}";
                case FloatLiteralKind.CanonicalNaN:
                    return $"{sign}nan";
                case FloatLiteralKind.Infinity:
                    return $"{sign}inf";
            }
        }

        /// <summary>
        /// Adds an integer to a float literal.
        /// </summary>
        /// <param name="first">An integer.</param>
        /// <param name="second">A float literal.</param>
        /// <returns>A float literal that is the sum of <paramref name="first"/> and <paramref name="second"/>.</returns>
        public static FloatLiteral operator+(BigInteger first, FloatLiteral second)
        {
            return Number(first, second.Base, 0) + second;
        }

        /// <summary>
        /// Computes the sum of two numeric float literals with equal bases.
        /// </summary>
        /// <param name="first">A first float literal.</param>
        /// <param name="second">A second float literal.</param>
        /// <returns>The sum of <paramref name="first"/> and <paramref name="second"/>.</returns>
        public static FloatLiteral operator+(FloatLiteral first, FloatLiteral second)
        {
            if (first.Kind != FloatLiteralKind.Number || second.Kind != FloatLiteralKind.Number)
            {
                throw new WasmException("Cannot add non-number float literals.");
            }
            else if (first.Base != second.Base)
            {
                throw new WasmException("Cannot add float literals with incompatible bases.");
            }

            if (first.Exponent == second.Exponent)
            {
                // If both numbers have the same exponent, then adding them is easy. Just
                // compute the sum of their significands.
                return Number(first.SignedSignificand + second.SignedSignificand, first.Base, first.Exponent);
            }
            else if (first.Exponent < second.Exponent)
            {
                // If the first number's exponent is less than the second number's, then we
                // can multiply the second number's significand by its base until the
                // exponents become equal.
                var secondSignificand = second.SignedSignificand;
                var firstExponent = first.Exponent;
                while (firstExponent != second.Exponent)
                {
                    secondSignificand *= first.Base;
                    firstExponent++;
                }
                return Number(first.SignedSignificand + secondSignificand, first.Base, first.Exponent);
            }
            else
            {
                return second + first;
            }
        }

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
                case FloatLiteralKind.NaNWithPayload:
                    result = CreateFloat64NaNWithSignificand((long)value.Significand);
                    break;
                case FloatLiteralKind.CanonicalNaN:
                    result = double.NaN;
                    break;
                case FloatLiteralKind.Number:
                default:
                    var exp = value.Exponent;
                    if (exp == 0)
                    {
                        result = (double)value.Significand;
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
                case FloatLiteralKind.NaNWithPayload:
                    result = CreateFloat32NaNWithSignificand((int)value.Significand);
                    break;
                case FloatLiteralKind.CanonicalNaN:
                    result = float.NaN;
                    break;
                case FloatLiteralKind.Number:
                default:
                    return (float)(double)value;
            }
            if (value.IsNegative)
            {
                result = -result;
            }
            return result;
        }

        /// <summary>
        /// Losslessly changes a float literal's base. Base changes only work if old base
        /// is a power of the new base.
        /// </summary>
        /// <param name="newBase">The new base.</param>
        /// <returns>An equivalent float literal with base <paramref name="newBase"/>.</returns>
        public FloatLiteral ChangeBase(int newBase)
        {
            if (Kind != FloatLiteralKind.Number || Base == newBase)
            {
                return this;
            }
            else if (Exponent == 0)
            {
                return FloatLiteral.Number(IsNegative, Significand, newBase, 0);
            }

            // Note: `x * (n ^ m) ^ k` equals `x * n ^ (m * k)`.
            var power = 1;
            var resultBase = Base;
            while (resultBase != newBase)
            {
                if (resultBase < newBase || resultBase % newBase != 0)
                {
                    throw new InvalidOperationException(
                        $"Float literal '{this}' with base '{Base}' cannot be transformed losslessly to float with base '{newBase}'.");
                }

                resultBase /= newBase;
                power++;
            }

            return FloatLiteral.Number(IsNegative, Significand, newBase, power * Exponent);
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
            int oldSignificand = bits & 0x7fffff;
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
        /// Indicates that a float literal represents a canonical Not-a-Number (NaN) value.
        /// </summary>
        CanonicalNaN,

        /// <summary>
        /// Indicates that a float literal represents a Not-a-Number (NaN) value
        /// with a custom payload.
        /// </summary>
        NaNWithPayload,

        /// <summary>
        /// Indicates that a float literal represents an infinite quantity.
        /// </summary>
        Infinity
    }
}
