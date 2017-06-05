using Microsoft.Practices.Unity;

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
        public CollectionPlaybackControlsViewModel(IUnityContainer container) : base(container)
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
