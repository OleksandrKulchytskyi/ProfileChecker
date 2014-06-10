using Caliburn.Micro;
using Nuance.Radiology.DNSProfileChecker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Common = DNSProfileChecker.Common;

namespace Nuance.Radiology.DNSProfileChecker.ViewModels
{
	public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, Common.Interfaces.IShell, IHandle<Infrastructure.Messages.LogEntry>
	{
		private readonly IEventAggregator _eventAggregator;

		public ShellViewModel(IEventAggregator eventAggregator)
		{
			_eventAggregator = eventAggregator;
			_eventAggregator.Subscribe(this);

			initializeMap();
			activateFirstScreen();
			_logs = new ObservableCollection<Infrastructure.Messages.LogEntry>();
			base.DisplayName = "DNS Profiles Checker";
		}

		private void activateFirstScreen()
		{
			var screen = new SourceSelectorViewModel(new WorkflowState());
			this.ActivateItem(screen);
		}

		private void initializeMap()
		{
			TransitionMap.Add<SourceSelectorViewModel, ProfileFilterViewModel>(StateTransition.SourceSelectorFinished);
			TransitionMap.Add<SourceSelectorViewModel, SourceSelectorViewModel>(StateTransition.Cancel);

			TransitionMap.Add<ProfileFilterViewModel, ProfileOptimizationViewModel>(StateTransition.ProfileFilteringFinished);
			TransitionMap.Add<ProfileFilterViewModel, SourceSelectorViewModel>(StateTransition.SourceSelector);
			TransitionMap.Add<ProfileFilterViewModel, SourceSelectorViewModel>(StateTransition.Cancel);

			TransitionMap.Add<ProfileOptimizationViewModel, ProfileFilterViewModel>(StateTransition.ProfileFiltering);
			TransitionMap.Add<ProfileOptimizationViewModel, SourceSelectorViewModel>(StateTransition.Cancel);
		}

		protected override IScreen DetermineNextItemToActivate(IList<IScreen> list, int lastIndex)
		{
			BaseViewModel theScreenThatJustClosed = list[lastIndex] as BaseViewModel;
			WorkflowState state = theScreenThatJustClosed.WorkflowState;

			Type nextScreenType = TransitionMap.GetNextScreenType(theScreenThatJustClosed);

			return (Activator.CreateInstance(nextScreenType, state) as IScreen);
		}

		public void Handle(Infrastructure.Messages.LogEntry message)
		{
			if (message != null)
			{
				_logs.Add(message);
			}
		}

		private ObservableCollection<Infrastructure.Messages.LogEntry> _logs;

		public ObservableCollection<Infrastructure.Messages.LogEntry> Logs
		{
			get { return _logs; }
			set
			{
				_logs = value;
				base.NotifyOfPropertyChange(() => Logs);
			}
		}
	}
}