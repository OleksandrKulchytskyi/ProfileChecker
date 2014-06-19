using Caliburn.Micro;
using DNSProfileChecker.Common;
using Nuance.Radiology.DNSProfileChecker.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Nuance.Radiology.DNSProfileChecker.ViewModels
{
	public sealed class SourceSelectorViewModel : BaseViewModel
	{
		private readonly WorkflowState _state;
		private readonly ILogger _aggregator;

		public SourceSelectorViewModel(WorkflowState state)
			: base(state)
		{
			_state = state;
			_aggregator = IoC.Get<ILogger>();
			_state.IsProfilesLoaded = false;

			if (_state != null && _state.SourcePath.IsNotNullOrEmpty())
				ProfileSource = _state.SourcePath;

			SearchResults = new ObservableCollection<SearchResult>();
		}

		private ObservableCollection<SearchResult> searchResults;

		public ObservableCollection<SearchResult> SearchResults
		{
			get { return searchResults; }
			set
			{
				searchResults = value;
				NotifyOfPropertyChange(() => SearchResults);
			}
		}

		private string _profileSource;

		public string ProfileSource
		{
			get { return _profileSource; }
			set
			{
				_profileSource = value;
				base.NotifyOfPropertyChange(() => ProfileSource);
				base.NotifyOfPropertyChange(() => CanGoNext);
			}
		}

		TreeItem _selectedFolder;
		public TreeItem SelectedFolder
		{
			get
			{
				return _selectedFolder;
			}
			set
			{
				_selectedFolder = value;
				NotifyOfPropertyChange(() => SelectedFolder);
				ProfileSource = Nuance.Radiology.DNSProfileChecker.Infrastructure.Helpers.PathNormalizer.NormalizePath(_selectedFolder.GetFullPath(), true);
			}
		}

		private SearchResult _Selected;
		public SearchResult SelectedResult
		{
			get { return _Selected; }
			set
			{
				_Selected = value;
				NotifyOfPropertyChange(() => SelectedResult);
				base.NotifyOfPropertyChange(() => CanGoNext);
			}
		}

		public async void OnLoaded(DependencyObject obj)
		{
			FileInfo fi = new FileInfo("searchresults.dat");
			if (fi.Exists)
			{
				using (StreamReader sr = fi.OpenText())
				{
					string line = null;
					while ((line = await sr.ReadLineAsync()) != null)
					{
						SearchResults.Add(new SearchResult() { Path = line });
					}
				}
			}
			else
				using (fi.CreateText()) { }

			NotifyOfPropertyChange(() => SearchResults);
		}

		public void GoNext()
		{
			System.Threading.Tasks.Task.Factory.StartNew(() =>
			{
				SearchResult result;
				if (ProfileSource.IsNotNullOrEmpty() && (result = SearchResults.FirstOrDefault(x => x.Path == ProfileSource)) == null)
				{
					FileInfo fi = new FileInfo("searchresults.dat");
					if (fi.Exists)
					{
						using (StreamWriter sw = fi.AppendText())
						{
							sw.WriteLine(ProfileSource);
						}
					}
				}
			});

			bool proceed = true;
			try
			{
				var di = new DirectoryInfo(ProfileSource);
				if (!di.Exists)
				{
					proceed = false;
					MessageBox.Show(string.Format("Path {0} doesn't exist or remote folder is currently unavailable.", ProfileSource), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
			catch (Exception ex)
			{
				proceed = false;
				_aggregator.LogData(LogSeverity.Error, ex.Message, ex);
			}

			if (!proceed)
				return;

			_aggregator.LogData(LogSeverity.UI, string.Format("Source folder: {0}", ProfileSource), null);
			this.NextTransition = Models.StateTransition.SourceSelectorFinished;
			_state.SourcePath = ProfileSource;
			_state.ClearState();

			if (SelectedResult != null)
			{
				if (ProfileSource != SelectedResult.Path)
					_state.SourcePath = SelectedResult.Path;
			}

			this.TryClose();
		}

		public bool CanGoNext
		{
			get
			{
				return (ProfileSource.IsNotNullOrEmpty() || (SelectedResult != null));
			}
		}
	}
}