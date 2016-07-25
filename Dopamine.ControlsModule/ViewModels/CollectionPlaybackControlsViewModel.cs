using Dopamine.Common.Services.Playback;
using Microsoft.Practices.Prism.Mvvm;

namespace Dopamine.ControlsModule.ViewModels
{
    public class CollectionPlaybackControlsViewModel : BindableBase
    {
        #region Variables
        private IPlaybackService playbackService;
        #endregion

        #region Properties
        public bool IsPlaying
        {
            get { return !this.playbackService.IsStopped & this.playbackService.IsPlaying; }
        }
        #endregion

        #region Construction
        public CollectionPlaybackControlsViewModel(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;

            this.playbackService.PlaybackFailed += (sender,e) => OnPropertyChanged(() => this.IsPlaying);
            this.playbackService.PlaybackPaused += (sender, e) => OnPropertyChanged(() => this.IsPlaying);
            this.playbackService.PlaybackResumed += (sender, e) => OnPropertyChanged(() => this.IsPlaying);
            this.playbackService.PlaybackStopped += (sender, e) => OnPropertyChanged(() => this.IsPlaying);
            this.playbackService.PlaybackSuccess += (isPlayingPreviousTrack) => OnPropertyChanged(() => this.IsPlaying);
        }
        #endregion
    }
}
