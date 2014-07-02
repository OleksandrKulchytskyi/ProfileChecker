using System.Linq;

namespace DNSProfileChecker.Common
{
	public static class Constants
	{
		private static int _taken = -1;
		private static int _takenPruning = -1;

		/// <summary>
		/// Gets the limit for container folder
		/// </summary>
		public static int FolderLimitSize
		{
			get
			{
				if (_taken == -1)
				{
					if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains("containerLimitSize"))
					{
						string value = System.Configuration.ConfigurationManager.AppSettings["containerLimitSize"];
						int.TryParse(value, out _taken);
					}
					else
						_taken = 1024 * 1024 * 500;//set default value to 500mb
				}
				return _taken;
			}
		}

		/// <summary>
		/// / Gets the threshold for the disctation folder size to end the pruning workflow.
		/// </summary>
		public static int EndPruningThreshold
		{
			get
			{
				if (_takenPruning == -1)
				{
					if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains("endPruningThreshold"))
					{
						string value = System.Configuration.ConfigurationManager.AppSettings["endPruningThreshold"];
						int.TryParse(value, out _takenPruning);
					}
					else
						_takenPruning = 1024 * 1024 * 300;//set default value to 500mb
				}
				return _takenPruning;
			}
		}
	}
}