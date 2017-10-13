using Digimezzo.Utilities.Utils;
using Dopamine.Common.Presentation.ViewModels.Base;
using Dopamine.Common.Presentation.Views;
using Dopamine.Common.Services.Dialog;
using Microsoft.Practices.Unity;
using Prism.Commands;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class CollectionPlaybackControlsViewModel : PlaybackControlsViewModelBase
    {
        public bool IsPlaying
        {
            get { return !this.playbackService.IsStopped & this.playbackService.IsPlaying; }
        }

        public DelegateCommand ShowEqualizerCommand { get; set; }

        public CollectionPlaybackControlsViewModel(IUnityContainer container, IDialogService dialogService) : base(container)
        {
            this.playbackService.PlaybackStopped += (_, __) =>
            {
                this.Reset();
                RaisePropertyChanged(nameof(this.IsPlaying));
            };

            this.ShowEqualizerCommand = new DelegateCommand(() =>
            {
                EqualizerControl view = container.Resolve<EqualizerControl>();
                view.DataContext = container.Resolve<EqualizerControlViewModel>();

                dialogService.ShowCustomDialog(
                    new EqualizerIcon() { IsDialogIcon = true },
                    0,
                    ResourceUtils.GetString("Language_Equalizer"),
                    view,
                    570,
                    0,
                    false,
                    true,
                    true,
                    false,
                    ResourceUtils.GetString("Language_Close"),
                    string.Empty,
                    null);
            });

            this.playbackService.PlaybackFailed += (_, __) => RaisePropertyChanged(nameof(this.IsPlaying));
            this.playbackService.PlaybackPaused += (_, __) => RaisePropertyChanged(nameof(this.IsPlaying));
            this.playbackService.PlaybackResumed += (_, __) => RaisePropertyChanged(nameof(this.IsPlaying));
            this.playbackService.PlaybackSuccess += (_) => RaisePropertyChanged(nameof(this.IsPlaying));
        }
    }
}
