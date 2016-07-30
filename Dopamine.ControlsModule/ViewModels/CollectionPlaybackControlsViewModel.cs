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

            this.playbackService.PlaybackFailed += (_, __) => OnPropertyChanged(() => this.IsPlaying);
            this.playbackService.PlaybackPaused += (_, __) => OnPropertyChanged(() => this.IsPlaying);
            this.playbackService.PlaybackResumed += (_, __) => OnPropertyChanged(() => this.IsPlaying);
            this.playbackService.PlaybackStopped += (_, __) => OnPropertyChanged(() => this.IsPlaying);
            this.playbackService.PlaybackSuccess += (_) => OnPropertyChanged(() => this.IsPlaying);
        }
        #endregion
    }
}
