using Dopamine.Common.Services.Playback;
using Prism.Commands;
using Prism.Mvvm;

namespace Dopamine.ControlsModule.ViewModels
{
    public class ShuffleAllControlViewModel : BindableBase
    {
        #region Private
        private IPlaybackService playbackService;
        #endregion

        #region Commands
        public DelegateCommand ShuffleAllCommand { get; set; }
        #endregion

        #region Construction
        public ShuffleAllControlViewModel(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;

            this.ShuffleAllCommand = new DelegateCommand(() =>
            {
                if (!this.playbackService.Shuffle) this.playbackService.SetShuffleAsync(true);
                this.playbackService.Enqueue();
            });
        }
        #endregion
    }
}
