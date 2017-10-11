using Dopamine.Common.Presentation.ViewModels.Base;
using Dopamine.Common.Presentation.ViewModels.Entities;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Utils;
using Microsoft.Practices.Unity;

namespace Dopamine.ControlsModule.ViewModels
{
    public class PlaybackControlsViewModelBase : ContextMenuViewModelBase
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
        public PlaybackControlsViewModelBase(IUnityContainer container) : base(container)
        {
            this.playbackService = container.Resolve<IPlaybackService>();

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
                Title = string.Empty,
                Artist = string.Empty,
                Album = string.Empty,
                Year = string.Empty,
                CurrentTime = string.Empty,
                TotalTime = string.Empty
            };
        }

        protected override void SearchOnline(string id)
        {
            // No implementation required here
        }
        #endregion
    }
}
