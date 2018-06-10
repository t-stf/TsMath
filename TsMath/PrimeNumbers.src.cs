using System;
using System.Collections.Generic;

namespace TsMath
{

	public partial class PrimeNumbers
	{
		/// <summary>
		/// Factors out <paramref name="d"/> = a*2^<paramref name="r"/>
		/// </summary>
		/// <param name="d">Number to factor</param>
		/// <param name="r">Exponent output.</param>
		/// <returns>a</returns>
		static BigInteger FactorPow2(BigInteger d, out long r)
		{
			r = 0;
			while ((d & 1) == 0)
			{
				r++;
				d >>= 1;
			}
			return d;
		}

		/// <summary>
		/// Guesses a prime factor of the number <paramref name="num"/>. 
		/// </summary>
		/// <param name="num">The number.</param>
		/// <returns>A possible prime factor.</returns>
		public static BigInteger GuessPrimeFactor(BigInteger num)
		{
			var d = FactorPow2(num - 1, out long r);
			var pm = IntegerMath.PowMod(2, d, num);
			var gcd = IntegerMath.Gcd(pm - 1, num);
			return gcd;
		}

		/// <summary>
		/// Determines whether <paramref name="n"/> is probably a prime number. Uses the 
		/// [Miller–Rabin primality test](https://en.wikipedia.org/wiki/Miller%E2%80%93Rabin_primality_test).
		/// </summary>
		/// <remarks>This is a probabilistic test. The probability to return <b>true</b> for a composite number
		/// is 4^(−<paramref name="k"/>). </remarks>
		/// <param name="n">Number to test (must be odd).</param>
		/// <param name="k">The number of random tests to do.</param>
		/// <param name="rand">The random number generator to use.</param>
		/// <returns>
		///   <b>true</b> if the number <paramref name="n"/> is probable a prime number; <b>false</b>, if the number is composite.
		/// </returns>
		public static bool IsProbablyPrime_MillerRabinTest(BigInteger n, BigInteger k, RandomXorShift rand)
		{
			var n1 = n - 1;
			var d = FactorPow2(n1, out long r);

			for (int i = 0; i < k; i++)
			{
				var a = rand.NextValue(n - 3) + 2;
				var x = IntegerMath.PowMod(a, d, n);
				if (x == 1 || x == n1)
					continue;
				bool continueI = false;
				for (int j = 0; j < r - 1; j++)
				{
					x = IntegerMath.PowMod(x, 2, n);
					if (x == 1)
						return false;
					if (x == n1)
					{
						continueI = true; break;
					}
				}
				if (continueI)
					continue;
				return false;
			}
			return true;
		}

		/// <summary>
		/// Deterministic Miller Rabin test for primality.
		/// </summary>
		/// <param name="n">Number to test (must be odd).</param>
		/// <param name="b">Base to test.</param>
		/// <returns>
		///   <b>true</b> if the number <paramref name="n"/> is probable a prime number; <b>false</b>, if the number is composite.
		/// </returns>
		public static bool SPRP(BigInteger n, BigInteger b)
		{
			var nMinus1 = n - 1;
			var t = FactorPow2(nMinus1, out long r);

			var test = IntegerMath.PowMod(b, t, n);
			if (test == 1 || test == nMinus1)
				return true;

			while (--r > 0)
			{
				test = IntegerMath.MulMod(test, test, n);
				if (test == nMinus1)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Get the prime factors of a number. Starting with the lowest.
		/// </summary>
		/// <param name="num">The number to factorize.</param>
		/// <param name="nextPrimeFunc">The function to retrieve the next (probable) prime number.</param>
		/// <returns>Enumerable of prime factors, as pairs (factor, exponent)</returns>
		static List<(BigInteger factor, int count)> GetPrimeFactors(BigInteger num, Func<BigInteger,BigInteger> nextPrimeFunc)
		{
			var list = new List<(BigInteger, int)>();
			BigInteger prime = 0;
			while (true)
			{
				prime = nextPrimeFunc(prime);
				int count = 0;
				while (num % prime == 0)
				{
					count++;
					num /= prime;
				}
				if (count > 0)
					list.Add((prime, count));
				if (num == 1)
					break;
			}
			return list;
		}

		//public static List<BigInteger> GetDivisors(BigInteger num)
		//{
		//	if (num < 1)
		//		throw new ArgumentOutOfRangeException(nameof(num));
		//	if (num == 1)
		//		return new List<BigInteger> { 1 };
		//	var list = new List<BigInteger>();
		//	var factors = GetPrimeFactors(num);
		//	var counter = new int[factors.Count];
		//	int index = 0;
		//	while (true)
		//	{
		//		if (index < counter.Length)
		//		{
		//			index++; continue;
		//		}


		//	}


		//}


		/// <summary>
		/// Generates the next prime number greater than <paramref name="num"/>.
		/// </summary>
		/// <param name="num">The starting number.</param>
		/// <param name="isPrimeFunc">The function to test a number for (probable) primality.</param>
		/// <returns>The next prime number greater than <paramref name="num"/>.</returns>
		/// <exception cref="OverflowException">The result would be greater than <see cref="Int64.MaxValue"/>.</exception>
		static BigInteger NextPrime(BigInteger num, Func<BigInteger, bool> isPrimeFunc)
		{
			if (num <= 1)
				return 2;
			if (num <= 2)
				return 3;
			if (num <= 3)
				return 5;

			num++;
			if (num.IsEven())
				num++;
			var r = num % 6;
			if (r == 3) // 3 | num
				num += 2;
			else if (r == 1)
			{
				if (isPrimeFunc(num))
					return num;
				num += 4;
			}
			while (true)
			{
				if (num < 0)
					throw new OverflowException();
				if (isPrimeFunc(num))
					return num;
				num += 2;
				if (isPrimeFunc(num))
					return num;
				num += 4;
			}
		}
	}
}

