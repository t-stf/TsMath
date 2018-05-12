using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsMath.Helpers
{
	/// <summary>
	/// Helper functions.
	/// </summary>
	public static class DivHelpers
	{

		/// <summary>
		/// Swaps the specified values.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a">a.</param>
		/// <param name="b">b.</param>
		public static void Swap<T>(ref T a, ref T b)
		{
			T tmp = a;
			a = b;
			b = tmp;
		}

		/// <summary>
		/// Swaps two elements in an array.
		/// </summary>
		/// <typeparam name="T">Type of the array.</typeparam>
		/// <param name="array">The array.</param>
		/// <param name="index1">Index of the first element.</param>
		/// <param name="index2">Index of the second element.</param>
		public static void Swap<T>(this T[] array, int index1, int index2)
		{
			T tmp = array[index1];
			array[index1] = array[index2];
			array[index2] = tmp;
		}

		/// <summary>
		/// Generates the next permutation of an given array.
		/// </summary>
		/// <remarks>
		/// <para>
		/// To enumerate all permutations the elements of the initial array must be lexicographically ordered, e.g. {1 2 3 4}.
		/// </para>
		/// <para>
		/// The array must not contain duplicate entries.
		/// </para>
		/// </remarks>
		/// <typeparam name="T">The element types.</typeparam>
		/// <param name="array">The permutation array.</param>
		/// <returns><b>true</b> if there is a next permutation; <b>false</b> otherwise.</returns>
		public static bool NextPermutation<T>(this T[] array) where T : IComparable<T>
		{
			var n = array.Length;

			for (var i = n - 1; i > 0; i--)
			{
				var el = array[i];
				if (el.CompareTo(array[i - 1]) < 0)
					continue;

				var prev = array[i - 1];
				var index = i;
				for (var j = i + 1; j < n; j++)
				{
					var temp = array[j];
					if (temp.CompareTo(el) < 0 && temp.CompareTo(prev) > 0)
					{
						el = temp;
						index = j;
					}
				}
				array[index] = prev;
				array[i - 1] = el;
				for (var j = n - 1; j > i; j--, i++)
				{
					Swap(array, i, j);
				}
				return true;
			}
			return false;
		}

	}
}
