using System;
using System.Collections.Generic;

namespace DNSProfileChecker.Common
{
	public interface IServiceProvider
	{
		Dictionary<Type, Tuple<Type,int>> GetServices();
	}
}
