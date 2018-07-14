using System;

namespace TsMath.Structures
{
	class GFractionArithmetic<N> : Arithmetic<GFraction<N>> where N : IEquatable<N>
	{
		Arithmetic<N> elArith = Arithmetic<N>.GetArithmetic();

		public override GFraction<N> Zero(GFraction<N> hint) => new GFraction<N>(elArith.Zero(hint.Numerator), elArith.One(hint.Numerator), true);

		public override GFraction<N> One(GFraction<N> hint) => new GFraction<N>(elArith.One(hint.Numerator), elArith.One(hint.Numerator), true);

		public override GFraction<N> MinusOne(GFraction<N> hint) => new GFraction<N>(elArith.MinusOne(hint.Numerator), elArith.One(hint.Numerator), true);

		public override GFraction<N> Add(GFraction<N> a, GFraction<N> b) => a + b;

		public override GFraction<N> Subtract(GFraction<N> a, GFraction<N> b) => a - b;

		public override GFraction<N> Multiply(GFraction<N> a, GFraction<N> b) => a * b;

		public override GFraction<N> Divide(GFraction<N> a, GFraction<N> b) => a / b;

		public override bool IsNegative(GFraction<N> a) => a.IsNegative();

		public override bool IsZero(GFraction<N> value) => value.IsZero();

		public override GFraction<N> Negate(GFraction<N> a) => -a;

		public override bool TryParse(string s, out GFraction<N> value) => GFraction<N>.TryParse(s, out value);

		public override string ToString(GFraction<N> value, int maxLen) => value.ToString(maxLen);

	}
}
