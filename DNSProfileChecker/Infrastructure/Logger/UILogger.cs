using Caliburn.Micro;
using DNSProfileChecker.Common;
using System;
using System.Linq;

namespace Nuance.Radiology.DNSProfileChecker.Infrastructure.Logger
{
	public sealed class UILogger : ILogger
	{
		private readonly IEventAggregator _eventAggregator;
		private string[] severities = null;
		public UILogger()
		{
			_eventAggregator = IoC.Get<IEventAggregator>();
			if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains("UILoggerSeverities"))
				severities = System.Configuration.ConfigurationManager.AppSettings["UILoggerSeverities"].Split(',');
		}

		public UILogger(IEventAggregator eventAggregator)
		{
			_eventAggregator = eventAggregator;
		}

		public void LogData(LogSeverity severity, string message, Exception ex)
		{
			if (severities.Any(x => x.Equals(severity.ToString())))
			{
				var msg = new Infrastructure.Messages.LogEntry() { Severity = severity, Message = message, Error = ex };
				_eventAggregator.PublishOnUIThread(msg);
			}
		}
	}
}