using Dopamine.Services.Contracts.Dialog;
using Dopamine.ViewModels.Common.Base;
using Prism.Ioc;

namespace Dopamine.ViewModels.Common
{
    public class CollectionPlaybackControlsViewModel : PlaybackControlsViewModelBase
    {
        public bool IsPlaying
        {
            get { return !this.PlaybackService.IsStopped & this.PlaybackService.IsPlaying; }
        }

        public CollectionPlaybackControlsViewModel(IContainerProvider container, IDialogService dialogService) : base(container)
        {
            this.PlaybackService.PlaybackStopped += (_, __) =>
            {
                this.Reset();
                RaisePropertyChanged(nameof(this.IsPlaying));
            };

            this.PlaybackService.PlaybackFailed += (_, __) => RaisePropertyChanged(nameof(this.IsPlaying));
            this.PlaybackService.PlaybackPaused += (_, __) => RaisePropertyChanged(nameof(this.IsPlaying));
            this.PlaybackService.PlaybackResumed += (_, __) => RaisePropertyChanged(nameof(this.IsPlaying));
            this.PlaybackService.PlaybackSuccess += (_,__) => RaisePropertyChanged(nameof(this.IsPlaying));
        }
    }
}
