using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DNSProfileChecker.Common;
using DNSProfileChecker.Common.Implementation;

namespace DNSProfileChecker.UnitTest
{
	[TestClass]
	public class WorkflowProviderUnitTest
	{
		private static string directory;

		[ClassInitializeAttribute]
		public static void Init(TestContext ctx)
		{
			var asm = System.Reflection.Assembly.GetExecutingAssembly();
			if (asm != null)
				directory = System.IO.Path.GetDirectoryName(asm.Location);
		}

		[TestMethod]
		public void TestMethod1()
		{
			IWorkflowProvider provider = new XmlWorkflowProvider();
			provider.Parameters = System.IO.Path.Combine(directory, "workflows.xml");
			var result = provider.Initialize();
			Assert.IsTrue(result[2].SubsequentWorkflows.Count == 2);
		}
	}
}
