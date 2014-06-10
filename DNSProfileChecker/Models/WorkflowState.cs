using System.Collections.Generic;

namespace Nuance.Radiology.DNSProfileChecker.Models
{
	public class WorkflowState
	{
		public string SourcePath { get; set; }

		public List<ProfileEntry> Profiles { get; set; }

		public List<ProfileEntry> PreviouslySelectedProfiles { get; set; }
	}
}