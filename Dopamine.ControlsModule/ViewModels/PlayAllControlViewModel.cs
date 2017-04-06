using Dopamine.Common.Services.Playback;
using Prism.Commands;
using Prism.Mvvm;

namespace Dopamine.ControlsModule.ViewModels
{
    public class PlayAllControlViewModel : BindableBase
    {
        #region Private
        private IPlaybackService playbackService;
        #endregion

        #region Commands
        public DelegateCommand PlayAllCommand { get; set; }
        #endregion

        #region Construction
        public PlayAllControlViewModel(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;

            this.PlayAllCommand = new DelegateCommand(() => this.playbackService.EnqueueAsync(false,true));
        }
        #endregion
    }
}
