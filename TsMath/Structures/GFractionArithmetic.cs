using System;

namespace TsMath.Structures
{
	class GFractionArithmetic<N> : Arithmetic<Fraction<N>> where N : IEquatable<N>
	{
		Arithmetic<N> elArith = Arithmetic<N>.GetArithmetic();

		public override Fraction<N> Zero(Fraction<N> hint) => new Fraction<N>(elArith.Zero(hint.Numerator), elArith.One(hint.Numerator), true);

		public override Fraction<N> One(Fraction<N> hint) => new Fraction<N>(elArith.One(hint.Numerator), elArith.One(hint.Numerator), true);

		public override Fraction<N> MinusOne(Fraction<N> hint) => new Fraction<N>(elArith.MinusOne(hint.Numerator), elArith.One(hint.Numerator), true);

		public override Fraction<N> Add(Fraction<N> a, Fraction<N> b) => a + b;

		public override Fraction<N> Subtract(Fraction<N> a, Fraction<N> b) => a - b;

		public override Fraction<N> Multiply(Fraction<N> a, Fraction<N> b) => a * b;

		public override Fraction<N> Divide(Fraction<N> a, Fraction<N> b) => a / b;

		public override bool IsNegative(Fraction<N> a) => a.IsNegative();

		public override bool IsZero(Fraction<N> value) => value.IsZero();

		public override Fraction<N> Negate(Fraction<N> a) => -a;

		public override bool TryParse(string s, out Fraction<N> value) => Fraction<N>.TryParse(s, out value);

		public override string ToString(Fraction<N> value, int maxLen) => value.ToString(maxLen);

	}
}
