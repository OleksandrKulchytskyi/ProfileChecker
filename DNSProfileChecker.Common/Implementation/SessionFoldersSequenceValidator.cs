using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DNSProfileChecker.Common.Implementation
{
	public sealed class SessionFoldersSequenceValidator : IValidator<DirectoryInfo[]>
	{
		private DirectoryInfo[] missedFolders;

		public bool Validate(DirectoryInfo[] sessions)
		{
			Ensure.Argument.NotNull(sessions, "sessions cannot be a null.");
			bool result = true;
			if (sessions.Length == 0)
				return result;
			string parentFolder = sessions[sessions.Length - 1].Parent.FullName;
			int[] values = new int[sessions.Length];

			for (int i = 0; i < sessions.Length; i++)
			{
				values[i] = int.Parse(sessions[i].Name.Remove(0, "session".Length));
			}

			List<DirectoryInfo> missed = new List<DirectoryInfo>();
			foreach (var missedIndx in Enumerable.Range(1, values.Last()).Except(values))
			{
				missed.Add(new DirectoryInfo(Path.Combine(parentFolder, "session" + missedIndx)));
			}

			if (missed.Count > 0)
			{
				result = false;
				missedFolders = missed.ToArray();
			}

			return result;
		}

		public DirectoryInfo[] MissedValues { get { return missedFolders; } }
	}
}