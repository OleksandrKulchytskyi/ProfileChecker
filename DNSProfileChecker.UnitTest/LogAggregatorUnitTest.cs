using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DNSProfileChecker.Common;
using DNSProfileChecker.Common.Implementation;

namespace DNSProfileChecker.UnitTest
{
	[TestClass]
	public class LogAggregatorUnitTest
	{
		[TestMethod]
		public void TestAppConfigLogAggregator()
		{
			ILogProvider logger = new AppConfigLogProvider();
			logger.GetLoggers();
		}
	}
}
