using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsMath.Helpers;

#pragma warning disable CS1591

namespace TsMath
{

	public class PrimeFactor<T>
	{
		public readonly T Factor;

		public int Multiplicity { get; internal set; }

		public bool IsPrime { get; internal set; }

		internal PrimeFactor(T factor, int multi = 1, bool isPrime = true)
		{
			this.Factor = factor;
			this.Multiplicity = multi;
			this.IsPrime = isPrime;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append(Factor);
			int pow = Multiplicity;
			if (pow == 2)
				sb.Append("²");
			else if (pow == 3)
				sb.Append("³");
			else if (pow > 3)
			{
				sb.Append('^');
				sb.Append(pow);
			}
			return sb.ToString();
		}

	}

	public static class PrimeFactorExtensions
	{
		///// <summary>
		///// Retrieves the multiplicity of a prime factor. If the factor is not a factor of <see cref="Number"/>
		///// </summary>
		///// <param name="factor">The prime factor to look for.</param>
		///// <returns>The multiplicity of <paramref name="factor"/> or zero if <paramref name="factor"/> is not 
		///// a prime factor of <see cref="Number"/>.</returns>
		//public static int GetMultiplicity<T>(IList<PrimeFactor<T>> primeFactors, T factor) where T : IComparable<T>
		//{
		//	var index = primeFactors.BinarySearch(primeFactors,factor, );
		//	return index < 0 ? 0 : multiplicity[index];
		//}

		public static string ToString<T>(this IList<PrimeFactor<T>> primeFactors)
		{
			var sb = new StringBuilder();
			for (int i = 0; i < primeFactors.Count; i++)
			{
				if (i > 0)
					sb.Append("*");
				sb.Append(primeFactors[i].ToString());
			}
			return sb.ToString();
		}
	}

	public class FactorizationResult<T>
	{
		public readonly bool IsExact;

		private readonly T[] primeFactors;

		private readonly int[] multiplicity;

		/// <summary>
		/// The number which was factorized.
		/// </summary>
		public readonly T Number;

		public IReadOnlyList<T> Factors => primeFactors;

		public IReadOnlyList<int> Multiplicity => multiplicity;

		public IEnumerable<(T factor, int multiplicity)> FactorsWithMultiplicities
		{
			get
			{
				for (int i = 0; i < primeFactors.Length; i++)
				{
					yield return (primeFactors[i], multiplicity[i]);
				}
			}
		}

		private FactorizationResult()
		{ }

		internal FactorizationResult(T number, T[] facArray, int[] countArray, bool isExact)
		{
			this.Number = number;
			this.primeFactors = facArray;
			this.multiplicity = countArray;
			this.IsExact = isExact;
		}

		/// <summary>
		/// Retrieves the multiplicity of a prime factor. If the factor is not a factor of <see cref="Number"/>
		/// </summary>
		/// <param name="factor">The prime factor to look for.</param>
		/// <returns>The multiplicity of <paramref name="factor"/> or zero if <paramref name="factor"/> is not 
		/// a prime factor of <see cref="Number"/>.</returns>
		public int GetMultiplicity(T factor)
		{
			var index = Array.BinarySearch(primeFactors, factor);
			return index < 0 ? 0 : multiplicity[index];
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			for (int i = 0; i < primeFactors.Length; i++)
			{
				if (i > 0)
					sb.Append("*");
				sb.Append(primeFactors[i]);
				int pow = multiplicity[i];
				if (pow == 2)
					sb.Append("²");
				else if (pow == 3)
					sb.Append("³");
				else if (pow > 3)
				{
					sb.Append('^');
					sb.Append(pow);
				}
			}
			return sb.ToString();
		}
	}

	abstract class PrimeFactorizer<T> where T : IComparable<T>
	{
		protected T number;

		protected Dictionary<T, PrimeFactor<T>> factors;

		protected Stack<T> numbersToCheck;

		public PrimeFactorizer(T number)
		{
			this.number = number;
			factors = new Dictionary<T, PrimeFactor<T>>();
			numbersToCheck = new Stack<T>();
		}

		protected void AddFactor(T factor, int multiplicity = 1, bool isPrime = true)
		{
			if (!factors.TryGetValue(factor, out PrimeFactor<T> fac))
			{
				fac = new PrimeFactor<T>(factor, multiplicity, isPrime);
				factors.Add(factor, fac);
				return;
			}
			fac.IsPrime = isPrime;
			fac.Multiplicity += multiplicity;
		}

		public PrimeFactor<T>[] BuildResult()
		{
			return factors.Values.OrderBy(pf => pf.Factor).ToArray();
		}
	}

	class LongFactorizer : PrimeFactorizer<long>
	{

		public LongFactorizer(long num) : base(num)
		{
		}

		long ExtractSmallFactors(long num)
		{
			for (int i = 0; i < PrimeNumbers.SmallPrimes.Length; i++)
			{
				var prime = PrimeNumbers.SmallPrimes[i];
				num = ExtractMultiple(num, prime, true);
				if (num == 1)
					break;
			}
			return num;
		}

		long ExtractMultiple(long num, long prime, bool isPrime)
		{
			int multiplicity = 0;
			while (true)
			{
				var r = num % prime;
				if (r != 0)
					break;
				num = num / prime;
				multiplicity++;
			}
			if (multiplicity > 0)
			{
				AddFactor(prime, multiplicity, isPrime);
			}
			return num;
		}

		void DoFactorize()
		{
			var num = ExtractSmallFactors(number);
			if (num == 1)
				return;
			numbersToCheck.Push(num);
			while (numbersToCheck.Count > 0)
			{
				num = numbersToCheck.Pop();
				Investigate(num);
			}
		}

		private void Investigate(long num)
		{
			if (num.IsPrime())
			{
				AddFactor(num);
				return;
			}
			var ffac = FermatFactor(num, 1000);
			if (ffac < 0)
			{
				AddFactor(num, 1, false);
				return;
			}
			numbersToCheck.Push(ffac);
			numbersToCheck.Push(num / ffac);
		}

		long FermatFactor(long num, int nRounds = int.MaxValue)
		{


			var a = IntegerMath.IntSqrt(num);
			var b2 = a * a - num;
			var cmp = b2.CompareTo(0);
			if (cmp == 0)
				return a;
			if (cmp < 0)
			{
				b2 += (a << 1) + 1;
				a++;
			}
			while (b2 <= num && nRounds-- > 0)
			{
				var root = b2.IntSqrt();
				if (root * root == b2)
					return a + root;
				b2 += (a << 1) + 1;
				a++;
			}
			return -1;

		}

		internal PrimeFactor<long>[] Factorize()
		{
			if (number <= 1)
				throw new ArgumentException("Only numbers >1 can be factorized");
			DoFactorize();
			return BuildResult();
		}

	}

	class BigIntegerFactorizer : PrimeFactorizer<BigInteger>
	{

		Stack<BigInteger> numbersToCheck;

		public BigIntegerFactorizer(BigInteger num) : base(num)
		{

			numbersToCheck = new Stack<BigInteger>();

			numbersToCheck.Push(num);
		}

		protected BigInteger ExtractSmallFactors(BigInteger num)
		{
			for (int i = 0; i < PrimeNumbers.SmallPrimes.Length; i++)
			{
				BigInteger prim = PrimeNumbers.SmallPrimes[i];
				int multiplicity = 0;
				while (true)
				{
					var (d, r) = num.Divide(prim);
					if (!r.IsZero())
						break;
					num = d;
					multiplicity++;
				}
				if (multiplicity > 0)
					AddFactor(prim, multiplicity);
			}
			return num;
		}
	}

	public class BigInteger_FermatFactorizer
	{
		public static BigInteger FindFactor(BigInteger num)
		{
			var a = IntegerMath.IntSqrt(num);
			var b2 = a * a - num;
			var cmp = b2.CompareTo(0);
			if (cmp == 0)
				return a;
			if (cmp < 0)
			{
				b2 += (a << 1) + 1;
				a++;
			}
			while (b2 <= num)
			{
				var root = b2.IntSqrt();
				if (root * root == b2)
					return a + root;
				b2 += (a << 1) + 1;
				a++;
			}
			return -1;
		}
	}
}
