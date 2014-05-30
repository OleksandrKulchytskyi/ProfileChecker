using DNSProfileChecker.Common;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DNSProfileChecker.Workflow
{
	public class TopicVerifyWorkflow : BaseWorkflow
	{
		public TopicVerifyWorkflow()
			: base()
		{
		}

		public override void Execute(object parameters)
		{
			State = WorkflowState.Processing;
			Ensure.Argument.NotNull(parameters, "parameters cannot be a null.");
			string sourceFolder = parameters as string;

			string currentFolder = Path.Combine(sourceFolder, "current");
			string filePath = Path.Combine(sourceFolder, "current\\topics.ini");
			bool isExists = File.Exists(filePath);
			if (!isExists && IsImportant)
			{
				State = WorkflowState.Failed;
				Description = "File topics.ini doesn't exist.";
			}
			else if (!isExists && IsImportant)
			{
				State = WorkflowState.Warn;
				Description = "File topics.ini doesn't exist.";
			}
			else if (isExists)
			{
				Dictionary<string, List<KeyValuePair<string, string>>> data = IniFileParser.GetSingleSection(filePath, "Topics");
				if (data.Count == 0)
				{
					State = WorkflowState.Failed;
					Description = "There is no [Topics] section in the topics.ini file.";
					return;
				}

				StringBuilder msgBuilder = new StringBuilder();
				bool isMainMissed = false;
				bool isContainerMissed = false;
				foreach (KeyValuePair<string, string> item in data["Topics"])
				{
					if (item.Value.IsNotNullOrEmpty())
					{
						DirectoryInfo topic = new DirectoryInfo(Path.Combine(currentFolder, item.Value));
						if (!topic.Exists)
						{
							isMainMissed = true;
							msgBuilder.AppendLine(string.Format("Topic folder {0} is missed.", item.Value));
							break;
						}
						string topicContainerName = item.Value + "_container";
						DirectoryInfo topicContainer = new DirectoryInfo(Path.Combine(currentFolder, topicContainerName));
						if (!topicContainer.Exists)
						{
							isContainerMissed = true;
							msgBuilder.AppendLine(string.Format("Topic container folder {0} is missed.", topicContainerName));
							continue;
						}
					}
				}

				if (isMainMissed)
				{
					Description = msgBuilder.ToString();
					if (IsImportant)
						State = WorkflowState.Failed;
					else
						State = WorkflowState.Warn;
				}
				else if (isContainerMissed)
				{
					Description = msgBuilder.ToString();
					State = WorkflowState.Warn;
				}
				else
					State = WorkflowState.Success;

				base.Execute(parameters);
			}
		}

	}
}
