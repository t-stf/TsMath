using System;
using System.Collections.Generic;

namespace TsMath.Structures
{
	/// <summary>
	/// Factory class to store and retrieve arithmetics for types.
	/// </summary>
	public static class ArithmeticFactory
	{

		static Dictionary<Type, object> arithmetics = new Dictionary<Type, object>();

		static ArithmeticFactory()
		{
			SetArithmetic(typeof(double), new DoubleArithmetic());
			SetArithmetic(typeof(long), new LongArithmetic());
			SetArithmetic(typeof(bool), new BoolArithmetic());
			SetArithmetic(typeof(BigInteger), new BigIntegerArithmetic());
		}

		/// <summary>
		/// Retrieves the arithmetic for a type. 
		/// </summary>
		/// <typeparam name="T">The type to retrieve the arithmetic for.</typeparam>
		/// <returns>The arithmetic for the type <typeparamref name="T"/>.</returns>
		/// <exception cref="ArithmeticException">For the type <typeparamref name="T"/> is no type in the factory.</exception>
		public static Arithmetic<T> GetArithmetic<T>()
		{
			if (!arithmetics.TryGetValue(typeof(T), out object val))
				throw new ArithmeticException($"No arithmetic for type {typeof(T).Name}");
			return (Arithmetic<T>)val;
		}

		/// <summary>
		/// Sets the arithmetic for <paramref name="type"/>.
		/// </summary>
		/// <typeparam name="T">The type parameter of the arithmetic.</typeparam>
		/// <param name="type">The type to set the arithmetic for.</param>
		/// <param name="arithmetic">The arithmetic.</param>
		public static void SetArithmetic<T>(Type type, Arithmetic<T> arithmetic)
		{
			arithmetics[type] = arithmetic;
		}
	}
}
