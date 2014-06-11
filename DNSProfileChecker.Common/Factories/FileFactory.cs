using System;
using System.IO;

namespace DNSProfileChecker.Common.Factories
{
	public class FileFactory : IFileFactory
	{
		public System.IO.StreamWriter CreateFile(string fpath, FileFactoryEnum type)
		{
			Ensure.Argument.NotNull(fpath, "file path cannot be a null.");

			StreamWriter sw = File.CreateText(fpath);
			switch (type)
			{
				case FileFactoryEnum.DRAFilesINI:
					sw.WriteLine("[Files]");
					sw.WriteLine(Environment.NewLine);
					sw.WriteLine("[Count]");
					sw.WriteLine("SeqNo=0");
					break;

				case FileFactoryEnum.AcarchiveINI:
					sw.WriteLine("[AO Archive]");
					sw.WriteLine("AO Archive Size Millisecs=0");
					sw.WriteLine("AO Enc Archive Size Millisecs=0");
					sw.WriteLine("AO Archive Processed=1");
					break;

				default:
					break;
			}

			return sw;
		}
	}
}
