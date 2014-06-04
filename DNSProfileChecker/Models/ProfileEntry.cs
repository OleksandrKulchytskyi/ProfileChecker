using System;
using System.IO;

namespace Nuance.Radiology.DNSProfileChecker.Models
{
	public sealed class ProfileEntry
	{
		private DirectoryInfo _di;
		
		public ProfileEntry(string path)
		{
			_di = new DirectoryInfo(path);
		}

		public String FullPath
		{
			get
			{
				return _di.FullName;
			}
		}

		public String Name
		{
			get
			{
				return _di.Name;
			}
		}

		public bool IsExists
		{
			get { return _di.Exists; }
		}

		public override string ToString()
		{
			return _di.Name;
		}

		public override int GetHashCode()
		{
			return _di.GetHashCode();
		}
	}
}
