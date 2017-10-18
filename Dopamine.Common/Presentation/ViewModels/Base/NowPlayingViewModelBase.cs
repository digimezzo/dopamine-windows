using Dopamine.Common.Services.Playback;
using Prism.Mvvm;

namespace Dopamine.Common.Presentation.ViewModels.Base
{
    public abstract class NowPlayingViewModelBase : BindableBase
    {
        private IPlaybackService playbackService;
        private int nowPlayingSelectedPageIndex;

        public int NowPlayingSelectedPageIndex
        {
            get { return nowPlayingSelectedPageIndex; }
            set { SetProperty<int>(ref this.nowPlayingSelectedPageIndex, value); }
        }

        public NowPlayingViewModelBase(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;
            this.playbackService.PlaybackSuccess += (_) => this.SetNowPlaying();
            this.SetNowPlaying();
        }

        private void SetNowPlaying()
        {
            this.NowPlayingSelectedPageIndex = this.playbackService.Queue.Count > 0 ? 1 : 0;
        }
    }
}
