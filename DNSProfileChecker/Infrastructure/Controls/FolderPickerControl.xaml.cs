using Nuance.Radiology.DNSProfileChecker.Infrastructure.Helpers;
using Nuance.Radiology.DNSProfileChecker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Nuance.Radiology.DNSProfileChecker.Infrastructure.Controls
{
	/// <summary>
	/// Interaction logic for FolderPicker.xaml
	/// </summary>
	public partial class FolderPickerControl : UserControl, INotifyPropertyChanged
	{
		#region Constants

		private const string EmptyItemName = "Empty";
		private const string NewFolderName = "New Folder";
		private const int MaxNewFolderSuffix = 10000;

		#endregion

		#region Properties

		public TreeItem Root
		{
			get
			{
				return root;
			}
			private set
			{
				root = value;
				NotifyPropertyChanged(() => Root);
			}
		}

		public TreeItem SelectedItem
		{
			get
			{
				return selectedItem;
			}
			private set
			{
				selectedItem = value;
				NotifyPropertyChanged(() => SelectedItem);
			}
		}

		public string SelectedPath { get; private set; }

		public string InitialPath
		{
			get
			{
				return initialPath;
			}
			set
			{
				initialPath = value;
				UpdateInitialPathUI();
			}
		}

		public Style ItemContainerStyle
		{
			get
			{
				return itemContainerStyle;
			}
			set
			{
				itemContainerStyle = value;
				OnPropertyChanged("ItemContainerStyle");
			}
		}

		public static DependencyProperty SelectedValueProperty = DependencyProperty.Register("SelectedFolder", typeof(TreeItem), typeof(FolderPickerControl), null);

		public TreeItem SelectedFolder
		{
			get { return (TreeItem)GetValue(SelectedValueProperty); }
			set { SetValue(SelectedValueProperty, value); }

		}

		DependencyProperty CursorStateProperty = DependencyProperty.Register(
		"CursorState", typeof(System.Windows.Input.Cursor), typeof(FolderPickerControl), new PropertyMetadata(null));

		public System.Windows.Input.Cursor CursorState
		{
			get { return (System.Windows.Input.Cursor)GetValue(CursorStateProperty); }
			set { SetValue(CursorStateProperty, value); }
		}
		#endregion

		public FolderPickerControl()
		{
			InitializeComponent();

			Init();
		}

		public void CreateNewFolder()
		{
			CreateNewFolderImpl(SelectedItem);
		}

		public void RefreshTree()
		{
			Root = null;
			Init();
		}

		#region INotifyPropertyChanged Members

		public void NotifyPropertyChanged<TProperty>(Expression<Func<TProperty>> property)
		{
			var lambda = (LambdaExpression)property;
			MemberExpression memberExpression;
			if (lambda.Body is UnaryExpression)
			{
				var unaryExpression = (UnaryExpression)lambda.Body;
				memberExpression = (MemberExpression)unaryExpression.Operand;
			}
			else memberExpression = (MemberExpression)lambda.Body;
			OnPropertyChanged(memberExpression.Member.Name);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region Private methods

		private void Init()
		{
			root = new TreeItem("root", null);
			LocalComputerTreeItem localPC = new LocalComputerTreeItem("Computer", root);

			DriveInfo[] systemDrives = DriveInfo.GetDrives();
			foreach (DriveInfo sd in systemDrives)
			{
				var item = new DriveTreeItem(sd.Name, sd.DriveType, localPC);
				item.Childs.Add(new TreeItem(EmptyItemName, item));
				localPC.Childs.Add(item);
			}
			root.Childs.Add(localPC);
			localPC.IsFullyLoaded = true;

			NetworkTreeItem network = new NetworkTreeItem("Network", root);
			network.Childs.Add(new TreeItem(EmptyItemName, network));
			root.Childs.Add(network);

			Root = root; // to notify UI
		}

		private void TreeView_Selected(object sender, RoutedEventArgs e)
		{
			var tvi = e.OriginalSource as TreeViewItem;
			if (tvi != null)
			{
				SelectedItem = tvi.DataContext as TreeItem;
				SelectedFolder = SelectedItem;
				SelectedPath = SelectedItem.GetFullPath();
			}
		}

		private Task<List<String>> ListNetworkComputers()
		{
			TaskCompletionSource<List<String>> task = new TaskCompletionSource<List<string>>();


			Task.Factory.StartNew(() =>
			{
				List<String> computerNames = new List<String>();
				String _ComputerSchema = "Computer";
				System.DirectoryServices.DirectoryEntry WinNTDirectoryEntries = new System.DirectoryServices.DirectoryEntry("WinNT:");
				foreach (System.DirectoryServices.DirectoryEntry AvailDomains in WinNTDirectoryEntries.Children)
				{
					foreach (System.DirectoryServices.DirectoryEntry _PCNameEntry in AvailDomains.Children)
					{
						if (_PCNameEntry.SchemaClassName.ToLower().Contains(_ComputerSchema.ToLower()))
						{
							computerNames.Add(_PCNameEntry.Name);
						}
					}
				}
				task.SetResult(computerNames);
			}).ContinueWith(prev => task.SetException(prev.Exception), TaskContinuationOptions.OnlyOnFaulted);

			return task.Task;
		}

		private async void TreeView_Expanded(object sender, RoutedEventArgs e)
		{
			var tvi = e.OriginalSource as TreeViewItem;
			var treeItem = tvi.DataContext as TreeItem;

			if (treeItem != null)
			{
				if (!treeItem.IsFullyLoaded)
				{
					treeItem.Childs.Clear();

					string path = treeItem.GetFullPath();

					if (treeItem is NetworkTreeItem)
					{
						try
						{
							CursorState = System.Windows.Input.Cursors.Wait;
							List<string> pcs = await ListNetworkComputers();
							foreach (string pc in pcs)
							{
								NetworkComputerTreeItem item = new NetworkComputerTreeItem(pc, treeItem);
								item.Childs.Add(new TreeItem(EmptyItemName, item));
								treeItem.Childs.Add(item);
							}
						}
						catch (Exception ex) { MessageBox.Show(ex.Message); }
						finally { CursorState = null; }

						treeItem.IsFullyLoaded = true;
						return;
					}
					else if (treeItem is NetworkComputerTreeItem)
					{
						ShareCollection shi = null;
						try
						{
							CursorState = System.Windows.Input.Cursors.Wait;
							shi = await ShareCollection.GetSharesAsync(PathNormalizer.NormalizePath(path, true));
						}
						catch { }
						finally { CursorState = null; }
						if (shi != null)
						{
							foreach (Share si in shi)
							{
								if (si.ShareType != ShareType.Disk)
									continue;
								TreeItem item = new TreeItem(si.NetName, treeItem);
								item.Childs.Add(new TreeItem(EmptyItemName, item));
								treeItem.Childs.Add(item);
							}
							treeItem.IsFullyLoaded = true;
						}
						return;
					}
					else
					{
						DirectoryInfo dir = new DirectoryInfo(PathNormalizer.NormalizePath(path, true));
						try
						{
							var subDirs = dir.GetDirectories();
							foreach (var sd in subDirs)
							{
								TreeItem item = new TreeItem(sd.Name, treeItem);
								item.Childs.Add(new TreeItem(EmptyItemName, item));

								treeItem.Childs.Add(item);
							}
						}
						catch { }

						treeItem.IsFullyLoaded = true;
					}
				}
			}
			else
				throw new Exception();
		}

		private void UpdateInitialPathUI()
		{
			if (!Directory.Exists(InitialPath))
				return;

			var initialDir = new DirectoryInfo(InitialPath);

			if (!initialDir.Exists)
				return;

			var stack = TraverseUpToRoot(initialDir);
			ItemContainerGenerator containerGenerator = TreeView.ItemContainerGenerator;
			var uiContext = TaskScheduler.FromCurrentSynchronizationContext();
			DirectoryInfo currentDir = null;
			var dirContainer = Root;

			AutoResetEvent waitEvent = new AutoResetEvent(true);

			Task processStackTask = Task.Factory.StartNew(() =>
			{
				while (stack.Count > 0)
				{
					waitEvent.WaitOne();

					currentDir = stack.Pop();

					Task waitGeneratorTask = Task.Factory.StartNew(() =>
					{
						if (containerGenerator == null) return;

						while (containerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
							Thread.Sleep(50);

					}, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);

					Task updateUiTask = waitGeneratorTask.ContinueWith((r) =>
					{
						try
						{
							var childItem = dirContainer.Childs.Where(c => c.Name == currentDir.Name).FirstOrDefault();
							var tvi = containerGenerator.ContainerFromItem(childItem) as TreeViewItem;
							dirContainer = tvi.DataContext as TreeItem;
							tvi.IsExpanded = true;

							tvi.Focus();

							containerGenerator = tvi.ItemContainerGenerator;
						}
						catch { }

						waitEvent.Set();
					}, uiContext);
				}

			}, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
		}

		private Stack<DirectoryInfo> TraverseUpToRoot(DirectoryInfo child)
		{
			if (child == null) return null;

			if (!child.Exists) return null;

			Stack<DirectoryInfo> queue = new Stack<DirectoryInfo>();
			queue.Push(child);
			DirectoryInfo ti = child.Parent;

			while (ti != null)
			{
				queue.Push(ti);
				ti = ti.Parent;
			}

			return queue;
		}

		private void CreateNewFolderImpl(TreeItem parent)
		{
			try
			{
				if (parent == null) return;

				var parentPath = parent.GetFullPath();
				var newDirName = GenerateNewFolderName(parentPath);
				var newPath = System.IO.Path.Combine(parentPath, newDirName);

				Directory.CreateDirectory(newPath);

				var childs = parent.Childs;
				var newChild = new TreeItem(newDirName, parent);
				childs.Add(newChild);
				parent.Childs = childs.OrderBy(c => c.Name).ToObservableCollection();
			}
			catch (Exception ex)
			{
				MessageBox.Show(String.Format("Can't create new folder. Error: {0}", ex.Message));
			}
		}

		private string GenerateNewFolderName(string parentPath)
		{
			string result = NewFolderName;

			if (Directory.Exists(System.IO.Path.Combine(parentPath, result)))
			{
				for (int i = 1; i < MaxNewFolderSuffix; ++i)
				{
					var nameWithIndex = String.Format(NewFolderName + " {0}", i);

					if (!Directory.Exists(System.IO.Path.Combine(parentPath, nameWithIndex)))
					{
						result = nameWithIndex;
						break;
					}
				}
			}
			return result;
		}

		private void CreateMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var item = sender as MenuItem;
			if (item != null)
			{
				var context = item.DataContext as TreeItem;
				CreateNewFolderImpl(context);
			}
		}

		private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var item = sender as MenuItem;
				if (item != null)
				{
					var context = item.DataContext as TreeItem;
					if (context != null && !(context is DriveTreeItem))
					{
						var confirmed =
							MessageBox.Show(string.Format("Do you really want to delete folder {0}?", context.Name), "Confirm folder removal", MessageBoxButton.YesNo);

						if (confirmed == MessageBoxResult.Yes)
						{
							Directory.Delete(context.GetFullPath());
							var parent = context.Parent;
							parent.Childs.Remove(context);
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(String.Format("Can't delete folder. Error: {0}", ex.Message));
			}
		}

		#endregion

		#region private fields

		private TreeItem root;
		private TreeItem selectedItem;
		private string initialPath;
		private Style itemContainerStyle;

		#endregion
	}
}
