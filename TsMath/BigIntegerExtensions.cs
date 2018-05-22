
using System;

namespace TsMath
{

	/// <summary>
	/// Helper functions for Big integers
	/// </summary>
	static public class BigIntegerExtensions
	{

		/// <summary>
		/// Memorize some powers we need.
		/// </summary>
		private static BigInteger[] powersOfTen;

		private static BigInteger[] halfOfPowersOfTen;

		static BigIntegerExtensions()
		{
			int n = TsMathGlobals.MaxCachedExponentForGetPowerOfTen + 1;
			powersOfTen = new BigInteger[n];
			halfOfPowersOfTen = new BigInteger[n];
		}

		static double log2Bylog10 = Math.Log(2) / Math.Log(10);

		/// <summary>
		/// Gets the absolute value of this <see cref="BigInteger"/>.
		/// </summary>
		/// <returns></returns>
		public static BigInteger Abs(this BigInteger value)
		{
			if (!value.IsNegative)
				return value;
			value.IsNegative = false;
			return value;
		}


		/// <summary>
		/// Returns the number of decimal digits of the provided number.
		/// </summary>
		/// <param name="num">The number.</param>
		/// <returns>The number of decimal digits of the provided number.</returns>
		public static int GetDecimalPrecision(this BigInteger num)
		{
			if (num.IsZero)
				return 1;
			num = num.Abs();
			int digitCount = (int)(log2Bylog10 * num.GetMostSignificantBitPosition() + 1);
			if (GetPowerOfTen(digitCount - 1) > num)
			{
				return digitCount - 1;
			}
			return digitCount;
		}

		/// <summary>
		/// Gets the power of ten for the provided exponent (i.e. 10^exponent).
		/// </summary>
		/// <remarks>
		/// This function caches the values up to <see cref="TsMathGlobals.MaxCachedExponentForGetPowerOfTen"/> 
		/// after the first use and reuses the cached values.
		/// </remarks>
		/// <param name="exponent">The exponent.</param>
		/// <returns>10^exponent.</returns>
		/// <exception cref="ArgumentException">If the exponent is negative.</exception>
		public static BigInteger GetPowerOfTen(this int exponent)
		{
			if (exponent < 0)
				throw new ArgumentException("exponent must be >=0");
			BigInteger pot = BigInteger.Zero;
			if (exponent < powersOfTen.Length)
				pot = powersOfTen[exponent];
			if (pot.IsZero)
			{
				pot = BigInteger.Pow(10, exponent);
				if (exponent < powersOfTen.Length)
					powersOfTen[exponent] = pot;
			}
			return pot;
		}

		internal static BigInteger GetHalfOfPowerOfTen(int exponent)
		{
			if (exponent < 0)
				throw new ArgumentException("exponent must be >=0");
			BigInteger pot = BigInteger.Zero;
			if (exponent < halfOfPowersOfTen.Length)
				pot = halfOfPowersOfTen[exponent];
			if (pot.IsZero)
			{
				pot = GetPowerOfTen(exponent) >> 1;
				if (exponent < halfOfPowersOfTen.Length)
					halfOfPowersOfTen[exponent] = pot;
			}
			return pot;
		}

		/// <summary>
		/// Converts an <see cref="int"/>-value to a <see cref="BigInteger"/>.
		/// </summary>
		/// <param name="l">The value to convert.</param>
		/// <returns>The converted value.</returns>
		public static BigInteger ToBigInteger(this int l)
		{
			return ToBigInteger((long)l);
		}

		/// <summary>
		/// Converts an <see cref="long"/>-value to a <see cref="BigInteger"/>.
		/// </summary>
		/// <param name="l">The value to convert.</param>
		/// <returns>The converted value.</returns>
		public static BigInteger ToBigInteger(this long l)
		{
			if (l == long.MinValue)
			{
				// special case, because the following code does not work
				// this is a little bit ugly, but works, and the chance to get the value is very small
				var bi = ToBigInteger(l + 1);
				return bi - 1;
			}

			bool fNeg = false;
			if (l < 0)
			{
				fNeg = true; l = -l;
			}
			if (l >= uint.MinValue && l <= uint.MaxValue)
				return new BigInteger((uint)l, fNeg);

			const int dc = sizeof(long) / sizeof(uint);
			uint[] da = new uint[dc];
			int i = 0;
			while (l != 0)
			{
				da[i] = (uint)l;
				l >>= BigInteger.BaseOps.BitsPerDigit;
				i++;
			}
			return new BigInteger(da, fNeg);
		}

		/// <summary>
		/// Converts an <see cref="string" />-value to a <see cref="BigInteger" />.
		/// </summary>
		/// <param name="s">The value to convert.</param>
		/// <returns>
		/// The converted value.
		/// </returns>
		/// <seealso cref="BigInteger.Parse(string, int)"/>
		public static BigInteger ToBigInteger(this string s)
		{
			return BigInteger.Parse(s);
		}





	}
}
