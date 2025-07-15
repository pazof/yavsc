using System;
using System.Threading.Tasks;

namespace yavscTests {

	public static class AssertAsync {
		/// <summary>
		/// Completes In 
		/// </summary>
		/// <param name="timeoutFromSecond"></param>
		/// <param name="action"></param>
		public static void CompletesIn(int timeoutFromSecond, Action action)
		{
			var task = Task.Run(action);
			var completedInTime = Task.WaitAll(new[] { task }, TimeSpan.FromSeconds(timeoutFromSecond));

			if (task.Exception != null)
			{
				if (task.Exception.InnerExceptions.Count == 1)
				{
					throw task.Exception.InnerExceptions[0];
				}

				throw task.Exception;
			}

			if (!completedInTime)
			{
				throw new TimeoutException($"Task did not complete in {timeoutFromSecond} seconds.");
			}
		}
	}

}



