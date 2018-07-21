using System;
using System.Collections.Generic;
using System.Text;

namespace TsMath.Structures
{
	internal class PolynomialArithmetic<T> : Arithmetic<Polynomial<T>> 
	{
		Arithmetic<T> arithmetic = Polynomial<T>.ElementArithmetic;

		public override Polynomial<T> Zero(Polynomial<T> hint) => new Polynomial<T>(arithmetic.Zero(hint.LeadingCoefficient));

		public override Polynomial<T> One(Polynomial<T> hint) => new Polynomial<T>(arithmetic.One(hint.LeadingCoefficient));

		public override bool IsZero(Polynomial<T> value) => value.IsZero();

		public override bool IsOne(Polynomial<T> value) => value.IsOne();

		public override Polynomial<T> Add(Polynomial<T> a, Polynomial<T> b) => a + b;

		public override Polynomial<T> Subtract(Polynomial<T> a, Polynomial<T> b) => a-b;

		public override Polynomial<T> Multiply(Polynomial<T> a, Polynomial<T> b) => a*b;

		public override Polynomial<T> DivideWithRemainder(Polynomial<T> a, Polynomial<T> b, out Polynomial<T> remainder)
		{
			return Polynomial<T>.DivRem(a, b, out remainder);
		}

		public override string ToString(Polynomial<T> value, int maxLen) => value.ToString(maxLen);

		public override bool Equals(Polynomial<T> x, Polynomial<T> y) => x.Equals(y);

		public override Polynomial<T> Gcd(Polynomial<T> a, Polynomial<T> b) => Polynomial<T>.Gcd(a,b);

		
	}
}
