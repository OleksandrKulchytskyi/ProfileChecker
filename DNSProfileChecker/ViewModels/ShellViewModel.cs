using Caliburn.Micro;
using Nuance.Radiology.DNSProfileChecker.Models;
using Nuance.Radiology.DNSProfileChecker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common = DNSProfileChecker.Common;

namespace Nuance.Radiology.DNSProfileChecker.ViewModels
{
	public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, Common.Interfaces.IShell
	{
		public ShellViewModel()
		{
			initializeMap();

			activateFirstScreen();
		}

		private void activateFirstScreen()
		{
			var screen = new SourceSelectorViewModel(new WorkflowState());
			this.ActivateItem(screen);
		}


		private void initializeMap()
		{
			TransitionMap.Add<SourceSelectorViewModel, ProfileFilerViewModel>(StateTransition.Input1Success);
			TransitionMap.Add<SourceSelectorViewModel, SourceSelectorViewModel>(StateTransition.Cancel);

			//TransitionMap.Add<Question2ViewModel, Input3ViewModel>(StateTransition.Option1);
			//TransitionMap.Add<Question2ViewModel, Finalize4ViewModel>(StateTransition.Option2);
			//TransitionMap.Add<Question2ViewModel, Input1ViewModel>(StateTransition.Cancel);

			//TransitionMap.Add<Input3ViewModel, Finalize4ViewModel>(StateTransition.Input3Success);
			//TransitionMap.Add<Input3ViewModel, Input1ViewModel>(StateTransition.Cancel);

			//TransitionMap.Add<Finalize4ViewModel, Input1ViewModel>(StateTransition.Cancel);
		}


		protected override IScreen DetermineNextItemToActivate(IList<IScreen> list, int lastIndex)
		{
			var theScreenThatJustClosed = list[lastIndex] as BaseViewModel;
			var state = theScreenThatJustClosed.WorkflowState;

			var nextScreenType = TransitionMap.GetNextScreenType(theScreenThatJustClosed);

			var nextScreen = Activator.CreateInstance(nextScreenType, state);

			return nextScreen as IScreen;
		}
	}
}
