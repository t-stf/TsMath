

namespace TsMath
{
	using System;

	public static partial class IntegerMath
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

		/// <summary>
		/// Computes the least common multiple.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>The least common multiple.</returns>
		public static int Lcm(int a, int b)
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
		public static int PowMod(int a, int exp, int mod)
		{
			if (exp < 0)
				throw new ArithmeticException("PowMod: negative exponent not allowed.");
			if (mod <= 0)
				throw new ArithmeticException("Modulus must be positive");
			if (exp == 0)
				return 1;
			var x = a;
			var m = exp;
			int temp = 1;
			while (m > 0)
			{
				while ((m & 1) == 0)
				{
					x = (x * x) % mod;
					m >>= 1;
				}
				temp = (temp * x) % mod;
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
		public static int ExtendedGcd(int a, int b, out int x, out int y)
		{
			int r0 = a, r1 = b;
			int zero = 0;
			int s0 = 1, s1 = zero;
			int t0 = s1, t1 = s0;
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
		/// Calculates the power of <paramref name="a"/> raised to <paramref name="exp"/>, which must be a non negative 
		/// natural number
		/// </summary>
		/// <param name="a">Number.</param>
		/// <param name="exp">The exponent.</param>
		/// <returns><paramref name="a"/>^<paramref name="exp"/></returns>
		/// <exception cref="System.ArgumentException">exp>=0 expected</exception>
		public static int Pow(int a, int exp)
		{
			if (exp < 0)
				throw new ArgumentException("exp>=0 expected");

			int prod = 1;
			int factor = a;
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
		/// Computes an a so, that a^2 &lt;=<paramref name="n"/> &lt; (a+1)^2. Very slow 
		/// </summary>
		/// <param name="n">Value</param>
		/// <returns>Square root estimate.</returns>
		public static int ISqrt(this int n)
		{
			if (n < 0)
				throw new ArgumentException("negative square root");
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

	}
}



namespace TsMath
{
	using System;

	public static partial class IntegerMath
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

		/// <summary>
		/// Computes the least common multiple.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>The least common multiple.</returns>
		public static long Lcm(long a, long b)
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
		public static long PowMod(long a, long exp, long mod)
		{
			if (exp < 0)
				throw new ArithmeticException("PowMod: negative exponent not allowed.");
			if (mod <= 0)
				throw new ArithmeticException("Modulus must be positive");
			if (exp == 0)
				return 1;
			var x = a;
			var m = exp;
			long temp = 1;
			while (m > 0)
			{
				while ((m & 1) == 0)
				{
					x = (x * x) % mod;
					m >>= 1;
				}
				temp = (temp * x) % mod;
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
		public static long ExtendedGcd(long a, long b, out long x, out long y)
		{
			long r0 = a, r1 = b;
			long zero = 0;
			long s0 = 1, s1 = zero;
			long t0 = s1, t1 = s0;
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
		/// Calculates the power of <paramref name="a"/> raised to <paramref name="exp"/>, which must be a non negative 
		/// natural number
		/// </summary>
		/// <param name="a">Number.</param>
		/// <param name="exp">The exponent.</param>
		/// <returns><paramref name="a"/>^<paramref name="exp"/></returns>
		/// <exception cref="System.ArgumentException">exp>=0 expected</exception>
		public static long Pow(long a, int exp)
		{
			if (exp < 0)
				throw new ArgumentException("exp>=0 expected");

			long prod = 1;
			long factor = a;
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
		/// Computes an a so, that a^2 &lt;=<paramref name="n"/> &lt; (a+1)^2. Very slow 
		/// </summary>
		/// <param name="n">Value</param>
		/// <returns>Square root estimate.</returns>
		public static long ISqrt(this long n)
		{
			if (n < 0)
				throw new ArgumentException("negative square root");
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

	}
}



namespace TsMath
{
	using System;

	public static partial class IntegerMath
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
					x = (x * x) % mod;
					m >>= 1;
				}
				temp = (temp * x) % mod;
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
		/// Calculates the power of <paramref name="a"/> raised to <paramref name="exp"/>, which must be a non negative 
		/// natural number
		/// </summary>
		/// <param name="a">Number.</param>
		/// <param name="exp">The exponent.</param>
		/// <returns><paramref name="a"/>^<paramref name="exp"/></returns>
		/// <exception cref="System.ArgumentException">exp>=0 expected</exception>
		public static BigInteger Pow(BigInteger a, int exp)
		{
			if (exp < 0)
				throw new ArgumentException("exp>=0 expected");

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
		/// Computes an a so, that a^2 &lt;=<paramref name="n"/> &lt; (a+1)^2. Very slow 
		/// </summary>
		/// <param name="n">Value</param>
		/// <returns>Square root estimate.</returns>
		public static BigInteger ISqrt(this BigInteger n)
		{
			if (n < 0)
				throw new ArgumentException("negative square root");
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

	}
}

