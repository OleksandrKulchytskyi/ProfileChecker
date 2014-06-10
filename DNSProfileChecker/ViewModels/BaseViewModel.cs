using Caliburn.Micro;
using DNSProfileChecker.Common;
using Nuance.Radiology.DNSProfileChecker.Models;

namespace Nuance.Radiology.DNSProfileChecker.ViewModels
{
	public class BaseViewModel : Screen
	{
		public WorkflowState WorkflowState { get; protected set; }

		public StateTransition NextTransition { get; protected set; }

		public BaseViewModel(WorkflowState workflowState)
		{
			Ensure.Argument.NotNull(workflowState);

			this.WorkflowState = workflowState;
		}

		public void Cancel()
		{
			NextTransition = StateTransition.Cancel;
			WorkflowState = new WorkflowState();
			this.TryClose();
		}
	}
}