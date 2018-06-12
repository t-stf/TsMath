using System;
using System.Collections.Generic;
using System.Text;

namespace TsMath
{
	/// <summary>
	/// Interface which implement all classes, which can generate primes through sieving.
	/// </summary>
	public interface IPrimeSieve
	{
		/// <summary>
		/// Get all prime numbers from 2 up to <paramref name="maxExclusive"/>  - 1. 
		/// </summary>
		/// <param name="maxExclusive">The exclusive upper bound for the prime number generation.</param>
		/// <returns>An enumerable for the prime numbers.</returns>
		IEnumerable<long> GetPrimes(long maxExclusive);
	}

	/// <summary>
	/// The type of the prime sieve.
	/// </summary>
	public enum PrimeSieveType
	{
		/// <summary>
		/// Generates primes through trial division. 
		/// </summary>
		/// <remarks>
		/// This sieve is the slowest of the available sieves. Its memory use is the smallest and does not depend on the maximum number.
		/// </remarks>
		TrialDivision,

		/// <summary>
		/// Sieve of Eratosthenes with some improvements.
		/// </summary>
		///<remarks>
		///	This sieve is best used for Its memory use depends on the maximum number.
		/// </remarks>
		Eratosthenes,

		/// <summary>
		/// A segmented version of the sieve of Eratosthenes. Its memory use does not depend on the maximum number.
		/// </summary>
		/// <remarks>
		/// It is a little slower than the <see cref="Eratosthenes"/> sieve but its memory use does not depend on the maximum number.
		/// </remarks>
		SegmentedEratosthenes,

		/// <summary>
		/// An incremental version of the sieve of Eratosthenes.
		/// </summary>
		/// <remarks>
		/// It is a little slower than <see cref="SegmentedEratosthenes"/> but it is usable even with very large maximum numbers.
		/// </remarks>
		IncrementalEratosthenes,

		/// <summary>
		/// Same as <see cref="SegmentedEratosthenes"/> as a compromise between speed and memory usage.
		/// </summary>
		Default = SegmentedEratosthenes,


	}

	class TrialDivisionSieve : IPrimeSieve
	{
		public IEnumerable<long> GetPrimes(long maxExclusive)
		{
			return PrimeNumbers.EnumeratePrimes(2, maxExclusive - 1);
		}
	}

	class Wheel2Eratosthenes : IPrimeSieve
	{
		public IEnumerable<long> GetPrimes(long maxExclusive)
		{
			if (maxExclusive <= 2)
				yield break;
			bool[] composite = new bool[maxExclusive / 2];
			var sqrt = maxExclusive.IntSqrt();

			for (long i = 3; i <= sqrt; i += 2)
			{
				if (composite[i >> 1])
					continue;
				for (long j = i * i; j < maxExclusive; j += i)
				{
					if ((j & 1) == 0)
						continue;
					composite[j >> 1] = true;
				}
			}
			yield return 2;
			for (long i = 1; i < maxExclusive / 2; i++)
			{
				if (!composite[i])
					yield return (i << 1) + 1;
			}
		}
	}

	class SegmentedEratosthenes : IPrimeSieve
	{
		long segmentThreshold;

		long maxExclusive;

		long segmentSize;

		long segmentCount;

		List<long> primes;

		public SegmentedEratosthenes(long segmentThreshold = 10_000)
		{
			this.segmentThreshold = segmentThreshold;
		}

		public IEnumerable<long> GetPrimes(long maxExclusive)
		{
			if (maxExclusive < segmentThreshold)
				return new Wheel2Eratosthenes().GetPrimes(maxExclusive);
			this.maxExclusive = maxExclusive;
			segmentSize = maxExclusive.IntSqrt();
			segmentCount = maxExclusive / segmentSize;

			return SegmentImpl();
		}

		private IEnumerable<long> SegmentImpl()
		{
			for (int i = 0; i < segmentCount; i++)
			{
				var en = i == 0 ? GetSegment0Primes() : GetSegmentPrimes(i);
				foreach (var prime in en)
				{
					yield return prime;
				}
			}
		}

		IEnumerable<long> GetSegment0Primes()
		{
			primes = new List<long>();
			foreach (var prime in new Wheel2Eratosthenes().GetPrimes(segmentSize + 1))
			{
				yield return prime;
				primes.Add(prime);
			}
		}

		IEnumerable<long> GetSegmentPrimes(long segIndex)
		{
			var sIndex = segIndex * segmentSize;
			var eIndex = segIndex == segmentCount - 1 ? maxExclusive : sIndex + segmentSize;
			eIndex--;
			return GetSegmentPrimes(sIndex, eIndex, primes);
		}

		IEnumerable<long> GetSegmentPrimes(long sIndex, long eIndex, IEnumerable<long> primes)
		{
			bool[] composite = new bool[eIndex - sIndex + 1];
			var sqrt = eIndex.IntSqrt();
			foreach (var p in primes)
			{
				if (p > sqrt)
					break;
				var startJ = p - sIndex % p;
				if (startJ == p)
					startJ = 0;
				for (long j = startJ; j < composite.LongLength; j += p)
				{
					composite[j] = true;
				}
			}
			for (long i = 0; i < composite.LongLength; i++)
			{
				if (!composite[i])
					yield return i + sIndex;
			}
		}

	}

	class IncrEratosthenesWheel : IPrimeSieve
	{

		protected static int[] shiftTable;

		protected static bool[] mod0;

		static void BuildShiftTable()
		{
			int len = 2 * 3 * 5 * 7;
			shiftTable = new int[len];
			mod0 = new bool[len];
			for (int i = 0; i < len; i++)
			{
				if (i % 2 == 0 || i % 3 == 0 || i % 5 == 0 || i % 7 == 0)
					mod0[i] = true;
			}
			int first = Array.IndexOf(mod0, false);
			for (int i = 0; i < len; i++)
			{
				int next = Array.IndexOf(mod0, false, mod0[i] ? i : i + 1);
				if (next < 0)
				{
					next = len + first;
				}
				shiftTable[i] = next - i;
			}
		}

		static IncrEratosthenesWheel()
		{
			BuildShiftTable();
		}

		class Factor
		{
			public long Value;

			public Factor Next;

			public Factor(long value)
			{
				this.Value = value;
			}

			public override string ToString()
			{
				var fac = this;
				var sb = new StringBuilder();
				while (fac != null)
				{
					sb.Append(fac.Value);
					sb.Append(' ');
					fac = fac.Next;
				}
				return sb.ToString();
			}
		}

		Dictionary<long, Factor> factorsDict = new Dictionary<long, Factor>();

		long maxExclusive;

		public IEnumerable<long> GetPrimes(long maxExclusive)
		{
			if (maxExclusive <= 2)
				yield break;
			yield return 2;
			if (maxExclusive <= 3)
				yield break;
			yield return 3;
			if (maxExclusive <= 5)
				yield break;
			yield return 5;
			if (maxExclusive <= 7)
				yield break;
			yield return 7;
			if (maxExclusive <= 11)
				yield break;

			this.maxExclusive = maxExclusive;

			var sqrt = maxExclusive.IntSqrt();

			long num = 11;
			const long modVal = 2 * 3 * 5 * 7;
			while (num < maxExclusive)
			{
				if (!factorsDict.TryGetValue(num, out Factor factor))
				{
					yield return num;
					if (num <= sqrt)
						factorsDict.Add(num * num, new Factor(num));
				}
				else
				{
					factorsDict.Remove(num);
					while (factor != null)
					{
						var next = factor.Next;
						factor.Next = null;
						var newNum = num + 2 * factor.Value;
						while (mod0[newNum % modVal])
							newNum += factor.Value;
						if (newNum < maxExclusive)
							AddFactors(newNum, factor);
						factor = next;
					}
				}
				num += shiftTable[num % modVal];
			}
		}

		private void AddFactors(long num, Factor root)
		{
			if (!factorsDict.TryGetValue(num, out Factor existing))
			{
				factorsDict.Add(num, root);
				return;
			}
			while (existing.Next != null)
				existing = existing.Next;
			existing.Next = root;

		}
	}


}
