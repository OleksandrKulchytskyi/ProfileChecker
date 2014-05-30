using DNSProfileChecker.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSProfileChecker.Workflow
{
	public class BaseWorkflow : IProfileWorkflow
	{
		public BaseWorkflow()
		{
			State = WorkflowState.None;
			Description = string.Empty;
		}

		public ILogger Logger { get; set; }

		public string Description { get; set; }
		public WorkflowState State { get; set; }
		public bool IsImportant { get; set; }

		protected List<IProfileWorkflow> _innerHandlers;
		public List<IProfileWorkflow> SubsequentWorkflows { get { return _innerHandlers; } }

		protected virtual void DoLog(LogSeverity severity, string message, Exception ex)
		{
			if (Logger != null)
				Logger.LogData(severity, message, ex);
		}

		public virtual void Execute(object parameter)
		{
			if (SubsequentWorkflows != null)
			{
				bool isExceptional = false;
				Exception exc = null;
				foreach (IProfileWorkflow w in SubsequentWorkflows)
				{
					try
					{
						w.Execute(parameter);
						if (w.State == WorkflowState.None)
							w.State = WorkflowState.Success;
					}
					catch (Exception ex)
					{
						exc = ex;
						isExceptional = true;
					}
					if (!isExceptional)
					{
						if (w.State == WorkflowState.NotApplied)
							continue;

						else if (w.IsImportant && w.State != WorkflowState.Success)
						{
							this.Description = w.Description;
							this.State = w.State;
							break;
						}
					}
					else
					{
						if (w.IsImportant)
						{
							this.State = WorkflowState.Exceptional;
							this.Description = exc.ToString();
							DoLog(LogSeverity.Error, string.Format("Error has been occurred in {0} workflow", w.GetType().ToString()), exc);
							break;
						}
						else
							DoLog(LogSeverity.Warn, string.Format("Error has been occurred in {0} workflow", w.GetType().ToString()), exc);
					}
				}
			}
		}
	}
}
