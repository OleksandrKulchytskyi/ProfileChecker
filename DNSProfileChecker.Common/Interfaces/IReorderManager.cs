using System;
using System.IO;

namespace DNSProfileChecker.Common
{
	public interface IReorderManager
	{
		bool Reorder(DirectoryInfo[] folders);

		AggregateException Errors { get; }
	}
}