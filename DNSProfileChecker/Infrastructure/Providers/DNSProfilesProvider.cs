using DNSProfileChecker.Common;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Nuance.Radiology.DNSProfileChecker.Infrastructure.Providers
{
	public class DNSProfilesProvider : IDNSSourceProvider
	{
		public DNSProfilesProvider()
		{
		}

		public Task<List<string>> GetProfiles(string source, IProfileAssurance assurance)
		{
			Ensure.Argument.NotNull(source, "source");
			TaskCompletionSource<List<string>> cts = new TaskCompletionSource<List<string>>();

			Task<List<string>> main = Task.Factory.StartNew<List<string>>(() =>
			{
				List<string> result = new List<string>();
				DirectoryInfo dir = new DirectoryInfo(source);
				if (dir != null && dir.Exists)
				{
					foreach (DirectoryInfo dirInfo in dir.EnumerateDirectories())
					{
						if (assurance != null)
							if (assurance.IsProfileFolder(dirInfo.FullName))
								result.Add(Path.Combine(source, dirInfo.Name));
							else
								result.Add(Path.Combine(source, dirInfo.Name));
					}
				}
				return result;
			});

			main.ContinueWith(t => { cts.SetException(t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
			main.ContinueWith(t => { cts.SetResult(t.Result); }, TaskContinuationOptions.NotOnFaulted);

			return cts.Task;
		}
	}
}