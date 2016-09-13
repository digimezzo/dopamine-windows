using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Utils;
using Prism.Mvvm;

namespace Dopamine.ControlsModule.ViewModels
{
    public class CollectionPlaybackControlsViewModel : PlaybackControlsViewModelBase
    {
        #region Properties
        public bool IsPlaying
        {
            get { return !this.playbackService.IsStopped & this.playbackService.IsPlaying; }
        }
        #endregion

        #region Construction
        public CollectionPlaybackControlsViewModel(IPlaybackService playbackService) : base(playbackService)
        {
            this.playbackService.PlaybackStopped += (_, __) =>
            {
                this.Reset();
                OnPropertyChanged(() => this.IsPlaying);
            };

            this.playbackService.PlaybackFailed += (_, __) => OnPropertyChanged(() => this.IsPlaying);
            this.playbackService.PlaybackPaused += (_, __) => OnPropertyChanged(() => this.IsPlaying);
            this.playbackService.PlaybackResumed += (_, __) => OnPropertyChanged(() => this.IsPlaying);
            this.playbackService.PlaybackSuccess += (_) => OnPropertyChanged(() => this.IsPlaying);
        }
        #endregion
    }
}
