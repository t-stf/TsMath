

namespace TsMath
{
	using System;

	public partial class IntegerMath
	{
		/// <summary>
		/// Computes the greatest common divisor.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>The greatest common divisor</returns>
		/// <exception cref="DivideByZeroException"><paramref name="b"/> is zero.</exception>
		public static int Gcd(int a, int b)
		{
			if (a == 0 || b == 0)
				throw new DivideByZeroException();
			int r;
			while (true)
			{
				if (b == 0)
					return a;
				r = a % b;
				a = b; b = r;
			}
		}
	}
}



namespace TsMath
{
	using System;

	public partial class IntegerMath
	{
		/// <summary>
		/// Computes the greatest common divisor.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>The greatest common divisor</returns>
		/// <exception cref="DivideByZeroException"><paramref name="b"/> is zero.</exception>
		public static long Gcd(long a, long b)
		{
			if (a == 0 || b == 0)
				throw new DivideByZeroException();
			long r;
			while (true)
			{
				if (b == 0)
					return a;
				r = a % b;
				a = b; b = r;
			}
		}
	}
}



namespace TsMath
{
	using System;

	public partial class IntegerMath
	{
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
	}
}

