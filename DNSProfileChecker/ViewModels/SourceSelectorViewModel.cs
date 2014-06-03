using Caliburn.Micro;
using DNSProfileChecker.Common;
using Nuance.Radiology.DNSProfileChecker.Models;
using System;

namespace Nuance.Radiology.DNSProfileChecker.ViewModels
{
	public sealed class SourceSelectorViewModel : BaseViewModel
	{
		private readonly WorkflowState _state;
		private readonly IEventAggregator _aggregator;
		public SourceSelectorViewModel(WorkflowState state)
			: base(state)
		{
			_state = state;
			if (_state.SourcePath.IsNotNullOrEmpty())
				ProfileSource = _state.SourcePath;

			_aggregator = IoC.Get<IEventAggregator>();
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
					System.Windows.MessageBox.Show(string.Format("Path {0} doesn't exist.", ProfileSource), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
				}
			}
			catch (Exception ex)
			{
				proceed = false;
				_aggregator.PublishOnUIThread(new Infrastructure.Messages.LogEntry() { Severity = LogSeverity.Error, Message = ex.Message, Error = ex });
			}

			if (!proceed)
				return;

			proceed = false;
			_aggregator.PublishOnUIThread(new Infrastructure.Messages.LogEntry() { Severity = LogSeverity.Info, Message = string.Format("Source folder: {0}", ProfileSource) });

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
	}
}
