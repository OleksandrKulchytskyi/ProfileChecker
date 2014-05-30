using DNSProfileChecker.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuance.Radiology.DNSProfileChecker.Infrastructure.Logger
{
	public sealed class UILogger:ILogger
	{
		public void LogData(LogSeverity severity, string message, Exception ex)
		{
			//throw new NotImplementedException();
		}
	}
}
