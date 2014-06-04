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
					DirectoryInfo[] sessions = containerDI.GetDirectories("session*");
					foreach (DirectoryInfo sessionDi in sessions)
					{
						if (!sessionDi.Exists)
							continue;

						DirectoryInfo draFolder = sessionDi.GetDirectories().FirstOrDefault(f => f.Name.Equals("drafiles", StringComparison.OrdinalIgnoreCase));
						if (draFolder != null && draFolder.Exists)
						{
							DoLog(LogSeverity.Info, string.Format("Deleting content of drafiles folder in {0} ", sessionDi.FullName), null);
							foreach (FileInfo fi in draFolder.GetFiles("*.*"))
							{
								AggregateException exc;
								Retry.Do<object>(() => { fi.Delete(); return null; }, TimeSpan.FromMilliseconds(200), 3, out  exc);
								if (exc != null)
									DoLog(LogSeverity.Warn, string.Format("Unable to delete file: {0} ", fi.FullName), exc);
							}

							FileInfo draINI = new FileInfo(Path.Combine(sessionDi.FullName, "drafiles.ini"));
							if (draINI.Exists)
							{
								Dictionary<string, List<string>> data = IniFileParser.GetSectionsDictionary(draINI.FullName);
								if (data.ContainsKey("Count"))
									data.Remove("Count");
								if (data.ContainsKey("Files"))
									data.Remove("Files");

								data["Count"] = new List<string>() { "seqNo=0" };
								data["Files"] = new List<string>();
								draINI.Delete();

								using (StreamWriter sw = draINI.CreateText())
								{
									foreach (var key in data.Keys)
									{
										sw.WriteLine(string.Format("[{0}]", key));
										foreach (string item in data[key])
										{
											sw.WriteLine(item);
										}
									}
									sw.Flush();
								}
							}
							else
								DoLog(LogSeverity.Warn, string.Format("drafiles.ini doesn't exist in folder: {0}", sessionDi.FullName), null);
						}
						else
							DoLog(LogSeverity.Warn, string.Format("Folder: {0} doesn't contain folder drafiles ", sessionDi.FullName), null);

					}
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
