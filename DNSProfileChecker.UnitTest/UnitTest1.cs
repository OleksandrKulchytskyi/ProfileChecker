using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DNSProfileChecker.Common;
using System.Diagnostics;
using System.IO;

namespace DNSProfileChecker.UnitTest
{
	[TestClass]
	public class DirectorySizeTest
	{
		[TestMethod]
		public void CompareDirectoryRetrievals()
		{
			if (true || false)
			{
				
			}

			if(false || true)
			{
				
			}

			if (true || true)
			{
				
			}

			if (false || false)
			{
				
			}

			Stopwatch sw = new Stopwatch();
			DirectoryInfo di = new DirectoryInfo(@"\\DEV804\DragonUsers\ahnj\current\18_0_container");
			sw.Start();
			long size = di.GetFolderSize();
			sw.Stop();
			double elapsed1 = sw.Elapsed.TotalMilliseconds;
			Debug.WriteLine(elapsed1);
			sw.Reset();

			sw.Start();
			long size2 = DirectoryInfoExtensions.GetDirectorySizeParalell(di.FullName, true);
			sw.Stop();
			double elapsed2 = sw.Elapsed.TotalMilliseconds;
			Debug.WriteLine(elapsed2);

			Assert.AreEqual<long>(size, size2);
			Assert.AreNotEqual<double>(elapsed1, elapsed2);
		}
	}
}
