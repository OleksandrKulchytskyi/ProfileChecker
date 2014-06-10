using Caliburn.Micro;
using DNSProfileChecker.Common;
using Nuance.Radiology.DNSProfileChecker.Models;
using System;

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

			if (_state != null && _state.SourcePath.IsNotNullOrEmpty())
				ProfileSource = _state.SourcePath;
			else
#if DEBUG
				ProfileSource = @"\\dev804\dragonusers";
#endif
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

		public void GoNext()
		{
			bool proceed = true;
			try
			{
				var di = new System.IO.DirectoryInfo(ProfileSource);
				if (!di.Exists)
				{
					proceed = false;
					System.Windows.MessageBox.Show(string.Format("Path {0} doesn't exist or remote folder is currently unavailable.", ProfileSource), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
				}
			}
			catch (Exception ex)
			{
				proceed = false;
				_aggregator.LogData(LogSeverity.Error, ex.Message, ex);
			}

			if (!proceed)
				return;

			proceed = false;
			_aggregator.LogData(LogSeverity.Info, string.Format("Source folder: {0}", ProfileSource), null);

			this.NextTransition = Models.StateTransition.SourceSelectorFinished;
			_state.SourcePath = ProfileSource;
			this.TryClose();
		}

		public bool CanGoNext
		{
			get
			{
				return ProfileSource.IsNotNullOrEmpty();
			}
		}

		public void Browse()
		{
			var dialog = new System.Windows.Forms.FolderBrowserDialog();
			System.Windows.Forms.DialogResult result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
				this.ProfileSource = dialog.SelectedPath;
		}
	}
}