using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DNSProfileChecker.Common
{
	public static class Retry
	{
		public static void Do(Action action, TimeSpan retryInterval, bool triggerError, int retryCount = 3)
		{
			AggregateException ex;
			Do<object>(() =>
			{
				action();
				return null;
			}, retryInterval, retryCount, out ex);
			if (ex != null && triggerError)
				throw ex;
		}
		public static T Do<T>(Func<T> action, TimeSpan retryInterval, int retryCount, out AggregateException exc)
		{
			exc = null;
			var exceptions = new List<Exception>();

			for (int retry = 0; retry < retryCount; retry++)
			{
				try
				{
					return action();
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
					Thread.Sleep(retryInterval);
				}
			}

			exc = new AggregateException(exceptions);
			return default(T);
		}
	}
}
