﻿using System;

namespace TsMath
{

	public static partial class IntegerMath
	{

		/// <summary>
		/// Calculates the power of <paramref name="a"/> raised to <paramref name="exp"/>, which must be a non negative 
		/// natural number
		/// </summary>
		/// <param name="a">Number.</param>
		/// <param name="exp">The exponent.</param>
		/// <returns><paramref name="a"/>^<paramref name="exp"/></returns>
		/// <exception cref="System.ArgumentOutOfRangeException">exp>=0 expected</exception>
		public static BigInteger Pow(BigInteger a, BigInteger exp)
		{
			if (exp < 0)
				throw new ArgumentOutOfRangeException("exp>=0 expected");

			BigInteger prod = 1;
			BigInteger factor = a;
			while (exp != 0)
			{
				if (!exp.IsEven())
					prod *= factor;
				factor = factor * factor;
				exp >>= 1;
			}
			return prod;
		}

		/// <summary>
		/// Computes the greatest common divisor.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>The greatest common divisor</returns>
		/// <exception cref="DivideByZeroException"><paramref name="b"/> is zero.</exception>
		public static BigInteger Gcd(BigInteger a, BigInteger b)
		{
			if (a == 0 || b == 0)
				throw new DivideByZeroException();
			BigInteger r;
			while (true)
			{
				if (b == 0)
					return a;
				r = a % b;
				a = b; b = r;
			}
		}

		/// <summary>
		/// Computes the least common multiple.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>The least common multiple.</returns>
		public static BigInteger Lcm(BigInteger a, BigInteger b)
		{
			var gcd = Gcd(a, b);
			return (a / gcd) * b;
		}

		/// <summary>
		/// Computes the module of the power (a^exp) % mod.
		/// </summary>
		/// <param name="a">Base number.</param>
		/// <param name="exp">Exponent.</param>
		/// <param name="mod">Modulus.</param>
		/// <exception cref="ArithmeticException">The exponent is negative or the modulus is not positive.</exception>
		/// <returns>(a^exp) % mod.</returns>
		public static BigInteger PowMod(BigInteger a, BigInteger exp, BigInteger mod)
		{
			if (exp < 0)
				throw new ArithmeticException("PowMod: negative exponent not allowed.");
			if (mod <= 0)
				throw new ArithmeticException("Modulus must be positive");
			if (exp == 0)
				return 1;
			var x = a;
			var m = exp;
			BigInteger temp = 1;
			while (m > 0)
			{
				while ((m & 1) == 0)
				{
					x = MulMod(x, x, mod);
					m >>= 1;
				}
				temp = MulMod(temp, x, mod);
				m--;
			}
			return temp;
		}

		/// <summary>
		/// Calculates the extended Euclidean Algorithm: gcd(a,b)= ax+by. 
		/// Expects a>b 
		/// </summary>
		/// <param name="a">1. number</param>
		/// <param name="b">2. number</param>
		/// <param name="x">1. factor</param>
		/// <param name="y">2. factor</param>
		/// <returns>GCD of a and b</returns>
		public static BigInteger ExtendedGcd(BigInteger a, BigInteger b, out BigInteger x, out BigInteger y)
		{
			BigInteger r0 = a, r1 = b;
			BigInteger zero = 0;
			BigInteger s0 = 1, s1 = zero;
			BigInteger t0 = s1, t1 = s0;
			int i = 1;
			while (!r1.Equals(zero))
			{
				var q = r0 / r1;
				var r = r0 - q * r1;
				var s = s0 - q * s1;
				var t = t0 - q * t1;
				i++;
				r0 = r1; r1 = r; t0 = t1; t1 = t;
				s0 = s1; s1 = s;
			}
			x = s0; y = t0;
			return r0;
		}


		/// <summary>
		/// Computes an a so, that a^2 &lt;=<paramref name="n"/> &lt; (a+1)^2. Very slow 
		/// </summary>
		/// <param name="n">Value</param>
		/// <returns>Square root estimate.</returns>
		public static BigInteger IntSqrt(this BigInteger n)
		{
			if (n < 0)
				throw new ArgumentOutOfRangeException("Negative square root");
			if (n <= 1)
				return n;
			var x = n >> 1;
			if (n <= 5)
				return x;
			int nIter = 0;
			while (true)
			{
				nIter++;
				var next = (n / x + x) >> 1;
				var diff = next - x;
				if (diff == 0)
					return next;
				// if n + 1 is a perfect square (e.g. n=15) there is no convergence but a cycle.
				if (diff == 1)
					return x;
				x = next;
			}
		}

		/// <summary>
		/// Calculates the integer logarithm x, so that base^x &lt;= num &lt; base^ (x+1).
		/// </summary>
		/// <param name="num">The number to calculate the logarithm for.</param>
		/// <param name="base">The base of the logarithm.</param>
		/// <returns>The integer logarithm.</returns>
		public static BigInteger IntLog(this BigInteger num, BigInteger @base)
		{
			if (@base <= 1)
				throw new ArgumentOutOfRangeException("base must > 1");
			if (num <= 0)
				throw new ArgumentOutOfRangeException(nameof(num));
			BigInteger result = 0, step = 1, oldStep = 1, factor = @base, oldFactor = factor;
			while (num >= factor)
			{
				oldFactor = factor;
				factor = factor * factor;
				oldStep = step;
				step <<= 1;
			}
			if (num >= @base)
			{
				step = oldStep;
				result = step;
				num /= oldFactor;
			}
			else
				step = 0;
			while (step > 0 && num != 1)
			{
				step >>= 1;
				factor = Pow(@base, step);
				if (num >= factor)
				{
					num /= factor;
					result += step;
				}
			}
			return result;
		}

		/// <summary>
		/// Calculates the integer logarithm of base 10, so that 10^x &lt;= num &lt; 10^(x+1).
		/// </summary>
		/// <param name="num">The number to calculate the base 10 logarithm for.</param>
		/// <returns>The integer base 10 logarithm.</returns>
		public static BigInteger IntLog10(this BigInteger num) => IntLog(num, 10);


	}
}