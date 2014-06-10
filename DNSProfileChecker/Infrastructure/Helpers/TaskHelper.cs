using DNSProfileChecker.Common;
using System;
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
	}
}