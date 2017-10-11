using Dopamine.Common.Services.Playback;
using Prism.Commands;
using Prism.Mvvm;

namespace Dopamine.Common.Presentation.ViewModels
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

            this.ShuffleAllCommand = new DelegateCommand(() => this.playbackService.EnqueueAsync(true, false));
        }
        #endregion
    }
}
