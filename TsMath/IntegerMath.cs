using System;
using System.Collections.Generic;
using System.Text;
using TsMath.Helpers;

namespace TsMath
{
	/// <summary>
	/// Collection of math routines with integer numbers.
	/// </summary>
	public static partial class IntegerMath
	{

		/// <summary>
		/// Determines whether the given number is even.
		/// </summary>
		/// <param name="num">The number to check.</param>
		/// <returns>
		///   <b>true</b> if the specified number is even; otherwise, <b>false</b>.
		/// </returns>
		public static bool IsEven(this int num)
		{
			return (num & 1) == 0;
		}

		/// <summary>
		/// Determines whether the given number is even.
		/// </summary>
		/// <param name="num">The number to check.</param>
		/// <returns>
		///   <b>true</b> if the specified number is even; otherwise, <b>false</b>.
		/// </returns>
		public static bool IsEven(this long num) => (num & 1) == 0;

		/// <summary>
		/// Calculates the factorial (<paramref name="n"/>!) of the given number.
		/// </summary>
		/// <param name="n">The number.</param>
		/// <returns>the factorial (<paramref name="n"/>!)</returns>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="n"/> is negative.</exception>
		public static BigInteger Factorial(this int n)
		{
			if (n < 0)
				throw new ArgumentOutOfRangeException("n>0 expected");
			if (n < 2)
				return 1;
			BigInteger prod = BigInteger.One;
			for (int i = 2; i <= n; i++)
			{
				prod *= i;
			}
			return prod;
		}

		/// <summary>
		/// Calculates the Legendre symbol. See https://en.wikipedia.org/wiki/Legendre_symbol for a definition.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The algorithm is not guaranteed to terminate if <paramref name="p"/> is not prime.
		/// </para>
		/// </remarks>
		/// <param name="a">Number</param>
		/// <param name="p">Prime number.</param>
		/// <returns>-1, 0 or 1.</returns>
		public static BigInteger LegendreSymbol(BigInteger a, BigInteger p)
		{
			var p1 = p - 1;
			var b = p1 >> 1;
			var ls = PowMod(a, b, p);
			if (p1 == ls)
				return -1;
			if (ls == BigInteger.One)
				return 1;
			return 0;
		}

		/// <summary>
		/// Find a quadratic residue (mod p) of 'a'. p	must be a prime.	 
		/// </summary>
		/// <remarks>
		/// This function calculates the values of x in the equation x^2 = a (mod p). It uses the Tonelli-Shanks algorithm.
		/// The result could be considered as square root of <paramref name="a"/> modulo <paramref name="p"/>.
		/// <para>
		/// The algorithm is not guaranteed to terminate if <paramref name="p"/> is not prime.
		/// </para>
		/// </remarks>
		/// <param name="a">First number.</param>
		/// <param name="p">Modulo number (must be a prime).</param>
		/// <returns>The quadratic residue or  <see cref="BigInteger.Zero"/>, if it does not exist.</returns>
		public static BigInteger QuadraticResidue(BigInteger a, BigInteger p)
		{
			if (a.Sign < 0 || p.Sign <= 0)
				throw new ArgumentException("Arguments must be positive.");
			var zero = BigInteger.Zero;
			if (a.IsZero() || LegendreSymbol(a, p) != 1)
				return zero;
			var two = BigInteger.Two;
			if (p == two)
				return a;
			var one = BigInteger.One;
			var q = p - one;
			int s = 0;
			while (q.IsEven())
			{
				q >>= 1;
				s++;
			}
			var z = two;
			while (LegendreSymbol(z, p) != -1)
				z++;
			var c = BigInteger.Pow(z, q);
			var r = PowMod(a, (q + one) >> 1, p);
			var t = PowMod(a, q, p);
			var m = s;
			while (true)
			{
				if (t == one)
					return r;
				int i = 0;
				var ts = t;
				while (true)
				{
					var mod = PowMod(t, one << i, p);
					if (mod == one)
						break;
					i++;
				}
				var b = PowMod(c, one << (m - i - 1), p);
				r = (r * b) % p;
				var b2 = b * b;
				t = (t * b2) % p;
				c = b2;
				m = i;
			}

		}

		/// <summary>
		/// Retrieves the position of the most significant bit of this number.
		/// </summary>
		/// <param name="num">The number to check.</param>
		/// <returns>The position of the most significant bit of this number.</returns>
		public static int GetMostSignificantBitPosition(this long num)
		{
			if (num < 0)
			{
				if (num == long.MinValue)
					return 64;
				num = -num;
			}
			long start = 1L << 62;
			int pos = 63;
			while (start > 0)
			{
				if ((start & num) != 0)
					return pos;
				pos--;
				start >>= 1;
			}
			return 0;
		}

		/// <summary>
		/// Division with remainder. Solves a = b * q + r;
		/// </summary>
		/// <remarks>
		/// You can control the switch from the naive algorithm to a divide an conquer approach with the parameter <see cref="TsMathGlobals.BigIntegerRecursiveDivRemThreshold"/>.
		/// </remarks>		
		/// <param name="dividend">Dividend.</param>
		/// <param name="divisor">Divisor.</param>
		/// <returns>A value tuple containing the quotient q and the remainder r.</returns>
		/// <exception cref="DivideByZeroException">The <paramref name="divisor"/> is zero.</exception>
		public static (BigInteger quotient, BigInteger remainder) Divide(this BigInteger dividend, BigInteger divisor)
		{
			var d = BigInteger.DivRem(dividend, divisor, out BigInteger r);
			return (d, r);
		}

		/// <summary>
		/// Division with remainder. Solves a = b * q + r;
		/// </summary>
		/// <param name="dividend">Dividend.</param>
		/// <param name="divisor">Divisor.</param>
		/// <returns>A value tuple containing the quotient q and the remainder r.</returns>
		/// <exception cref="DivideByZeroException">The <paramref name="divisor"/> is zero.</exception>
		public static (long quotient, long remainder) Divide(this long dividend, long divisor)
			=> (dividend / divisor, dividend % divisor);


		/// <summary>
		/// Calculates (<paramref name="x"/> * <paramref name="y"/>) mod <paramref name="modVal"/>.
		/// </summary>
		/// <param name="x">First factor.</param>
		/// <param name="y">Second factor.</param>
		/// <param name="modVal">Modulo value, must be greater than 0.</param>
		/// <returns>The modulo value between 0 and <paramref name="modVal"/>-1.</returns>
		public static BigInteger MulMod(BigInteger x, BigInteger y, BigInteger modVal)
		{
			if (modVal <= 0)
				throw new ArithmeticException($"{nameof(modVal)} > 0 expected");
			x %= modVal;
			y %= modVal;
			if (x.IsNegative)
				x += modVal;
			if (y.IsNegative)
				y += modVal;
			return (x * y) % modVal;
		}

		/// <summary>
		/// Calculates (<paramref name="x"/> * <paramref name="y"/>) mod <paramref name="modVal"/>.
		/// </summary>
		/// <remarks>
		/// This function is safe to use, so that the result is correct, even if product overflows or 
		/// underflows.
		/// </remarks>
		/// <param name="x">First factor.</param>
		/// <param name="y">Second factor.</param>
		/// <param name="modVal">Modulo value, must be greater than 0.</param>
		/// <returns>The modulo value between 0 and <paramref name="modVal"/>-1.</returns>
		public static long MulMod(long x, long y, long modVal)
		{
			if (modVal <= 0)
				throw new ArithmeticException($"{nameof(modVal)} > 0 expected");
			x = x % modVal;
			y = y % modVal;
			if (x < 0)
				x += modVal;
			if (y < 0)
				y += modVal;
			if (x < int.MaxValue && y < int.MaxValue)
				return (x * y) % modVal;
			var result = MulMod((ulong)x, (ulong)y, (ulong)modVal);
			return (long)result;
		}

		static ulong MulMod(ulong a, ulong b, ulong modVal)
		{
			if (a > b)
				DivHelpers.Swap(ref a, ref b);
			ulong res = 0;
			while (a != 0)
			{
				if ((a & 1) != 0)
					res = (res + b) % modVal;
				a >>= 1;
				b = (b << 1) % modVal; // safe, because we use max 63 bits
			}
			return res;
		}


	}

}
