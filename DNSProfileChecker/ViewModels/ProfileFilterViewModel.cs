using Caliburn.Micro;
using DNSProfileChecker.Common;
using Nuance.Radiology.DNSProfileChecker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Nuance.Radiology.DNSProfileChecker.ViewModels
{
	public sealed class ProfileFilterViewModel : BaseViewModel
	{
		private readonly IEventAggregator _eventMediator;
		readonly WorkflowState _state;

		public ProfileFilterViewModel(WorkflowState state)
			: base(state)
		{
			_eventMediator = IoC.Get<IEventAggregator>();
			_state = state;
			DismissedProfiles = new ObservableCollection<ProfileEntry>();
		}

		private ObservableCollection<ProfileEntry> _profiles;
		public ObservableCollection<ProfileEntry> AvaliableProfiles
		{
			get { return _profiles; }
			set
			{
				_profiles = value;
				NotifyOfPropertyChange(() => DismissedProfiles);
				NotifyOfPropertyChange(() => CanGoNext);
			}
		}

		private ObservableCollection<ProfileEntry> _dismissed;
		public ObservableCollection<ProfileEntry> DismissedProfiles
		{
			get { return _dismissed; }
			set
			{
				_dismissed = value;
				NotifyOfPropertyChange(() => DismissedProfiles);
				NotifyOfPropertyChange(() => CanGoNext);
			}
		}

		public async void OnLoaded(DependencyObject obj)
		{
			IDNSSourceProvider provider = IoC.Get<IDNSSourceProvider>();
			List<string> profiles = null;
			try
			{
				profiles = await provider.GetProfiles(_state.SourcePath);
				AvaliableProfiles = new ObservableCollection<ProfileEntry>(profiles.Select(x => new ProfileEntry(x)));
				_eventMediator.PublishOnUIThread(new Infrastructure.Messages.LogEntry()
				{
					Severity = LogSeverity.Info,
					Message = string.Format("Retrieved {0} profile(s)", AvaliableProfiles.Count)
				});
			}
			catch (Exception ex)
			{
				_eventMediator.PublishOnUIThread(new Infrastructure.Messages.LogEntry() { Severity = LogSeverity.Error, Message = "Error has been ocurred during retrieving profiles.", Error = ex });
			}
		}

		public void GoPrevious()
		{
			this.NextTransition = Models.StateTransition.SourceSelector;
			this.TryClose();
		}

		public void GoNext()
		{
			this.NextTransition = Models.StateTransition.ProfileFilteringFinished;
			this.TryClose();
		}

		public bool CanGoNext
		{
			get
			{
				return (AvaliableProfiles != null && AvaliableProfiles.Count > 0);
			}
		}
	}
}
