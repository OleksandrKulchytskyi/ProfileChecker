﻿using System;

namespace DNSProfileChecker.Common
{
	public enum LogSeverity
	{
		Info = 0,
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