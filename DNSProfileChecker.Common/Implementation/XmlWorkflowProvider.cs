using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DNSProfileChecker.Common.Implementation
{
	public class XmlWorkflowProvider : IWorkflowProvider
	{
		public XmlWorkflowProvider()
		{
		}
		 
		public List<IProfileWorkflow> Initialize()
		{
			Ensure.Argument.NotNull(Parameters, "Parameters cannot be a null.");
			if (!System.IO.File.Exists(Parameters as string))
				throw new System.IO.FileNotFoundException(string.Format("File: {0} doenst exist.", Parameters as string));
			XDocument document = XDocument.Load(Parameters as string);
			IEnumerable<XElement> decendants = document.Root.Elements("workflow");
			List<IProfileWorkflow> result = new List<IProfileWorkflow>(decendants.Count());
			foreach (XElement node in decendants)
			{
				IProfileWorkflow workflow = provideWorkflow(node);
				if (workflow != null)
					result.Add(workflow);
			}

			return result;
		}

		private IProfileWorkflow provideWorkflow(XElement node)
		{
			if (node.Name == "workflow")
			{
				var typeAttribute = node.Attribute("type");
				Type workflowType = Type.GetType(typeAttribute.Value);
				var important = node.Attribute("important");
				bool isCritical = important == null ? false : important.Value == "1" ? true : false;
				object instance = Activator.CreateInstance(workflowType);
				IProfileWorkflow result = (instance as IProfileWorkflow);
				if (result != null) result.IsImportant = isCritical;

				if (node.HasElements)
				{
					IEnumerable<XElement> decendants = node.Element("subsequent").Elements("workflow");
					List<IProfileWorkflow> subsequent = new List<IProfileWorkflow>(decendants.Count());
					foreach (XElement inner in decendants)
					{
						IProfileWorkflow innerWorkflow = provideWorkflow(inner);
						if (inner != null)
							subsequent.Add(innerWorkflow);
					}
					if (subsequent.Count > 0)
					{
						Type wType = instance.GetType();
						var fieldInfo = wType.GetField("_innerHandlers", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetField | System.Reflection.BindingFlags.NonPublic);
						fieldInfo.SetValue(instance, subsequent);
					}
				}
				return result;
			}
			return null;
		}

		public object Parameters { get; set; }

	}
}
