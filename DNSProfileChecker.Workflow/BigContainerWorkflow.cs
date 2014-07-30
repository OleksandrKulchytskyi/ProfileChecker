using DNSProfileChecker.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSProfileChecker.Workflow
{
	public class BigContainerWorkflow : BaseWorkflow
	{
		public BigContainerWorkflow()
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
					bool approachTaken = false;
					DoLog(LogSeverity.UI, string.Format("Dictation source ({0}) is too large, trimming is necessary.", containerDI.Name), null);
					DirectoryInfo[] sessions = containerDI.GetDirectories("session*", SearchOption.TopDirectoryOnly).OrderBy(f => int.Parse(f.Name.Remove(0, "session".Length))).ToArray();

					int interruptedIndex = -1;//index at which pruning workflow has been completed due to the min size limit.
					DirectoryInfo[] notProcessed = null;

					foreach (DirectoryInfo sessionDi in sessions)
					{
						if (!sessionDi.Exists) continue;
						//DirectoryInfo draFolder = sessionDi.GetDirectories().FirstOrDefault(f => f.Name.Equals("drafiles", StringComparison.OrdinalIgnoreCase));
						approachTaken = false;
						DirectoryInfo draFolder = sessionDi.GetDirectories("drafile*", SearchOption.TopDirectoryOnly).FirstOrDefault();
						if (draFolder != null && draFolder.Exists)
						{
							DoLog(LogSeverity.Info, string.Format("Deleting content of drafiles folder in {0} ", sessionDi.FullName), null);
							if (!IsSimulationMode)
							{
								foreach (FileInfo fi in draFolder.GetFiles("*.*", SearchOption.TopDirectoryOnly))
								{
									approachTaken = true;
									AggregateException exc;
									Retry.Do<object>(() => { fi.Delete(); return null; }, TimeSpan.FromMilliseconds(800), 2, out exc);
									if (exc != null)
										DoLog(LogSeverity.Error, string.Format("Unable to delete a file named: {0} ", fi.FullName), exc);
								}
							}
						}
						// drafiles directory doesn't exist
						else
							DoLog(LogSeverity.Warn, string.Format("Folder: [{0}] doesn't contains folder named [drafiles].", sessionDi.FullName), null);

						base.Execute(sessionDi.FullName);

						if (approachTaken &&
							((size = DirectoryInfoExtensions.GetDirectorySizeParalell(containerDI.FullName, true)) < Constants.EndPruningThreshold))
						{
							interruptedIndex = Array.IndexOf<DirectoryInfo>(sessions, sessionDi);
							if (++interruptedIndex < sessions.Length)
							{
								notProcessed = new DirectoryInfo[sessions.Length - interruptedIndex];
								Array.Copy(sessions, interruptedIndex, notProcessed, 0, notProcessed.Length);
							}
							string trimCompleted = "Trimming is complete.  New size of dictation source ({0}) is {1} Mb";
							DoLog(LogSeverity.UI, string.Format(trimCompleted, containerDI.Name, size.ConvertToMegabytes()), null);
							break; // end pruning session directories.
						}
					}//end of foreach statement for sessions

					if (interruptedIndex > -1 && notProcessed != null)
					{
						foreach (DirectoryInfo session in notProcessed)
						{
							base.Execute(session.FullName);
						}
					}

					//according to the Scott's request after the trimming workflow, tool has to reoreder folders if needed.
					DirectoryInfo[] sessionsAfterTrimming = containerDI.GetDirectories("session*", SearchOption.TopDirectoryOnly).OrderBy(f => int.Parse(f.Name.Remove(0, "session".Length))).ToArray();
					IValidator<DirectoryInfo[]> sessionsValidator = new Common.Implementation.SessionFoldersSequenceValidator();
					if (sessionsAfterTrimming.Length > 0 && !sessionsValidator.Validate(sessionsAfterTrimming))
					{
						string missedProfiles = string.Join(",", sessionsValidator.MissedValues.Select(x => x.Name));
						DoLog(LogSeverity.Warn, string.Format("Dictation source ({0}) has some missed session folder(s): {1}",
								containerDI.Name, missedProfiles), null);

						DoLog(LogSeverity.UI, "Begin to perform session(s) reordering workflow", null);
						if (!IsSimulationMode)
						{
							IReorderManager reorderManager = new DNSProfileChecker.Common.Implementation.FolderReorderManager();
							if (!reorderManager.Reorder(sessionsAfterTrimming))
							{
								DoLog(LogSeverity.Error, string.Format("Some error(s) occurred during re-ordering sessions{0}{1}", Environment.NewLine, GetMessage(reorderManager.Errors)), null);
								State = WorkflowStates.Failed;
							}
							else
								DoLog(LogSeverity.UI, "Session re-ordering workflow has completed successfully.", null);
						}
					}
					else
						DoLog(LogSeverity.UI, "Session folder order is correct, no renumbering is necessary.", null);
				}//end if scope for size check
				else
				{
					State = WorkflowStates.NotApplied;
					Description = string.Format("Folder [{0}] has a size {1} which is less than a set threshold {2}{3} Workflow: {4} won't be applied.", containerDI.Name, size, Constants.FolderLimitSize, Environment.NewLine, this.GetType().ToString());
					DoLog(LogSeverity.Info, Description, null);
				}
			}
			else if (IsImportant)
			{
				State = WorkflowStates.Failed;
				Description = string.Format("Folder doesn't exist. {0}Folder path: {1}", Environment.NewLine, folderPath);
				DoLog(LogSeverity.Warn, Description, null);
			}

			base.Execute(parameter);
		}
	}
}
