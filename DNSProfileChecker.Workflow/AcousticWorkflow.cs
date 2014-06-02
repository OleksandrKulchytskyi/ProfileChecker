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
			if (!isExists && IsImportant)
			{
				State = WorkflowStates.Failed;
				Description = "File acoustic.ini doesn't exist.";
			}
			else if (!isExists && IsImportant)
			{
				State = WorkflowStates.Warn;
				Description = "File acoustic.ini doesn't exist.";
			}
			else if (isExists)
			{
				State = WorkflowStates.Processing;
				Dictionary<string, List<KeyValuePair<string, string>>> data = IniFileParser.GetSingleSection(filePath, "Acoustics");
				if (data.Count == 0)
				{
					State = WorkflowStates.Failed;
					Description = "Unable to find [Acoustics] section in the acoustic.ini file.";
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
						msgBuilder.AppendLine(string.Format("Acoustic folder {0} is missed.", item.Value));
						isMainMissed = true;
						break;
					}

					string container = item.Value + "_container";
					DirectoryInfo diContainer = new DirectoryInfo(Path.Combine(currentFolder, container));
					if (!di.Exists)
					{
						msgBuilder.AppendLine(string.Format("Acoustic container folder {0} has been created.", container));
						di.Create();
					}
					else
					{
						base.Execute(diContainer.FullName);
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

			}
		}
	}
}
