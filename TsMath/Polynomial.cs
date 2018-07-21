using System;
using System.Text;
using TsMath.Structures;

namespace TsMath
{
	/// <summary>
	/// A polynomial.
	/// </summary>
	/// <typeparam name="T">Supported are <see cref="Double"/>, <see cref="Fraction{T}"/> or any type with an arithmetic, that 
	/// supports division.</typeparam>
	public class Polynomial<T> : IEquatable<Polynomial<T>>
	{
		static readonly Arithmetic<T> arithmetic = Arithmetic<T>.GetArithmetic();

		T[] coeffs;

		T zero;

		/// <summary>
		/// The degree of the polynomial.
		/// </summary>
		/// <remarks>The degree of the zero polynomial is 0, the same as a non zero constant polynomial.</remarks>
		public int Degree { get; private set; }

		/// <summary>
		/// Checks if this polynomial is zero, meaning that all coefficients are zero.
		/// </summary>
		/// <returns><b>true</b> if this polynomial is zero; <b>false</b> otherwise.</returns>
		public bool IsZero() => Degree == 0 & arithmetic.IsZero(this[0]);

		/// <summary>
		/// Constructs a polynomial.
		/// </summary>
		/// <param name="coefficients">The coefficients in ascending order.</param>
		/// <remarks>
		/// <para>
		/// The resulting polynomial is of the form: <paramref name="coefficients"/>[0]+<paramref name="coefficients"/>[1]x+<paramref name="coefficients"/>[2]x²...
		/// </para>
		/// <para>The array <paramref name="coefficients"/> is not copied! So all outside changes to this array are propagated
		/// to the coefficients to the polynomial.</para>
		/// </remarks>
		public Polynomial(params T[] coefficients)
		{
			Init(coefficients);
		}

		private Polynomial(T value, int exponent)
		{
			coeffs = new T[exponent + 1];
			coeffs[exponent] = value;
			Degree = exponent;
			zero = arithmetic.Zero(value);
			for (int i = 0; i < exponent; i++)
			{
				coeffs[i] = zero;
			}
		}

		private void Init(T[] coefficients)
		{
			if (coefficients == null || coefficients.Length == 0)
			{
				throw new ArgumentException("Empty polynomial construction not possible");
			}
			zero = arithmetic.Zero(coefficients[0]);
			coeffs = coefficients;
			int i;
			for (i = coefficients.Length - 1; i > 0; i--)
			{
				if (!arithmetic.IsZero(coefficients[i]))
					break;
			}
			Degree = i;
		}

		/// <summary>
		/// Get the coefficient at position x^<paramref name="index"/>.
		/// </summary>
		/// <param name="index">The power index.</param>
		/// <returns>The coefficient at position x^<paramref name="index"/>.</returns>
		public T this[int index] => index >= coeffs.Length ? zero : coeffs[index];

		/// <summary>
		/// Adds two polynomials.
		/// </summary>
		/// <param name="a">The first summand.</param>
		/// <param name="b">The second summand.</param>
		/// <returns>The result of the addition.</returns>
		public static Polynomial<T> operator +(in Polynomial<T> a, in Polynomial<T> b)
		{
			int n = Math.Max(a.Degree, b.Degree) + 1;
			var result = new T[n];
			for (int i = 0; i < n; i++)
			{
				result[i] = arithmetic.Add(a[i], b[i]);
			}
			return new Polynomial<T>(result);
		}

		/// <summary>
		/// Subtracts two polynomials.
		/// </summary>
		/// <param name="a">The first summand.</param>
		/// <param name="b">The second summand.</param>
		/// <returns>The result of the addition.</returns>
		public static Polynomial<T> operator -(in Polynomial<T> a, in Polynomial<T> b)
		{
			int n = Math.Max(a.Degree, b.Degree) + 1;
			var result = new T[n];
			for (int i = 0; i < n; i++)
			{
				result[i] = arithmetic.Subtract(a[i], b[i]);
			}
			return new Polynomial<T>(result);
		}

		/// <summary>
		/// Implements the negation (unary minus) operator.
		/// </summary>
		/// <param name="a">The number to negate.</param>
		/// <returns>
		/// The negated value.
		/// </returns>
		public static Polynomial<T> operator -(Polynomial<T> a)
		{
			int n = a.Degree + 1;
			var result = new T[n];
			for (int i = 0; i < n; i++)
			{
				result[i] = arithmetic.Negate(a[i]);
			}
			return new Polynomial<T>(result);
		}

		/// <summary>
		/// Multiplies two polynomials.
		/// </summary>
		/// <param name="a">First number to multiply.</param>
		/// <param name="b">Second number to multiply.</param>
		/// <returns>The product.</returns>
		public static Polynomial<T> operator *(Polynomial<T> a, Polynomial<T> b)
		{
			int n = a.Degree + b.Degree + 1;
			var result = new T[n];
			for (int i = 0; i < n; i++)
			{
				result[i] = a.zero;
			}
			for (int i = 0; i <= a.Degree; i++)
			{
				var ai = a[i];
				for (int j = 0; j <= b.Degree; j++)
				{
					result[i + j] = arithmetic.Add(result[i + j], arithmetic.Multiply(ai, b[j]));
				}
			}
			return new Polynomial<T>(result);
		}

		/// <summary>
		/// Division with remainder. Solves <paramref name="dividend"/> = <paramref name="divisor"/> * q + <paramref name="remainder"/>;
		/// </summary>
		/// <param name="dividend">Dividend.</param>
		/// <param name="divisor">Divisor.</param>
		/// <param name="remainder">Remainder; can be negative.</param>
		/// <returns>Quotient q in the equation above.</returns>
		/// <exception cref="DivideByZeroException">The <paramref name="divisor"/> is zero.</exception>
		public static Polynomial<T> DivRem(Polynomial<T> dividend, Polynomial<T> divisor, out Polynomial<T> remainder)
		{
			if (divisor.IsZero())
				throw new DivideByZeroException();
			var q = new Polynomial<T>(dividend.zero);
			remainder = dividend;
			while (!remainder.IsZero() && remainder.Degree >= divisor.Degree)
			{
				var tcoeff = arithmetic.Divide(remainder[remainder.Degree], divisor[divisor.Degree]);
				var t = new Polynomial<T>(tcoeff, remainder.Degree - divisor.Degree);
				q += t;
				remainder -= t * divisor;
			}
			return q;
		}

		/// <summary>
		/// Returns a <see cref="string" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="string" /> that represents this instance.
		/// </returns>	
		public override string ToString() => ToString(TsMathGlobals.DefaultToStringLength);

		/// <summary>
		/// Returns a <see cref="string" /> that represents this instance.
		/// </summary>
		/// <param name="maxLen">The maximum requested length of the string.</param>
		/// <param name="varName">The variable name to use in the string form.</param>
		/// <returns>
		/// A <see cref="string" /> that represents this instance.
		/// </returns>	
		public string ToString(int maxLen, string varName = "x")
		{
			var sb = new StringBuilder();
			for (int i = Degree; i >= 0; i--)
			{
				var ce = coeffs[i];
				if (arithmetic.IsZero(ce))
					continue;
				if (sb.Length > maxLen)
				{
					sb.Append("...");
					break;
				}
				var rlen = Math.Max(10, maxLen - sb.Length);
				var s = arithmetic.ToString(ce, rlen);
				if (!(s[0] == '+' || s[0] == '-'))
					sb.Append('+');
				sb.Append(s);
				if (i >= 1)
				{
					sb.Append(varName);
					if (i > 1)
						if (i == 2)
							sb.Append('²');
						else if (i == 3)
							sb.Append('³');
						else
							sb.Append("^" + i);
				}
			}
			if (sb.Length == 0)
				sb.Append('0');
			return sb.ToString();
		}

		#region Equality
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// <b>true</b> if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, <b>false</b>.
		/// </returns>
		public bool Equals(Polynomial<T> other) => Equals(this, other);

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <b>true</b> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <b>false</b>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (!(obj is Polynomial<T> poly))
				return false;
			return Equals(this, poly);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="a">First polynomial.</param>
		/// <param name="b">Second polynomial.</param>
		/// <returns>
		/// <b>true</b> if both numbers are equal; <b>false</b> otherwise.
		/// </returns>
		public static bool operator ==(Polynomial<T> a, Polynomial<T> b) => Equals(a, b);

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="a">First polynomial.</param>
		/// <param name="b">Second polynomial.</param>
		/// <returns>
		/// <b>true</b> if both numbers are not equal; <b>false</b> otherwise.
		/// </returns>
		public static bool operator !=(Polynomial<T> a, Polynomial<T> b) => !Equals(a, b);

		private static bool Equals(Polynomial<T> a, Polynomial<T> b)
		{
			if (ReferenceEquals(a, b))
				return true;
			if (a is null || b is null)
				return false;
			if (a.Degree != b.Degree)
				return false;
			for (int i = 0; i <= a.Degree; i++)
			{
				if (!arithmetic.Equals(a[i], b[i]))
					return false;
			}
			return true;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			int hc = Degree.GetHashCode();
			for (int i = 0; i <= Degree; i++)
			{
				hc <<= 7;
				hc += arithmetic.GetHashCode(this[i]);
			}
			return hc;
		}

		#endregion

		/// <summary>
		/// Calculates the greatest common denominator for two <see cref="Polynomial{T}"/>s.
		/// </summary>
		/// <param name="a">First polynomial.</param>
		/// <param name="b">Second polynomial.</param>
		/// <returns>The greatest common denominator.</returns>
		public static Polynomial<T> Gcd(Polynomial<T> a, Polynomial<T> b)
		{
			Polynomial<T> r;
			if (a.Degree < b.Degree)
			{
				r = a; a = b; b = r;
			}
			while (true)
			{
				DivRem(a, b, out r);
				if (r.IsZero())
					return b;
				r.MakeLeadingCoefficientOne();
				a = b; b = r;
			}
		}

		private void MakeLeadingCoefficientOne()
		{
			var leading = coeffs[Degree];
			if (arithmetic.IsOne(leading))
				return;
			for (int i = 0; i <= Degree; i++)
			{
				coeffs[i] = arithmetic.Divide(coeffs[i], leading);
			}
		}

	}
}
