using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Utils;
using Prism.Mvvm;

namespace Dopamine.ControlsModule.ViewModels
{
    public class PlaybackControlsViewModelBase : BindableBase
    {
        #region Variables
        protected PlaybackInfoViewModel playbackInfoViewModel;
        protected IPlaybackService playbackService;
        #endregion

        #region Properties
        public PlaybackInfoViewModel PlaybackInfoViewModel
        {
            get { return this.playbackInfoViewModel; }
            set { SetProperty<PlaybackInfoViewModel>(ref this.playbackInfoViewModel, value); }
        }
        #endregion

        #region Construction
        public PlaybackControlsViewModelBase(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;

            this.playbackService.PlaybackProgressChanged += (_, __) => this.UpdateTime();

            this.Reset();
        }
        #endregion

        #region Private
        protected void UpdateTime()
        {
            this.PlaybackInfoViewModel.CurrentTime = FormatUtils.FormatTime(this.playbackService.GetCurrentTime);
            this.PlaybackInfoViewModel.TotalTime = " / " + FormatUtils.FormatTime(this.playbackService.GetTotalTime);
        }

        protected void Reset()
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
