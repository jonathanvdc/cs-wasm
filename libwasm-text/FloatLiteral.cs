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
                    // To convert a float literal to a float, we need to do the following:
                    //   1. Convert the literal to base 2.
                    //   2. Increment the exponent until the fractional part achieves the form
                    //      required by the IEEE 754 standard.

                    var exp = value.Exponent;
                    var frac = value.Significand;

                    if (frac == 0)
                    {
                        result = 0;
                        break;
                    }

                    // Decompose the base into a binary base that is a factor of the base
                    // and a remainder, such that `base = 2 ^ binBase * baseRemainder`, where
                    // binBase is maximal.
                    var binBase = 0;
                    var baseRemainder = value.Base;
                    while (baseRemainder % 2 == 0)
                    {
                        binBase++;
                        baseRemainder /= 2;
                    }

                    // Now we can make the following observation:
                    //
                    //   base ^ exp = (2 ^ binBase * baseRemainder) ^ exp
                    //              = 2 ^ (binBase * exp) * baseRemainder ^ exp
                    //
                    // We hence tentatively set our binary exponent to `binBase * exp`.
                    var binExp = binBase * (int)exp;

                    // We will now fold `baseRemainder ^ exp` into our fractional part. This is
                    // easy if `exp` is positive---just multiply the fractional part by `baseRemainder ^ exp`.
                    bool negExp = exp < 0;
                    if (negExp)
                    {
                        exp = -exp;
                    }

                    const int doubleBitLength = 52;
                    bool nonzeroRemainder = false;
                    int bitLength;
                    if (negExp)
                    {
                        // If `exp` is negative then things are more complicated; we need to ensure that we do
                        // not lose information due to integer division. For instance, if we were to naively
                        // convert `1 * 3 ^ 1` to base 2 using the same method as above but with division instead
                        // of multiplication, then we would get `1 / 3 = 0` as the fractional part of our resulting
                        // float. That's not what we want.
                        //
                        // To avoid these types of mishaps, we will pad the fractional part with zeros and update
                        // the exponent to compensate.
                        //
                        // To find out how many zeros we need to pad the fractional part with, we consider the following:
                        //   * We want to end up with at least `doubleBitLength + 2` bits of precision (the first bit
                        //     is always '1' and is implied for normal floats and the last bit is used for rounding).
                        //   * Dividing by `baseRemainder` will reduce the number of bits in the final number.
                        //
                        // We will hence extend the fractional part to `doubleBitLength + 2 + log2(supBaseRemainder) * exp`,
                        // where `supBaseRemainder` is the smallest power of two greater than `baseRemainder`.
                        var supBaseRemainder = 1;
                        var supBaseRemainderLog2 = 0;
                        while (baseRemainder > supBaseRemainder)
                        {
                            supBaseRemainder *= 2;
                            supBaseRemainderLog2++;
                        }

                        // Extend the fractional part to at least the desired bit length.
                        var desiredBitLength = doubleBitLength + 2 + supBaseRemainderLog2 * exp;
                        bitLength = GetBitLength(frac);
                        while (bitLength < desiredBitLength)
                        {
                            bitLength++;
                            frac <<= 1;
                            binExp--;
                        }

                        // Now repeatedly divide it by `baseRemainder`.
                        BigInteger divisor = 1;
                        for (BigInteger i = 0; i < exp; i++)
                        {
                            divisor *= baseRemainder;
                        }
                        var oldFrac = frac;
                        frac = BigInteger.DivRem(frac, divisor, out BigInteger rem);
                        nonzeroRemainder = rem > 0;
                    }
                    else
                    {
                        for (BigInteger i = 0; i < exp; i++)
                        {
                            frac *= baseRemainder;
                        }
                    }

                    // At this point, `frac * 2 ^ binExp` equals the absolute value of our float literal.
                    // However, `frac` and `binExpr` are not yet normalized. To normalize them, we will
                    // change `frac` until its bit length is exactly equal to the number of bits in the
                    // fractional part of a double (52) plus one (=53). After that, we drop the first bit
                    // and keep only the 52 bits that trail it. We increment the exponent by 52.

                    // 2. Increment the exponent.
                    binExp += doubleBitLength;

                    // Make sure the bit length equals 53 exactly.
                    var (finalFrac, finalBinExp) = Round(frac, binExp, nonzeroRemainder, doubleBitLength + 1);

                    if (finalBinExp > 1023)
                    {
                        // If the exponent is greater than 1023, then we round toward infinity.
                        result = double.PositiveInfinity;
                        break;
                    }
                    else if (finalBinExp < -1022)
                    {
                        if (finalBinExp >= -1022 - doubleBitLength)
                        {
                            // If the exponent is less than -1022 but greater than (-1022 - 52), then
                            // we'll try to create a subnormal number.
                            var precision = doubleBitLength;
                            while (precision > 0)
                            {
                                // TODO: get rounding right for subnormals.
                                (finalFrac, finalBinExp) = Round(frac, binExp, nonzeroRemainder, precision);
                                precision--;
                                if (finalBinExp >= -1022)
                                {
                                    return CreateNormalFloat64(value.IsNegative, -1023, finalFrac);
                                }
                            }
                        }
                        // Otherwise, we'll just round toward zero.
                        result = 0;
                        break;
                    }

                    // Convert the fractional part to a 64-bit integer and drop the
                    // leading one. Compose the double.
                    result = CreateNormalFloat64(false, finalBinExp, finalFrac);
                    break;
            }
            return Interpret.ValueHelpers.Setsign(result, value.IsNegative);
        }

        /// <summary>
        /// Takes a positive significand and a binary exponent, sets the significand's
        /// bit length to a particular precision (updating the exponent accordingly
        /// such that approximately the same number is represented), and rounds
        /// the significand to produce a number that is as close to the original
        /// number as possible.
        /// </summary>
        /// <param name="significand">A positive significand.</param>
        /// <param name="exponent">A binary exponent.</param>
        /// <param name="nonzeroRemainder">
        /// Tells if the significand has trailing ones not encoded in <paramref name="significand"/>.
        /// </param>
        /// <param name="precision">The bit precision of the resulting significand.</param>
        /// <returns>A (significand, exponent) pair.</returns>
        private static (BigInteger significand, int exponent) Round(
            BigInteger significand,
            int exponent,
            bool nonzeroRemainder,
            int precision)
        {
            var bitLength = GetBitLength(significand);
            int delta = precision - bitLength;
            if (delta >= 0)
            {
                // If the significand is insufficiently precise, then we can
                // just add more bits of precision by appending zero bits,
                // i.e., shifting to the left.
                return (significand << delta, exponent - delta);
            }
            else
            {
                // If the significand is too precise, then we need to eliminate
                // bits of precision. We also need to round in this step.

                // Rounding implies that we find a minimal range `[lower, upper]` such that
                // `significand \in [lower, upper]`. Then, we pick either `lower` or `upper`
                // as our result, depending on which is closer or a tie-breaking round-to-even
                // rule.

                // Find `lower`, `upper`.
                delta = -delta;
                var lower = significand >> delta;
                var lowerExponent = exponent + delta;
                var upper = lower + 1;
                var upperExponent = lowerExponent;
                if (GetBitLength(upper) == precision + 1)
                {
                    upper >>= 1;
                    upperExponent++;
                }

                // Now we just need to pick either `lower` or `upper`. The digits in the
                // significand that are not included in `lower` are decisive here.
                var lowerRoundingError = significand - (lower << delta);
                var midpoint = 1 << (delta - 1);
                if (lowerRoundingError < midpoint
                    || (lowerRoundingError == midpoint && !nonzeroRemainder && lower % 2 == 0))
                {
                    return (lower, lowerExponent);
                }
                else
                {
                    return (upper, upperExponent);
                }
            }
        }

        private static int GetBitLength(BigInteger value)
        {
            int length = 0;
            while (value > 0)
            {
                value >>= 1;
                length++;
            }
            return length;
        }

        /// <summary>
        /// Creates a double from a sign, an exponent and a significand.
        /// </summary>
        /// <param name="isNegative">
        /// <c>true</c> if the double-precision floating-point number is negated; otherwise, <c>false</c>.
        /// </param>
        /// <param name="exponent">
        /// The exponent to which the number's base (2) is raised.
        /// </param>
        /// <param name="fraction">
        /// The fractional part of the float.
        /// </param>
        /// <returns>
        /// A floating-point number that is equal to (-1)^<paramref name="isNegative"/> * 2^(<paramref name="exponent"/> - 1023) * 1.<paramref name="fraction"/>.
        /// </returns>
        private static double CreateNormalFloat64(bool isNegative, int exponent, BigInteger fraction)
        {
            return CreateNormalFloat64(isNegative, exponent, (long)fraction & 0x000fffffffffffffL);
        }

        /// <summary>
        /// Creates a double from a sign, an exponent and a significand.
        /// </summary>
        /// <param name="isNegative">
        /// <c>true</c> if the double-precision floating-point number is negated; otherwise, <c>false</c>.
        /// </param>
        /// <param name="exponent">
        /// The exponent to which the number's base (2) is raised.
        /// </param>
        /// <param name="fraction">
        /// The fractional part of the float.
        /// </param>
        /// <returns>
        /// A floating-point number that is equal to (-1)^<paramref name="isNegative"/> * 2^(<paramref name="exponent"/> - 1023) * 1.<paramref name="fraction"/>.
        /// </returns>
        private static double CreateNormalFloat64(bool isNegative, int exponent, long fraction)
        {
            return Wasm.Interpret.ValueHelpers.ReinterpretAsFloat64(
                ((isNegative ? 1L : 0L) << 63)
                | ((long)(exponent + 1023) << 52)
                | fraction);
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
            return Interpret.ValueHelpers.Setsign(result, value.IsNegative);
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
