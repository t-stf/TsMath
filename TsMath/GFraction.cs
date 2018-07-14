using System;
using TsMath.Structures;

namespace TsMath
{
	/// <summary>
	/// A fraction. Internally all fractions are irreducible.
	/// </summary>
	/// <typeparamref name="T">Allowed types <see cref="long"/>, <see cref="BigInteger"/> and all user types with a registered arithmetic (<see cref="Arithmetic{T}.GetArithmetic"/>).</typeparamref>
	public struct GFraction<T> : IEquatable<GFraction<T>>, IComparable<GFraction<T>> where T : IEquatable<T>
	{

		static readonly Arithmetic<T> arithmetic = Arithmetic<T>.GetArithmetic();

		/// <summary>
		/// The <see cref="GFraction{T}"/> representing 0.
		/// </summary>
		public static readonly GFraction<T> Zero = new GFraction<T>(arithmetic.Zero(default), arithmetic.One(default), true);

		/// <summary>
		/// The <see cref="GFraction{T}"/> representing 1.
		/// </summary>
		public static readonly GFraction<T> One = new GFraction<T>(arithmetic.One(default), arithmetic.One(default), true);

		/// <summary>
		/// The <see cref="Fraction"/> representing -1.
		/// </summary>
		public static readonly GFraction<T> MinusOne = new GFraction<T>(arithmetic.MinusOne(default), arithmetic.One(default), true);

		private T num, den;

		/// <summary>
		/// Get the numerator of this fraction.
		/// </summary>
		/// <value>
		/// The numerator.
		/// </value>
		public T Numerator => this.num;

		/// <summary>
		/// Gets the denominator of this fraction.
		/// </summary>
		/// <value>
		/// The denominator.
		/// </value>
		public T Denominator => this.den;

		/// <summary>
		/// Constructs a <see cref="GFraction{T}"/>.
		/// </summary>
		/// <remarks>The constructed fraction is irreducible.</remarks>
		/// <param name="numerator">The numerator.</param>
		/// <param name="denominator">The denominator.</param>
		public GFraction(T numerator, T denominator)
		{
			this.num = numerator;
			this.den = denominator;
			Init();
		}

		/// <summary>
		/// Constructs a <see cref="Fraction"/> from an integer.
		/// </summary>
		/// <param name="numerator">The numerator.</param>
		public GFraction(T numerator) : this(numerator, arithmetic.One(numerator), true)
		{
		}

		internal GFraction(T a, T b, bool irreducible)
		{
			this.num = a;
			this.den = b;
			if (!irreducible)
				Init();
		}

		private void Init()
		{
			if (arithmetic.IsZero(den))
				throw new DivideByZeroException("Denominator is zero");
			if (arithmetic.IsZero(num))
			{
				den = arithmetic.One(num);
				return;
			}
			if (arithmetic.IsNegative(den))
			{
				num = arithmetic.Negate(num);
				den = arithmetic.Negate(den);
			}
			var gcd = arithmetic.Gcd(num, den);
			gcd = arithmetic.Abs(gcd);
			if (!arithmetic.Equals(gcd, arithmetic.One(num)))
			{
				K<T> g = gcd;
				num /= g;
				den /= g;
			}
		}

		/// <summary>
		/// Checks if the <see cref="GFraction{T}"/> is zero.
		/// </summary>
		/// <returns><b>true</b> if the fraction is zero; <b>false</b> otherwise.</returns>
		public bool IsZero() => arithmetic.IsZero(num) && !arithmetic.IsZero(den);

		/// <summary>
		/// Checks if the <see cref="GFraction{T}"/> is negative (less than zero).
		/// </summary>
		/// <returns><b>true</b> if the fraction is negative; <b>false</b> otherwise.</returns>
		public bool IsNegative() => arithmetic.IsNegative(num);

		/// <summary>
		/// Converts a <see cref="GFraction{T}"/> from a <typeparamref name="T"/>.
		/// </summary>
		/// <param name="num">The number to convert.</param>
		public static implicit operator GFraction<T>(T num) => new GFraction<T>(num);

		/// <summary>
		/// Tries to convert a string to a <see cref="GFraction{T}"/>..
		/// </summary>
		/// <param name="s">The number string. Numerator and denominator should be separated by a /</param>
		/// <param name="result">The result, or <see cref="GFraction{T}.Zero"/> if the conversion fails.</param>
		/// <returns><b>true</b>if the string contained a valid number; <b>false</b> otherwise.</returns>
		public static bool TryParse(string s, out GFraction<T> result)
		{
			result = Zero;
			int idx = s.IndexOf('/');
			string numStr = null, denStr = null;
			if (idx < 0)
			{
				numStr = s;
			}
			else
			{
				numStr = s.Substring(0, idx);
				denStr = s.Substring(idx + 1);
			}
			if (!arithmetic.TryParse(numStr, out T num))
				return false;
			var den = arithmetic.One(num);
			if (denStr != null)
				if (!arithmetic.TryParse(denStr, out den))
					return false;
			result = new GFraction<T>(num, den);
			return true;
		}

		/// <summary>
		/// Converts the specified string to a <see cref="GFraction{T}"/> number.
		/// </summary>
		/// <param name="s">The string.</param>
		/// <returns>The parsed number.</returns>
		/// <exception cref="System.FormatException">The string does not represent a fraction.</exception>
		public static GFraction<T> Parse(string s)
		{
			if (!TryParse(s, out GFraction<T> fraction))
				throw new FormatException("Invalid number format");
			return fraction;
		}

		/// <summary>
		/// Converts a <see cref="GFraction{T}"/> from a string.
		/// </summary>
		/// <param name="str">The string to convert.</param>
		public static explicit operator GFraction<T>(string str) => string.IsNullOrEmpty(str) ? Zero : Parse(str);

		/// <summary>
		/// Multiplies two <see cref="GFraction{T}"/>s.
		/// </summary>
		/// <param name="a">First number to multiply.</param>
		/// <param name="b">Second number to multiply.</param>
		/// <returns>The product.</returns>
		public static GFraction<T> operator *(in GFraction<T> a, in GFraction<T> b)
		{
			return new GFraction<T>(arithmetic.Multiply(a.num, b.num), arithmetic.Multiply(a.den, b.den));
		}

		/// <summary>
		/// Divides two <see cref="GFraction{T}"/>s.
		/// </summary>
		/// <param name="a">Dividend.</param>
		/// <param name="b">Divisor.</param>
		/// <returns>Quotient.</returns>
		public static GFraction<T> operator /(in GFraction<T> a, in GFraction<T> b)
		{
			if (b.IsZero())
				throw new DivideByZeroException();
			return new GFraction<T>(arithmetic.Multiply(a.num, b.den), arithmetic.Multiply(a.den, b.num));
		}

		/// <summary>
		/// Adds two numbers.
		/// </summary>
		/// <param name="a">The first summand.</param>
		/// <param name="b">The second summand.</param>
		/// <returns>The result of the addition.</returns>
		public static GFraction<T> operator +(in GFraction<T> a, in GFraction<T> b)
		{
			K<T> gcd = arithmetic.Gcd(a.den, b.den);
			K<T> da = a.den / gcd;
			K<T> db = b.den / gcd;
			K<T> den = da * b.den;

			return new GFraction<T>((K<T>)(a.num * db) + b.num * da, den);
		}

		/// <summary>
		/// Subtracts two numbers.
		/// </summary>
		/// <param name="a">The minuend.</param>
		/// <param name="b">The subtrahend.</param>
		/// <returns>
		/// The difference of the two numbers.
		/// </returns>
		public static GFraction<T> operator -(in GFraction<T> a, in GFraction<T> b)
		{
			K<T> gcd = arithmetic.Gcd(a.den, b.den);
			K<T> da = a.den / gcd;
			K<T> db = b.den / gcd;
			K<T> den = da * b.den;
			return new GFraction<T>(a.num * db - b.num * da, den);
		}

		/// <summary>
		/// Implements the negation (unary minus) operator.
		/// </summary>
		/// <param name="a">The number to negate.</param>
		/// <returns>
		/// The negated value.
		/// </returns>
		public static GFraction<T> operator -(GFraction<T> a)
		{
			return new GFraction<T>(arithmetic.Negate(a.num), a.den, true);
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
		/// <returns>
		/// A <see cref="string" /> that represents this instance.
		/// </returns>	
		public string ToString(int maxLen)
		{
			var numString= arithmetic.ToString(num, maxLen);
			if (arithmetic.Equals(den, arithmetic.One(den)))
				return numString;
			return numString + "/" + arithmetic.ToString(den, Math.Max(10, maxLen - numString.Length));
		}

		private static bool Equals(GFraction<T> a, GFraction<T> b)
		{
			if (!a.num.Equals(b.num))
				return false;
			if (!a.den.Equals(b.den))
				return false;
			return true;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// <b>true</b> if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, <b>false</b>.
		/// </returns>
		public bool Equals(GFraction<T> other) => Equals(this, other);

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <b>true</b> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <b>false</b>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (!(obj is GFraction<T> f))
				return false;
			return Equals(this, f);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			int hc = num.GetHashCode();
			hc <<= 7;
			return hc + den.GetHashCode();
		}

		private static int Compare(GFraction<T> a, GFraction<T> b)
		{
			var d = a - b;
			if (d.IsZero())
				return 0;
			return d.IsNegative() ? -1 : 1;
		}

		/// <summary>
		/// Compares this instance to an other one.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns>-1, 0 or 1; depending on if this instance is less than, equal or greater then <paramref name="other"/>.</returns>
		public int CompareTo(GFraction<T> other) => Compare(this, other);

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>
		/// <b>true</b> if both numbers are equal; <b>false</b> otherwise.
		/// </returns>
		public static bool operator ==(GFraction<T> a, GFraction<T> b) => Equals(a, b);

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>
		/// <b>true</b> if both numbers are not equal; <b>false</b> otherwise.
		/// </returns>
		public static bool operator !=(GFraction<T> a, GFraction<T> b) => !Equals(a, b);

		/// <summary>
		/// Implements the operator &lt;=.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>
		/// <b>true</b> if both <paramref name="a"/> is less than or equal to <paramref name="b"/>; <b>false</b> otherwise.
		/// </returns>
		public static bool operator <=(GFraction<T> a, GFraction<T> b) => Compare(a, b) <= 0;

		/// <summary>
		/// Implements the operator &gt;=.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>
		/// <b>true</b> if both <paramref name="a"/> is greater than or equal to <paramref name="b"/>; <b>false</b> otherwise.
		/// </returns>
		public static bool operator >=(GFraction<T> a, GFraction<T> b) => Compare(a, b) >= 0;

		/// <summary>
		/// Implements the operator &lt;.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>
		/// <b>true</b> if both <paramref name="a"/> is less than <paramref name="b"/>; <b>false</b> otherwise.
		/// </returns>
		public static bool operator <(GFraction<T> a, GFraction<T> b) => Compare(a, b) < 0;

		/// <summary>
		/// Implements the operator &gt;.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>
		/// <b>true</b> if both <paramref name="a"/> is greater than <paramref name="b"/>; <b>false</b> otherwise.
		/// </returns>
		public static bool operator >(GFraction<T> a, GFraction<T> b) => Compare(a, b) > 0;

	}

}
