using Nuance.Radiology.DNSProfileChecker.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using Common = DNSProfileChecker.Common;

namespace Nuance.Radiology.DNSProfileChecker.Infrastructure.Providers
{
	public sealed class AppConfigServiceProvider : Common.IServiceProvider
	{
		public AppConfigServiceProvider()
		{
		}

		public Dictionary<Type, Tuple<Type, int>> GetServices()
		{
			Dictionary<Type, Tuple<Type, int>> services = new Dictionary<Type, Tuple<Type, int>>();
			ServiceProviderConfig config = null;
			try
			{
				config = ServiceProviderConfig.GetConfig();
			}
			catch (Exception ex)
			{
				throw;
			}
			foreach (Service item in config.Services)
			{
				Type key = Type.GetType(item.bind);
				Tuple<Type, int> data = new Tuple<Type, int>(Type.GetType(item.to), item.singleton);
				if (!services.ContainsKey(key))
					services[key] = data;
			}

			return services;
		}
	}
}