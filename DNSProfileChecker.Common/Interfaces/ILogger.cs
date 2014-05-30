using System;

namespace DNSProfileChecker.Common
{
	public enum LogSeverity
	{
		Info = 0,
		Warn,
		Error,
		Fatal
	}

	public interface ILogger
	{
		void LogData(LogSeverity severity, string message, Exception ex);
	}
}
