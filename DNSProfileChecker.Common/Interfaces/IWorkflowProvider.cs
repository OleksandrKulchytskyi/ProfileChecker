using System.Collections.Generic;

namespace DNSProfileChecker.Common
{
	public interface IWorkflowProvider
	{
		object Parameters { get; set; }

		List<IProfileWorkflow> Initialize();
	}
}
