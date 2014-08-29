using DNSProfileChecker.Common;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DNSProfileChecker.Workflow
{
	public class AcousticWorkflow : BaseWorkflow
	{
		public AcousticWorkflow()
			: base()
		{
		}

		public override void Execute(object parameters)
		{
			Ensure.Argument.NotNull(parameters, "parameters cannot be a null.");
			string sourceFolder = parameters as string;

			string currentFolder = Path.Combine(sourceFolder, "current");
			string acousticFilePath = Path.Combine(sourceFolder, "current\\acoustic.ini");

			bool isExists = File.Exists(acousticFilePath);
			if (!isExists)
			{
				if (IsImportant)
				{
					State = WorkflowStates.Failed;
					Description = "File acoustic.ini doesn't exist.";
				}
				else if (!IsImportant)
				{
					State = WorkflowStates.Warn;
					Description = "File acoustic.ini doesn't exist.";
				}
				DoLog(LogSeverity.Error, Description, null);
			}
			else
			{
				Dictionary<string, List<KeyValuePair<string, string>>> data = null;
				try
				{
					data = IniFileParser.GetSingleSection(acousticFilePath, "Acoustics");
				}
				catch (System.Exception ex)
				{
					DoLog(LogSeverity.Error, (Description = "Error occured during the reading of acoustic.ini file."), ex);
					State = WorkflowStates.Exceptional;
					return;
				}

				if (data.Count == 0)
				{
					State = WorkflowStates.Failed;
					Description = "File acoustic.ini doesn't have section called [Acoustics].";
					//DoLog(LogSeverity.Error, Description, null);
					return;
				}
				else if (data["Acoustics"].Count == 0)
				{
					State = WorkflowStates.Failed;
					Description = "[Acoustics] section has no entires in the acoustic.ini file.";
					//DoLog(LogSeverity.Error, Description, null);
					return;
				}

				Queue<KeyValuePair<string, string>> missedContainer = new Queue<KeyValuePair<string, string>>();

				StringBuilder msgBuilder = new StringBuilder();
				bool isMainMissed = false;
				bool isContainerMissed = false;
				foreach (KeyValuePair<string, string> item in data["Acoustics"])
				{
					DirectoryInfo di = new DirectoryInfo(Path.Combine(currentFolder, item.Value));
					DirectoryInfo diContainer = new DirectoryInfo(Path.Combine(currentFolder, item.Value + "_container"));

					if (!di.Exists && !diContainer.Exists)
						missedContainer.Enqueue(item);

					if (!di.Exists)
					{
						string msg = string.Format("Acoustic dictation source folder {0} is missed.", item.Value);
						DoLog(LogSeverity.Error, msg, null);
						msgBuilder.AppendLine(msg);
						isMainMissed = true;
						continue;
					}

					if (!diContainer.Exists)
					{
						DoLog(LogSeverity.Warn, string.Format("Acoustic container folder has been missed in the root {0}", currentFolder), null);
						if (!IsSimulationMode)
							diContainer.Create();
						string addMsg = string.Format("Acoustic container folder {0} has been created.", diContainer.Name);
						msgBuilder.AppendLine(addMsg);
						DoLog(LogSeverity.Success, addMsg, null);
					}
					else
						base.Execute(diContainer.FullName);
				}//end foreach

				if (missedContainer.Count > 0)
				{
					Dictionary<string, List<KeyValuePair<string, string>>> content = IniFileParser.GetSections(acousticFilePath);
					if (content.Count > 0)
					{
						while (missedContainer.Count != 0)
						{
							KeyValuePair<string, string> forPurge = missedContainer.Dequeue();
							foreach (string section in content.Keys)
							{
								int clearIndx = -1;
								foreach (var sectionData in content[section])
								{
									if (sectionData.Key == forPurge.Key)
									{
										clearIndx = content[section].IndexOf(sectionData);
										break;
									}

								}
								if (clearIndx != -1)
									content[section].RemoveAt(clearIndx);
							}
						}
						File.Delete(acousticFilePath);

						using (StreamWriter sw = new StreamWriter(acousticFilePath))
						{
							foreach (string section in content.Keys)
							{
								sw.WriteLine("[" + section + "]");
								foreach (var sectionData in content[section])
								{
									sw.WriteLine(sectionData.Key + "=" + sectionData.Value);
								}
								sw.WriteLine(System.Environment.NewLine);
							}
							sw.Flush();
							Logger.LogData(LogSeverity.Success, "Acoustic.ini has been fixed redundant data was purged from a file.", null);
						}
					}
				}

				if (isMainMissed)
				{
					Description = msgBuilder.ToString();
					if (IsImportant)
						State = WorkflowStates.Failed;
					else
						State = WorkflowStates.Warn;
				}
				else if (isContainerMissed)
				{
					State = WorkflowStates.Warn;
					Description = msgBuilder.ToString();
				}
				else
					State = WorkflowStates.Success;
			}//end else region
		}
	}
}