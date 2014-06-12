using System;
using System.IO;

namespace Nuance.Radiology.DNSProfileChecker.Models
{
	public sealed class ProfileEntry : Caliburn.Micro.PropertyChangedBase
	{
		private DirectoryInfo _di;

		public ProfileEntry(string path)
		{
			_di = new DirectoryInfo(path);
			NotifyOfPropertyChange(() => Name);
			NotifyOfPropertyChange(() => FullPath);
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

		private bool selected;
		public bool IsSelected
		{
			get { return selected; }
			set
			{
				selected = value;
				base.NotifyOfPropertyChange(() => IsSelected);
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