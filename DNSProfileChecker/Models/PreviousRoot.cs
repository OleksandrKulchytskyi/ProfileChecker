using System;

namespace Nuance.Radiology.DNSProfileChecker.Models
{
	public sealed class SearchResult : Caliburn.Micro.PropertyChangedBase
	{
		private string path;
		public string Path
		{
			get { return path; }
			set
			{
				path = value;
				base.NotifyOfPropertyChange(() => Path);
			}
		}
	}
}
