using System;
using System.Collections.Generic;

namespace TsMath
{
	/// <summary>
	/// Utility functions for generating and working with prime numbers.
	/// </summary>
	public static partial class PrimeNumbers
	{

		/// <summary>
		/// Some small prime number for quick checks
		/// </summary>
		internal static int[] SmallPrimes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53 };

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
			for (int i = 2; i < SmallPrimes.Length; i++)
			{
				var testNum = SmallPrimes[i];
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
		/// Enumerates the primes starting with 2. 
		/// </summary>
		/// <remarks>
		/// Use this method if you does not know the upper bound for your prime numbers.
		/// It uses a incremental sieve of Eratosthenes and is slower as any specialized sieve 
		/// you can get with a call to <see cref="GetSieve(PrimeSieveType)"/>.
		/// </remarks>
		/// <returns>Enumerable for prime numbers.</returns>
		public static IEnumerable<long> EnumeratePrimes() => new IncrementalEratosthenes().GetPrimes(long.MaxValue);

		/// <summary>
		/// Enumerates the primes starting with 2 up to <paramref name="maxExclusive"/>.
		/// </summary>
		/// <remarks>This is a shortcut for sieving with the default sieve (<see cref="GetSieve(PrimeSieveType)"/>).
		/// </remarks>
		/// <param name="maxExclusive">Maximum number to iterate to (exclusive).</param>
		/// <returns>Enumerable for prime numbers.</returns>
		public static IEnumerable<long> EnumeratePrimes(long maxExclusive) => GetSieve().GetPrimes(maxExclusive);

		/// <summary>
		/// Enumerates a range of primes between <paramref name="minInclusive"/> and <paramref name="maxExclusive"/>.
		/// </summary>
		/// <remarks>
		/// This method uses a variant of the segmented Eratosthenes sieve (<see cref="PrimeSieveType.SegmentedEratosthenes"/>).
		/// For very large starting numbers <paramref name="minInclusive"/> there is a significant preprocessing time
		/// required before the iteration yields the first value.
		/// </remarks>
		/// <param name="minInclusive">The starting number (inclusive) to iterate from.</param>
		/// <param name="maxExclusive">Maximum number to iterate to (exclusive).</param>
		/// <returns>Enumerable for prime numbers.</returns>
		public static IEnumerable<long> EnumeratePrimeRange(long minInclusive, long maxExclusive )
			=> new SegmentedEratosthenes().GetPrimeRange(minInclusive, maxExclusive);

		/// <summary>
		/// Generates the first <paramref name="count"/> primes.
		/// </summary>
		/// <param name="count">The number of primes to generate.</param>
		/// <param name="sieve">The sieve to use for the prime number generation.</param>
		/// <returns>Containing an array with <paramref name="count"/> primes: 2, 3, 5, ...</returns>
		public static long[] GeneratePrimes(int count, IPrimeSieve sieve = null)
		{
			var result = new long[count];
			sieve = sieve ?? GetSieve();
			int i = 0;
			long endNumber = (long)(100 + 1.1056 * count / Math.Log(count));
			foreach (var prime in sieve.GetPrimes(endNumber))
			{
				result[i++] = prime;
				if (i >= count)
					break;
			}
			return result;
		}

		/// <summary>
		/// Generates primes less than <paramref name="maxExlusive"/>.
		/// </summary>
		/// <param name="maxExlusive">The number up to which to create primes.</param>
		/// <param name="sieve">The sieve to use for the prime number generation.</param>
		/// <returns>An array with primes less than <paramref name="maxExlusive"/>.</returns>
		public static long[] GeneratePrimesUpTo(int maxExlusive, IPrimeSieve sieve = null)
		{
			var result = new List<long>();
			sieve = sieve ?? GetSieve();
			foreach (var prime in sieve.GetPrimes(maxExlusive))
			{
				if (prime >= maxExlusive)
					break;
				result.Add(prime);
			}
			return result.ToArray();
		}

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


		/// <summary>
		/// Retrieves a sieve for the generation of prime numbers.
		/// </summary>
		/// <param name="sieveType">The type of sieve requested.</param>
		/// <returns>A sieve to generate prime numbers starting from 2.</returns>
		public static IPrimeSieve GetSieve(PrimeSieveType sieveType = PrimeSieveType.Default)
		{
			switch (sieveType)
			{
				case PrimeSieveType.Eratosthenes: return new Wheel2357Eratosthenes();
				case PrimeSieveType.SegmentedEratosthenes: return new SegmentedEratosthenes();
				case PrimeSieveType.TrialDivision: return new TrialDivisionSieve();
			}
			throw new ArgumentException("Invalid sieve type", nameof(sieveType));
		}

		/// <summary>
		///  Retrieves the prime factors of a number.
		/// </summary>
		/// <param name="num">The number to factorize.</param>
		/// <returns>The result of the factorization, containing the prime factors.</returns>
		public static PrimeFactor<long>[] Factorize(long num)
		{
			var fact = new LongFactorizer(num);
			return fact.Factorize();
		}

	}
}
