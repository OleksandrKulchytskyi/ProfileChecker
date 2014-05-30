using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DNSProfileChecker.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DNSProfileChecker.UnitTest
{
	[TestClass]
	public class ProfilesSourceProviderUnitTest
	{
		[TestMethod]
		public void TestDNSProfilesProvider()
		{
			IDNSSourceProvider provider = new Nuance.Radiology.DNSProfileChecker.Infrastructure.Providers.DNSProfilesProvider();
			Task<List<string>> profilesTask = provider.GetProfiles(@"\\dev804\dragonUsers");
			try
			{
				profilesTask.Wait();
				if (profilesTask.Result != null)
				{					
				}
			}
			catch (AggregateException ex)
			{
				
			}

		}
	}
}
