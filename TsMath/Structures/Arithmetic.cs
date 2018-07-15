using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
#pragma warning disable 1591

namespace TsMath.Structures
{
	/// <summary>
	/// Describes the behavior of numbers and mathematical structures.
	/// </summary>
	/// <typeparam name="T">The type the behavior is applied to.</typeparam>
	public abstract class Arithmetic<T> : IEqualityComparer<T>
	{
		static Dictionary<Type, object> arithmetics;

		public virtual string DomainName => typeof(T).Name;

		public virtual T Zero(T hint) => ThrowNoDefException();

		public virtual T One(T hint) => ThrowNoDefException();

		public virtual T MinusOne(T hint) => Negate(One(hint));

		public virtual T Add(T a, T b) => ThrowNoDefException();

		public virtual T Subtract(T a, T b) => ThrowNoDefException();

		public virtual T Negate(T a) => Subtract(Zero(a), a);

		public virtual T Multiply(T a, T b) => ThrowNoDefException();

		public virtual T Divide(T a, T b) => ThrowNoDefException();

		public virtual T DivideWithRemainder(T a, T b, out T remainder)
		{
			remainder = Zero(a);
			return ThrowNoDefException();
		}

		public virtual T Invert(T a) => Divide(One(a), a);

		public virtual int Sign(T a) => throw new ArithmeticException($"{DomainName} defines no sign function");

		public virtual bool IsNegative(T a) => Sign(a) < 0;

		public virtual T Abs(T a) => Sign(a) < 0 ? Negate(a) : a;

		/// <summary>
		/// Compares the magnitudes of two values. 
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <remarks>This method is used in algorithms which required some kind of a pivot.
		/// </remarks>
		/// <returns>-1,0 or 1 depending on the magnitudes of the arguments.</returns>
		public virtual int CompareMagnitude(T a, T b)
		{
			var aZero = !IsZero(a);
			var bZero = !IsZero(b);
			return aZero.CompareTo(bZero);
		}

		public virtual bool TryParse(string s, out T value)
		{
			value = default;
			return false;
		}

		public virtual string ToString(T value, int maxLen) => value.ToString();

		public virtual T Gcd(T a, T b)
		{
			if (IsZero(a) || IsZero(b))
				throw new DivideByZeroException();
			var zero = Zero(a);
			while (true)
			{
				if (Equals(b, zero))
					return a;
				DivideWithRemainder(a, b, out T r);
				a = b; b = r;
			}
		}

		protected T ThrowNoDefException([CallerMemberName] string methodName = null)
		{
			throw new ArithmeticException($"{DomainName} does not define {methodName}");
		}

		public virtual bool IsZero(T value) => Equals(value, Zero(value));

		public virtual bool Equals(T x, T y) => x.Equals(y);

		public virtual int GetHashCode(T obj) => obj.GetHashCode();

		public static Arithmetic<T> GetArithmetic()
		{
			if (arithmetics == null)
				InitDefaultArithmetics();
			if (!arithmetics.TryGetValue(typeof(T), out object val))
				throw new ArithmeticException($"No arithmetic for type {typeof(T).Name}");
			return (Arithmetic<T>)val;
		}

		public static void SetArithmetic(Type type, Arithmetic<T> arithmetic)
		{
			arithmetics[type] = arithmetic;
		}

		private static void InitDefaultArithmetics()
		{
			arithmetics = new Dictionary<Type, object>
			{
				{ typeof(double), new DoubleArithmetic() },
				{ typeof(long), new LongArithmetic() },
				{ typeof(BigInteger), new BigIntegerArithmetic() },
			};
  //		arithmetics.Add(typeof(Matrix<double>), new MatrixArithmetic<double>());
			arithmetics.Add(typeof(Fraction<long>), new FractionArithmetic<long>());
			arithmetics.Add(typeof(Fraction<BigInteger>), new FractionArithmetic<BigInteger>());
		}

	}
}
