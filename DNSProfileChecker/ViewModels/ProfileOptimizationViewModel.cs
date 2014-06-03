using Caliburn.Micro;
using DNSProfileChecker.Common;
using Nuance.Radiology.DNSProfileChecker.Infrastructure.Messages;
using Nuance.Radiology.DNSProfileChecker.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Nuance.Radiology.DNSProfileChecker.ViewModels
{
	public sealed class ProfileOptimizationViewModel : BaseViewModel
	{
		private readonly ConcurrentQueue<ProfileEntry> toProcess;
		private volatile bool isStarted = false;
		
		readonly IWorkflowProvider _provider;
		readonly IEventAggregator _eventMediator;
		readonly WorkflowState _state;
		
		public ProfileOptimizationViewModel(WorkflowState state)
			: base(state)
		{
			_state = state;
			_provider = IoC.Get<IWorkflowProvider>();
			_eventMediator = IoC.Get<IEventAggregator>();
			_provider.Parameters = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Workflows.xml");

			ProfilesOverall = state.Profiles == null ? 0 : state.Profiles.Count;
			toProcess = new ConcurrentQueue<ProfileEntry>(state.Profiles);
		}

		private ProfileEntry current;
		public ProfileEntry CurrentProfile
		{
			get { return current; }
			set
			{
				current = value;
				NotifyOfPropertyChange(() => CurrentProfile);
			}
		}

		private int processed;
		public int ProcessedProfiles
		{
			get { return processed; }
			set
			{
				processed = value;
				NotifyOfPropertyChange(() => ProcessedProfiles);
			}
		}

		private int overall;
		public int ProfilesOverall
		{
			get { return overall; }
			set
			{
				overall = value;
				NotifyOfPropertyChange(() => ProfilesOverall);
			}
		}

		public void GoPrevious()
		{
			this.NextTransition = Models.StateTransition.ProfileFiltering;
			this.TryClose();
		}

		public bool CanGoPrevious
		{
			get
			{
				return !isStarted;
			}
		}

		public async void BeginProcess()
		{
			isStarted = true;
			NotifyCtrls();
			List<IProfileWorkflow> workflows = null;
			try
			{
				workflows = await Task.Factory.StartNew<List<IProfileWorkflow>>(() =>
				{
					_eventMediator.PublishOnUIThread(new LogEntry() { Severity = LogSeverity.Info, Message = "Initializing workflows." });
					List<IProfileWorkflow> result = _provider.Initialize();
					_eventMediator.PublishOnUIThread(new LogEntry() { Severity = LogSeverity.Info, Message = "Workflows have been initialized." });
					return result;
				});
			}
			catch (Exception ex)
			{
				_eventMediator.PublishOnUIThread(new LogEntry() { Severity = LogSeverity.Error, Message = "Unable to initialize workflows.", Error = ex });
				return;
			}

			_eventMediator.PublishOnUIThread(new LogEntry() { Severity = LogSeverity.Info, Message = "Begin processing profile(s)." });
			ProfileEntry entry = null;
			while (toProcess.TryDequeue(out entry))
			{
				CurrentProfile = entry;

				foreach (IProfileWorkflow workflow in workflows)
				{
					//workflow.Execute(entry.FullPath);
					//if (workflow.IsImportant && (workflow.State != WorkflowStates.Success || workflow.State != WorkflowStates.Warn))
					//{
					//	if (workflow.State == WorkflowStates.Failed || workflow.State == WorkflowStates.Exceptional)
					//		_eventMediator.PublishOnUIThread(new LogEntry() { Severity = LogSeverity.Error, Message = workflow.Description });
					//}
					//else
					//{
						
					//}
				}

				await Task.Delay(TimeSpan.FromSeconds(1));
				ProcessedProfiles++;
			}

			CurrentProfile = null;
			_eventMediator.PublishOnUIThread(new LogEntry() { Severity = LogSeverity.Info, Message = "Profile(s) processed." });

			isStarted = false;
			NotifyCtrls();
		}

		void NotifyCtrls()
		{
			NotifyOfPropertyChange(() => CanBeginProcess);
			NotifyOfPropertyChange(() => CanGoPrevious);
		}

		public bool CanBeginProcess
		{
			get
			{
				return !isStarted;
			}
		}

	}
}
