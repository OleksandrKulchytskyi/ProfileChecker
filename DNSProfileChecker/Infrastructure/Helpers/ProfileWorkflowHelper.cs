using DNSProfileChecker.Common;

namespace Nuance.Radiology.DNSProfileChecker.Infrastructure.Helpers
{
	public static class ProfileWorkflowHelper
	{
		public static bool IsProfileMatchState(this IProfileWorkflow workflow, WorkflowStates state)
		{
			Ensure.Argument.NotNull(workflow, "workflow parameter cannot be a null.");

			bool result = true;
			if (workflow.State != state)
			{
				if (workflow.State != WorkflowStates.NotApplied)
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