using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Core.Base;
using Dopamine.Core.Prism;
using Dopamine.FullPlayerModule.Views;
using System.Threading.Tasks;

namespace Dopamine.FullPlayerModule.ViewModels
{
    public class NowPlayingScreenPlaylistViewModel : NowPlayingViewModel
    {
        #region Construction
        public NowPlayingScreenPlaylistViewModel() : base()
        {
            this.eventAggregator.GetEvent<RemoveSelectedTracks>().Subscribe((screenName) =>
            {
                if (screenName == typeof(NowPlayingScreenPlaylist).FullName) this.RemoveFromNowPlayingCommand.Execute();
            });
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
        #endregion
    }
}
