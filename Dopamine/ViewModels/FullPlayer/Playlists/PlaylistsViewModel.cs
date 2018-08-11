using Digimezzo.WPFControls.Enums;
using Dopamine.Core.Base;
using Dopamine.Core.Enums;
using Dopamine.Core.Prism;
using Dopamine.Views.FullPlayer.Playlists;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace Dopamine.ViewModels.FullPlayer.Playlists
{
    public class PlaylistsViewModel : BindableBase
    {
        private int slideInFrom;
        private IRegionManager regionManager;

        public int SlideInFrom
        {
            get { return this.slideInFrom; }
            set { SetProperty<int>(ref this.slideInFrom, value); }
        }

        public PlaylistsViewModel(IEventAggregator eventAggregator, IRegionManager regionManager)
        {
            this.regionManager = regionManager;

            eventAggregator.GetEvent<IsPlaylistsPageChanged>().Subscribe(tuple =>
            {
                this.NagivateToPage(tuple.Item1, tuple.Item2);
            });
        }

        private void NagivateToPage(SlideDirection direction, PlaylistsPage page)
        {
            this.SlideInFrom = direction == SlideDirection.RightToLeft ? Constants.SlideDistance : -Constants.SlideDistance;

            switch (page)
            {
                case PlaylistsPage.Playlists:
                    this.regionManager.RequestNavigate(RegionNames.PlaylistsRegion, typeof(PlaylistsPlaylists).FullName);
                    break;
                //case CollectionPage.Genres:
                //    this.regionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionGenres).FullName);
                //    break;
                default:
                    break;
            }
        }
    }
}
