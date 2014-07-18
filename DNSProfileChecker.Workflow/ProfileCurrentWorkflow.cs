using DNSProfileChecker.Common;
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
			Ensure.Argument.NotNull(parameters, "parameters cannot be a null.");
			
			DirectoryInfo currentDI = new DirectoryInfo(Path.Combine(parameters as string, "current"));
			if (!currentDI.Exists)
			{
				Description = string.Format("Current folder doesn't exist in the profile folder: [{0}] ", currentDI.Parent.Name);
				if (IsImportant)
					State = WorkflowStates.Failed;
				else
					State = WorkflowStates.Warn;
				DoLog(LogSeverity.Error, Description, null);
			}
			else
				State = WorkflowStates.Success;

			base.Execute(parameters);
		}
	}
}
