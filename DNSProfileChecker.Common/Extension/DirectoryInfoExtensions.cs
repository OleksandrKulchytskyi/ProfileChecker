using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DNSProfileChecker.Common
{
	public static class DirectoryInfoExtensions
	{
		public static long GetFolderSize(this DirectoryInfo di)
		{
			Ensure.Argument.NotNull(di, "di parameter cannot ba a null.");
			if (di.Exists)
			{
				//return di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
				var files = di.GetFiles("*.*", SearchOption.AllDirectories);
				long result = 0;
				foreach (FileInfo fileInfo in files)
				{
					result += fileInfo.Length;
				}
				return result;
			}
			else
				return -1;
		}

		public static long GetDirectorySizeParalell(string sourceDir, bool recurse)
		{
			long size = 0;
			string[] fileEntries = Directory.GetFiles(sourceDir);

			foreach (string fileName in fileEntries)
			{
				Interlocked.Add(ref size, (new FileInfo(fileName)).Length);
			}

			if (recurse)
			{
				string[] subdirEntries = Directory.GetDirectories(sourceDir);

				Parallel.For<long>(0, subdirEntries.Length, () => 0, (i, loop, subtotal) =>
				{
					if ((File.GetAttributes(subdirEntries[i]) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
					{
						subtotal += GetDirectorySizeParalell(subdirEntries[i], true);
						return subtotal;
					}
					return 0;
				},
					(x) => Interlocked.Add(ref size, x)
				);
			}
			return size;
		}

		public static void CopyFrom(this DirectoryInfo destination, DirectoryInfo source)
		{
			//Now Create all of the directories
			foreach (string dirPath in Directory.GetDirectories(source.FullName, "*", SearchOption.AllDirectories))
				Directory.CreateDirectory(dirPath.Replace(source.FullName, destination.FullName));

			//Copy all the files & Replaces any files with the same name
			foreach (string newPath in Directory.GetFiles(source.FullName, "*.*", SearchOption.AllDirectories))
				File.Copy(newPath, newPath.Replace(source.FullName, destination.FullName), true);
		}

		public static void Move(this DirectoryInfo destination, DirectoryInfo source)
		{
			Directory.Move(source.FullName, destination.FullName);
		}
	}
}