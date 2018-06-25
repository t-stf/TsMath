using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace TsMath
{

	/// <summary>
	/// A fraction containing <see cref="BigInteger"/> values. Internally all fractions are irreducible.
	/// </summary>
	[DebuggerDisplay("{ToString()}")]
	public struct Fraction : IEquatable<Fraction>, IComparable<Fraction>
	{

		/// <summary>
		/// The <see cref="Fraction"/> representing 0.
		/// </summary>
		public static readonly Fraction Zero = new Fraction(BigInteger.Zero, BigInteger.One, true);

		/// <summary>
		/// The <see cref="Fraction"/> representing 1.
		/// </summary>
		public static readonly Fraction One = new Fraction(BigInteger.One, BigInteger.One, true);

		/// <summary>
		/// The <see cref="Fraction"/> representing -1.
		/// </summary>
		public static readonly Fraction MinusOne = new Fraction(BigInteger.MinusOne, BigInteger.One, true);

		private BigInteger num, den;

		/// <summary>
		/// Get the numerator of this fraction.
		/// </summary>
		/// <value>
		/// The numerator.
		/// </value>
		public BigInteger Numerator => this.num;

		/// <summary>
		/// Gets the denominator of this fraction.
		/// </summary>
		/// <value>
		/// The denominator.
		/// </value>
		public BigInteger Denominator => this.den;

		/// <summary>
		/// Constructs a <see cref="Fraction"/>.
		/// </summary>
		/// <remarks>The constructed fraction is irreducible.</remarks>
		/// <param name="numerator">The numerator.</param>
		/// <param name="denominator">The denominator.</param>
		public Fraction(BigInteger numerator, BigInteger denominator)
		{
			this.num = numerator;
			this.den = denominator;
			Init();
		}

		/// <summary>
		/// Constructs a <see cref="Fraction"/> from an integer.
		/// </summary>
		/// <param name="numerator">The numerator.</param>
		public Fraction(BigInteger numerator) : this(numerator, BigInteger.One, true)
		{
		}

		private Fraction(BigInteger a, BigInteger b, bool irreducible)
		{
			this.num = a;
			this.den = b;
			if (!irreducible)
				Init();
		}

		private void Init()
		{
			if (den.IsZero())
				throw new DivideByZeroException("Denominator is zero");
			if (num.IsZero())
			{
				den = BigInteger.One;
				return;
			}
			if (den.IsNegative())
			{
				num = -num; den = -den;
			}
			var gcd = IntegerMath.Gcd(num, den).Abs();
			if (gcd != BigInteger.One)
			{
				num /= gcd; den /= gcd;
			}
		}

		/// <summary>
		/// Checks if the <see cref="Fraction"/> is zero.
		/// </summary>
		/// <returns><b>true</b> if the fraction is zero; <b>false</b> otherwise.</returns>
		public bool IsZero() => num.IsZero() && !den.IsZero();

		/// <summary>
		/// Checks if the <see cref="Fraction"/> is negative (less than zero).
		/// </summary>
		/// <returns><b>true</b> if the fraction is negative; <b>false</b> otherwise.</returns>
		public bool IsNegative() => num.IsNegative();

		/// <summary>
		/// Converts a <see cref="Fraction"/> from a <see cref="BigInteger"/>.
		/// </summary>
		/// <param name="num">The number to convert.</param>
		public static implicit operator Fraction(BigInteger num) => new Fraction(num);

		/// <summary>
		/// Converts a <see cref="Fraction"/> from a <see cref="long"/> value.
		/// </summary>
		/// <param name="num">The number to convert.</param>
		public static implicit operator Fraction(long num) => new Fraction(num);

		/// <summary>
		/// Tries to convert a string to a <see cref="Fraction"/>..
		/// </summary>
		/// <param name="s">The number string. Numerator and denominator should be separated by a /</param>
		/// <param name="result">The result, or <see cref="Fraction.Zero"/> if the conversion fails.</param>
		/// <returns><b>true</b>if the string contained a valid number; <b>false</b> otherwise.</returns>
		public static bool TryParse(string s, out Fraction result)
		{
			result = new Fraction();
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
			if (!BigInteger.TryParse(numStr, out BigInteger num))
				return false;
			BigInteger den = BigInteger.One;
			if (denStr != null)
				if (!BigInteger.TryParse(denStr, out den))
					return false;
			result = new Fraction(num, den);
			return true;
		}

		/// <summary>
		/// Converts the specified string to a <see cref="Fraction"/> number.
		/// </summary>
		/// <param name="s">The string.</param>
		/// <returns>The parsed number.</returns>
		/// <exception cref="System.FormatException">The string does not represent a fraction.</exception>
		public static Fraction Parse(string s)
		{
			if (!TryParse(s, out Fraction fraction))
				throw new FormatException("Invalid number format");
			return fraction;
		}

		/// <summary>
		/// Converts a <see cref="Fraction"/> from a string.
		/// </summary>
		/// <param name="str">The string to convert.</param>
		public static explicit operator Fraction(string str) => string.IsNullOrEmpty(str) ? Zero : Parse(str);

		/// <summary>
		/// Multiplies two <see cref="Fraction"/>s.
		/// </summary>
		/// <param name="a">First number to multiply.</param>
		/// <param name="b">Second number to multiply.</param>
		/// <returns>The product.</returns>
		public static Fraction operator *(in Fraction a, in Fraction b)
		{
			return new Fraction(a.num * b.num, a.den * b.den);
		}

		/// <summary>
		/// Divides two <see cref="Fraction"/>s.
		/// </summary>
		/// <param name="a">Dividend.</param>
		/// <param name="b">Divisor.</param>
		/// <returns>Quotient.</returns>
		public static Fraction operator /(in Fraction a, in Fraction b)
		{
			if (b.IsZero())
				throw new DivideByZeroException();
			return new Fraction(a.num * b.den, a.den * b.num);
		}

		/// <summary>
		/// Adds two numbers.
		/// </summary>
		/// <param name="a">The first summand.</param>
		/// <param name="b">The second summand.</param>
		/// <returns>The result of the addition.</returns>
		public static Fraction operator +(in Fraction a, in Fraction b)
		{
			var gcd = IntegerMath.Gcd(a.den, b.den);
			BigInteger da = a.den / gcd, db = b.den / gcd;
			BigInteger den = da * b.den;
			return new Fraction(a.num * db + b.num * da, den);
		}

		/// <summary>
		/// Subtracts two numbers.
		/// </summary>
		/// <param name="a">The minuend.</param>
		/// <param name="b">The subtrahend.</param>
		/// <returns>
		/// The difference of the two numbers.
		/// </returns>
		public static Fraction operator -(in Fraction a, in Fraction b)
		{
			var gcd = IntegerMath.Gcd(a.den, b.den);
			BigInteger da = a.den / gcd, db = b.den / gcd;
			BigInteger den = da * b.den;
			return new Fraction(a.num * db - b.num * da, den);
		}

		/// <summary>
		/// Implements the negation (unary minus) operator.
		/// </summary>
		/// <param name="a">The number to negate.</param>
		/// <returns>
		/// The negated value.
		/// </returns>
		public static Fraction operator -(Fraction a)
		{
			return new Fraction(-a.num, a.den, true);
		}

		/// <summary>
		/// Returns a <see cref="string" /> that represents this instance.
		/// </summary>
		/// <remarks>The numerator and denominator output shows only a fixed number of decimal digits as provided in <see cref="TsMathGlobals.MaxBigIntegerToStringDigitCount"/>.</remarks>
		/// <returns>
		/// A <see cref="string" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return ToString(TsMathGlobals.MaxBigIntegerToStringDigitCount);
		}

		/// <summary>
		/// Returns a <see cref="string" /> that represents this instance.
		/// </summary>
		/// <param name="nDigits">The maximum number of decimal digits for numerator and denominator.</param>
		/// <returns>
		/// A <see cref="string" /> that represents this instance.
		/// </returns>	
		public string ToString(int nDigits)
		{
			if (den == BigInteger.One)
				return num.ToString(nDigits);
			return num.ToString(nDigits) + "/" + den.ToString(nDigits);
		}

		/// <summary>
		/// Converts this fraction to a decimal string
		/// </summary>
		/// <param name="nMaxDecDigits">The maximum number of decimal digits to use.</param>
		/// <param name="ci">The culture to use, if <b>null</b> <see cref="CultureInfo.CurrentCulture"/> is used.</param>
		/// <returns>The decimal string representing this fraction.</returns>
		public string ToDecimalString(int nMaxDecDigits, CultureInfo ci = null)
		{
			ci = ci ?? CultureInfo.CurrentCulture;
			var numf = ci.NumberFormat;
			StringBuilder sb = new StringBuilder();
			var rem = this;
			if (IsNegative())
			{
				sb.Append(numf.NegativeSign);
				rem = -rem;
			}
			BigInteger hi = rem.num / rem.den;
			sb.Append(hi.ToString());
			rem -= hi;
			for (int i = 0; i < nMaxDecDigits; i++)
			{
				if (rem.IsZero())
					break;
				if (i == 0)
					sb.Append(numf.NumberDecimalSeparator);
				rem *= 10;
				hi = rem.num / rem.den;
				sb.Append(hi.ToString());
				rem -= hi;
			}
			if (!rem.IsZero())
				sb.Append("...");
			return sb.ToString();
		}

		private static bool Equals(Fraction a, Fraction b)
		{
			if (a.num != b.num)
				return false;
			if (a.den != b.den)
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
		public bool Equals(Fraction other) => Equals(this, other);

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <b>true</b> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <b>false</b>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (!(obj is Fraction f))
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

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>
		/// <b>true</b> if both numbers are equal; <b>false</b> otherwise.
		/// </returns>
		public static bool operator ==(Fraction a, Fraction b) => Equals(a, b);

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>
		/// <b>true</b> if both numbers are not equal; <b>false</b> otherwise.
		/// </returns>
		public static bool operator !=(Fraction a, Fraction b) => !Equals(a, b);

		private static int Compare(Fraction a, Fraction b)
		{
			Fraction d = a - b;
			if (d.IsZero())
				return 0;
			return d.IsNegative() ? -1 : 1;
		}

		/// <summary>
		/// Compares this instance to an other one.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns>-1, 0 or 1; depending on if this instance is less than, equal or greater then <paramref name="other"/>.</returns>
		public int CompareTo(Fraction other) => Compare(this, other);

		/// <summary>
		/// Implements the operator &lt;=.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>
		/// <b>true</b> if both <paramref name="a"/> is less than or equal to <paramref name="b"/>; <b>false</b> otherwise.
		/// </returns>
		public static bool operator <=(Fraction a, Fraction b) => Compare(a, b) <= 0;

		/// <summary>
		/// Implements the operator &gt;=.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>
		/// <b>true</b> if both <paramref name="a"/> is greater than or equal to <paramref name="b"/>; <b>false</b> otherwise.
		/// </returns>
		public static bool operator >=(Fraction a, Fraction b) => Compare(a, b) >= 0;

		/// <summary>
		/// Implements the operator &lt;.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>
		/// <b>true</b> if both <paramref name="a"/> is less than <paramref name="b"/>; <b>false</b> otherwise.
		/// </returns>
		public static bool operator <(Fraction a, Fraction b) => Compare(a, b) < 0;

		/// <summary>
		/// Implements the operator &gt;.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>
		/// <b>true</b> if both <paramref name="a"/> is greater than <paramref name="b"/>; <b>false</b> otherwise.
		/// </returns>
		public static bool operator >(Fraction a, Fraction b) => Compare(a, b) > 0;


	}
}
