using System;
using System.Threading;

namespace TsMath
{

	/// <summary>
	/// Pseudo random number generator using a variant of [xorshift](https://en.wikipedia.org/wiki/Xorshift)
	/// </summary>
	public class RandomXorShift : Random
	{
		static long InitCounter = 1;

		ulong s0, s1;

		internal Func<ulong> nextSampleFunc;

		ulong NextSample()
		{
			var x = s0;
			var y = s1;
			s0 = y;
			x ^= x << 23;
			s1 = x ^ y ^ (x >> 17) ^ (y >> 26);
			return s1 + y;
		}

		ulong NextSampleThreadSafe()
		{
			lock (this)
			{
				return NextSample();
			}
		}

		private RandomXorShift(ulong seed0, ulong seed1, bool threadSafe)
		{
			if (seed0 == 0 && seed1 == 0)
				throw new ArgumentException("At least one seed must be nonzero");
			this.s0 = seed0 ^ 0xBF58476D1CE4E5B9L;
			this.s1 = seed1 ^ 0x9E3779B97F4A7C15L;
			nextSampleFunc = threadSafe ? (Func<ulong>)NextSampleThreadSafe : NextSample;
		}

		/// <summary>
		/// Initializes a new instance of the random number generator class, using a time-dependent default seed value.
		/// </summary>
		/// <param name="threadSafe">If <b>true</b> all calls to instance members are thread safe.</param>
		/// <remarks>The starting value is derived from a combination of the system clock and a static variable that is
		/// incremented with every constructor call. 
		/// As a result, different <see cref="RandomXorShift"/> objects that are created in close succession by a call to the 
		/// default constructor will never have identical seed values.
		/// </remarks>
		public RandomXorShift(bool threadSafe = false)
			: this((ulong)DateTime.Now.Ticks, (ulong)Interlocked.Increment(ref InitCounter), threadSafe)
		{
		}

		/// <summary>
		/// Initializes a new instance of the random number generator class using the specified seed.
		/// </summary>
		/// <param name="seed">The seed value, used as starting value for the number generator.</param>
		/// <param name="threadSafe">If <b>true</b> all calls to instance members are thread safe.</param>
		/// <remarks>
		/// Providing an identical <paramref name="seed"/> value to different <see cref="RandomXorShift"/> objects 
		/// causes each instance to produce an identical sequences of random numbers. 
		/// </remarks>
		public RandomXorShift(int seed, bool threadSafe = false) : this((ulong)seed, 0x55555555_55555555L, threadSafe) { }

		/// <summary>
		/// Returns a random floating-point number between 0.0 and 1.0 (exclusive).
		/// </summary>
		/// <returns>A number that is greater than or equal to 0.0, and less than 1.0.</returns>
		protected override double Sample()
		{
			double d0 = nextSampleFunc() << 1;
			double d1 = (ulong.MaxValue << 1) + 1;
			return d0 / d1;
		}

		/// <summary>
		/// Returns a non-negative random integer.
		/// </summary>
		/// <returns>A integer that is greater than or equal to 0 and less than <see cref="int.MaxValue"/>.</returns>
		public override int Next() => (int)(nextSampleFunc() % int.MaxValue);

		/// <summary>
		/// Returns a non-negative random integer less than <paramref name="maxValue"/>.
		/// </summary>
		/// <param name="maxValue">The exclusive upper bound of the random number to be generated.
		/// It must be greater than or equal to 0.</param>
		/// <returns>A random number >=0 and &lt; <paramref name="maxValue"/>. 0 if <paramref name="maxValue"/>=0. </returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
		public override int Next(int maxValue) => this.NextValue(maxValue);

		/// <summary>
		/// Returns a random integer that is within a specified range.
		/// </summary>
		/// <param name="minValue">The inclusive lower bound of the random number returned.</param>
		/// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
		/// <returns>A integer greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>.
		/// </returns>
		public override int Next(int minValue, int maxValue) => minValue + this.NextValue(maxValue - minValue);

		/// <summary>
		/// Fills the elements of a specified array of bytes with random numbers.
		/// </summary>
		/// <param name="buffer">An array of bytes to fill with random numbers.</param>
		public override void NextBytes(byte[] buffer)
		{
			var rval = nextSampleFunc();
			int rIndex = 0;
			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i] = (byte)rval;
				rIndex++;
				if (rIndex >= 8)
				{
					rIndex = 0;
					rval = nextSampleFunc();
				}
				else
					rval >>= 8;
			}
		}

	}

	/// <summary>
	/// Extension function for random number generation.
	/// </summary>
	public static class RandomXorShiftExtensions
	{

		/// <summary>
		/// Returns a non-negative random integer less than <paramref name="maxValue"/>.
		/// </summary>
		/// <param name="rand">The random number generator to use.</param>
		/// <param name="maxValue">The exclusive upper bound of the random number to be generated.
		/// It must be greater than or equal to 0.</param>
		/// <returns>A random number >=0 and &lt; <paramref name="maxValue"/>. 0 if <paramref name="maxValue"/>=0. </returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
		public static int NextValue(this RandomXorShift rand, int maxValue)
		{
			if (maxValue <= 0)
			{
				if (maxValue == 0)
					return 0;
				throw new ArgumentOutOfRangeException();
			}
			return (int)(rand.nextSampleFunc() % (ulong)maxValue);
		}

		/// <summary>
		/// Returns a non-negative random <see cref="Int64"/> integer less than <paramref name="maxValue"/>.
		/// </summary>
		/// <param name="rand">The random number generator to use.</param>
		/// <param name="maxValue">The exclusive upper bound of the random number to be generated.
		/// It must be greater than or equal to 0.</param>
		/// <returns>A random number >=0 and &lt; <paramref name="maxValue"/>. 0 if <paramref name="maxValue"/>=0. </returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
		public static long NextValue(this RandomXorShift rand, long maxValue)
		{
			if (maxValue <= 0)
			{
				if (maxValue == 0)
					return 0;
				throw new ArgumentOutOfRangeException();
			}
			return (long)(rand.nextSampleFunc() % (ulong)maxValue);
		}

		/// <summary>
		/// Returns a random <see cref="Int64"/> integer that is within a specified range.
		/// </summary>
		/// <param name="rand">The random number generator to use.</param>
		/// <param name="minValue">The inclusive lower bound of the random number returned.</param>
		/// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
		/// <returns>A integer greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>.
		/// </returns>
		public static long NextValue(this RandomXorShift rand, long minValue, long maxValue) => minValue + rand.NextValue(maxValue - minValue);

		/// <summary>
		/// Returns a non-negative random <see cref="BigInteger"/> integer less than <paramref name="maxValue"/>.
		/// </summary>
		/// <param name="rand">The random number generator to use.</param>
		/// <param name="maxValue">The exclusive upper bound of the random number to be generated.
		/// It must be greater than or equal to 0.</param>
		/// <returns>A random number >=0 and &lt; <paramref name="maxValue"/>. 0 if <paramref name="maxValue"/>=0. </returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
		public static BigInteger NextValue(this RandomXorShift rand, BigInteger maxValue)
		{
			if (maxValue.Sign <= 0)
			{
				if (maxValue.IsZero)
					return 0;
				throw new ArgumentOutOfRangeException();
			}
			int ndigits = maxValue.DigitCount;
			uint[] digits = new uint[ndigits];
			var rval = rand.nextSampleFunc();
			int rIndex = 0;
			for (int i = 0; i < ndigits; i++)
			{
				digits[i] = (uint)rval;
				rIndex++;
				if (rIndex < 2)
					rval >>= BigInteger.BaseOps.BitsPerDigit;
				else
				{
					rIndex = 0;
					rval = rand.nextSampleFunc();
				}
			}
			var num = new BigInteger(digits, false);
			return num % maxValue;
		}

		/// <summary>
		/// Generates a next random number for a random variable of the normal (Gauss) distribution.
		/// </summary>
		/// <remarks>
		/// The method uses the [Box–Muller transform](https://en.wikipedia.org/wiki/Box%E2%80%93Muller_transform)
		/// which is reasonably fast.
		/// </remarks>
		/// <param name="rand">The random number generator to use for generating uniformly distributed numbers.</param>
		/// <param name="mean">The mean of the normal distribution.</param>
		/// <param name="stdDev">The standard deviation of the normal distribution.</param>
		/// <returns>A normally distributed random number.</returns>
		public static double NextGaussian(this Random rand, double mean = 0, double stdDev = 1)
		{
			double u1 = 1.0 - rand.NextDouble();
			double u2 = rand.NextDouble();
			
			const double twoPi = 2.0 * Math.PI;
			double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(twoPi * u2); //random normal(0,1)
			return mean + stdDev * randStdNormal;
		}

	}

}
