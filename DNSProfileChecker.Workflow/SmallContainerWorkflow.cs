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

				DirectoryInfo[] sessions = containerDI.GetDirectories("session*", SearchOption.TopDirectoryOnly).OrderBy(f => int.Parse(f.Name.Remove(0, "session".Length))).ToArray();
				IValidator<DirectoryInfo[]> sessionValidator = new Common.Implementation.SessionFoldersSequenceValidator();
				if (!sessionValidator.Validate(sessions))
				{
					DoLog(LogSeverity.Warn, string.Format("Folder {0} has some missed session(s) folder: {1}",
							containerDI.Name, string.Join(",", sessionValidator.MissedValues.Select(x => x.Name))), null);

					IReorderManager reorderManager = new DNSProfileChecker.Common.Implementation.FolderReorderManager();
					if (!reorderManager.Reorder(sessions))
					{
						DoLog(LogSeverity.Error, string.Format("Some error(s) occurred during reordering sessions{0}{1}", Environment.NewLine, GetMessage(reorderManager.Errors)), null);
						State = WorkflowStates.Warn;
					}
					else
						DoLog(LogSeverity.Info, "Session reordering workflow has completed successfully.", null);

					DoLog(LogSeverity.Info, "Begin to verify each session folder.", null);
					foreach (DirectoryInfo sessionDI in containerDI.GetDirectories("session*", SearchOption.TopDirectoryOnly))
					{
						base.Execute(sessionDI.FullName);
					}
					State = WorkflowStates.Success;
				}
				else
				{
					DoLog(LogSeverity.Info, "Session folders are reside in the consistent way, no reordering is needed.", null);
					DoLog(LogSeverity.Info, "Begin to verify each session folder.", null);

					foreach (DirectoryInfo session in sessions)
					{
						base.Execute(session.FullName);
					}
					State = WorkflowStates.Success;
				}
			}
			else
			{
				State = WorkflowStates.Failed;
				Description = string.Format("Folder doesn't exist. {0}Folder path: {1}", Environment.NewLine, folderPath);
			}
		}

		private string GetMessage(AggregateException ex)
		{
			if (ex == null)
				return string.Empty;

			StringBuilder sb = new StringBuilder();
			foreach (Exception e in ex.InnerExceptions)
			{
				sb.AppendLine(e.Message);
			}
			return sb.ToString();
		}
	}
}
