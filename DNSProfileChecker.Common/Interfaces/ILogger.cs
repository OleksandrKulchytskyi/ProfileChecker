using System;

namespace DNSProfileChecker.Common
{
	public enum LogSeverity
	{
		Debug = 0,
		Info,
		UI,
		Warn,
		Error,
		Fatal,
		Success
	}

	public interface ILogger
	{
		void LogData(LogSeverity severity, string message, Exception ex);
	}
}