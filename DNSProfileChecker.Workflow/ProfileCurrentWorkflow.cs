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
			State = WorkflowState.Started;
			State = WorkflowState.Processing;
			Ensure.Argument.NotNull(parameters, "parameters cannot be a null.");
			string sourceFolder = parameters as string;
			bool isExists = Directory.Exists(Path.Combine(sourceFolder, "current"));
			State = WorkflowState.Processing;
			if (!isExists && IsImportant)
			{
				State = WorkflowState.Failed;
				Description = "Current folder is missed the profile root folder. ";
			}
			else if (!isExists && !IsImportant)
			{
				State = WorkflowState.Warn;
				Description = "Current folder is missed in the profile root folder.";
			}
			else
				State = WorkflowState.Success;

			base.Execute(parameters);
		}
	}
}
