using System;
using System.Collections.Generic;
using System.Linq;
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
		/// Same as <see cref="SegmentedEratosthenes"/> as a compromise between speed and memory usage.
		/// </summary>
		Default = SegmentedEratosthenes,


	}

	class BasePrimeSieve : IPrimeSieve
	{
		public IEnumerable<long> EnumeratePrimes(long startInclusive, long endExclusive)
		{
			foreach (var item in GetPrimes(endExclusive))
			{
				if (item >= startInclusive)
					yield return item;
			}
		}

		public IEnumerable<long> GetPrimes(long maxExclusive)
		{
			throw new NotImplementedException();
		}
	}

	class TrialDivisionSieve :  IPrimeSieve
	{
		/// <summary>
		/// Enumerates the primes starting with next prime greater than or equal to <paramref name="startNumber"/>
		/// up to <paramref name="endNumber"/>.
		/// </summary>
		///<param name="startNumber">The staring number.</param>
		/// <param name="endNumber">Maximum number to iterate to (inclusive).</param>
		/// <returns>Enumerable for prime numbers.</returns>
		public IEnumerable<long> EnumeratePrimes(long startNumber, long endNumber)
		{
			if (endNumber <= 2)
				yield break;
			endNumber--;
			if (startNumber < 2)
				startNumber = 2;
			if (startNumber > endNumber)
				yield break;
			if (endNumber < 2)
				yield break;
			if (startNumber <= 2)
				yield return 2;
			if (endNumber < 3)
				yield break;
			if (startNumber <= 3)
				yield return 3;
			if (endNumber < 5)
				yield break;
			if (startNumber <= 5)
				yield return 5;
			var num = Math.Max(startNumber, 7);
			if ((num & 1) == 0)
				num++;
			var r = num % 6;
			if (r == 3) // 3 | num
				num += 2;
			else if (r == 1)
			{
				if (num <= endNumber && PrimeNumbers.IsPrime(num))
					yield return num;
				num += 4;
			}
			while (num <= endNumber)
			{
				if (PrimeNumbers.IsPrime(num))
					yield return num;
				num += 2;
				if (num > endNumber)
					break;
				if (PrimeNumbers.IsPrime(num))
					yield return num;
				num += 4;
			}

		}


		public IEnumerable<long> GetPrimes(long maxExclusive)
		{
			return EnumeratePrimes(2, maxExclusive);
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

	class Wheel2357Eratosthenes : IPrimeSieve
	{
		protected static int[] shiftTable;

		static int[] downShift;

		protected static bool[] mod0;

		static int ngood;

		const int len = 2 * 3 * 5 * 7;

		static void BuildShiftTable()
		{
			shiftTable = new int[len];
			downShift = new int[len];
			mod0 = new bool[len];
			for (int i = 0; i < len; i++)
			{
				if (i % 2 == 0 || i % 3 == 0 || i % 5 == 0 || i % 7 == 0)
					mod0[i] = true;
				else
					ngood++;
			}
			int first = Array.IndexOf(mod0, false);
			int gCounter = -1;
			for (int i = 0; i < len; i++)
			{
				int next = Array.IndexOf(mod0, false, mod0[i] ? i : i + 1);
				if (next < 0)
				{
					next = len + first;
				}
				shiftTable[i] = next - i;
				if (!mod0[i])
					gCounter++;
				downShift[i] = gCounter;
			}
		}

		static Wheel2357Eratosthenes()
		{
			BuildShiftTable();
		}

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

			var size = maxExclusive * ngood / shiftTable.Length + ngood;
			bool[] composite = new bool[size];
			var sqrt = maxExclusive.IntSqrt();
			long num = 11;
			int remainder = 11;
			int blockNr = 0;
			while (num < maxExclusive)
			{
				var idx = blockNr * ngood + downShift[remainder];
				if (!composite[idx])
				{
					yield return num;
					if (num <= sqrt)
					{
						for (long i = num * num; i < maxExclusive; i += num)
						{
							var shi = i % len;
							if (mod0[shi])
								continue;
							var ix = (i / len) * ngood + downShift[shi];
							composite[ix] = true;
						}
					}
				}
				var shift = shiftTable[remainder];
				num += shift;
				remainder += shift;
				if (remainder >= len)
				{
					remainder -= len;
					blockNr++;
				}
			}
		}
	}


	class SegmentedEratosthenes : IPrimeSieve
	{
		int segmentThreshold;

		long maxExclusive, minInclusive;

		long segmentSize;

		long segmentCount;

		List<long> primes;

		public SegmentedEratosthenes(int segmentThreshold = 10_000)
		{
			this.segmentThreshold = segmentThreshold;
		}

		public IEnumerable<long> GetPrimes(long maxExclusive)
		{
			if (maxExclusive < segmentThreshold)
				return new Wheel2Eratosthenes().GetPrimes(maxExclusive);
			this.maxExclusive = maxExclusive;
			segmentSize = maxExclusive.IntSqrt() + 1;
			segmentCount = maxExclusive / segmentSize;
			return SegmentImpl();
		}

		public IEnumerable<long> GetPrimeRange(long minInclusive, long maxExclusive)
		{
			if (minInclusive <= 2)
				return GetPrimes(maxExclusive);
			if (maxExclusive < segmentThreshold)
				return GetPrimes(maxExclusive).Where(p => p >= minInclusive);
			this.maxExclusive = maxExclusive;
			this.minInclusive = minInclusive;
			segmentSize = maxExclusive.IntSqrt() + 1;
			segmentCount = maxExclusive / segmentSize;

			return SegmentImplRange();
		}

		private IEnumerable<long> SegmentImpl()
		{
			for (long i = 0; i < segmentCount; i++)
			{
				var en = i == 0 ? GetSegment0Primes() : GetSegmentPrimes(i);
				foreach (var prime in en)
				{
					yield return prime;
				}
			}
		}

		private IEnumerable<long> SegmentImplRange()
		{
			var minSeg = minInclusive / segmentSize;
			if (minSeg == segmentCount)
				minSeg--;
			if (minSeg > 0)
				GetSegment0Primes();
			for (long i = minSeg; i < segmentCount; i++)
			{
				var en = i == 0 ? GetSegment0Primes() : GetSegmentPrimes(i);
				foreach (var prime in en)
				{
					if (prime >= minInclusive)
						yield return prime;
				}
			}
		}

		IEnumerable<long> GetSegment0Primes()
		{
			primes = new List<long>();
			foreach (var prime in new Wheel2357Eratosthenes().GetPrimes(segmentSize))
			{
				primes.Add(prime);
			}
			return primes;
		}

		IEnumerable<long> GetSegmentPrimes(long segIndex)
		{
			var sIndex = segIndex * segmentSize;
			var segLen = segIndex == segmentCount - 1 ? maxExclusive - sIndex : segmentSize;
			return GetSegmentPrimes(sIndex, segLen);
		}

		IEnumerable<long> GetSegmentPrimes(long sIndex, long segLen)
		{
			bool[] composite = new bool[segLen];
			var sqrt = (sIndex + segLen).IntSqrt();
			foreach (var p in primes)
			{
				if (p > sqrt)
					break;
				var startJ = p - sIndex % p;
				if (startJ == p)
					startJ = 0;
				for (long j = startJ; j < segLen; j += p)
					composite[j] = true;
			}
			for (long i = 0; i < segLen; i++)
			{
				if (!composite[i])
					yield return i + sIndex;
			}
		}
	}

	class IncrementalEratosthenes : IPrimeSieve
	{
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
			this.maxExclusive = maxExclusive;
			var sqrt = maxExclusive.IntSqrt();
			for (long num = 3; num < maxExclusive; num += 2)
			{
				if (!factorsDict.TryGetValue(num, out Factor factor))
				{
					yield return num;
					if (num <= sqrt)
						factorsDict.Add(num * num, new Factor(num));
					continue;
				}
				factorsDict.Remove(num);
				while (factor != null)
				{
					var next = factor.Next;
					factor.Next = null;
					var newNum = num + factor.Value;
					while (newNum % 2 == 0)
						newNum += factor.Value;
					if (newNum < maxExclusive)
						AddFactors(newNum, factor);
					factor = next;
				}
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
