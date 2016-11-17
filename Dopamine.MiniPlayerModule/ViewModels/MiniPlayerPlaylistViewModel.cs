using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Core.Base;
using Dopamine.Core.Prism;
using Dopamine.MiniPlayerModule.Views;
using System.Threading.Tasks;

namespace Dopamine.MiniPlayerModule.ViewModels
{
    public class MiniPlayerPlaylistViewModel : NowPlayingViewModel
    {
        #region Construction
        public MiniPlayerPlaylistViewModel() : base()
        {
            this.eventAggregator.GetEvent<RemoveSelectedTracks>().Subscribe((screenName) =>
            {
                if (screenName == typeof(MiniPlayerPlaylist).FullName) this.RemoveFromNowPlayingCommand.Execute();
            });
        }
        #endregion

        #region Overrides
        protected override async Task LoadedCommandAsync()
        {
            if (this.isFirstLoad)
            {
                this.isFirstLoad = false;

                await Task.Delay(Constants.MiniPlayerListLoadDelay); // Wait for the UI to slide in
                await this.FillListsAsync(); // Fill all the lists
            }
        }
        #endregion
    }

}
