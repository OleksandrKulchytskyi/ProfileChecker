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
			if (!draFolder.Exists)
			{
				State = WorkflowStates.Warn;
				isWarned = true;
				Description = string.Format("Folder drafiles doesn't exist in the root folder: {0}", folderPath);
				DoLog(LogSeverity.Warn, Description, null);

				try
				{
					Directory.CreateDirectory(draFolder.FullName);
				}
				catch (Exception ex)
				{
					DoLog(LogSeverity.Error, string.Format("Unable to craete drafiles folder in the root folder: {0}", folderPath), ex);
				}
			}

			//check for existance of drafiles.ini
			FileInfo draIniFile = new FileInfo(Path.Combine(folderPath, "drafiles.ini"));
			if (!draIniFile.Exists)
			{
				isWarned = true;
				Description = string.Format("File drafiles.ini doesn't exist in the root folder: {0}", folderPath);
				DoLog(LogSeverity.Warn, Description, null);

				StreamWriter sw = null;
				try
				{
					sw = fact.CreateFile(draIniFile.FullName, FileFactoryEnum.DRAFilesINI);
					sw.Flush();
					DoLog(LogSeverity.Success, string.Format("File drafiles.ini has been created in the root: {0}", folderPath), null);
				}
				catch (Exception ex)
				{
					State = WorkflowStates.Failed;
					DoLog(LogSeverity.Error, "Unable to create drafiles.ini file.", ex);
				}
				finally { if (sw != null) sw.Dispose(); }
			}

			//check for existance of acarchive.ini
			FileInfo acarchiveINI = new FileInfo(Path.Combine(folderPath, "acarchive.ini"));
			if (!acarchiveINI.Exists)
			{
				isWarned = true;
				Description = string.Format("File acarchive.ini doesn't exist in the root folder: {0}", folderPath);
				DoLog(LogSeverity.Warn, Description, null);

				StreamWriter sw = null;
				try
				{
					sw = fact.CreateFile(acarchiveINI.FullName, FileFactoryEnum.AcarchiveINI);
					DoLog(LogSeverity.Success, string.Format("File acarchive.ini has been created in: {0}", folderPath), null);
				}
				catch (Exception ex)
				{
					State = WorkflowStates.Failed;
					DoLog(LogSeverity.Error, string.Format("Unable to create acarchive.ini file in the directory {0}.", folderPath), ex);
				}
				finally { if (sw != null) sw.Dispose(); }
			}

			//check for existance of files with extensions: nwv and enwv 
			FileInfo acarchiveNWM = new FileInfo(Path.Combine(folderPath, "acarchive.nwv"));
			FileInfo acarchiveENWM = new FileInfo(Path.Combine(folderPath, "acarchive.enwv"));
			bool missedBoth = (!acarchiveENWM.Exists && !acarchiveNWM.Exists);
			if (missedBoth)
			{
				State = WorkflowStates.Failed;
				Description = string.Format("Both files acarchive.nwv and acarchive.enwv aren't exist in the session folder: {0}", folderPath);
				DoLog(LogSeverity.Warn, Description, null);

				try
				{
					Directory.Delete(folderPath, true);
					DoLog(LogSeverity.Success, string.Format("Folder {0} has been deleted.", folderPath), null);
				}
				catch (Exception ex)
				{
					DoLog(LogSeverity.Error, string.Format("Unable to delete session folder: {0}.", folderPath), ex);
				}
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
