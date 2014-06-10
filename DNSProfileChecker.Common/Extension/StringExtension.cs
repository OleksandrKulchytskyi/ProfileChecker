using System.Text.RegularExpressions;

namespace DNSProfileChecker.Common
{
	public static class StringExtension
	{
		public static System.Text.RegularExpressions.Regex RegexSection { get; private set; }

		static StringExtension()
		{
			RegexSection = new Regex(@"(?ms)^\[[^]\r\n]+](?:(?!^\[[^]\r\n]+]).)*", System.Text.RegularExpressions.RegexOptions.Compiled);
		}

		public static bool IsNotNullOrEmpty(this string value)
		{
			return !value.IsNullOrEmpty();
		}

		public static bool IsNullOrEmpty(this string value)
		{
			return string.IsNullOrEmpty(value);
		}

		public static bool StartsWithIgnoreSpaces(this string source, string search, bool ignoreCase = true)
		{
			if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(search))
				return false;
			else
			{
				int indx = 0;
				while (char.IsWhiteSpace(source[indx]) && (indx != source.Length - 1))
				{
					indx++;
				}
				bool result = true;

				if (ignoreCase)
				{
					for (int i = indx, j = 0; i < source.Length; i++, j++)
					{
						if (char.ToLowerInvariant(source[i]) != char.ToLowerInvariant(search[j]))
						{
							result = false;
							break;
						}
						if (j == search.Length - 1)
							break;
					}
				}
				else
				{
					for (int i = indx, j = 0; i < source.Length; i++, j++)
					{
						if (source[i] != search[j])
						{
							result = false;
							break;
						}
						if (j == search.Length - 1)
							break;
					}
				}
				return result;
			}
		}

		public static bool EndsWithIgnoreSpaces(this string source, string search, bool ignoreCase = true)
		{
			if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(search))
				return false;
			else
			{
				int indx = source.Length - 1;
				while (char.IsWhiteSpace(source[indx]) && (indx != 0))
				{
					indx--;
				}
				bool result = true;
				if (ignoreCase)
				{
					for (int i = indx, j = search.Length - 1; j != -1; i--, j--)
					{
						if (char.ToLowerInvariant(source[i]) != char.ToLowerInvariant(search[j]))
						{
							result = false;
							break;
						}
					}
				}
				else
				{
					for (int i = indx, j = search.Length - 1; j != -1; i--, j--)
					{
						if (source[i] != search[j])
						{
							result = false;
							break;
						}
					}
				}
				return result;
			}
		}
	}
}