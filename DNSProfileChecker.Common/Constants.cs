using System.Linq;

namespace DNSProfileChecker.Common
{
	public static class Constants
	{
		private static int _taken = -1;

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
	}
}