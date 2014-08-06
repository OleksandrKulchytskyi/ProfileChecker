
namespace DNSProfileChecker.Common.Extension
{
	public static class ProfileWorkflowExtension
	{
		public static bool IsProfileMatchState(this IProfileWorkflow workflow, WorkflowStates state)
		{
			Ensure.Argument.NotNull(workflow, "workflow parameter cannot be a null.");

			bool result = true;
			if (workflow.State != state)
			{
				if (workflow.State != WorkflowStates.NotApplied && workflow.State != WorkflowStates.Warn)
				{
					return false;
				}
			}

			if (workflow.SubsequentWorkflows != null && workflow.SubsequentWorkflows.Count > 0)
			{
				foreach (IProfileWorkflow w in workflow.SubsequentWorkflows)
				{
					if (!(result = w.IsProfileMatchState(state)))
						break;
				}
			}

			return result;
		}
	}
}
