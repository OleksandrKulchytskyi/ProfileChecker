using DNSProfileChecker.Common;
using Nuance.Radiology.DNSProfileChecker.Models;

namespace Nuance.Radiology.DNSProfileChecker.ViewModels
{
	public sealed class SourceSelectorViewModel : BaseViewModel
	{
		private readonly WorkflowState _state;
		public SourceSelectorViewModel(WorkflowState state)
			: base(state)
		{
			_state = state;
		}

		private string _profileSource;
		public string ProfileSource
		{
			get { return _profileSource; }
			set
			{
				_profileSource = value;
				base.NotifyOfPropertyChange(() => ProfileSource);
				base.NotifyOfPropertyChange(() => CanGoNext);
			}
		}

		public void GoNext()
		{
			var di = new System.IO.DirectoryInfo(ProfileSource);
			if (!di.Exists)
			{
				System.Windows.MessageBox.Show(string.Format("Path {0} doesn't exist.", ProfileSource), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
			}

			this.NextTransition = Models.StateTransition.SourceSelectorFinished;
			_state.SourcePath = ProfileSource;
			this.TryClose();
		}

		public bool CanGoNext
		{
			get
			{
				return ProfileSource.IsNotNullOrEmpty();
			}
		}
	}
}
