using Nuance.Radiology.DNSProfileChecker.Models;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Nuance.Radiology.DNSProfileChecker.Converters
{
	public class DriveIconConverter : IValueConverter
	{
		private static BitmapImage removable;
		private static BitmapImage drive;
		private static BitmapImage netDrive;
		private static BitmapImage cdrom;
		private static BitmapImage ram;
		private static BitmapImage folder;
		private static BitmapImage network;
		private static BitmapImage desktop;

		public DriveIconConverter()
		{
			if (removable == null)
				removable = CreateImage("pack://application:,,,/Nuance.Radiology.DNSProfileChecker;component/Images/shell32_8.ico");

			if (drive == null)
				drive = CreateImage("pack://application:,,,/Nuance.Radiology.DNSProfileChecker;component/Images/shell32_9.ico");

			if (netDrive == null)
				netDrive = CreateImage("pack://application:,,,/Nuance.Radiology.DNSProfileChecker;component/Images/shell32_10.ico");

			if (cdrom == null)
				cdrom = CreateImage("pack://application:,,,/Nuance.Radiology.DNSProfileChecker;component/Images/shell32_12.ico");

			if (ram == null)
				ram = CreateImage("pack://application:,,,/Nuance.Radiology.DNSProfileChecker;component/Images/shell32_303.ico");

			if (folder == null)
				folder = CreateImage("pack://application:,,,/Nuance.Radiology.DNSProfileChecker;component/Images/shell32_264.ico");

			if (network == null)
				network = CreateImage("pack://application:,,,/Nuance.Radiology.DNSProfileChecker;component/Images/_network.ico");

			if (desktop == null)
				desktop = CreateImage("pack://application:,,,/Nuance.Radiology.DNSProfileChecker;component/Images/_desk.ico");
		}

		private BitmapImage CreateImage(string uri)
		{
			BitmapImage img = new BitmapImage();
			img.BeginInit();
			img.UriSource = new Uri(uri);
			img.EndInit();
			return img;
		}

		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var treeItem = value as TreeItem;
			if (treeItem == null)
				throw new ArgumentException("Illegal item type");

			if (treeItem is DriveTreeItem)
			{
				DriveTreeItem driveItem = treeItem as DriveTreeItem;
				switch (driveItem.DriveType)
				{
					case DriveType.CDRom:
						return cdrom;
					case DriveType.Fixed:
						return drive;
					case DriveType.Network:
						return netDrive;
					case DriveType.NoRootDirectory:
						return drive;
					case DriveType.Ram:
						return ram;
					case DriveType.Removable:
						return removable;
					case DriveType.Unknown:
						return drive;
				}
			}
			else if (treeItem is NetworkComputerTreeItem)
				return desktop;
			else if (treeItem is NetworkTreeItem)
				return network;			
			else
				return folder;

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class NullToBoolConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return false;

			return true;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
