using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DNSProfileChecker.Common;
using DNSProfileChecker.Common.Implementation;

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

			Assert.IsTrue(wf.State == WorkflowState.None);
			wf.Execute(@"\\DEV804\DragonUsers\AM10316_2\current\18_0_container");

			Assert.IsTrue(wf.State == WorkflowState.Success && wf.Description == string.Empty);
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
	}
}
