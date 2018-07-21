using System;

namespace TsMath.Structures
{
	class LongArithmetic : Arithmetic<long>
	{
		public override long Zero(long hint) => 0;

		public override long One(long hint) => 1;

		public override long MinusOne(long hint) => -1;

		public override long Add(long a, long b)
		{
			checked { return a + b; }
		}

		public override long Subtract(long a, long b)
		{
			checked { return a - b; }
		}

		public override long Multiply(long a, long b)
		{
			checked { return a * b; }
		}

		public override long Divide(long a, long b)
		{
			checked { return a / b; }
		}

		public override long DivideWithRemainder(long a, long b, out long remainder)
		{
			checked
			{
				remainder = a % b;
				return a / b;
			}
		}

		public override int Sign(long a) => Math.Sign(a);

		public override long Gcd(long a, long b) => IntegerMath.Gcd(a, b);

		public override bool IsZero(long value) => value == 0;

		public override bool IsOne(long value) => value == 1;

		public override bool TryParse(string s, out long value) => long.TryParse(s, out value);

		public override int CompareMagnitude(long a, long b) => Math.Abs(a).CompareTo(Math.Abs(b));

	}
}
