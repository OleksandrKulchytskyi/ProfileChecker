using DNSProfileChecker.Common;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace DNSProfileChecker.Workflow
{
	public class SmallContainerWorkflow : BaseWorkflow
	{
		public SmallContainerWorkflow()
			: base()
		{
		}

		public override void Execute(object parameter)
		{
			Ensure.Argument.NotNull(parameter, "parameter value cannot be a null.");
			State = WorkflowStates.Success;
			string folderPath = parameter as string;

			DirectoryInfo containerDI = new DirectoryInfo(folderPath);
			if (containerDI.Exists)
			{
				long size = containerDI.GetFolderSize();
				if (size > Constants.FolderLimitSize)
				{
					Description = string.Format("Folder [{0}] has a size {1} which is great than a set threshold {2}{3} Workflow: {4} won't be applied.", containerDI.Name, size, Constants.FolderLimitSize, Environment.NewLine, this.GetType().ToString());
					DoLog(LogSeverity.Info, Description, null);
					State = WorkflowStates.NotApplied;
					return;
				}

				if (SubsequentWorkflows != null && SubsequentWorkflows.Count > 0)
				{
					IProfileWorkflow workflow = SubsequentWorkflows[0];
					workflow.Execute(folderPath);
					if (workflow.State == WorkflowStates.Failed)
					{
						this.State = workflow.State;
						this.Description = workflow.Description;
						return;
					}
					else if (workflow.State == WorkflowStates.Success)
					{
						DoLog(LogSeverity.Info, string.Format("Container folder verification has completed succesfully, {0}", containerDI.Name), null);
					}
				}

				DoLog(LogSeverity.Info, "Begin to verify each session folder.", null);
				foreach (DirectoryInfo sessionDI in containerDI.GetDirectories("session*", SearchOption.TopDirectoryOnly))
				{
					base.Execute(sessionDI.FullName);
				}

				DirectoryInfo[] sessions = containerDI.GetDirectories("session*", SearchOption.TopDirectoryOnly).OrderBy(f => int.Parse(f.Name.Remove(0, "session".Length))).ToArray();
				IValidator<DirectoryInfo[]> sessionsValidator = new Common.Implementation.SessionFoldersSequenceValidator();
				if (!sessionsValidator.Validate(sessions))
				{
					string missedProfiles = string.Join(",", sessionsValidator.MissedValues.Select(x => x.Name));
					DoLog(LogSeverity.Warn, string.Format("Directory [{0}] has some missed session(s) entries: {1}", containerDI.Name, missedProfiles), null);

					DoLog(LogSeverity.UI, "Begin to perform session(s) reordering workflow", null);
					IReorderManager reorderManager = new DNSProfileChecker.Common.Implementation.FolderReorderManager();
					if (!reorderManager.Reorder(sessions))
					{
						DoLog(LogSeverity.Error, string.Format("Some error(s) occurred during re-ordering sessions{0}{1}", Environment.NewLine, GetMessage(reorderManager.Errors)), null);
						State = WorkflowStates.Failed;
					}
					else
						DoLog(LogSeverity.UI, "Session re-ordering workflow has completed successfully.", null);
				}
				else
					DoLog(LogSeverity.UI, "Session folders are reside in the consistent way, no re-ordering is needed.", null);
			}
			else
			{
				State = WorkflowStates.Failed;
				Description = string.Format("Folder doesn't exist. {0}Folder path: {1}", Environment.NewLine, folderPath);
			}
		}
	}
}
