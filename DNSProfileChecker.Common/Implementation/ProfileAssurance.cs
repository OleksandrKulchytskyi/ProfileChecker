using System.IO;
using System.Linq;

namespace DNSProfileChecker.Common.Implementation
{
	public sealed class ProfileAssurance : IProfileAssurance
	{
		public bool IsProfileFolder(string profilePath)
		{
			Ensure.Argument.NotNull(profilePath, "profilePath cannot be empty.");
			DirectoryInfo di = new DirectoryInfo(profilePath);
			if (!di.Exists)
				return false;

			return di.EnumerateDirectories("current", SearchOption.TopDirectoryOnly).FirstOrDefault() != null;
		}
	}
}
