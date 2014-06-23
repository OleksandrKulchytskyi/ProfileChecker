using DNSProfileChecker.Common;
using System;

namespace Nuance.Radiology.DNSProfileChecker.Infrastructure.Messages
{
	public class LogEntry : Caliburn.Micro.PropertyChangedBase
	{

		private LogSeverity sever;
		public LogSeverity Severity
		{
			get { return sever; }
			set
			{
				sever = value;
				NotifyOfPropertyChange(() => Severity);
				NotifyOfPropertyChange(() => NeedBolding);
			}
		}

		public string Message { get; set; }

		public Exception Error { get; set; }

		public bool NeedBolding
		{
			get { return Severity == LogSeverity.Success || Severity == LogSeverity.Error; }
		}
	}
}