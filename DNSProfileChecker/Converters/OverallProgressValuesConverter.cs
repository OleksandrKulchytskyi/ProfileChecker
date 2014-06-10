using System;
using System.Windows.Data;

namespace Nuance.Radiology.DNSProfileChecker.Converters
{
	public sealed class OverallProgressValuesConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (values == null)
				return "Processed: 0 From: 0";
			return string.Format("Processed: {0} From: {1}", values[0], values[1]);
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}