using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Base;
using Dopamine.Common.Prism;
using Dopamine.MiniPlayerModule.Views;
using Microsoft.Practices.Unity;
using System.Threading.Tasks;

namespace Dopamine.MiniPlayerModule.ViewModels
{
    public class MiniPlayerPlaylistViewModel : NowPlayingViewModel
    {
        #region Construction
        public MiniPlayerPlaylistViewModel(IUnityContainer container) : base(container)
        {
            this.eventAggregator.GetEvent<RemoveSelectedTracksWithKeyDelete>().Subscribe((screenName) =>
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
