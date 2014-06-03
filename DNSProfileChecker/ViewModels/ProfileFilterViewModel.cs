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
		private readonly WorkflowState _state;

		public ProfileFilterViewModel(WorkflowState state)
			: base(state)
		{
			_eventMediator = IoC.Get<IEventAggregator>();
			_state = state;

			SelectedDismiss = new ObservableCollection<ProfileEntry>();
			SelectedAvaliable = new ObservableCollection<ProfileEntry>();

			SelectedDismiss.CollectionChanged += SelectedDismiss_CollectionChanged;
			SelectedAvaliable.CollectionChanged += SelectedAvaliable_CollectionChanged;

			DismissedProfiles = new ObservableCollection<ProfileEntry>();
			ToDismissContent = ">>";
			ToAvaliableContent = "<<";
		}

		void SelectedDismiss_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			NotifyOfPropertyChange(() => CanMoveToAvaliable);
		}

		void SelectedAvaliable_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			NotifyOfPropertyChange(() => CanMoveToDismiss);
		}

		private string toDismiss;
		public string ToDismissContent
		{
			get { return toDismiss; }
			set { toDismiss = value; NotifyOfPropertyChange(() => ToDismissContent); }
		}

		private string toAvaliable;
		public string ToAvaliableContent
		{
			get { return toAvaliable; }
			set { toAvaliable = value; NotifyOfPropertyChange(() => ToAvaliableContent); }
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


		private ObservableCollection<ProfileEntry> selectedDismiss;
		public ObservableCollection<ProfileEntry> SelectedDismiss
		{
			get { return selectedDismiss; }
			set
			{
				selectedDismiss = value;
				NotifyOfPropertyChange(() => SelectedDismiss);
				NotifyOfPropertyChange(() => CanMoveToDismiss);
			}
		}

		private ObservableCollection<ProfileEntry> selectedAvaliable;
		public ObservableCollection<ProfileEntry> SelectedAvaliable
		{
			get { return selectedAvaliable; }
			set
			{
				selectedAvaliable = value;
				NotifyOfPropertyChange(() => SelectedAvaliable);
				NotifyOfPropertyChange(() => CanMoveToAvaliable);
			}
		}



		public async void OnLoaded(DependencyObject obj)
		{
			IDNSSourceProvider provider = IoC.Get<IDNSSourceProvider>();
			List<string> profiles = null;
			try
			{
				profiles = await provider.GetProfiles(_state.SourcePath);
				AvaliableProfiles = new ObservableCollection<ProfileEntry>(profiles.Select(x => new ProfileEntry(x)).OrderBy(p => p.Name));
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
			finally
			{
				NotifyOfPropertyChange(() => AvaliableProfiles);
			}
		}

		public void MoveToDismiss()
		{
			List<ProfileEntry> toProcess = new List<ProfileEntry>(SelectedAvaliable);
			foreach (ProfileEntry pe in toProcess)
			{
				DismissedProfiles.Add(pe);
			}

			foreach (ProfileEntry pe in toProcess)
			{
				AvaliableProfiles.Remove(pe);
			}

			RefreshData();
		}

		public bool CanMoveToDismiss
		{
			get
			{
				return (SelectedAvaliable != null && SelectedAvaliable.Count > 0);
			}
		}

		public void MoveToAvaliable()
		{
			RefreshData();
		}

		public bool CanMoveToAvaliable
		{
			get
			{
				return (SelectedDismiss != null && SelectedDismiss.Count > 0);
			}
		}

		private void RefreshData()
		{
			NotifyOfPropertyChange(() => AvaliableProfiles);
			NotifyOfPropertyChange(() => DismissedProfiles);
		}

		public void GoPrevious()
		{
			FreeSelectedSubscription();
			this.NextTransition = Models.StateTransition.SourceSelector;
			this.TryClose();
		}

		public void GoNext()
		{
			FreeSelectedSubscription();
			this.NextTransition = Models.StateTransition.ProfileFilteringFinished;
			_state.Profiles = AvaliableProfiles.ToList();
			this.TryClose();
		}

		public bool CanGoNext
		{
			get
			{
				return (AvaliableProfiles != null && AvaliableProfiles.Count > 0);
			}
		}

		private void FreeSelectedSubscription()
		{
			SelectedDismiss.CollectionChanged -= SelectedDismiss_CollectionChanged;
			SelectedAvaliable.CollectionChanged -= SelectedAvaliable_CollectionChanged;
		}
	}
}
