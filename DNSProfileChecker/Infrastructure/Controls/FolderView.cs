using System.Windows;
using System.Windows.Controls;

namespace Nuance.Radiology.DNSProfileChecker.Infrastructure.Controls
{
	public class FolderView : ViewBase
	{
		protected override object DefaultStyleKey
		{
			get { return new ComponentResourceKey(GetType(), "FolderView"); }
		}

		protected override object ItemContainerDefaultStyleKey
		{
			get { return new ComponentResourceKey(GetType(), "FolderViewItem"); }
		}
	}
}