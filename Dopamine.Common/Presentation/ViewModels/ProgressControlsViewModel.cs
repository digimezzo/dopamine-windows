using Dopamine.Common.Services.Playback;
using Microsoft.Practices.Prism.Mvvm;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class ProgressControlsViewModel : BindableBase
    {
        #region Variables
        protected IPlaybackService playBackService;
        private double progressValue;
        #endregion
        private bool canReportProgress;

        #region Properties
        public double ProgressValue
        {
            get { return this.progressValue; }
            // We misuse the Property Setter to only set the PlayBackService Progress.
            // OnPropertyChanged is fired by the returning PlayBackService.PlaybackProgressChanged event.
            // This prevents a StackOverflow (infinite loop between the ProgressValue Property and the 
            // PlayBackService.PlaybackProgressChanged event.
            set { SetPlayBackServiceProgress(value); }
        }

        public bool CanReportProgress
        {
            get { return this.canReportProgress; }
            set { SetProperty<bool>(ref this.canReportProgress, value); }
        }
        #endregion

        #region Construction
        public ProgressControlsViewModel(IPlaybackService playBackService)
        {
            this.playBackService = playBackService;

            this.playBackService.PlaybackProgressChanged += (sender,e) => this.GetPlayBackServiceProgress();
            this.playBackService.PlaybackFailed += (sender, e) => this.GetPlayBackServiceProgress();
            this.playBackService.PlaybackStopped += (sender, e) => this.GetPlayBackServiceProgress();
            this.playBackService.PlaybackSuccess += (isPlayingPreviousTrack) => this.GetPlayBackServiceProgress();
        }
        #endregion

        #region Private
        private void SetPlayBackServiceProgress(double iProgress)
        {
            this.playBackService.Skip(iProgress);
        }

        protected virtual void GetPlayBackServiceProgress()
        {
            // Important: set ProgressValue directly, not the ProgressValue 
            // Property, because the ProgressValue Property Setter is empty!
            this.progressValue = this.playBackService.Progress;
            OnPropertyChanged(() => this.ProgressValue);

            // This makes sure the progress bar is not clickable when the player is not playing
            if (!this.playBackService.IsStopped)
            {
                this.CanReportProgress = true;
            }
            else
            {
                this.CanReportProgress = false;
            }
        }
        #endregion
    }

}
