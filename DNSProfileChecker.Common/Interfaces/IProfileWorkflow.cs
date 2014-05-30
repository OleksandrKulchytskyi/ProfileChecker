using System.Collections.Generic;

namespace DNSProfileChecker.Common
{
	public enum WorkflowState
	{
		None = 0,
		Started = 1,
		NotApplied,
		Processing,
		Exceptional,
		Success,
		Warn,
		Failed
	}

	public interface IProfileWorkflow
	{
		ILogger Logger { get; set; }

		string Description { get; set; }
		WorkflowState State { get; set; }

		bool IsImportant { get; set; }

		List<IProfileWorkflow> SubsequentWorkflows { get; }

		void Execute(object parameter);
	}
}
