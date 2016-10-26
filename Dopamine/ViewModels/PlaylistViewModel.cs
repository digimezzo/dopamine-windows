using Dopamine.Common.Services.Playback;
using Dopamine.ControlsModule.Views;
using Dopamine.Core.Prism;
using Dopamine.MiniPlayerModule.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace Dopamine.ViewModels
{
    public class PlaylistViewModel : BindableBase
    {

        #region Variables
        private IPlaybackService playbackService;
        private IRegionManager regionManager;
        #endregion

        #region Commands
        public DelegateCommand LoadedCommand { get; set; }
        #endregion

        #region Construction
        public PlaylistViewModel(IPlaybackService playbackService, IRegionManager regionManager)
        {
            this.playbackService = playbackService;
            this.regionManager = regionManager;

            this.LoadedCommand = new DelegateCommand(() => this.SetNowPlaying());

            this.playbackService.PlaybackSuccess += (_) => this.SetNowPlaying();
        }
        #endregion

        #region Private
        private void SetNowPlaying()
        {
            if (this.playbackService.Queue.Count > 0)
            {
                this.regionManager.RequestNavigate(RegionNames.MiniPlayerPlaylistRegion, typeof(MiniPlayerPlaylist).FullName);
            }
            else
            {
                this.regionManager.RequestNavigate(RegionNames.MiniPlayerPlaylistRegion, typeof(NothingPlayingControl).FullName);
            }
        }
        #endregion
    }
}
