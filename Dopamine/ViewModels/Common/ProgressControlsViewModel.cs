using Dopamine.Services.Playback;
using Prism.Mvvm;

namespace Dopamine.ViewModels.Common
{
    public class ProgressControlsViewModel : BindableBase
    {
        protected IPlaybackService playBackService;
        private double progressValue;
        private bool canReportProgress;
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
   
        public ProgressControlsViewModel(IPlaybackService playBackService)
        {
            this.playBackService = playBackService;

            this.playBackService.PlaybackProgressChanged += (sender,e) => this.GetPlayBackServiceProgress();
            this.playBackService.PlaybackFailed += (_, __) => this.GetPlayBackServiceProgress();
            this.playBackService.PlaybackStopped += (_, __) => this.GetPlayBackServiceProgress();
            this.playBackService.PlaybackSuccess += (_,__) => this.GetPlayBackServiceProgress();
        }
        
        private void SetPlayBackServiceProgress(double progress)
        {
            this.playBackService.SkipProgress(progress);
        }

        protected virtual void GetPlayBackServiceProgress()
        {
            // Important: set ProgressValue directly, not the ProgressValue 
            // Property, because the ProgressValue Property Setter is empty!
            this.progressValue = this.playBackService.Progress;
            RaisePropertyChanged(nameof(this.ProgressValue));

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
    }
}
