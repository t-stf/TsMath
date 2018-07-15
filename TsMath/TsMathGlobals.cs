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
		/// The maximum length for ToString operations, to avoid very large strings.
		/// </summary>
		public static int DefaultToStringLength = 80;

		/// <summary>
		/// The maximum number of parallel operations. Set this value to zero, if you want a single threaded execution.
		/// </summary>
		public static int MaxParallel = 2 * System.Environment.ProcessorCount;

		/// <summary>
		/// Number of digits (<see cref="uint"/> values) of a number to switch to Karatsuba multiplication. 
		/// </summary>
		public static int BigIntegerKaratsubaThreshold = 20;

		/// <summary>
		/// The difference of digits (<see cref="uint"/> values) between dividend and divisor, where the algorithm switches from naive division to a
		/// divide and conquer recursive version.
		/// </summary>
		public static int BigIntegerRecursiveDivRemThreshold = 10;

		/// <summary>
		/// Number of matrix elements, when the algorithms switch to a multi threaded version.
		/// </summary>
		public static int MatrixMultithreadingThreshold = 1_000;

	}

}
