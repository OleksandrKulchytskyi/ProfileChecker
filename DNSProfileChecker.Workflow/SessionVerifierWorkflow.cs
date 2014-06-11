using DNSProfileChecker.Common;
using System;
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
			IFileFactory fact = new Common.Factories.FileFactory();

			DirectoryInfo draFolder = new DirectoryInfo(Path.Combine(folderPath, "drafiles"));
			if (!draFolder.Exists && IsImportant)
			{
				State = WorkflowStates.Failed;
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
			if (!draIniFile.Exists)
			{
				isWarned = true;
				Description = string.Format("File drafiles.ini doesn't exist in the root folder: {0}", folderPath);
				DoLog(LogSeverity.Error, Description, null);

				StreamWriter sw = null;
				try
				{
					sw = fact.CreateFile(draIniFile.FullName, FileFactoryEnum.DRAFilesINI);
					sw.Flush();
					DoLog(LogSeverity.Success, "File drafiles.ini has been successfully created.", null);
				}
				catch (Exception ex)
				{
					State = WorkflowStates.Failed;
					DoLog(LogSeverity.Error, "Unable to create drafiles.ini file.", ex);
				}
				finally { if (sw != null) sw.Dispose(); }
			}

			FileInfo acarchiveINI = new FileInfo(Path.Combine(folderPath, "acarchive.ini"));
			if (!acarchiveINI.Exists)
			{
				isWarned = true;
				Description = string.Format("File acarchive.ini doesn't exist in the root folder: {0}", folderPath);
				DoLog(LogSeverity.Error, Description, null);

				StreamWriter sw = null;
				try
				{
					sw = fact.CreateFile(acarchiveINI.FullName, FileFactoryEnum.AcarchiveINI);
					DoLog(LogSeverity.Success, "File acarchive.ini has been successfully created.", null);
				}
				catch (Exception ex)
				{
					State = WorkflowStates.Failed;
					DoLog(LogSeverity.Error, "Unable to create acarchive.ini file.", ex);
				}
				finally { if (sw != null) sw.Dispose(); }
			}

			FileInfo acarchiveNWM = new FileInfo(Path.Combine(folderPath, "acarchive.nwv"));
			FileInfo acarchiveENWM = new FileInfo(Path.Combine(folderPath, "acarchive.enwv"));
			bool missedBoth = (!acarchiveENWM.Exists && !acarchiveNWM.Exists);
			if (missedBoth)
			{
				State = WorkflowStates.Failed;
				Description = string.Format("Both files acarchive.nwv and acarchive.enwv aren't exist in the session folder: {0}", folderPath);
				DoLog(LogSeverity.Error, Description, null);
				DoLog(LogSeverity.Warn, string.Format("Folder {0} will be deleted.", folderPath), null);
				return;
			}
			else
			{
				State = WorkflowStates.Success;
				if (!acarchiveNWM.Exists)
					Description = string.Format("File acarchive.nwv doesn't exist in the session folder: {0}", folderPath);
				if (!acarchiveENWM.Exists)
					Description = string.Format("File acarchive.enwv doesn't exist in the session folder: {0}", folderPath);

				DoLog(LogSeverity.Warn, Description, null);
			}

			if (isWarned)
				State = WorkflowStates.Warn;
			else
			{
				State = WorkflowStates.Success;
				DoLog(LogSeverity.Info, string.Format("Session folder [{0}] is correct.", folderPath), null);
			}
		}
	}
}
