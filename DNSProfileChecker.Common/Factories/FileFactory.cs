using System;
using System.IO;

namespace DNSProfileChecker.Common.Factories
{
	public class FileFactory : IFileFactory
	{
		public StreamWriter CreateFile(string fpath, FileFactoryEnum type)
		{
			Ensure.Argument.NotNull(fpath, "file path cannot be a null.");

			StreamWriter sw = File.CreateText(fpath);
			switch (type)
			{
				case FileFactoryEnum.DRAFilesINI:
					sw.WriteLine("[Count]");
					sw.WriteLine("SeqNo=0");
					sw.WriteLine(Environment.NewLine); // guided by the Scott's request, inserting line-brake between the section declarations
					sw.WriteLine("[Files]");
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
