using Digimezzo.WPFControls.Enums;
using Dopamine.Core.Enums;
using Dopamine.Common.Presentation.ViewModels.Base;
using Dopamine.Common.Prism;
using Dopamine.Common.Services.Playback;
using Dopamine.Views.NowPlaying;
using Prism.Events;
using Prism.Regions;

namespace Dopamine.ViewModels.NowPlaying
{
    public class NowPlayingViewModel : NowPlayingViewModelBase
    {
        private SlideDirection direction;
        private IRegionManager regionManager;

        public SlideDirection Direction
        {
            get { return this.direction; }
            set
            {
                SetProperty<SlideDirection>(ref this.direction, value);
            }
        }

        public NowPlayingViewModel(IPlaybackService playbackService, IRegionManager regionManager, IEventAggregator eventAggregator) : base(playbackService)
        {
            this.regionManager = regionManager;

            eventAggregator.GetEvent<IsNowPlayingSubPageChanged>().Subscribe(tuple =>
            {
                this.NagivateToSelectedPage(tuple.Item1, tuple.Item2);
            });
        }

        private void NagivateToSelectedPage(SlideDirection direction, NowPlayingSubPage page)
        {
            this.Direction = direction;
           
            switch (page)
            {
                case NowPlayingSubPage.ArtistInformation:
                    this.regionManager.RequestNavigate(RegionNames.NowPlayingSubPageRegion, typeof(NowPlayingArtistInformation).FullName);
                    break;
                case NowPlayingSubPage.Lyrics:
                    this.regionManager.RequestNavigate(RegionNames.NowPlayingSubPageRegion, typeof(NowPlayingLyrics).FullName);
                    break;
                case NowPlayingSubPage.Playlist:
                    this.regionManager.RequestNavigate(RegionNames.NowPlayingSubPageRegion, typeof(NowPlayingPlaylist).FullName);
                    break;
                case NowPlayingSubPage.ShowCase:
                    this.regionManager.RequestNavigate(RegionNames.NowPlayingSubPageRegion, typeof(NowPlayingShowcase).FullName);
                    break;
                default:
                    break;
            }
        }
    }
}
