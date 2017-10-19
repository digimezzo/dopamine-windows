using Dopamine.Common.Enums;
using Dopamine.Common.Services.Playback;
using Prism.Mvvm;
using System;

namespace Dopamine.Common.Presentation.ViewModels.Base
{
    public abstract class NowPlayingViewModelBase : BindableBase
    {
        private IPlaybackService playbackService;
        private NowPlayingPage selectedNowPlayingPage;
        private int nowPlayingSelectedPageIndex;

        public Int32 SelectedNowPlayingPageIndex
        {
            get { return (Int32)this.selectedNowPlayingPage; }
        }

        public NowPlayingViewModelBase(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;
            this.playbackService.PlaybackSuccess += (_) => this.SetNowPlaying();
            this.SetNowPlaying();
        }

        private void SetNowPlaying()
        {
            NowPlayingPage page = this.playbackService.Queue.Count > 0 ? NowPlayingPage.NowPlaying : NowPlayingPage.NothingPlaying;
            this.SetSelectedNowPlayingPage(page);
        }

        private void SetSelectedNowPlayingPage(NowPlayingPage page)
        {
            this.selectedNowPlayingPage = page;
            RaisePropertyChanged(nameof(this.SelectedNowPlayingPageIndex));
        }
    }
}
