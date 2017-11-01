using Dopamine.Common.Presentation.ViewModels.Base;
using Dopamine.Common.Services.Dialog;
using Microsoft.Practices.Unity;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class CollectionPlaybackControlsViewModel : PlaybackControlsViewModelBase
    {
        public bool IsPlaying
        {
            get { return !this.PlaybackService.IsStopped & this.PlaybackService.IsPlaying; }
        }

        public CollectionPlaybackControlsViewModel(IUnityContainer container, IDialogService dialogService) : base(container)
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
