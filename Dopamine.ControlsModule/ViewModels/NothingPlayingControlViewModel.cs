using Dopamine.Common.Services.Playback;
using Prism.Commands;
using Prism.Mvvm;

namespace Dopamine.ControlsModule.ViewModels
{
    public class NothingPlayingControlViewModel : BindableBase
    {
        #region Private
        private IPlaybackService playbackService;
        #endregion

        #region Commands
        public DelegateCommand PlayAllCommand { get; set; }
        public DelegateCommand ShuffleAllCommand { get; set; }
        #endregion

        #region Construction
        public NothingPlayingControlViewModel(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;

            this.PlayAllCommand = new DelegateCommand(() =>
            {
                if (this.playbackService.Shuffle)
                    this.playbackService.SetShuffle(false);
                this.playbackService.Enqueue();
            });
            this.ShuffleAllCommand = new DelegateCommand(() =>
            {
                if (!this.playbackService.Shuffle)
                    this.playbackService.SetShuffle(true);
                this.playbackService.Enqueue();
            });
        }
        #endregion
    }
}
