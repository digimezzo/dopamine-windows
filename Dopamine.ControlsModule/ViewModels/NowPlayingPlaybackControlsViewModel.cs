using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Utils;
using Prism.Mvvm;

namespace Dopamine.ControlsModule.ViewModels
{
    public class NowPlayingPlaybackControlsViewModel : PlaybackControlsViewModelBase
    {
        #region ReadOnly Properties
        public bool IsShowcaseAvailable
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
                OnPropertyChanged(() => this.IsShowcaseAvailable);
            };

            this.playbackService.PlaybackFailed += (_, __) => OnPropertyChanged(() => this.IsShowcaseAvailable);
            this.playbackService.PlaybackPaused += (_, __) => OnPropertyChanged(() => this.IsShowcaseAvailable);
            this.playbackService.PlaybackResumed += (_, __) => OnPropertyChanged(() => this.IsShowcaseAvailable);
            this.playbackService.PlaybackSuccess += (_) => OnPropertyChanged(() => this.IsShowcaseAvailable);
        }
        #endregion
    }
}
