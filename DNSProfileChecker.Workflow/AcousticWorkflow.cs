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
			string filePath = Path.Combine(sourceFolder, "current\\acoustic.ini");

			bool isExists = File.Exists(filePath);
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
					data = IniFileParser.GetSingleSection(filePath, "Acoustics");
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

				StringBuilder msgBuilder = new StringBuilder();
				bool isMainMissed = false;
				bool isContainerMissed = false;
				foreach (KeyValuePair<string, string> item in data["Acoustics"])
				{
					DirectoryInfo di = new DirectoryInfo(Path.Combine(currentFolder, item.Value));
					if (!di.Exists)
					{
						string msg = string.Format("Acoustic dictation source folder {0} is missed.", item.Value);
						DoLog(LogSeverity.Error, msg, null);
						msgBuilder.AppendLine(msg);
						isMainMissed = true;
						continue;
					}

					DirectoryInfo diContainer = new DirectoryInfo(Path.Combine(currentFolder, item.Value + "_container"));
					if (!di.Exists)
					{
						DoLog(LogSeverity.Warn, string.Format("Acoustic container folder has been missed in the root {0}", currentFolder), null);
						if (!IsSimulationMode)
							di.Create();
						string addMsg = string.Format("Acoustic container folder {0} has been created.", diContainer.Name);
						msgBuilder.AppendLine(addMsg);
						DoLog(LogSeverity.Success, addMsg, null);
					}
					else
						base.Execute(diContainer.FullName);
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
