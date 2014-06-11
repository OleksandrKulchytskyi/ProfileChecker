using System.IO;

namespace DNSProfileChecker.Common
{
	public enum FileFactoryEnum
	{
		DRAFilesINI,
		AcarchiveINI
	}

	public interface IFileFactory
	{
		StreamWriter CreateFile(string fpath, FileFactoryEnum type);
	}
}
