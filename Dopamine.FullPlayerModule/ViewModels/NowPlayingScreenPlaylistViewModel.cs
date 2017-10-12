using Dopamine.Common.Base;
using Dopamine.Common.Presentation.ViewModels.Base;
using Microsoft.Practices.Unity;
using System.Threading.Tasks;

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
            if (!this.IsFirstLoad()) return;
            await Task.Delay(Constants.NowPlayingListLoadDelay);  // Wait for the UI to slide in
            await this.FillListsAsync(); // Fill all the lists
        }
        #endregion
    }
}
