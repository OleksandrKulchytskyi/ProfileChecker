using DNSProfileChecker.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nuance.Radiology.DNSProfileChecker.Infrastructure.Helpers
{
	public static class PathNormalizer
	{
		public static string NormalizePath(string path, bool includeShare = false)
		{
			Ensure.Argument.NotNull(path, "path cannot be a null or empty");

			if (path.StartsWith("Network") && path.Length > 7)
			{
				string result = path.Remove(0, "Network".Length + 1);
				if (includeShare)
					result = result.Insert(0, "\\\\");
				return result;
			}
			else
				return path;
		}
	}
}
