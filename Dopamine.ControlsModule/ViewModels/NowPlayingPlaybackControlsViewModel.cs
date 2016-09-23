using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Utils;
using Prism.Mvvm;

namespace Dopamine.ControlsModule.ViewModels
{
    public class NowPlayingPlaybackControlsViewModel : PlaybackControlsViewModelBase
    {
        #region ReadOnly Properties
        public bool HasPlaybackInfo
        {
            get { return !this.playbackService.IsStopped; }
        }
        #endregion

        #region Construction
        public NowPlayingPlaybackControlsViewModel(IPlaybackService playbackService) : base(playbackService)
        {
            this.playbackService.PlaybackStopped += (_, __) =>
            {
                this.Reset();
                OnPropertyChanged(() => this.HasPlaybackInfo);
            };

            this.playbackService.PlaybackFailed += (_, __) => OnPropertyChanged(() => this.HasPlaybackInfo);
            this.playbackService.PlaybackPaused += (_, __) => OnPropertyChanged(() => this.HasPlaybackInfo);
            this.playbackService.PlaybackResumed += (_, __) => OnPropertyChanged(() => this.HasPlaybackInfo);
            this.playbackService.PlaybackSuccess += (_) => OnPropertyChanged(() => this.HasPlaybackInfo);
        }
        #endregion
    }
}
