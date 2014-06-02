using DNSProfileChecker.Common;
using System.Collections.Generic;
using System.IO;

namespace DNSProfileChecker.Workflow
{
	public class ProfileCurrentWorkflow : BaseWorkflow
	{
		public ProfileCurrentWorkflow()
			: base()
		{
		}

		public override void Execute(object parameters)
		{
			State = WorkflowStates.Started;
			State = WorkflowStates.Processing;
			Ensure.Argument.NotNull(parameters, "parameters cannot be a null.");
			string sourceFolder = parameters as string;
			bool isExists = Directory.Exists(Path.Combine(sourceFolder, "current"));
			State = WorkflowStates.Processing;
			if (!isExists && IsImportant)
			{
				State = WorkflowStates.Failed;
				Description = "Current folder is missed the profile root folder. ";
			}
			else if (!isExists && !IsImportant)
			{
				State = WorkflowStates.Warn;
				Description = "Current folder is missed in the profile root folder.";
			}
			else
				State = WorkflowStates.Success;

			base.Execute(parameters);
		}
	}
}
