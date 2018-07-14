using System;
using System.Collections.Generic;
using System.Diagnostics;
using TsMath.Helpers;
using static TsMath.TsMathGlobals;

namespace TsMath
{

	/// <summary>
	/// Arbitrary large integer (immutable). This struct is optimized for a minimum of allocations.
	/// </summary>
	/// <remarks>
	/// If the integer is small enough it will stored in a field, for larger BigInts arrays will be
	/// allocated. +,-,* never allocates additional arrays!
	/// <para>
	/// It will use fast multiplication algorithms, if the numbers are large enough.
	/// </para>
	/// </remarks>
	[DebuggerDisplay("{ToString()}")]
	public struct BigInteger : IEquatable<BigInteger>, IComparable<BigInteger>
	{

		internal class BaseOps
		{

			public const int BitsPerDigit = sizeof(uint) * 8;

			public static uint MinusOne;

			public const uint HalfValue = (uint.MaxValue >> 1) + 1;

			public const ulong MaxDigitValue = uint.MaxValue;

			public static double Log2DivLogE;

			static BaseOps()
			{
				unchecked
				{
					MinusOne = (uint)(-1);
				}
				Log2DivLogE = Math.Log(2) / Math.Log(Math.E);
			}

			public static int AddTo(uint[] res, BigInteger a, int len)
			{
				ulong carry = 0;
				for (int i = 0; i < len; i++)
				{
					uint da = a[i];
					carry = (ulong)res[i] + da + carry;
					res[i] = (uint)carry;
					carry >>= BitsPerDigit;
				}
				if (carry != 0)
				{
					res[len++] = (uint)carry;
				}
				return len;
			}

			public static void SubFrom(uint[] res, BigInteger a, int len)
			{
				ulong übertrag = 0;
				ulong Zehner = (ulong)1 << BitsPerDigit;
				for (int i = 0; i < len; i++)
				{
					//übertrag <<= BitsPerDigit;
					ulong aa = (ulong)a[i] + übertrag;
					ulong rr = res[i];
					if (rr >= aa)
					{
						res[i] = (uint)(rr - aa);
						übertrag = 0;
					}
					else
					{
						res[i] = (uint)(rr + Zehner - aa);
						übertrag = 1;
					}

				}
			}

			public static int Multiply(uint[] res, uint val, int len)
			{
				ulong übertrag = 0;
				for (int i = 0; i < len; i++)
				{
					übertrag = (ulong)res[i] * val + übertrag;
					res[i] = (uint)übertrag;
					übertrag >>= BitsPerDigit;
				}
				if (übertrag != 0)
				{
					res[len++] = (uint)übertrag;
				}
				return len;
			}

			/// <summary>
			/// Calculates res=res+a*fak
			/// </summary>
			/// <param name="res"></param>
			/// <param name="a"></param>
			/// <param name="fak"></param>
			/// <param name="shift">Position in res where the calculation starts</param>
			/// <returns>number of used digits</returns>
			public static int MulAdd(uint[] res, BigInteger a, uint fak, int shift)
			{
				ulong carryOver = 0;
				int digitCount = a.DigitCount;
				uint[] aDigits = a.digits;
				if (aDigits == null)
					aDigits = new uint[1] { a.lenUsedOrDigit0 };
				int iShift = shift;
				for (int i = 0; i < digitCount; i++, iShift++)
				{
					carryOver = (ulong)aDigits[i] * fak + carryOver;
					carryOver += res[iShift];
					res[iShift] = (uint)carryOver;
					carryOver >>= BitsPerDigit;
				}
				int n = digitCount + shift;
				while (carryOver != 0)
				{
					carryOver += res[n];
					res[n] = (uint)carryOver;
					carryOver >>= BitsPerDigit;
					n++;
				}
				return n;
			}

			public static int MulSub(uint[] res, BigInteger a, uint fak)
			{
				ulong carryOver = 0;
				int digitCount = a.DigitCount;
				uint[] aDigits = a.digits;
				if (aDigits == null)
					aDigits = new uint[1] { a.lenUsedOrDigit0 };

				for (int i = 0; i < digitCount; i++)
				{
					carryOver = (ulong)aDigits[i] * fak + carryOver;
					uint rr = res[i];
					uint ri = rr - (uint)carryOver;
					res[i] = ri;
					carryOver >>= BitsPerDigit;
					if (ri > rr)
						carryOver++;
				}
				int n = digitCount;
				while (carryOver != 0)
				{
					uint rn = res[n];
					uint dds = (uint)(rn - (uint)carryOver);
					res[n] = dds;

					carryOver >>= BitsPerDigit;
					if (dds > rn)
						carryOver++;
					n++;
				}
				return n;
			}

			public static uint DigitIndex(char c)
			{
				if (c >= '0' && c <= '9')
					return (uint)(c - '0');
				if (c >= 'a' && c <= 'z')
					return (uint)(c - 'a' + 10);
				if (c >= 'A' && c <= 'Z')
					return (uint)(c - 'A' + 10);
				return MinusOne;
			}


			public static int DigitShiftUp(uint[] a, int len, uint newDigit)
			{
				while (len > 0 && a[len - 1] == 0)
					len--;
				if (len >= a.Length)
					len--;
				for (int i = len; i > 0; i--)
				{
					a[i] = a[i - 1];
				}
				a[0] = newDigit;
				return len + 1;
			}

			public static double Log2(double d)
			{
				return Math.Log(d) / Math.Log(2);
			}

			public static int CompareDigits(uint[] x, int startX, uint[] y, int startY, int len)
			{
				for (int i = len - 1; i >= 0; i--)
				{
					int isx = i + startX, isy = i + startY;
					uint dx = isx < 0 || x == null ? 0 : x[isx];
					uint dy = isy < 0 || y == null ? 0 : y[isy];
					if (dx < dy)
						return -1;
					if (dx > dy)
						return 1;
				}
				return 0;
			}

			public static int BitLengthOfDigit(uint x)
			{
				int num = 0;
				while (x != 0)
				{
					x >>= 1;
					++num;
				}
				return num;
			}

			/// <summary>
			/// Naive division algorithm.
			/// </summary>
			/// <param name="a">Dividend.</param>
			/// <param name="b">Divisor.</param>
			/// <param name="r">Remainder.</param>
			/// <returns>Result as digit array.</returns>
			public static BigInteger NaiveDivision(BigInteger a, BigInteger b, out BigInteger r)
			{
				//Lange Division für |a| > |b|
				int aLen = a.DigitCount, bLen = b.DigitCount;
				uint[] dar = new uint[aLen - bLen + 1];
				var rar = new uint[bLen + 1];
				for (int i = 0; i < b.DigitCount; i++)
					rar[i] = a[aLen - bLen + i];

				int dIdx = 0;
				int aIdx = aLen - bLen - 1;
				int rLen = b.DigitCount;
				ulong bHi = (ulong)(b[b.DigitCount - 1]);
				ulong bHiPlus1 = bHi; bHiPlus1++;
				do
				{
					int cmpRes = CompareUnsigned(new BigInteger(rar, false), b);
					if (cmpRes < 0)
					{
						dIdx++;
						if (aIdx < 0)
							break;

						rLen = BaseOps.DigitShiftUp(rar, rar.Length - 1, a[aIdx--]);
						//dar[dIdx++] = 0;
						continue;
					}
					if (cmpRes == 0)
					{
						dar[dIdx] += 1;
						Array.Clear(rar, 0, rar.Length); rLen = 1;
						continue;
					}
					//r>b
					ulong rHi = rar[rLen - 1];
					if (rHi < bHiPlus1)
					{
						if (rHi != bHi)
						{
							rHi <<= BaseOps.BitsPerDigit;
							rHi |= rar[rLen - 2];
						}
					}
					uint newDigit = (uint)(rHi / bHiPlus1);
					if (newDigit == 0)
						newDigit++;
					dar[dIdx] += newDigit;
					//Vielfaches abziehen
					rLen = BaseOps.MulSub(rar, b, newDigit);

				} while (true);
				for (int i = 0; i < dIdx / 2; i++)
				{
					uint td = dar[i];
					dar[i] = dar[dIdx - i - 1];
					dar[dIdx - i - 1] = td;
				}
				r = new BigInteger(rar, false);
				return new BigInteger(dar, false);
			}

			internal static int GetNormalization(BigInteger a)
			{
				var topDigit = a[a.DigitCount - 1];
				int result = 0;
				while (topDigit < HalfValue)
				{
					result++; topDigit <<= 1;
				}
				return result;
			}

			public static BigInteger NaiveDivision2(BigInteger a, BigInteger b, out BigInteger r)
			{
				//normalize numbers
				int normalizeShift = GetNormalization(b);
				a <<= normalizeShift;
				b <<= normalizeShift;
				int n = b.DigitCount;
				int m = a.DigitCount - b.DigitCount;
				uint[] q = new uint[m + 1];
				if (a[a.DigitCount - 1] > b[n - 1])
				{
					q[m] = 1;
					a -= b << (m * BitsPerDigit);
				}
				for (int j = m - 1; j >= 0; j--)
				{
					ulong qs = (ulong)a[n + j] << BitsPerDigit | a[n + j - 1];
					qs /= b[n - 1];
					uint qj = uint.MaxValue;
					if (qs < qj)
						qj = (uint)qs;
					var bb = b << (j * BitsPerDigit);
					a -= qj * bb;
					while (a.Sign < 0)
					{
						qj--;
						a += bb;
					}
					q[j] = qj;
				}
				r = a >> normalizeShift;
				return new BigInteger(q, false);
			}

		}

		/// <summary>
		/// Used length of digits. or the digit for single digit numbers
		/// </summary>
		uint lenUsedOrDigit0;

		private bool isNegative;

		/// <summary>
		/// Digits in lo to hi order; null if number has only one digit
		/// </summary>
		private uint[] digits;

		/// <summary>
		/// Gets a value indicating whether this instance is negative.
		/// </summary>
		/// <returns>
		/// <b>true</b> if this instance is negative; otherwise, <b>false</b>.
		/// </returns>
		public bool IsNegative() => isNegative;

		/// <summary>
		/// The <see cref="BigInteger"/> representing zero (0).
		/// </summary>
		public static readonly BigInteger Zero = new BigInteger(0);

		/// <summary>
		/// The <see cref="BigInteger"/> representing one (1).
		/// </summary>
		public static readonly BigInteger One = new BigInteger(1);

		/// <summary>
		/// The <see cref="BigInteger"/> representing minus one (-1).
		/// </summary>
		public static readonly BigInteger MinusOne = new BigInteger(-1);

		/// <summary>
		/// The <see cref="BigInteger"/> representing one (2).
		/// </summary>
		public static readonly BigInteger Two = new BigInteger(2);

		/// <summary>
		/// The <see cref="BigInteger"/> representing ten (10).
		/// </summary>
		public static readonly BigInteger Ten = new BigInteger(10);

		static BigInteger()
		{
		}

		/// <summary>
		/// Gets the sign of this number.
		/// </summary>
		/// <value>
		/// -1 if the number is negative; 0 if the number is zero and +1 if the number is positive.
		/// </value>
		public int Sign
		{
			get
			{
				if (IsNegative())
					return -1;
				if (this == 0)
					return 0;
				return 1;
			}
		}

		void Trim()
		{
			bool fIsZero = true;
			for (int msdIdx = digits.Length - 1; msdIdx >= 0; msdIdx--)
				if (digits[msdIdx] != 0)
				{
					lenUsedOrDigit0 = (uint)(msdIdx + 1);
					fIsZero = false;
					break;
				}

			if (lenUsedOrDigit0 == 0)
				lenUsedOrDigit0 = 1;
			if (lenUsedOrDigit0 == 1)
			{
				lenUsedOrDigit0 = digits[0];
				digits = null;
			}
			if (fIsZero)
				isNegative = false;
		}

		internal BigInteger(uint[] digs, bool fNeg)
		{
			this.digits = digs; this.isNegative = fNeg; this.lenUsedOrDigit0 = 0;
			Trim();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BigInteger"/> struct.
		/// </summary>
		/// <param name="d">The single digit number.</param>
		/// <param name="fNeg">Set to <b>true</b> to indicate a negative number.</param>
		public BigInteger(uint d, bool fNeg)
		{
			digits = null;
			isNegative = fNeg;
			if (d == 0)
				isNegative = false;
			lenUsedOrDigit0 = d;
		}

		private BigInteger(ulong dd, bool fNeg)
		{
			isNegative = fNeg && dd != 0;
			if (dd <= BaseOps.MaxDigitValue)
			{
				digits = null;
				lenUsedOrDigit0 = (uint)dd;
				return;
			}
			digits = new uint[2];
			digits[0] = (uint)dd;
			digits[1] = (uint)(dd >> BaseOps.BitsPerDigit);
			lenUsedOrDigit0 = 2;
		}

		internal BigInteger(BigInteger a)
		{
			this.digits = a.digits;
			this.isNegative = a.IsNegative();
			this.lenUsedOrDigit0 = a.lenUsedOrDigit0;
		}

		/// <summary>
		/// Converts the specified string to a <see cref="BigInteger"/> number.
		/// </summary>
		/// <param name="s">The string.</param>
		/// <param name="base">The base.</param>
		/// <returns>The parsed number.</returns>
		/// <exception cref="System.FormatException">The string does not represent a number.</exception>
		public static BigInteger Parse(string s, int @base = 10)
		{
			if (!TryParse(s, @base, out BigInteger a))
				throw new FormatException("Invalid BigInteger format");
			return a;
		}

		/// <summary>
		/// Tries to convert a string to a <see cref="BigInteger"/>.
		/// </summary>
		/// <param name="s">The number string to convert.</param>
		/// <param name="a">The resulting number.</param>
		/// <returns><b>true</b>if the string contained a valid number; <b>false</b> otherwise.</returns>
		public static bool TryParse(string s, out BigInteger a) => TryParse(s, 10, out a);

		/// <summary>
		/// Tries to convert a string to a <see cref="BigInteger"/> using a number base.
		/// </summary>
		/// <param name="s">The number string to convert.</param>
		/// <param name="base">The number base</param>
		/// <param name="result">The result, or <see cref="BigInteger.Zero"/> if the conversion fails.</param>
		/// <returns><b>true</b>if the string contained a valid number; <b>false</b> otherwise.</returns>
		public static bool TryParse(string s, int @base, out BigInteger result)
		{
			result = new BigInteger();
			if (!result.TryParseIn(s, @base, false))
			{
				result = Zero;
				return false;
			}
			return true;
		}

		private bool TryParseIn(string s, int @base, bool ignoreSign)
		{
			if (string.IsNullOrEmpty(s))
				return false;
			bool locNeg = false;
			lenUsedOrDigit0 = 0;
			int start = 0;
			if (!ignoreSign)
			{
				if (s[start] == '-')
				{
					locNeg = true;
					start = 1;
				}
				else
					if (s[start] == '+')
					start = 1;
			}
			if (s.Length == start)
				return false;
			int lr = (int)(BaseOps.Log2(@base) / BaseOps.BitsPerDigit * (s.Length - start)) + 1;
			digits = new uint[lr];
			int dlen = 1;
			uint[] dummy = new uint[1];

			for (int i = start; i < s.Length; i++)
			{
				dlen = BaseOps.Multiply(digits, (uint)@base, dlen);
				uint di = BaseOps.DigitIndex(s[i]);
				if (di == BaseOps.MinusOne || di >= @base)
					return false;
				BigInteger biDigit = new BigInteger(di, false);
				dlen = BaseOps.AddTo(digits, biDigit, dlen);
			}
			this.isNegative = locNeg;
			Trim();
			return true;
		}

		/// <summary>
		/// Gets the digit count (e.g. number of positions of the underlaying <see cref="uint"/> struct).
		/// </summary>
		/// <value>
		/// The digit count.
		/// </value>
		internal int DigitCount => digits == null ? 1 : (int)lenUsedOrDigit0;

		internal uint this[int index]
		{
			get
			{
				if (digits == null)
				{
					return index == 0 ? lenUsedOrDigit0 : (uint)0;
				}
				if (index >= digits.Length)
					return (uint)0;
				return digits[index];
			}
		}

		private static int CompareUnsigned(in BigInteger a, in BigInteger b)
		{
			// quick check if both numbers are small
			if (a.digits == null && b.digits == null)
			{
				if (a.lenUsedOrDigit0 < b.lenUsedOrDigit0)
					return -1;
				if (a.lenUsedOrDigit0 > b.lenUsedOrDigit0)
					return 1;
				return 0;
			}

			int dca = a.DigitCount;
			int dcb = b.DigitCount;
			int d = dca - dcb;
			if (d != 0)
				return Math.Sign(d);

			for (int i = dca - 1; i >= 0; i--)
			{
				uint ai = a[i], bi = b[i];
				if (ai < bi)
					return -1;
				if (ai > bi)
					return 1;
			}
			return 0;
		}

		void CopyDigitsTo(uint[] da)
		{
			if (digits == null)
				da[0] = lenUsedOrDigit0;
			else
				Array.Copy(digits, da, (int)lenUsedOrDigit0);
		}

		private static BigInteger Add(in BigInteger a, in BigInteger b)
		{
			if (a.digits == null && b.digits == null)
			{
				ulong lsum = (ulong)a.lenUsedOrDigit0 + b.lenUsedOrDigit0;
				return new BigInteger(lsum, a.isNegative);
			}
			//at least one number is not small 
			int dstLen = Math.Max(a.DigitCount, b.DigitCount) + 1;
			uint[] sumDigs = new uint[dstLen];
			a.CopyDigitsTo(sumDigs);
			BaseOps.AddTo(sumDigs, b, dstLen - 1);
			return new BigInteger(sumDigs, a.isNegative);
		}

		private static BigInteger Sub(BigInteger a, BigInteger b)
		{
			bool resSign = a.isNegative;
			var cmp = CompareUnsigned(a, b);
			if (cmp == 0)
				return Zero;
			if (cmp < 0)
			{
				BigInteger c = a; a = b; b = c;
				resSign = !b.isNegative;
			}
			if (a.digits == null && b.digits == null)
			{
				ulong ldiff = (ulong)a.lenUsedOrDigit0 - b.lenUsedOrDigit0;
				return new BigInteger(ldiff, resSign);
			}
			int dstLen = a.DigitCount;
			uint[] sumDigs = new uint[dstLen];
			a.CopyDigitsTo(sumDigs);
			BaseOps.SubFrom(sumDigs, b, dstLen);
			return new BigInteger(sumDigs, resSign);
		}

		/// <summary>
		/// Adds two numbers.
		/// </summary>
		/// <param name="a">The first summand.</param>
		/// <param name="b">The second summand.</param>
		/// <returns>The result of the addition.</returns>
		public static BigInteger operator +(BigInteger a, BigInteger b)
		{
			return a.isNegative == b.isNegative ? Add(a, b) : Sub(a, -b);
		}

		/// <summary>
		/// Subtracts two numbers.
		/// </summary>
		/// <param name="a">The minuend.</param>
		/// <param name="b">The subtrahend.</param>
		/// <returns>
		/// The difference of the two numbers.
		/// </returns>
		public static BigInteger operator -(BigInteger a, BigInteger b)
		{
			return a.isNegative == b.isNegative ? Sub(a, b) : Add(a, -b);
		}

		/// <summary>
		/// Increments the given number by 1.
		/// </summary>
		/// <param name="a">The number.</param>
		/// <returns>The incremented number.</returns>
		public static BigInteger operator ++(in BigInteger a)
		{
			if (a.digits == null && a.lenUsedOrDigit0 < BaseOps.MaxDigitValue)
				return new BigInteger(a.isNegative ? a.lenUsedOrDigit0 - 1 : a.lenUsedOrDigit0 + 1, a.isNegative);
			return a + One;
		}

		/// <summary>
		/// Decrements the given number by 1.
		/// </summary>
		/// <param name="a">The number.</param>
		/// <returns>The decremented number.</returns>
		public static BigInteger operator --(in BigInteger a)
		{
			if (a.digits == null && a > 0 && a.lenUsedOrDigit0 < BaseOps.MaxDigitValue)
				return new BigInteger(a.isNegative ? a.lenUsedOrDigit0 + 1 : a.lenUsedOrDigit0 - 1, a.isNegative);
			return a - One;
		}

		/// <summary>
		/// Implements the negation (unary minus) operator.
		/// </summary>
		/// <param name="a">The number to negate.</param>
		/// <returns>
		/// The negated value.
		/// </returns>
		public static BigInteger operator -(BigInteger a)
		{
			if (!a.IsZero())
				a.isNegative = !a.isNegative;
			return a;
		}

		/// <summary>
		/// Multiplies two <see cref="BigInteger"/> numbers.
		/// </summary>
		/// <remarks>
		/// Use <see cref="BigIntegerKaratsubaThreshold"/> to control, when instead of the naive algorithm,
		/// the Karatsuba multiplication will be used (if both numbers have more digits).
		/// </remarks>
		/// <param name="a">First number to multiply.</param>
		/// <param name="b">Second number to multiply.</param>
		/// <returns>The product.</returns>
		public static BigInteger operator *(BigInteger a, BigInteger b)
		{
			bool fNeg = a.IsNegative() != b.IsNegative();
			// check for fast
			if (a.digits == null && b.digits == null)
			{
				ulong dd = a.lenUsedOrDigit0;
				dd *= b.lenUsedOrDigit0;
				return new BigInteger(dd, fNeg);
			}
			if (a.DigitCount < BigIntegerKaratsubaThreshold || b.DigitCount < BigIntegerKaratsubaThreshold)
			{
				// naive multiplication -> faster for small numbers
				int nResDigit = a.DigitCount + b.DigitCount + 1;
				uint[] res = new uint[nResDigit];
				for (int i = 0; i < b.DigitCount; i++)
				{
					BaseOps.MulAdd(res, a, b[i], i);
				}
				return new BigInteger(res, fNeg);
			}
			return MultiplyKaratsuba(a, b);
		}

		/// <summary>
		/// Fast multiplication: http://en.wikipedia.org/wiki/Karatsuba_algorithm
		/// </summary>
		private static BigInteger MultiplyKaratsuba(BigInteger x, BigInteger y)
		{
			int xlen = x.DigitCount;
			int ylen = y.DigitCount;

			int half = (Math.Max(xlen, ylen) + 1) / 2;

			BigInteger x0 = x.GetLower(half);
			BigInteger x1 = x.GetUpper(half);
			BigInteger y0 = y.GetLower(half);
			BigInteger y1 = y.GetUpper(half);

			BigInteger z0 = Zero, z2 = Zero;
			Action action0 = () => z0 = x0 * y0;
			Action action2 = () => z2 = x1 * y1;

			ParallelHelper.RunParallel(action0, action2);

			BigInteger z1 = (x0 + x1) * (y0 + y1) - z0 - z2;
			int b = BaseOps.BitsPerDigit * half;

			BigInteger result = (z2 << (2 * b)) + (z1 << b) + z0;
			result.isNegative = x.isNegative != y.isNegative;
			return result;
		}

		static BigInteger DivRemUnbalanced(BigInteger a, BigInteger b, out BigInteger r)
		{
			int n = b.DigitCount;
			int m = a.DigitCount - b.DigitCount;
			int normalShift = BaseOps.GetNormalization(b);
			a <<= normalShift;
			b <<= normalShift;

			var q = Zero;
			int nShift = n * BaseOps.BitsPerDigit;
			BigInteger r1, q1;
			while (m > n)
			{
				int shift = (m - n) * BaseOps.BitsPerDigit;
				q1 = DivRemWorker(a.GetUpper(m - n), b, out r1);
				q = (q << nShift) + q1;
				a = (r1 << shift) + a.GetLower(m - n);
				m -= n;
			}
			q1 = DivRemWorker(a, b, out r1);
			q = (q << (m * BaseOps.BitsPerDigit)) + q1;
			r = r1 >> normalShift;
			return q;
		}

		static BigInteger DivRemRecursive(BigInteger a, BigInteger b, out BigInteger r)
		{
			int n = b.DigitCount;
			int m = a.DigitCount - b.DigitCount;
			if (m > n)
				return DivRemUnbalanced(a, b, out r);

			int k = m / 2;

			//normalize numbers
			int normalShift = BaseOps.GetNormalization(b);
			a <<= normalShift;
			b <<= normalShift;
			var b0 = b.GetLower(k);
			var b1 = b.GetUpper(k);
			BigInteger q1 = Zero, r1 = Zero, q0 = Zero, r0 = Zero;

			var a0 = a.GetLower(2 * k);
			q1 = DivRemWorker(a.GetUpper(2 * k), b1, out r1);

			var kShift = k * BaseOps.BitsPerDigit;

			var ap = (r1 << (2 * kShift)) + a0 - ((q1 * b0) << kShift);
			var bks = Zero;
			while (ap.Sign < 0)
			{
				q1--;
				if (bks.IsZero())
					bks = b << kShift;
				ap += bks;
			}

			q0 = DivRemWorker(ap.GetUpper(k), b1, out r0);
			var app = (r0 << kShift) + ap.GetLower(k) - q0 * b0;
			while (app.Sign < 0)
			{
				q0--;
				app += b;
			}
			var q = (q1 << kShift) + q0;
			r = app;

			//var q = Combine(q0, q1, k);
			//r = Combine(r0, r1, k);
			r >>= normalShift;
			return q;
		}

		private BigInteger GetLower(int n)
		{
			if (digits == null)
				return this;
			int len = DigitCount;
			if (n >= len)
			{
				return this.Abs();
			}
			uint[] lowerInts = new uint[n];
			Array.Copy(this.digits, lowerInts, n);
			return new BigInteger(lowerInts, false);
		}

		private BigInteger GetUpper(int n)
		{
			int len = DigitCount;
			if (n >= len || digits == null)
			{
				return Zero;
			}
			int upperLen = len - n;
			uint[] upperInts = new uint[upperLen];
			Array.Copy(digits, n, upperInts, 0, upperLen);
			return new BigInteger(upperInts, false);
		}

		internal static BigInteger SingleDigitDivRemWorker(in BigInteger a, uint b, out uint r)
		{
			if (a.DigitCount <= 2)
			{
				ulong dd = a[1];
				dd <<= BaseOps.BitsPerDigit;
				dd |= a[0];
				ulong ad = dd;
				dd /= b;
				ad -= (dd * b);
				r = (uint)ad;
				return new BigInteger(dd, false);
			}
			var resultArray = new uint[a.DigitCount];
			ulong digit = a[a.DigitCount - 1];
			for (int i = a.DigitCount - 2; i >= 0; i--)
			{
				digit = (digit << BaseOps.BitsPerDigit) | a[i];
				var div = digit / b;
				if (div > BaseOps.MaxDigitValue)
				{
					resultArray[i + 1] += (uint)(div >> BaseOps.BitsPerDigit);
				}
				resultArray[i] = (uint)div;
				digit -= div * b;
			}
			r = (uint)digit;
			return new BigInteger(resultArray, false);
		}

		static BigInteger DivRemWorker(in BigInteger a, in BigInteger b, out BigInteger r)
		{
			//special case small divisors
			if (b.digits == null)
			{
				var retval = SingleDigitDivRemWorker(a, b.lenUsedOrDigit0, out uint rint);
				r = new BigInteger(rint, false);
				return retval;
			}
			int cmpRes = CompareUnsigned(a, b);
			if (cmpRes < 0) //|a|<=|b|
			{
				r = a;
				return Zero;
			}
			if (cmpRes == 0)
			{
				r = Zero;
				return One;
			}
			BigInteger quotient;
			if ((a.DigitCount - b.DigitCount) >= TsMathGlobals.BigIntegerRecursiveDivRemThreshold)
				quotient = DivRemRecursive(a, b, out r);
			else
				quotient = BaseOps.NaiveDivision(a, b, out r);
			return quotient;
		}

		/// <summary>
		/// Division with remainder. Solves a = b * q + r;
		/// </summary>
		/// <remarks>
		/// You can control the switch from the naive algorithm to a divide an conquer approach with the parameter <see cref="TsMathGlobals.BigIntegerRecursiveDivRemThreshold"/>.
		/// </remarks>		
		/// <param name="dividend">Dividend.</param>
		/// <param name="divisor">Divisor.</param>
		/// <param name="remainder">Remainder; can be negative.</param>
		/// <returns>Quotient q in the equation above.</returns>
		/// <exception cref="DivideByZeroException">The <paramref name="divisor"/> is zero.</exception>
		public static BigInteger DivRem(BigInteger dividend, BigInteger divisor, out BigInteger remainder)
		{
			if (divisor.digits == null && divisor.lenUsedOrDigit0 == 0)
				throw new DivideByZeroException();
			bool fQNeg = dividend.IsNegative() != divisor.IsNegative();
			bool fRNeg = dividend.IsNegative();

			var quotient = DivRemWorker(dividend.Abs(), divisor.Abs(), out remainder);
			if (!remainder.IsZero())
				remainder.isNegative = fRNeg;
			quotient.isNegative = fQNeg && !quotient.IsZero();
			return quotient;
		}

		/// <summary>
		/// Divides two <see cref="BigInteger"/> values.
		/// </summary>
		/// <param name="dividend">The value to be divided.</param>
		/// <param name="divisor">The value to divide by.</param>
		/// <returns>The result of the division.</returns>
		public static BigInteger operator /(BigInteger dividend, BigInteger divisor) => DivRem(dividend, divisor, out BigInteger r);

		/// <summary>
		/// Calculates the remainder of the division of two <see cref="BigInteger"/> values.
		/// </summary>
		/// <param name="dividend">The value to be divided.</param>
		/// <param name="divisor">The value to divide by.</param>
		/// <returns>The remainder of the division.</returns>
		public static BigInteger operator %(BigInteger dividend, BigInteger divisor)
		{
			DivRem(dividend, divisor, out BigInteger r);
			return r;
		}

		/// <summary>
		/// Conversion from <see cref="String"/> to <see cref="BigInteger"/>.
		/// </summary>
		/// <param name="str">The string to convert.</param>
		/// <exception cref="FormatException">The number format for the BigInteger is invalid.</exception>
		public static explicit operator BigInteger(string str) => string.IsNullOrEmpty(str) ? Zero : Parse(str);

		/// <summary>
		/// Conversion from <see cref="int"/> to <see cref="BigInteger"/>.
		/// </summary>
		/// <param name="number">The number to convert.</param>
		public static implicit operator BigInteger(int number) => BigIntegerExtensions.ToBigInteger((long)number);

		/// <summary>
		/// Conversion from <see cref="int"/> to <see cref="BigInteger"/>.
		/// </summary>
		/// <param name="number">The number to convert.</param>
		public static implicit operator BigInteger(long number) => number.ToBigInteger();

		/// <summary>
		/// Explicit conversion of this value to a <see cref="long"/>. See <see cref="ToLong"/> for the exact semantic.
		/// </summary>
		/// <param name="a">The value to convert.</param>
		public static explicit operator long(BigInteger a) => a.ToLong();

		/// <summary>
		/// Converts this big integer to a double value.
		/// </summary>
		/// <returns>The double value.</returns>
		public double ToDouble()
		{
			var a = this;
			double d = 0, fak = 1;
			bool sign = a.IsNegative();
			a = a.Abs();
			while (a != 0)
			{
				a = DivRem(a, 10, out BigInteger r);
				d += fak * r.lenUsedOrDigit0;
				fak *= 10D;
			}
			return sign ? -d : d;
		}

		/// <summary>
		/// Converts this <see cref="BigInteger"/> to a <see cref="long"/>. 
		/// </summary>
		/// <remarks>If the number is outside of the bounds of a <see cref="long"/> this method will 
		/// return <see cref="long.MinValue"/> resp. <see cref="long.MaxValue"/>. </remarks>
		/// <returns>The long value.</returns>
		public long ToLong()
		{
			const int dc = sizeof(long) / sizeof(uint);
			if (DigitCount > dc)
				return IsNegative() ? long.MinValue : long.MaxValue;
			ulong res = 0;
			for (int i = dc - 1; i >= 0; i--)
			{
				res <<= BaseOps.BitsPerDigit;
				res |= this[i];
			}
			ulong lmax = long.MaxValue;
			if (res > long.MaxValue)
			{
				if (!IsNegative())
					return long.MaxValue;
				lmax++;
				return res >= lmax ? long.MinValue : long.MinValue + 1;
			}
			long l = (long)res;

			if (this.IsNegative())
				l = -l;
			return l;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="BigInteger"/> to <see cref="System.Double"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>
		/// The result of the conversion.
		/// </returns>
		public static explicit operator double(BigInteger value)
		{
			return value.ToDouble();
		}

		/// <summary>
		/// Returns a <see cref="string" /> that represents this instance.
		/// </summary>
		/// <param name="maxDecimalDigits">The maximum number of digits for the result string, exceeding digits will be cut off.</param>
		/// <param name="base">The base of the number conversion.</param>
		/// <returns>A <see cref="string" /> that represents this instance.</returns>
		public string ToString(int maxDecimalDigits, int @base = 10)
		{
			if (@base <= 1 || @base >= 36)
				throw new ArgumentOutOfRangeException(nameof(@base));
			List<char> lc = new List<char>();
			BigInteger a = this;
			uint r = 0;
			if (a.IsNegative())
				a.isNegative = false;
			do
			{
				a = SingleDigitDivRemWorker(a, (uint)@base, out r);
				lc.Add(Index2Char(r));
			} while (a != 0 && lc.Count < maxDecimalDigits);
			if (isNegative)
				lc.Add('-');
			if (a != 0)
				for (int i = 0; i < 3; i++)
				{
					lc.Add('.');
				}
			lc.Reverse();

			char[] ca = lc.ToArray();
			string sret = new string(ca);
			if (a != 0)
				sret += string.Format(" [{0}]", (int)(BigInteger.Log(this, @base) + 0.5));
			return sret;

			char Index2Char(uint i)
			{
				if (i < 10)
					return (char)('0' + i);
				i -= 10;
				return (char)('a' + i);
			}
		}

		/// <summary>
		/// Returns a <see cref="string" /> that represents this instance.
		/// </summary>
		/// <remarks>The output shows only a fixed number of decimal digits as provided in <see cref="TsMathGlobals.DefaultToStringLength"/>.</remarks>
		/// <returns>
		/// A <see cref="string" /> that represents this instance.
		/// </returns>
		public override string ToString() => ToString(TsMathGlobals.DefaultToStringLength, 10);

		/// <summary>
		/// Compares this instance to an other one.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns>-1, 0 or 1; depending on if this instance is less than, equal or greater then <paramref name="other"/>.</returns>
		public int CompareTo(BigInteger other)
		{
			if (this.IsNegative() != other.IsNegative())
				return IsNegative() ? -1 : 1;
			int res = CompareUnsigned(this, other);
			if (IsNegative())
				res = -res;
			return res;
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <b>true</b> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <b>false</b>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (!(obj is BigInteger))
				return false;
			BigInteger a = (BigInteger)obj;
			return DoEquals(this, a);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			int hash = 13;
			hash = 1303 * hash + DigitCount;
			hash += 1303 * hash + (int)this[0];
			return hash;
		}

		static bool DoEquals(BigInteger a, BigInteger b)
		{
			bool aNull = a.IsZero(), bNull = b.IsZero();
			if (aNull && bNull)
				return true;
			if (aNull || bNull)
				return false;
			return a.CompareTo(b) == 0;
		}

		/// <summary>
		/// Gets a value indicating whether this instance is zero.
		/// </summary>
		/// <returns>
		/// <b>true</b> if this instance is zero; otherwise, <b>false</b>.
		/// </returns>
		public bool IsZero() => digits == null && lenUsedOrDigit0 == 0 && !IsNegative();

		/// <summary>
		/// Gets a value indicating whether this instance is one.
		/// </summary>
		/// <value>
		/// <b>true</b> if this instance is one; otherwise, <b>false</b>.
		/// </value>
		public bool IsOne => digits == null && lenUsedOrDigit0 == 1 && !IsNegative();

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>
		/// <b>true</b> if both numbers are equal; <b>false</b> otherwise.
		/// </returns>
		public static bool operator ==(BigInteger a, BigInteger b) => DoEquals(a, b);

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>
		/// <b>true</b> if both numbers are not equal; <b>false</b> otherwise.
		/// </returns>
		public static bool operator !=(BigInteger a, BigInteger b) => !DoEquals(a, b);

		/// <summary>
		/// Implements the operator &lt;=.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>
		/// <b>true</b> if both <paramref name="a"/> is less than or equal to <paramref name="b"/>; <b>false</b> otherwise.
		/// </returns>
		public static bool operator <=(BigInteger a, BigInteger b) => a.CompareTo(b) <= 0;

		/// <summary>
		/// Implements the operator &gt;=.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>
		/// <b>true</b> if both <paramref name="a"/> is greater than or equal to <paramref name="b"/>; <b>false</b> otherwise.
		/// </returns>
		public static bool operator >=(BigInteger a, BigInteger b) => a.CompareTo(b) >= 0;

		/// <summary>
		/// Implements the operator &lt;.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>
		/// <b>true</b> if both <paramref name="a"/> is less than <paramref name="b"/>; <b>false</b> otherwise.
		/// </returns>
		public static bool operator <(BigInteger a, BigInteger b) => a.CompareTo(b) < 0;

		/// <summary>
		/// Implements the operator &gt;.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>
		/// <b>true</b> if both <paramref name="a"/> is greater than <paramref name="b"/>; <b>false</b> otherwise.
		/// </returns>
		public static bool operator >(BigInteger a, BigInteger b) => a.CompareTo(b) > 0;

		/// <summary>
		/// The position of the most significant bit of this number.
		/// </summary>
		/// <example>
		/// <code source="Example.cs" region="ExMostSignificantBitPosition" />
		/// </example>
		public int GetMostSignificantBitPosition()
		{
			int n = DigitCount - 1;
			while (n >= 0 && this[n] == 0)
				n--;
			if (n < 0)
				return 0;
			int bc = (n + 1) * BaseOps.BitsPerDigit;
			uint d = this[n];
			uint bit = ((uint)1 << (BaseOps.BitsPerDigit - 1));
			while ((d & bit) == 0)
			{
				bc--;
				bit >>= 1;
			}
			return bc;
		}

		/// <summary>
		/// Calculates the power <paramref name="a"/> with the exponent <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The base.</param>
		/// <param name="b">The exponent (must be non negative).</param>
		/// <returns>a<sup>b</sup>.</returns>
		/// <exception cref="System.ArithmeticException">Negative exponents not allowed.</exception>
		public static BigInteger Pow(BigInteger a, BigInteger b)
		{
			if (b.IsNegative())
				throw new ArithmeticException("Negative exponents not allowed.");
			BigInteger res = 1;
			BigInteger prod = a;
			while (b > 0)
			{
				if (b.IsOdd())
					res *= prod;
				prod = prod * prod;
				b >>= 1;
			}
			return res;
		}

		/// <summary>
		/// Gets a value indicating whether this instance is even.
		/// </summary>
		/// <returns>
		///   <b>true</b> if this instance is even; otherwise, <b>false</b>.
		/// </returns>
		public bool IsEven()
		{
			return (this[0] & 1) == 0;
		}

		/// <summary>
		/// Gets a value indicating whether this instance is odd.
		/// </summary>
		/// <returns>
		///   <b>true</b> if this instance is odd; otherwise, <b>false</b>.
		/// </returns>
		public bool IsOdd()
		{
			return (this[0] & 1) != 0;
		}

		/// <summary>
		/// Calculates the bitwise and of two <see cref="BigInteger"/> numbers.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>The result.</returns>
		public static BigInteger operator &(BigInteger a, BigInteger b)
		{
			int nd = Math.Max(a.DigitCount, b.DigitCount);
			uint[] dig = new uint[nd];
			for (int i = 0; i < nd; i++)
				dig[i] = (uint)(a[i] & b[i]);
			return new BigInteger(dig, a.IsNegative() && b.IsNegative());
		}

		/// <summary>
		/// Calculates the bitwise or of two <see cref="BigInteger"/> numbers.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>The result.</returns>
		public static BigInteger operator |(BigInteger a, BigInteger b)
		{
			int nd = Math.Max(a.DigitCount, b.DigitCount);
			uint[] dig = new uint[nd];
			for (int i = 0; i < nd; i++)
				dig[i] = (uint)(a[i] | b[i]);
			return new BigInteger(dig, a.IsNegative() || b.IsNegative());
		}

		/// <summary>
		/// Calculates the bitwise xor of two <see cref="BigInteger"/> numbers.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>The result.</returns>
		public static BigInteger operator ^(BigInteger a, BigInteger b)
		{
			int nd = Math.Max(a.DigitCount, b.DigitCount);
			uint[] dig = new uint[nd];
			for (int i = 0; i < nd; i++)
				dig[i] = (uint)(a[i] ^ b[i]);
			return new BigInteger(dig, a.IsNegative() != b.IsNegative());
		}

		/// <summary>
		/// Implements the operator &gt;&gt;. Shifts the bits oft this number <paramref name="count"/> bits to the right.
		/// </summary>
		/// <param name="a">The number to shift.</param>
		/// <param name="count">The count to shift.</param>
		/// <returns>
		/// The result of the shift operator.
		/// </returns>
		public static BigInteger operator >>(BigInteger a, int count)
		{
			if (count < 0)
				return a << -count;
			int nfull = count / BaseOps.BitsPerDigit;
			int len = a.DigitCount - nfull;
			if (len <= 0)
				return Zero;
			uint[] res = new uint[len];
			if (a.digits == null)
				res[0] = a.lenUsedOrDigit0;
			else
				Array.Copy(a.digits, nfull, res, 0, len);
			int nRest = count % BaseOps.BitsPerDigit;
			if (nRest > 0)
			{
				int leftShift = BaseOps.BitsPerDigit - nRest;
				uint mask = ((uint)1 << nRest) - 1;
				for (int i = 0; i < res.Length - 1; i++)
				{
					res[i] = (res[i] >> nRest) | ((res[i + 1] & mask) << leftShift);
				}
				res[res.Length - 1] >>= nRest;
			}
			return new BigInteger(res, a.IsNegative());
		}

		/// <summary>
		/// Implements the operator &lt;&lt;. Shifts the bits oft this number <paramref name="count"/> bits to the left.
		/// </summary>
		/// <param name="a">The number to shift.</param>
		/// <param name="count">The count to shift.</param>
		/// <returns>
		/// The result of the shift operator.
		/// </returns>
		public static BigInteger operator <<(BigInteger a, int count)
		{
			if (count < 0)
				return a >> -count;
			int nfull = count / BaseOps.BitsPerDigit;
			int nRest = count % BaseOps.BitsPerDigit;
			int resLen = a.DigitCount + nfull;
			if (nRest > 0)
				resLen++;
			uint[] res = new uint[resLen];
			if (a.digits == null)
				res[nfull] = a.lenUsedOrDigit0;
			else
				Array.Copy(a.digits, 0, res, nfull, a.DigitCount);
			if (nRest > 0)
			{
				int rightShift = BaseOps.BitsPerDigit - nRest;
				for (int i = resLen - 1; i > 0; i--)
				{
					res[i] = (res[i] << nRest) | (res[i - 1] >> rightShift);
				}
				res[0] <<= nRest;
			}
			return new BigInteger(res, a.IsNegative());
		}

		/// <summary>
		/// Calculates the greatest common denominator for two <see cref="BigInteger"/>s.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns>The greatest common denominator.</returns>
		public static BigInteger Gcd(BigInteger a, BigInteger b)
		{
			a = a.Abs(); b = b.Abs();
			BigInteger d, r;
			if (a < b)
			{
				d = a; a = b; b = d;
			}
			do
			{
				d = DivRem(a, b, out r);
				a = b; b = r;
			} while (r != 0);
			return a;
		}

		/// <summary>
		/// Calculates the logarithm with base <paramref name="baseValue"/> of the given number.
		/// </summary>
		/// <param name="value">The number to calculate the logarithm.</param>
		/// <param name="baseValue">The base of the logarithm (default e).</param>
		/// <returns>The logarithm of <paramref name="value"/>.</returns>
		public static double Log(BigInteger value, double baseValue = Math.E)
		{
			if (value.IsNegative() || baseValue == 1.0)
				return double.NaN;
			if (baseValue == double.PositiveInfinity)
			{
				return !value.IsOne ? double.NaN : 0.0;
			}
			else
			{
				if (baseValue == 0.0 && !value.IsOne)
					return double.NaN;
				if (value.digits == null)
					return Math.Log((double)value.lenUsedOrDigit0, baseValue);
				double d = 0.0;
				double logTemp = 0.5;
				int digitCount = value.DigitCount;
				int bitsInDigit = BaseOps.BitLengthOfDigit(value.digits[digitCount - 1]);
				int totalBits = (digitCount - 1) * BaseOps.BitsPerDigit + bitsInDigit;
				uint num5 = (uint)(1 << bitsInDigit - 1);
				for (int index = digitCount - 1; index >= 0; --index)
				{
					while ((int)num5 != 0)
					{
						if ((value.digits[index] & num5) != 0)
							d += logTemp;
						logTemp *= 0.5;
						num5 >>= 1;
					}
					num5 = BaseOps.HalfValue;
				}
				return (Math.Log(d) + BaseOps.Log2DivLogE * (double)totalBits) / Math.Log(baseValue);
			}
		}

		/// <summary>
		/// Calculates the logarithm with base 10 of the given number.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static double Log10(BigInteger value) => Log(value, 10);

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// <b>true</b> if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, <b>false</b>.
		/// </returns>
		public bool Equals(BigInteger other) => DoEquals(this, other);

	}





}
