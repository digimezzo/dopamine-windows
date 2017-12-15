using Dopamine.Core.Utils;
using Dopamine.Services.Contracts.Playback;
using Microsoft.Practices.ServiceLocation;
using System;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class ProgressControlsWithTimeViewModel : ProgressControlsViewModel
    {
        private string currentTime;
        private string totalTime;
      
        public string CurrentTime
        {
            get { return this.currentTime; }
            set { SetProperty<string>(ref this.currentTime, value); }
        }

        public string TotalTime
        {
            get { return this.totalTime; }
            set { SetProperty<string>(ref this.totalTime, value); }
        }
  
        public ProgressControlsWithTimeViewModel() : base(ServiceLocator.Current.GetInstance<IPlaybackService>())
        {
            this.CurrentTime = FormatUtils.FormatTime(new TimeSpan(0));
            this.TotalTime = FormatUtils.FormatTime(new TimeSpan(0));
        }
    
        protected override void GetPlayBackServiceProgress()
        {
            base.GetPlayBackServiceProgress();

            this.CurrentTime = FormatUtils.FormatTime(this.playBackService.GetCurrentTime);
            this.TotalTime = FormatUtils.FormatTime(this.playBackService.GetTotalTime);
        }
    }
}
