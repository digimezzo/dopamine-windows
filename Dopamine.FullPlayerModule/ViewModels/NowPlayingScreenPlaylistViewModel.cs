using Dopamine.Common.Base;
using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Prism;
using Dopamine.FullPlayerModule.Views;
using Microsoft.Practices.Unity;
using System.Threading.Tasks;
using System;
using Dopamine.Common.Presentation.ViewModels.Base;

namespace Dopamine.FullPlayerModule.ViewModels
{
   public class NowPlayingScreenPlaylistViewModel : NowPlayingViewModelBase
    {
        #region Construction
        public NowPlayingScreenPlaylistViewModel(IUnityContainer container) : base(container)
        {
        }
        #endregion

        #region Overrides
        protected async override Task LoadedCommandAsync()
        {
            if (this.isFirstLoad)
            {
                this.isFirstLoad = false;

                await Task.Delay(Constants.NowPlayingListLoadDelay);  // Wait for the UI to slide in
                await this.FillListsAsync(); // Fill all the lists
            }
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
