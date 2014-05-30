using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSProfileChecker.Common
{
	public interface ILogProvider
	{
		object Settings { get; set; }
		Dictionary<string, ILogger> GetLoggers();
	}
}
