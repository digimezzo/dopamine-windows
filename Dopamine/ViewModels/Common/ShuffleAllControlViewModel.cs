using Dopamine.Services.Playback;
using Dopamine.Services.Playback;
using Prism.Commands;
using Prism.Mvvm;

namespace Dopamine.ViewModels.Common
{
    public class ShuffleAllControlViewModel : BindableBase
    {
        private IPlaybackService playbackService;
     
        public DelegateCommand ShuffleAllCommand { get; set; }
     
        public ShuffleAllControlViewModel(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;

            this.ShuffleAllCommand = new DelegateCommand(() => this.playbackService.EnqueueAsync(true, false));
        }
    }
}
