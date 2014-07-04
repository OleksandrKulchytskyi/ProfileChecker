
namespace DNSProfileChecker.Common
{
	public static class ConversionHelper
	{

		public static int ConvertToMegabytes(this long bytes)
		{
			if (bytes == 0)
				return 0;

			return (int)((bytes / 1024) / 1024);
		}

		public static int ConvertToBytes(this long Mb)
		{
			if (Mb == 0)
				return 0;

			return (int)((Mb * 1024) * 1024);
		}
	}
}
