using DNSProfileChecker.Common;
using System.IO;

namespace DNSProfileChecker.Workflow
{
	public class SessionVerifierWorkflow : BaseWorkflow
	{
		public SessionVerifierWorkflow()
			: base()
		{
		}

		public override void Execute(object parameter)
		{
			Ensure.Argument.NotNull(parameter, "parameter value cannot be a null.");
			string folderPath = parameter as string;
			bool isWarned = false;

			DirectoryInfo draFolder = new DirectoryInfo(Path.Combine(folderPath, "drafiles"));
			if (!draFolder.Exists && IsImportant)
			{
				State = WorkflowState.Failed;
				Description = string.Format("Folder drafiles doesn't exist in the root folder: {0}", folderPath);
				DoLog(LogSeverity.Error, Description, null);
				return;
			}
			else if (!draFolder.Exists && !IsImportant)
			{
				DoLog(LogSeverity.Warn, string.Format("Folder drafiles doesn't exist in the root folder: {0}", folderPath), null);
				isWarned = true;
			}

			FileInfo draIniFile = new FileInfo(Path.Combine(folderPath, "drafiles.ini"));
			if (!draIniFile.Exists && IsImportant)
			{
				State = WorkflowState.Failed;
				Description = string.Format("File drafiles.ini doesn't exist in the root folder: {0}", folderPath);
				DoLog(LogSeverity.Error, Description, null);
				return;
			}
			else if (!draFolder.Exists && !IsImportant)
			{
				DoLog(LogSeverity.Warn, string.Format("File drafiles.ini doesn't exist in the root folder: {0}", folderPath), null);
				isWarned = true;
			}

			FileInfo acarchiveNWM = new FileInfo(Path.Combine(folderPath, "acarchive.nwv"));
			if (!acarchiveNWM.Exists && IsImportant)
			{
				State = WorkflowState.Failed;
				Description = string.Format("File acarchive.nwv doesn't exist in the root folder: {0}", folderPath);
				DoLog(LogSeverity.Error, Description, null);
				return;
			}
			else if (!acarchiveNWM.Exists && !IsImportant)
			{
				DoLog(LogSeverity.Warn, string.Format("File acarchive.nwv doesn't exist in the root folder: {0}", folderPath), null);
				isWarned = true;
			}

			FileInfo acarchiveENWM = new FileInfo(Path.Combine(folderPath, "acarchive.enwv"));
			if (!acarchiveENWM.Exists && IsImportant)
			{
				State = WorkflowState.Failed;
				Description = string.Format("File acarchive.enwvdoesn't exist in the root folder: {0}", folderPath);
				DoLog(LogSeverity.Error, Description, null);
				return;
			}
			else if (!acarchiveENWM.Exists && IsImportant)
			{
				DoLog(LogSeverity.Warn, string.Format("File acarchive.enwv doesn't exist in the root folder: {0}", folderPath), null);
				isWarned = true;
			}

			if (isWarned)
				State = WorkflowState.Warn;
			else
			{
				State = WorkflowState.Success;
				DoLog(LogSeverity.Info, string.Format("Session folder named {0} is correct.", folderPath), null);
			}
		}
	}
}
