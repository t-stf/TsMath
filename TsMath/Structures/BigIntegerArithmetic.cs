namespace TsMath.Structures
{
	class BigIntegerArithmetic : Arithmetic<BigInteger>
	{
		public override BigInteger Zero(BigInteger hint) => BigInteger.Zero;

		public override BigInteger One(BigInteger hint) => BigInteger.One;

		public override BigInteger MinusOne(BigInteger hint) => BigInteger.MinusOne;

		public override BigInteger Add(BigInteger a, BigInteger b) => a + b;

		public override BigInteger Subtract(BigInteger a, BigInteger b) => a - b;

		public override BigInteger Multiply(BigInteger a, BigInteger b) => a * b;

		public override BigInteger DivideWithRemainder(BigInteger a, BigInteger b, out BigInteger remainder) => BigInteger.DivRem(a, b, out remainder);

		public override BigInteger Divide(BigInteger a, BigInteger b) => a / b;

		public override BigInteger Gcd(BigInteger a, BigInteger b) => IntegerMath.Gcd(a, b);

		public override int Sign(BigInteger a) => a.Sign;

		public override bool IsZero(BigInteger value) => value.IsZero();

		public override bool TryParse(string s, out BigInteger value) => BigInteger.TryParse(s, out value);

		public override string ToString(BigInteger value, int maxLen) => value.ToString(maxLen);

		public override int CompareMagnitude(BigInteger a, BigInteger b) => a.Abs().CompareTo(b.Abs());

	}
}
