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
			if (!isExists)
			{
				Description = "Current folder doesn't exist in the profile root folder.";
				if (IsImportant)
					State = WorkflowStates.Failed;
				else
					State = WorkflowStates.Warn;
			}
			else
				State = WorkflowStates.Success;

			base.Execute(parameters);
		}
	}
}
