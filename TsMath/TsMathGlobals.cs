namespace TsMath
{
	/// <summary>
	/// This class stores global parameters, the user of the library can tweak.
	/// </summary>
	/// <remarks>
	/// If you change the values at runtime you must do it before calling any other function of the library.
	/// </remarks>
	public static class TsMathGlobals
	{

		/// <summary>
		/// The maximum cached exponent for <see cref="BigIntegerExtensions.GetPowerOfTen(int)"/>.
		/// </summary>
		public static int MaxCachedExponentForGetPowerOfTen = 100;

		/// <summary>
		/// The maximum big integer digit count used in <see cref="BigInteger.ToString()"/>.
		/// </summary>
		public static int MaxBigIntegerToStringDigitCount = 80;


		/// <summary>
		/// The maximum parallel number of operations that could execute in parallel. Set this value to zero,
		/// if you want a single threaded execution.
		/// </summary>
		public static int MaxParallel = 2 * System.Environment.ProcessorCount;


		/// <summary>
		/// The difference of digits between dividend and divisor, where the algorithm switches from naive division to a
		/// divide and conquer recursive version.
		/// </summary>
		public static int BigIntegerRecursiveDivRemThreshold = 10;


	}

}
