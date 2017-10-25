using Dopamine.Common.Services.Playback;
using Prism.Commands;
using Prism.Mvvm;

namespace Dopamine.Common.Presentation.ViewModels
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
