using DNSProfileChecker.Common;
using log4net;
using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace DNSProfileChecker.log4NetLogger
{
	public sealed class Log4NetLogger : ILogger, IDisposable
	{
		private log4net.ILog _logger;

		public Log4NetLogger()
		{
			try
			{
				_logger = LogManager.GetLogger(typeof(Log4NetLogger).FullName);
				log4net.Config.XmlConfigurator.Configure();
			}
			catch (Exception ex)
			{
				throw;
			}			
		}

		public void LogData(LogSeverity severity, string message, Exception ex)
		{
			switch (severity)
			{
				case LogSeverity.Success:
				case LogSeverity.Info:
					_logger.Info(message);
					break;
				case LogSeverity.Warn:
					if (ex == null)
						_logger.Warn(message);
					_logger.Warn(message, ex);
					break;
				case LogSeverity.Error:
					if (ex == null)
						_logger.Error(message);
					_logger.Error(message, ex);
					break;
				case LogSeverity.Fatal:
					if (ex == null)
						_logger.Fatal(message);
					_logger.Fatal(message, ex);
					break;
				default:
					break;
			}
		}

		private string GetExceptionData( Exception exception)
		{
			var properties = exception.GetType().GetProperties();
			var fields = properties
							 .Select(property => new
							 {
								 Name = property.Name,
								 Value = property.GetValue(exception, null)
							 })
							 .Select(x => String.Format("{0} = {1}", x.Name, x.Value != null ? x.Value.ToString() : String.Empty));
			return String.Join("\n", fields);
		}

		private void WriteExceptionDetails(Exception exception, StringBuilder builderToFill, int level)
		{
			var indent = new string(' ', level);

			if (level > 0)
			{
				builderToFill.AppendLine(indent + "=== INNER EXCEPTION ===");
			}

			Action<string> append = (prop) =>
			{
				var propInfo = exception.GetType().GetProperty(prop);
				var val = propInfo.GetValue(exception);

				if (val != null)
				{
					builderToFill.AppendFormat("{0}{1}: {2}{3}", indent, prop, val.ToString(), Environment.NewLine);
				}
			};

			append("Message");
			append("HResult");
			append("HelpLink");
			append("Source");
			append("StackTrace");
			append("TargetSite");

			foreach (DictionaryEntry de in exception.Data)
			{
				builderToFill.AppendFormat("{0} {1} = {2}{3}", indent, de.Key, de.Value, Environment.NewLine);
			}

			if (exception.InnerException != null)
			{
				WriteExceptionDetails(exception.InnerException, builderToFill, ++level);
			}
		}

		private string GetExceptionDetails(Exception ex)
		{
			StringBuilder builder = new StringBuilder();
			WriteExceptionDetails(ex, builder, 0);
			return builder.ToString();
		}

		public void Dispose()
		{
			LogManager.Shutdown();
		}
	}

}
