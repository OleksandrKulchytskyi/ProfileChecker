using Caliburn.Micro;
using DNSProfileChecker.Common;
using System;

namespace Nuance.Radiology.DNSProfileChecker.Infrastructure.Logger
{
	public sealed class UILogger : ILogger
	{
		private readonly IEventAggregator _eventAggregator;

		public UILogger()
		{
			_eventAggregator = IoC.Get<IEventAggregator>();
		}

		public UILogger(IEventAggregator eventAggregator)
		{
			_eventAggregator = eventAggregator;
		}

		public void LogData(LogSeverity severity, string message, Exception ex)
		{
			var msg = new Infrastructure.Messages.LogEntry() { Severity = severity, Message = message, Error = ex };
			_eventAggregator.PublishOnUIThread(msg);
		}
	}
}