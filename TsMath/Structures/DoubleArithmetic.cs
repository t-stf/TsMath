using System;
using System.Globalization;

namespace TsMath.Structures
{
	class DoubleArithmetic : Arithmetic<double>
	{
		public override string DomainName => "double";

		public override double Zero(double hint) => 0;

		public override double One(double hint) => 1;

		public override double MinusOne(double hint) => -1;

		public override double Add(double a, double b) => a + b;

		public override double Subtract(double a, double b) => a - b;

		public override double Negate(double a) => -a;

		public override double Multiply(double a, double b) => a * b;

		public override double Divide(double a, double b) => a / b;

		public override double Invert(double a) => 1.0 / a;

		public override int Sign(double a) => Math.Sign(a);

		public override bool IsNegative(double a) => a < 0;

		public override double Abs(double a) => a < 0 ? -a : a;

		public override bool IsZero(double value) => value == 0;

		public override bool TryParse(string s, out double value) => double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out value);

		public override string ToString(double value, int maxLen) => value.ToString(CultureInfo.InvariantCulture);

	}
}
