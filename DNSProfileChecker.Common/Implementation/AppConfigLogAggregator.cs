using DNSProfileChecker.Common.Configuration;
using System;
using System.Collections.Generic;

namespace DNSProfileChecker.Common.Implementation
{
	public sealed class AppConfigLogProvider : ILogProvider
	{
		private LogProviderConfig config;
		Dictionary<string, ILogger> _container;
		public AppConfigLogProvider()
		{
			config = LogProviderConfig.GetConfig();
		}

		public object Settings { get; set; }

		public Dictionary<string, ILogger> GetLoggers()
		{
			if (_container == null)
			{
				_container = new Dictionary<string, ILogger>();
				foreach (Logger item in config.Loggers)
				{
					if (!_container.ContainsKey(item.name))
					{
						Type loggerType = Type.GetType(item.type);
						_container[item.name] = Activator.CreateInstance(loggerType) as ILogger;
					}
				}

				return _container;
			}
			return _container;
		}
	}
}
