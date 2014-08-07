using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DNSProfileChecker.Common;
using DNSProfileChecker.Common.Extension;
using DNSProfileChecker.Common.Implementation;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace DNSProfileChecker.UnitTest
{
	[TestClass]
	public class WorkflowsUnitTests
	{
		private static string directory;
		private static ILogger logger;

		[ClassInitializeAttribute]
		public static void Init(TestContext ctx)
		{
			var asm = System.Reflection.Assembly.GetExecutingAssembly();
			if (asm != null)
				directory = System.IO.Path.GetDirectoryName(asm.Location);

			ILogProvider provider = new AppConfigLogProvider();
			LoggerBridge bridge = new LoggerBridge(provider);

			logger = bridge;
		}

		[TestMethod]
		public void TestSmallWorkflow()
		{
			IWorkflowProvider provider = new XmlWorkflowProvider();
			provider.Parameters = System.IO.Path.Combine(directory, "workflows.xml");
			var result = provider.Initialize();

			IProfileWorkflow wf = result[2].SubsequentWorkflows.FirstOrDefault(x => x.GetType() == typeof(DNSProfileChecker.Workflow.SmallContainerWorkflow));
			assignLogger(wf, logger);

			Assert.IsTrue(wf.State == WorkflowStates.None);
			wf.Execute(@"\\DEV804\DragonUsers\AM10316_2\current\18_0_container");

			Assert.IsTrue(wf.State == WorkflowStates.Success && wf.Description == string.Empty);
		}

		private void assignLogger(IProfileWorkflow wf, ILogger logger)
		{
			if (logger != null)
			{
				wf.Logger = logger;
				if (wf.SubsequentWorkflows != null)
				{
					foreach (var item in wf.SubsequentWorkflows)
					{
						assignLogger(item, logger);
					}
				}
			}
		}

		[TestMethod]
		public void TestProfileFixMethod()
		{
			PerformProfileCleanup(@"\\DEV804\DragonUsers2\abdalahmr");
		}

		private void PerformProfileCleanup(string userProfile)
		{
			if (!ConfigurationManager.AppSettings.AllKeys.Contains("enableProfileCheck"))
				return;

			if (ConfigurationManager.AppSettings["enableProfileCheck"].ToLower() != "true")
				return;

			ILogger logger = new DebugLogger();
			IProfileAssurance profileAssert = new ProfileAssurance();
			IFileFactory filesFactory = new DNSProfileChecker.Common.Factories.FileFactory();
			IWorkflowProvider workflowsProvider = new XmlWorkflowProvider();
			workflowsProvider.Parameters = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Workflows.xml");

			List<IProfileWorkflow> workflows = null;
			try
			{
				workflows = workflowsProvider.Initialize();
				foreach (IProfileWorkflow workflow in workflows)
				{
					assignLoggerAndStateToWorkflow(workflow, logger);
				}
			}
			catch (Exception ex)
			{
				logger.LogData(LogSeverity.Error, "Unable to initialize workflows.", ex);
				return;
			}

			bool isErrorOccurred = false;
			Exception error = null;
			bool isProfileCorrect = true;

			logger.LogData(LogSeverity.UI, string.Format("Begin to process {0} DNS profile.", userProfile), null);
			foreach (IProfileWorkflow workflow in workflows)
			{
				try
				{
					isErrorOccurred = false;
					error = null;
					workflow.Execute(userProfile);
				}
				catch (Exception ex)
				{
					isErrorOccurred = true;
					error = ex;
				}

				if (isErrorOccurred)
				{
					isProfileCorrect = false;
					logger.LogData(LogSeverity.Error, error.Message, error);
					continue;
				}

				if (workflow.IsImportant)
				{
					if (workflow.State == WorkflowStates.Failed || workflow.State == WorkflowStates.Exceptional || workflow.State == WorkflowStates.Warn)
					{
						if (workflow.State != WorkflowStates.Warn && workflow.Description.IsNotNullOrEmpty())
							logger.LogData(LogSeverity.Error, workflow.Description, null);
						isProfileCorrect = false;
						continue;
					}
				}
				else
				{
					if (workflow.State == WorkflowStates.Failed || workflow.State == WorkflowStates.Exceptional || workflow.State == WorkflowStates.Warn)
					{
						logger.LogData(LogSeverity.Warn, workflow.Description, null);
						isProfileCorrect = false;
						continue;
					}
				}

				isProfileCorrect = workflow.IsProfileMatchState(WorkflowStates.Success);
			}//end loop for workwflows

			if (isProfileCorrect)
				logger.LogData(LogSeverity.Success, string.Format("Profile {0} has been checked with no error(s).", userProfile), null);
			else
				logger.LogData(LogSeverity.UI, string.Format("Profile [{0}] has been verified.", userProfile), null);
		}

		private void assignLoggerAndStateToWorkflow(IProfileWorkflow wf, ILogger logger)
		{
			if (logger != null)
			{
				wf.Logger = logger;
				wf.IsSimulationMode = false;
				if (wf.SubsequentWorkflows == null)
					return;

				foreach (var item in wf.SubsequentWorkflows)
				{
					assignLoggerAndStateToWorkflow(item, logger);
				}
			}
		}
	}
}
