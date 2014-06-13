using System.Collections.Generic;

namespace Nuance.Radiology.DNSProfileChecker.Models
{
	public class WorkflowState
	{
		public string SourcePath { get; set; }

		public List<ProfileEntry> ProfilesToCheck { get; set; }

		public List<ProfileEntry> PreviouslySelectedProfiles { get; set; }
		public List<ProfileEntry> OldAvaliable { get; set; }
		public List<ProfileEntry> OldToCheck { get; set; }
		/// <summary>
		/// Flag indicating that profiles have been already loaded for some specific path
		/// </summary>
		public bool IsProfilesLoaded { get; set; }


		public void ClearState()
		{
			IsProfilesLoaded = false;
			OldAvaliable = null;
			OldToCheck = null;
			PreviouslySelectedProfiles = null;
		}
	}
}