using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DNSProfileChecker.Common;
using System.IO;
namespace DNSProfileChecker.UnitTest
{
	[TestClass]
	public class IniParserUnitTest
	{
		private static string file = Environment.GetEnvironmentVariable("Temp") + "\\test.ini";

		[ClassInitialize]
		public static void InitTestSuite(TestContext testContext)
		{
			using (StreamWriter sw = File.CreateText(file))
			{
				sw.WriteLine("[General]");
				sw.WriteLine("NextTopicID=2");
				sw.WriteLine(Environment.NewLine);
				sw.WriteLine("[Topics]");
				sw.WriteLine("US English | Large | PS360 Radiology=US_Engli");
				sw.WriteLine("US English | Large | PS360 Radiology=US_Engli");
			}
		}

		[TestMethod]
		public void TestMethod1()
		{
			var data=IniFileParser.GetSingleSection(file, "Topics");
			Assert.IsTrue(data["Topics"].Count == 2);
		}

		[ClassCleanup]
		public static void CleanUp()
		{
			File.Delete(file);
		}
	}
}
