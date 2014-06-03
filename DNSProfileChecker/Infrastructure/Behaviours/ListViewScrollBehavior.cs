using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Nuance.Radiology.DNSProfileChecker.Infrastructure.Behaviours
{
	public class ListViewScrollBehavior
	{
		static Dictionary<ListView, Capture> Associations = new Dictionary<ListView, Capture>();

		public static bool GetScrollOnNewItem(DependencyObject obj)
		{
			return (bool)obj.GetValue(ScrollOnNewItemProperty);
		}

		public static void SetScrollOnNewItem(DependencyObject obj, bool value)
		{
			obj.SetValue(ScrollOnNewItemProperty, value);
		}

		public static readonly DependencyProperty ScrollOnNewItemProperty =
			DependencyProperty.RegisterAttached("ScrollOnNewItem", typeof(bool),
				typeof(ListViewScrollBehavior), new UIPropertyMetadata(false, OnScrollOnNewItemChanged));

		public static void OnScrollOnNewItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var listBox = d as ListView;
			if (listBox == null) return;
			bool oldValue = (bool)e.OldValue, newValue = (bool)e.NewValue;
			if (newValue == oldValue) return;
			if (newValue)
			{
				listBox.Loaded += new RoutedEventHandler(ListBox_Loaded);
				listBox.Unloaded += new RoutedEventHandler(ListBox_Unloaded);
			}
			else
			{
				listBox.Loaded -= ListBox_Loaded;
				listBox.Unloaded -= ListBox_Unloaded;
				if (Associations.ContainsKey(listBox))
					Associations[listBox].Dispose();
			}
		}

		static void ListBox_Unloaded(object sender, RoutedEventArgs e)
		{
			var listBox = (ListView)sender;
			if (Associations.ContainsKey(listBox))
				Associations[listBox].Dispose();
			listBox.Unloaded -= ListBox_Unloaded;
		}

		static void ListBox_Loaded(object sender, RoutedEventArgs e)
		{
			var listBox = (ListView)sender;
			var incc = listBox.Items as INotifyCollectionChanged;
			if (incc == null) return;
			listBox.Loaded -= ListBox_Loaded;
			Associations[listBox] = new Capture(listBox);
		}

		class Capture : IDisposable
		{
			public ListView listBox { get; set; }
			public INotifyCollectionChanged incc { get; set; }

			public Capture(ListView listBox)
			{
				this.listBox = listBox;
				incc = listBox.ItemsSource as INotifyCollectionChanged;
				if (incc != null)
				{
					incc.CollectionChanged +=
						new NotifyCollectionChangedEventHandler(incc_CollectionChanged);
				}
			}

			void incc_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				if (e.Action == NotifyCollectionChangedAction.Add)
				{
					listBox.ScrollIntoView(e.NewItems[0]);
					listBox.SelectedItem = e.NewItems[0];
				}
			}

			public void Dispose()
			{
				if (incc != null)
					incc.CollectionChanged -= incc_CollectionChanged;
			}
		}
	}
}
