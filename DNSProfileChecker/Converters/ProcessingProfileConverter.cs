using System;
using System.Windows.Data;

namespace Nuance.Radiology.DNSProfileChecker.Converters
{
	public sealed class ProcessingProfileConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
				return "Current processing profile: NONE";
			return string.Format("Current processing profile: {0}", value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
