using System;
using System.Collections.Generic;

namespace DNSProfileChecker.Common
{
	public class LoggerBridge : ILogger
	{
		private class LogComposite : ILogger
		{
			private readonly Dictionary<string, ILogger> _loggers;

			public LogComposite(Dictionary<string, ILogger> loggers)
			{
				Ensure.Argument.NotNull(loggers);
				_loggers = loggers;
			}

			public void LogData(LogSeverity severity, string message, Exception ex)
			{
				foreach (ILogger logger in _loggers.Values)
				{
					Retry.Do(() => logger.LogData(severity, message, ex), TimeSpan.FromMilliseconds(150), false, 2);
				}
			}
		}

		private readonly ILogProvider _provider;
		private readonly ILogger _logger;

		public LoggerBridge(ILogProvider provider)
		{
			Ensure.Argument.NotNull(provider);
			_provider = provider;
			_logger = new LogComposite(_provider.GetLoggers());
		}

		public void LogData(LogSeverity severity, string message, Exception ex)
		{
			_logger.LogData(severity, message, ex);
		}
	}
}