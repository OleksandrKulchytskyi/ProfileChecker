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
			IOrderedEnumerable<DirectoryInfo> orderedExp = containerDI.GetDirectories("session*", SearchOption.TopDirectoryOnly).OrderBy(f => int.Parse(f.Name.Remove(0, "session".Length)));
			if (containerDI.Exists)
			{
				long size = containerDI.GetFolderSize();
				if (size > Constants.FolderLimitSize)
				{
					DoLog(LogSeverity.UI, "Dictation source is to large, trimming is nedded.", null);
					DirectoryInfo[] sessions = orderedExp.ToArray();
					foreach (DirectoryInfo sessionDi in sessions)
					{
						if (!sessionDi.Exists)
							continue;

						//DirectoryInfo draFolder = sessionDi.GetDirectories().FirstOrDefault(f => f.Name.Equals("drafiles", StringComparison.OrdinalIgnoreCase));
						DirectoryInfo draFolder = sessionDi.GetDirectories("drafile*", SearchOption.TopDirectoryOnly).FirstOrDefault();
						if (draFolder != null && draFolder.Exists)
						{
							bool approachTaken = false;
							DoLog(LogSeverity.Info, string.Format("Deleting content of drafiles folder in {0} ", sessionDi.FullName), null);
							foreach (FileInfo fi in draFolder.GetFiles("*.*", SearchOption.TopDirectoryOnly))
							{
								approachTaken = true;
								AggregateException exc;
								Retry.Do<object>(() => { fi.Delete(); return null; }, TimeSpan.FromMilliseconds(800), 3, out  exc);
								if (exc != null)
									DoLog(LogSeverity.Error, string.Format("Unable to delete a file named: {0} ", fi.FullName), exc);
							}

							FileInfo draINI = new FileInfo(Path.Combine(sessionDi.FullName, "drafiles.ini"));
							if (approachTaken)
							{
								try
								{
									if (draINI.Exists)
										draINI.Delete();

									IFileFactory factory = new Common.Factories.FileFactory();
									StreamWriter sw = factory.CreateFile(draINI.FullName, FileFactoryEnum.DRAFilesINI);
									sw.Flush();
									sw.Dispose();
									DoLog(LogSeverity.Success, "drafiles.ini has been successfully re-created.", null);
								}
								catch (Exception ex)
								{
									DoLog(LogSeverity.Error, "Unable to create drafiles.ini file.", ex);
								}
							}

							#region commented
							//Dictionary<string, List<string>> data = IniFileParser.GetSectionsDictionary(draINI.FullName);
							//if (data.ContainsKey("Count"))
							//	data.Remove("Count");
							//if (data.ContainsKey("Files"))
							//	data.Remove("Files");

							//data["Count"] = new List<string>() { "SeqNo=0" };
							//data["Files"] = new List<string>();


							//using (StreamWriter sw = draINI.CreateText())
							//{
							//	foreach (var key in data.Keys)
							//	{
							//		sw.WriteLine(string.Format("[{0}]", key));
							//		foreach (string item in data[key])
							//		{
							//			sw.WriteLine(item);
							//		}
							//		sw.WriteLine(Environment.NewLine);// guided by the Scott's request, inserting line-brake between sections 
							//	}
							//	sw.Flush();
							//} 
							#endregion
						}
						// drafiles directory doesn't exist
						else
							DoLog(LogSeverity.Warn, string.Format("Folder: {0} doesn't contains folder named Drafiles.", sessionDi.FullName), null);

						if ((size = DirectoryInfoExtensions.GetDirectorySizeParalell(containerDI.FullName, true)) < Constants.EndPruningThreshold)
						{
							DoLog(LogSeverity.UI, string.Format("Trimming is ended at size {0}.", size), null);
							break; // end pruning session directories.
						}

					}//end of foreach statement for sessions

					//according to the Scott's request after the trimming workflow, tool has to reoreder folders if needed.
					DirectoryInfo[] sessionsDI = orderedExp.ToArray();
					IValidator<DirectoryInfo[]> sessionsValidator = new Common.Implementation.SessionFoldersSequenceValidator();
					if (sessionsDI.Length > 0 && !sessionsValidator.Validate(sessionsDI))
					{
						DoLog(LogSeverity.UI, string.Format("Folder {0} has some missed session(s) folder: {1}",
								containerDI.Name, string.Join(",", sessionsValidator.MissedValues.Select(x => x.Name))), null);

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
