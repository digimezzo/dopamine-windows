using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Utils;
using Prism.Mvvm;

namespace Dopamine.ControlsModule.ViewModels
{
    public class NowPlayingPlaybackControlsViewModel : BindableBase
    {
        #region Variables
        private PlaybackInfoViewModel playbackInfoViewModel;
        private IPlaybackService playbackService;
        #endregion

        #region ReadOnly Properties
        public bool IsShowcaseAvailable
        {
            get { return !this.playbackService.IsStopped; }
        }
        #endregion

        #region Properties
        public PlaybackInfoViewModel PlaybackInfoViewModel
        {
            get { return this.playbackInfoViewModel; }
            set { SetProperty<PlaybackInfoViewModel>(ref this.playbackInfoViewModel, value); }
        }
        #endregion

        #region Construction
        public NowPlayingPlaybackControlsViewModel(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;

            this.playbackService.PlaybackProgressChanged += (_, __) => this.UpdateTime();
            this.playbackService.PlaybackStopped += (_, __) =>
            {
                this.Reset();
                OnPropertyChanged(() => this.IsShowcaseAvailable);
            };

            this.playbackService.PlaybackFailed += (_, __) => OnPropertyChanged(() => this.IsShowcaseAvailable);
            this.playbackService.PlaybackPaused += (_, __) => OnPropertyChanged(() => this.IsShowcaseAvailable);
            this.playbackService.PlaybackResumed += (_, __) => OnPropertyChanged(() => this.IsShowcaseAvailable);
            this.playbackService.PlaybackSuccess += (_) => OnPropertyChanged(() => this.IsShowcaseAvailable);

            this.Reset();
        }
        #endregion

        #region Private
        private void UpdateTime()
        {
            this.PlaybackInfoViewModel.CurrentTime = FormatUtils.FormatTime(this.playbackService.GetCurrentTime);
            this.PlaybackInfoViewModel.TotalTime = " / " + FormatUtils.FormatTime(this.playbackService.GetTotalTime);
        }

        private void Reset()
        {
            this.PlaybackInfoViewModel = new PlaybackInfoViewModel
            {
                Title = "",
                Artist = "",
                Album = "",
                Year = "",
                CurrentTime = "",
                TotalTime = ""
            };
        }
        #endregion
    }
}
