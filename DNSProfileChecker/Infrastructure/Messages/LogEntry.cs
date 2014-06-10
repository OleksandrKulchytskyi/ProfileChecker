using DNSProfileChecker.Common;
using System;

namespace Nuance.Radiology.DNSProfileChecker.Infrastructure.Messages
{
	public class LogEntry
	{
		public LogSeverity Severity { get; set; }

		public string Message { get; set; }

		public Exception Error { get; set; }
	}
}