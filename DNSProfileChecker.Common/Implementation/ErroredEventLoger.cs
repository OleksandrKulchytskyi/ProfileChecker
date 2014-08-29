using System;
using System.Diagnostics;

namespace DNSProfileChecker.Common.Implementation
{
	public sealed class ErroredEventLoger : ILogger
	{
		private const string EventLogName = "Speech Utility Server";

		private string m_eventLogSource;

		public ErroredEventLoger()
		{
			InitEventLogSource("Profile Check");
		}

		private void InitEventLogSource(string eventSourceName)
		{
			Ensure.Argument.NotNullOrEmpty(eventSourceName);

			EventLog eventLog = null;
			try
			{
				bool evtSourceExists = EventLog.SourceExists(eventSourceName);
				if (!evtSourceExists)
				{
					var escd = new EventSourceCreationData(eventSourceName, EventLogName);
					//Log.WriteInfo("Associating event source {0} with log {1}", eventSourceName, EventLogName);
					EventLog.CreateEventSource(eventSourceName, EventLogName);

					//Log.WriteInfo("Modifying Overflow policy on Event Log {0}", EventLogName);
					eventLog = new EventLog(EventLogName, ".", eventSourceName);
					eventLog.ModifyOverflowPolicy(OverflowAction.OverwriteAsNeeded, 0);
				}
			}
			catch (Exception)
			{	//WriteWarning("Failure occured while creating Event Source: sourceName: {0} - EventLogName: {1} - Error: {2}", eventSourceName, EventLogName, exception.Message);
				return;
			}

			if (eventLog == null)
			{	//Log.WriteInfo("Creating Event Log {0} with Event Source {1}", EventLogName, eventSourceName);
				eventLog = new EventLog(EventLogName, ".", eventSourceName);
			}

			m_eventLogSource = eventSourceName;
		}

		public void LogData(LogSeverity severity, string message, Exception ex)
		{
			EventLogEntryType eventType;
			switch (severity)
			{
				case LogSeverity.Error:
					eventType = EventLogEntryType.Error;
					using (var eventLog = new EventLog(EventLogName, ".", m_eventLogSource))
					{
						eventLog.WriteEntry(message, eventType, (int)200, (short)200);
					}
					break;
				case LogSeverity.Fatal:
					eventType = EventLogEntryType.Error;
					using (var eventLog = new EventLog(EventLogName, ".", m_eventLogSource))
					{
						eventLog.WriteEntry(message, eventType, (int)200, (short)201);
					}
					break;

				default:
					break;
			}
		}
	}
}
