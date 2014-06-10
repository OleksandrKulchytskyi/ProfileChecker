using System.Collections.Generic;

namespace DNSProfileChecker.Common
{
	public interface ILogProvider
	{
		object Settings { get; set; }

		Dictionary<string, ILogger> GetLoggers();
	}
}