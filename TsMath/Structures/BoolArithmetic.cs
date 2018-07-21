using System;
using System.Collections.Generic;
using System.Text;

namespace TsMath.Structures
{
	class BoolArithmetic : Arithmetic<bool>
	{

		public override bool Zero(bool hint) => false;

		public override bool One(bool hint) => true;

		public override bool IsZero(bool value) => !value;

		public override bool IsOne(bool value) => value;

		public override bool Equals(bool x, bool y) => x == y;

		public override int GetHashCode(bool obj) => obj.GetHashCode();

		public override int CompareMagnitude(bool a, bool b) => a.CompareTo(b);

		public override bool Add(bool a, bool b) => a ^ b;

		public override bool Subtract(bool a, bool b) => a ^ b;

		public override bool Multiply(bool a, bool b) => a && b;

		public override bool Divide(bool a, bool b) 
		{
			if (!b)
				throw new DivideByZeroException();
			return a;
		}

		public override string ToString(bool value, int maxLen) => value ? "1" : "0";

		static bool ParseBoolLazy(string s)
		{
			if (string.IsNullOrEmpty(s))
				return false;
			s = s.ToLowerInvariant();
			if (s == "true")
				return true;
			if (s == "false")
				return false;
			int.TryParse(s, out int iv);
			return iv != 0;
		}

		public override bool TryParse(string s, out bool value)
		{
			value = ParseBoolLazy(s);
			return true;
		}

		
	}
}
