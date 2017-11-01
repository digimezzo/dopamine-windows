using Dopamine.Common.Enums;
using Dopamine.Common.Services.Playback;
using Prism.Mvvm;

namespace Dopamine.Common.Presentation.ViewModels.Base
{
    public abstract class NowPlayingViewModelBase : BindableBase
    {
        private IPlaybackService playbackService;
        private NowPlayingPage selectedNowPlayingPage;

        public NowPlayingPage SelectedNowPlayingPage
        {
            get { return selectedNowPlayingPage; }
            set { SetProperty<NowPlayingPage>(ref this.selectedNowPlayingPage, value); }
        }

        public NowPlayingViewModelBase(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;
            this.playbackService.PlaybackSuccess += (_,__) => this.SetNowPlaying();
            this.SetNowPlaying();
        }

        private void SetNowPlaying()
        {
            this.SelectedNowPlayingPage = this.playbackService.Queue.Count > 0 ? NowPlayingPage.NowPlaying : NowPlayingPage.NothingPlaying;
        }
    }
}
