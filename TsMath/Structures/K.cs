namespace TsMath.Structures
{
	struct K<T>
	{
		static Arithmetic<T> arith = Arithmetic<T>.GetArithmetic();

		T value;

		public K(T value)
		{
			this.value = value;
		}

		public static K<T> operator +(in K<T> a, in K<T> b) => new K<T>(arith.Add(a.value, b.value));

		public static K<T> operator -(in K<T> a, in K<T> b) => new K<T>(arith.Subtract(a.value, b.value));

		public static K<T> operator -(in K<T> a) => new K<T>(arith.Negate(a.value));

		public static K<T> operator *(in K<T> a, in K<T> b) => new K<T>(arith.Multiply(a.value, b.value));

		public static K<T> operator /(in K<T> a, in K<T> b) => new K<T>(arith.Divide(a.value, b.value));

		public static implicit operator K<T>(T value) => new K<T>(value);

		public static implicit operator T(K<T> value) => value.value;

		public override string ToString() => value.ToString();

	}
}
