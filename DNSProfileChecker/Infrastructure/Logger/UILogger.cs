using Caliburn.Micro;
using DNSProfileChecker.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			var msg = new Infrastructure.Messages.LogEntry() { Error = ex, Message = message, Severity = severity };
			_eventAggregator.PublishOnUIThread(msg);
		}
	}
}
