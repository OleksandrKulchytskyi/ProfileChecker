using DNSProfileChecker.Common;
using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Nuance.Radiology.DNSProfileChecker.Converters
{
	public class SeverityToImageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			BitmapImage logo = new BitmapImage();
			logo.BeginInit();
			LogSeverity severity = (LogSeverity)value;
			switch (severity)
			{
				case LogSeverity.Info:
					logo.UriSource = new Uri("pack://application:,,,/Nuance.Radiology.DNSProfileChecker;component/Images/iconinfo.png");
					break;

				case LogSeverity.Warn:
					logo.UriSource = new Uri("pack://application:,,,/Nuance.Radiology.DNSProfileChecker;component/Images/warn.ico");
					break;

				case LogSeverity.Error:
					logo.UriSource = new Uri("pack://application:,,,/Nuance.Radiology.DNSProfileChecker;component/Images/error.ico");
					break;

				case LogSeverity.Fatal:
					logo.UriSource = new Uri("pack://application:,,,/Nuance.Radiology.DNSProfileChecker;component/Images/fatal.ico");
					break;

				case LogSeverity.Success:
					logo.UriSource = new Uri("pack://application:,,,/Nuance.Radiology.DNSProfileChecker;component/Images/success.ico");
					break;

				default:
					break;
			}

			logo.EndInit();
			return logo;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}