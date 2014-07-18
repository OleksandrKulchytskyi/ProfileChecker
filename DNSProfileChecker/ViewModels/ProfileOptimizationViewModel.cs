using Caliburn.Micro;
using DNSProfileChecker.Common;
using Nuance.Radiology.DNSProfileChecker.Infrastructure.Helpers;
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
		private ConcurrentQueue<ProfileEntry> toProcess;
		private volatile bool isStarted = false;
		private volatile bool isStopped = false;
		private List<IProfileWorkflow> workflows = null;

		private readonly IWorkflowProvider _provider;
		private readonly WorkflowState _state;
		private readonly ILogger _logger;

		public ProfileOptimizationViewModel(WorkflowState state)
			: base(state)
		{
			_state = state;
			_provider = IoC.Get<IWorkflowProvider>();
			_logger = IoC.Get<ILogger>();
			_provider.Parameters = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Workflows.xml");

			ProfilesOverall = state.ProfilesToCheck == null ? 0 : state.ProfilesToCheck.Count;
			_logger.LogData(LogSeverity.UI, string.Format("Selected {0} profile(s) for checking.", ProfilesOverall), null);
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
			this.WorkflowState = _state;
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

		public void GoNext()
		{
		}

		public bool CanGoNext { get { return false; } }

		public async void BeginProcess()
		{
			if (workflows == null)
			{
				try
				{
					workflows = await Task.Factory.StartNew<List<IProfileWorkflow>>(() =>
					{
						_logger.LogData(LogSeverity.Info, "Initializing workflows.", null);
						List<IProfileWorkflow> result = _provider.Initialize();
						_logger.LogData(LogSeverity.Info, "Workflows have been initialized.", null);
						return result;
					});

					foreach (IProfileWorkflow w in workflows)
						assignLoggerAndStateToWorkflow(w, _logger, _state);
				}
				catch (Exception ex)
				{
					_logger.LogData(LogSeverity.Fatal, "Unable to initialize workflows.", ex);
					return;
				}
			}

			if (toProcess == null)
				toProcess = new ConcurrentQueue<ProfileEntry>(_state.ProfilesToCheck);
			else if (isStopped && toProcess != null && CurrentProfile != null)
			{
				List<ProfileEntry> entries = new List<ProfileEntry>(toProcess.Count + 1);
				entries.Add(CurrentProfile);
				entries.AddRange(toProcess.ToArray());
				toProcess = null;
				toProcess = new ConcurrentQueue<ProfileEntry>(entries);
			}

			isStarted = true;
			isStopped = false;
			NotifyCtrls();
			if (CurrentProfile == null)
			{
				ProcessedProfiles = 0;
				_logger.LogData(LogSeverity.UI, string.Format("Begin processing DNS profile(s)."), null);
			}

			ProfileEntry entry = null;
			bool isErrorOccurred = false;
			Exception error = null;
			bool isProfileCorrect = true;
			while (toProcess.TryDequeue(out entry))
			{
				CurrentProfile = entry;

				if (isStopped) return;// terminate our process in case when stopped

				if (_state.IsSimulationMode)
					_logger.LogData(LogSeverity.UI, "Run in simulation mode.", null);
				_logger.LogData(LogSeverity.UI, string.Format("Begin to process {0} DNS profile.", CurrentProfile.Name), null);

				isProfileCorrect = true;
				foreach (IProfileWorkflow workflow in workflows)
				{
					try
					{
						isErrorOccurred = false;
						error = null;
						await TaskHelper.CreateTaskFromParametrizedAction(workflow.Execute, entry.FullPath);
					}
					catch (Exception ex)
					{
						isErrorOccurred = true;
						error = ex;
					}

					if (isErrorOccurred)
					{
						isProfileCorrect = false;
						_logger.LogData(LogSeverity.Fatal, error.Message, error);
						continue;
						//break;
					}

					if (workflow.IsImportant)
					{
						if (workflow.State == WorkflowStates.Failed || workflow.State == WorkflowStates.Exceptional || workflow.State == WorkflowStates.Warn)
						{
							if (workflow.State != WorkflowStates.Warn && workflow.Description.IsNotNullOrEmpty())
								_logger.LogData(LogSeverity.Error, workflow.Description, null);
							isProfileCorrect = false;
							continue;
						}
					}
					else
					{
						if (workflow.State == WorkflowStates.Failed || workflow.State == WorkflowStates.Exceptional || workflow.State == WorkflowStates.Warn)
						{
							_logger.LogData(LogSeverity.Warn, workflow.Description, null);
							isProfileCorrect = false;
							continue;
						}
					}

					isProfileCorrect = workflow.IsProfileMatchState(WorkflowStates.Success);
				}//end for loop

				ProcessedProfiles++;
				if (isProfileCorrect)
					_logger.LogData(LogSeverity.Success, string.Format("Profile {0} has been checked with no error(s).", CurrentProfile.Name), null);
				else
					_logger.LogData(LogSeverity.UI, string.Format("Profile [{0}] has been verified.", CurrentProfile.Name), null);

				await TaskHelper.Delay((int)TimeSpan.FromSeconds(1).TotalMilliseconds);
			}//end while loop

			CurrentProfile = null;
			toProcess = null;
			_logger.LogData(LogSeverity.UI, "All DNS profile(s) have been checked.", null);

			isStarted = false;
			isStopped = false;

			NotifyCtrls();
		}

		public bool CanBeginProcess
		{
			get
			{
				return !isStarted;
			}
		}

		public void StopProcess()
		{
			isStopped = true;
			isStarted = false;
			NotifyCtrls();
		}

		public bool CanStopProcess
		{
			get
			{
				return isStarted;
			}
		}

		private void NotifyCtrls()
		{
			NotifyOfPropertyChange(() => CanStopProcess);
			NotifyOfPropertyChange(() => CanBeginProcess);
			NotifyOfPropertyChange(() => CanGoPrevious);
		}

		private void assignLoggerAndStateToWorkflow(IProfileWorkflow wf, ILogger logger, WorkflowState state)
		{
			if (logger != null)
			{
				wf.Logger = logger;
				wf.IsSimulationMode = state.IsSimulationMode;
				if (wf.SubsequentWorkflows == null)
					return;

				foreach (var item in wf.SubsequentWorkflows)
				{
					assignLoggerAndStateToWorkflow(item, logger, state);
				}
			}
		}
	}
}