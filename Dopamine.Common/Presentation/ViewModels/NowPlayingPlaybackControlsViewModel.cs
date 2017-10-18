using Dopamine.Common.Presentation.ViewModels.Base;
using Microsoft.Practices.Unity;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class NowPlayingPlaybackControlsViewModel : PlaybackControlsViewModelBase
    {
        public bool HasPlaybackQueue
        {
            get { return this.PlaybackService.Queue.Count > 0; }
        }

        public NowPlayingPlaybackControlsViewModel(IUnityContainer container) : base(container)
        {
            this.PlaybackService.PlaybackSuccess += (_) => RaisePropertyChanged(nameof(this.HasPlaybackQueue));
            this.PlaybackService.PlaybackStopped += (_, __) => this.Reset();
        }
    }
}
