using DNSProfileChecker.Common;
using System;
using System.Diagnostics;

namespace DNSProfileChecker.UnitTest
{
	public sealed class DebugLogger : ILogger
	{
		string ProvideMessage(string message, Exception ex)
		{
			return string.Format("{1}{0}{2}", Environment.NewLine, message == null ? string.Empty : message, ex == null ? string.Empty : ex.ToString());
		}

		public void LogData(LogSeverity severity, string message, Exception ex)
		{
			switch (severity)
			{
				case LogSeverity.Info:
					Debug.WriteLine("INFO" + Environment.NewLine + ProvideMessage(message, ex));
					break;
				case LogSeverity.Warn:
					Debug.WriteLine("WARN" + Environment.NewLine + ProvideMessage(message, ex));
					break;
				case LogSeverity.Error:
					Debug.WriteLine("ERROR" + Environment.NewLine + ProvideMessage(message, ex));
					break;
				case LogSeverity.Fatal:
					Debug.WriteLine("FATAL" + Environment.NewLine + ProvideMessage(message, ex));
					break;
				default:
					break;
			}
		}
	}
}
