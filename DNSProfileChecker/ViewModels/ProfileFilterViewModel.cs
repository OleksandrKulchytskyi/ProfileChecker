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
		private WorkflowState _state;
		private readonly ILogger _logger;

		public ProfileFilterViewModel(WorkflowState state)
			: base(state)
		{
			_logger = IoC.Get<ILogger>();
			_state = state;

			SelectedToCheck = new ObservableCollection<ProfileEntry>();
			SelectedAvaliable = new ObservableCollection<ProfileEntry>();

			SelectedToCheck.CollectionChanged += SelectedToCheck_CollectionChanged;
			SelectedAvaliable.CollectionChanged += SelectedAvaliable_CollectionChanged;

			if (_state.IsProfilesLoaded && _state.PreviouslySelectedProfiles != null)
			{
				foreach (ProfileEntry item in _state.PreviouslySelectedProfiles)
				{
					SelectedAvaliable.Add(item);
				}
			}

			ProfilesToCheck = new ObservableCollection<ProfileEntry>();
			ToCheckContent = ">>";
			ToAvaliableContent = "<<";
		}

		private void SelectedToCheck_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			NotifyOfPropertyChange(() => CanMoveToAvaliable);
			NotifyOfPropertyChange(() => CanGoNext);
		}

		private void SelectedAvaliable_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			NotifyOfPropertyChange(() => CanMoveToCheck);
			NotifyOfPropertyChange(() => CanGoNext);
			NotifyOfPropertyChange(() => CanSelectAll);
			NotifyOfPropertyChange(() => CanDeselectAll);
		}

		private string toDismiss;

		public string ToCheckContent
		{
			get { return toDismiss; }
			set { toDismiss = value; NotifyOfPropertyChange(() => ToCheckContent); }
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
				NotifyOfPropertyChange(() => ProfilesToCheck);
				NotifyOfPropertyChange(() => CanGoNext);
			}
		}

		private ObservableCollection<ProfileEntry> _dismissed;

		public ObservableCollection<ProfileEntry> ProfilesToCheck
		{
			get { return _dismissed; }
			set
			{
				_dismissed = value;
				NotifyOfPropertyChange(() => ProfilesToCheck);
				NotifyOfPropertyChange(() => CanGoNext);
			}
		}

		private ObservableCollection<ProfileEntry> selectedDismiss;

		public ObservableCollection<ProfileEntry> SelectedToCheck
		{
			get { return selectedDismiss; }
			set
			{
				selectedDismiss = value;
				NotifyOfPropertyChange(() => SelectedToCheck);
				NotifyOfPropertyChange(() => CanMoveToCheck);
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
				if (!_state.IsProfilesLoaded)
				{
					IProfileAssurance assurance = IoC.Get<IProfileAssurance>();
					profiles = await provider.GetProfiles(_state.SourcePath, assurance);
					AvaliableProfiles = new ObservableCollection<ProfileEntry>(profiles.Select(x => new ProfileEntry(x)).OrderBy(p => p.Name));
					_state.IsProfilesLoaded = true;

					_logger.LogData(LogSeverity.UI, string.Format("Retrieved {0} profile(s)", AvaliableProfiles.Count), null);
				}
				else
				{
					AvaliableProfiles = new ObservableCollection<ProfileEntry>(_state.OldAvaliable.OrderBy(p => p.Name));
					ProfilesToCheck = new ObservableCollection<ProfileEntry>(_state.ProfilesToCheck);
				}
			}
			catch (Exception ex)
			{
				_state.IsProfilesLoaded = false;
				_logger.LogData(LogSeverity.Error, "Error has been ocurred during retrieving profiles.", ex);
			}
			finally
			{
				RefreshUIData();
			}
		}

		public void MoveToCheck()
		{
			List<ProfileEntry> toProcess = new List<ProfileEntry>(SelectedAvaliable);
			foreach (ProfileEntry pe in toProcess)
			{
				ProfilesToCheck.Add(pe);
			}

			foreach (ProfileEntry pe in toProcess)
			{
				AvaliableProfiles.Remove(pe);
				pe.IsSelected = false;
			}

			RefreshUIData();
		}

		public bool CanMoveToCheck
		{
			get
			{
				return (SelectedAvaliable != null && SelectedAvaliable.Count > 0);
			}
		}

		public void MoveToAvaliable()
		{
			List<ProfileEntry> toProcess = new List<ProfileEntry>(SelectedToCheck);

			foreach (ProfileEntry pe in toProcess)
			{
				AvaliableProfiles.Add(pe);
				pe.IsSelected = false;
			}
			foreach (ProfileEntry pe in toProcess)
			{
				ProfilesToCheck.Remove(pe);
			}

			RefreshUIData();
		}

		public bool CanMoveToAvaliable
		{
			get
			{
				return (SelectedToCheck != null && SelectedToCheck.Count > 0);
			}
		}

		private void RefreshUIData()
		{
			NotifyOfPropertyChange(() => AvaliableProfiles);
			NotifyOfPropertyChange(() => ProfilesToCheck);
			NotifyOfPropertyChange(() => CanSelectAll);
			NotifyOfPropertyChange(() => CanDeselectAll);
		}

		public void GoPrevious()
		{
			FreeSelectedSubscription();
			this.NextTransition = Models.StateTransition.SourceSelector;
			this.WorkflowState = _state;
			this.TryClose();
		}

		public void GoNext()
		{
			FreeSelectedSubscription();

			this.NextTransition = Models.StateTransition.ProfileFilteringFinished;
			this.WorkflowState = _state;

			_state.ProfilesToCheck = ProfilesToCheck.ToList();
			_state.OldAvaliable = AvaliableProfiles.ToList();
			_state.PreviouslySelectedProfiles = SelectedAvaliable.ToList();
			
			this.TryClose();
		}

		public bool CanGoNext
		{
			get
			{
				return (ProfilesToCheck != null && ProfilesToCheck.Count > 0);
			}
		}

		public void SelectAll()
		{
			if (SelectedAvaliable.Count > 0)
				SelectedAvaliable.Clear();

			foreach (ProfileEntry pi in AvaliableProfiles)
				SelectedAvaliable.Add(pi);

			RefreshUIData();
		}

		public bool CanSelectAll
		{
			get { return (AvaliableProfiles != null && AvaliableProfiles.Count > 0 && (AvaliableProfiles.Count != SelectedAvaliable.Count)); }
		}

		public void DeselectAll()
		{
			if (SelectedAvaliable != null && SelectedAvaliable.Count > 0)
				SelectedAvaliable.Clear();

			RefreshUIData();
		}

		public bool CanDeselectAll
		{
			get { return (SelectedAvaliable != null && SelectedAvaliable.Count > 0); }
		}

		private void FreeSelectedSubscription()
		{
			SelectedToCheck.CollectionChanged -= SelectedToCheck_CollectionChanged;
			SelectedAvaliable.CollectionChanged -= SelectedAvaliable_CollectionChanged;
		}
	}
}