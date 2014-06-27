using System.IO;

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