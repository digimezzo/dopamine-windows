using Dopamine.Common.Base;
using Dopamine.Common.Presentation.ViewModels.Base;
using Microsoft.Practices.Unity;
using Prism.Commands;
using System.Threading.Tasks;
using System;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class NowPlayingControlViewModel : NowPlayingViewModelBase
    {
        #region Commands
        public DelegateCommand<bool?> UpdateShowTrackArtCommand { get; set; }
        #endregion

        #region Construction
        public NowPlayingControlViewModel(IUnityContainer container) : base(container)
        {
            this.UpdateShowTrackArtCommand = new DelegateCommand<bool?>((showTrackArt) =>
            {
                this.UpdateShowTrackArtAsync(showTrackArt.Value);
            });
        }
        #endregion

        #region Overrides
        protected async override Task LoadedCommandAsync()
        {
            if (!this.IsFirstLoad()) return;
            await Task.Delay(Constants.NowPlayingListLoadDelay);  // Wait for the UI to slide in
            await this.FillListsAsync(); // Fill all the lists
        }

        protected override void Subscribe()
        {
            // Not required here
        }

        protected override void Unsubscribe()
        {
            // Not required here
        }
        #endregion
    }
}
