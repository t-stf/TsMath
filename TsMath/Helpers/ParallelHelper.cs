using System;
using System.Threading;
using System.Threading.Tasks;

namespace TsMath.Helpers
{

	/// <summary>
	/// Internal helper class to switch multi threading behavior.
	/// </summary>
	public class ParallelHelper
	{

		/// <summary>
		/// Main switch, to enable parallel execution. If set to <b>false</b>, nothing will be executed 
		/// with multi threading. (This switch is for debugging purposes mostly and should not be used
		/// in normal code)
		/// </summary>
		static bool ParallelAllowed => CurrentParallel < TsMathGlobals.MaxParallel;

		/// <summary>
		/// The number of currently running operations in parallel.
		/// </summary>
		private static int CurrentParallel = 0;

		/// <summary>
		/// Switchable parallel for loop.
		/// </summary>
		/// <param name="fromInclusive">From inclusive.</param>
		/// <param name="toExclusive">To exclusive.</param>
		/// <param name="action">The action.</param>
		/// <param name="fParallel">if set to <b>true</b> the parallel for will be used.</param>
		public static void For(int fromInclusive, int toExclusive, Action<int> action, bool fParallel = true)
		{
			if (fParallel && ParallelAllowed)
				Parallel.For(fromInclusive, toExclusive, 
					(idx)=>
					{
						Interlocked.Increment(ref CurrentParallel);
						try
						{
							action(idx);
						}
						finally
						{
							Interlocked.Decrement(ref CurrentParallel);
						}
					});
			else
			{
				for (int i = fromInclusive; i < toExclusive; i++)
					action(i);
			}
		}

		/// <summary>
		/// Runs the provided actions the parallel, if possible.
		/// </summary>
		/// <param name="actions">The actions.</param>
		public static void RunParallel(params Action[] actions)
		{
			For(0, actions.Length, idx => actions[idx]());
		}

	}
}
