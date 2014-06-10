using System.Collections.Generic;
using System.Threading.Tasks;

namespace DNSProfileChecker.Common
{
	public interface IDNSSourceProvider
	{
		Task<List<string>> GetProfiles(string source);
	}
}