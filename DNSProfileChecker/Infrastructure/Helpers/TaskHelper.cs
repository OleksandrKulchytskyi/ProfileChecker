using DNSProfileChecker.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nuance.Radiology.DNSProfileChecker.Infrastructure.Helpers
{
	public static class TaskHelper
	{
		public static Task CreateTaskFromParametrizedAction(Action<object> action, object state)
		{
			Ensure.Argument.NotNull(action, "action cannot be a null.");
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

			Task mainRunner = Task.Factory.StartNew(action, state);
			mainRunner.ContinueWith((prevTask) => tcs.SetResult(null), TaskContinuationOptions.OnlyOnRanToCompletion);
			mainRunner.ContinueWith((prevTask) => tcs.SetException(prevTask.Exception), TaskContinuationOptions.OnlyOnFaulted);

			return tcs.Task;
		}

		public static Task Delay(int millisecondsDelay)
		{
			var taskCompletionSource = new TaskCompletionSource<bool>();

			var timer = new Timer(self =>
			{
				((Timer)self).Dispose();
				try
				{
					
					taskCompletionSource.SetResult(true);
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
				}
			});
			timer.Change(millisecondsDelay, millisecondsDelay);

			return taskCompletionSource.Task;
		}

		public static Task Delay(double milliseconds)
		{
			var tcs = new TaskCompletionSource<bool>();
			System.Timers.Timer timer = new System.Timers.Timer();
			timer.Elapsed += (obj, args) =>
			{
				tcs.TrySetResult(true);
			};
			timer.Interval = milliseconds;
			timer.AutoReset = false;
			timer.Start();
			return tcs.Task;
		}
	}
}