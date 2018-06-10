using System;
using System.Collections.Generic;

namespace TsMath
{
	/// <summary>
	/// Utility functions for generating and working with prime numbers.
	/// </summary>
	public static partial class PrimeNumbers
	{

		static long[] primes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53 };

		/// <summary>
		/// Tests a number for primality. The result is always exact.
		/// </summary>
		/// <param name="num">The number to test.</param>
		/// <returns><b>true</b> if the number is a prime number; <b>false</b> if it is composite.</returns>
		public static bool IsPrime(this long num)
		{
			if (num <= 1)
				return false;
			if (num <= 3)
				return true;
			if ((num & 1) == 0)
				return false;
			var end = IntegerMath.IntSqrt(num);
			for (int i = 2; i < primes.Length; i++)
			{
				var testNum = primes[i];
				if (testNum > end)
					return true;
				if (num % testNum == 0)
					return false;
			}
			return SPRPLong(num);
		}

		// numbers from http://primes.utm.edu/prove/prove2_3.html
		private static bool SPRPLong(long num)
		{
			if (!SPRP(num, 2))
				return false;
			if (!SPRP(num, 3))
				return false;
			if (num < 1_373_653)
				return true;
			if (!SPRP(num, 5))
				return false;
			if (num < 25_326_001)
				return true;
			if (!SPRP(num, 7))
				return false;
			if (!SPRP(num, 11))
				return false;
			if (num < 2_152_302_898_747)
				return true;
			if (!SPRP(num, 13))
				return false;
			if (num < 3_474_749_660_383)
				return true;
			if (!SPRP(num, 17))
				return false;
			if (num < 341_550_071_728_321)
				return true;
			if (!SPRP(num, 19))
				return false;
			if (!SPRP(num, 23))
				return false;
			if (num < 3_825_123_056_546_413_051)
				return true;
			return SPRP(num, 29) && SPRP(num, 31) && SPRP(num, 37);
		}

		/// <summary>
		/// Tests a number for primality.
		/// </summary>
		/// <remarks>
		/// If <paramref name="num"/> is less than <see cref="Int64.MaxValue"/> the result is exact.
		/// Otherwise the probability that a composite number is identified a prime is approximately
		/// 10^(-10).
		/// </remarks>
		/// <param name="num">The number to test.</param>
		/// <returns><b>true</b> if the number is probably a prime; <b>false</b> if it is composite.</returns>
		public static bool IsProbablePrime(this BigInteger num)
		{
			if (num <= 1)
				return false;
			if (num < long.MaxValue)
				return IsPrime(num.ToLong());
			return IsProbablyPrime_MillerRabinTest(num, 20, new RandomXorShift());
		}

		/// <summary>
		/// Generates the next prime number greater than <paramref name="num"/>.
		/// </summary>
		/// <param name="num">The starting number.</param>
		/// <returns>The next prime number greater than <paramref name="num"/>.</returns>
		/// <exception cref="OverflowException">The result would be greater than <see cref="Int64.MaxValue"/>.</exception>
		public static long NextPrime(this long num) => NextPrime(num, p => IsPrime(p));

		/// <summary>
		/// Generates the next probable prime number greater than <paramref name="num"/>.
		/// </summary>
		/// <param name="num">The starting number.</param>
		/// <returns>The next probable prime number greater than <paramref name="num"/>.</returns>
		public static BigInteger NextProbablePrime(this BigInteger num) => NextPrime(num, p => IsProbablePrime(p));

		/// <summary>
		/// Generates the first <paramref name="count"/> primes.
		/// </summary>
		/// <param name="count">The number of primes to generate.</param>
		/// <returns>Containing an array with <paramref name="count"/> primes: 2, 3, 5, ...</returns>
		public static long[] GeneratePrimes(int count)
		{
			var result = new long[count];
			int i = 0;
			foreach (var prime in EnumeratePrimes())
			{
				result[i++] = prime;
				if (i >= count)
					break;
			}
			return result;
		}

		/// <summary>
		/// Generates primes less than or equal to <paramref name="number"/>.
		/// </summary>
		/// <param name="number">The number up to which to create primes.</param>
		/// <returns>An array with primes less than or equal to <paramref name="number"/>.</returns>
		public static long[] GeneratePrimesUpTo(int number)
		{
			var result = new List<long>();
			foreach (var prime in EnumeratePrimes())
			{
				if (prime > number)
					break;
				result.Add(prime);
			}
			return result.ToArray();
		}

		/// <summary>
		/// Enumerates the primes starting with next prime greater than or equal to <paramref name="startNumber"/>
		/// up to <paramref name="endNumber"/>.
		/// </summary>
		///<param name="startNumber">The staring number.</param>
		/// <param name="endNumber">Maximum number to iterate to (inclusive).</param>
		/// <returns>Enumerable for prime numbers.</returns>
		public static IEnumerable<long> EnumeratePrimes(long startNumber, long endNumber)
		{
			if (IsPrime(startNumber))
				yield return startNumber;
			var num = startNumber;
			while (true)
			{
				num = NextPrime(num);
				if (num > endNumber)
					break;
				yield return num;
			}
		}

		/// <summary>
		/// Enumerates the primes starting with next prime greater than or equal to <paramref name="startNumber"/>.
		/// </summary>
		///<param name="startNumber">The staring number.</param>
		/// <returns>Enumerable for prime numbers.</returns>
		public static IEnumerable<long> EnumeratePrimes(long startNumber = 2) => EnumeratePrimes(startNumber, long.MaxValue);

		/// <summary>
		/// Get the prime factors of a number. Starting with the lowest.
		/// </summary>
		/// <param name="num">The number to factorize.</param>
		/// <returns>Enumerable of prime factors, as pairs (factor, exponent)</returns>
		public static List<(long factor, int count)> GetPrimeFactors(long num) => GetPrimeFactors(num, NextPrime);

		/// <summary>
		/// Get the prime factors of a number. Starting with the lowest.
		/// </summary>
		/// <param name="num">The number to factorize.</param>
		/// <returns>Enumerable of prime factors, as pairs (factor, exponent)</returns>
		public static List<(BigInteger factor, int count)> GetPrimeFactors(BigInteger num) => GetPrimeFactors(num, NextProbablePrime);

	}
}
