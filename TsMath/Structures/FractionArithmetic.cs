using System;

namespace TsMath.Structures
{
	class FractionArithmetic<T> : Arithmetic<Fraction<T>> where T : IEquatable<T>
	{
		Arithmetic<T> elArith = Arithmetic<T>.GetArithmetic();

		public override Fraction<T> Zero(Fraction<T> hint) => new Fraction<T>(elArith.Zero(hint.Numerator), elArith.One(hint.Numerator), true);

		public override Fraction<T> One(Fraction<T> hint) => new Fraction<T>(elArith.One(hint.Numerator), elArith.One(hint.Numerator), true);

		public override Fraction<T> MinusOne(Fraction<T> hint) => new Fraction<T>(elArith.MinusOne(hint.Numerator), elArith.One(hint.Numerator), true);

		public override Fraction<T> Add(Fraction<T> a, Fraction<T> b) => a + b;

		public override Fraction<T> Subtract(Fraction<T> a, Fraction<T> b) => a - b;

		public override Fraction<T> Multiply(Fraction<T> a, Fraction<T> b) => a * b;

		public override Fraction<T> Divide(Fraction<T> a, Fraction<T> b) => a / b;

		public override bool IsNegative(Fraction<T> a) => a.IsNegative();

		public override bool IsZero(Fraction<T> value) => value.IsZero();

		public override Fraction<T> Negate(Fraction<T> a) => -a;

		public override bool TryParse(string s, out Fraction<T> value) => Fraction<T>.TryParse(s, out value);

		public override string ToString(Fraction<T> value, int maxLen) => value.ToString(maxLen);

	}
}
