using Digimezzo.WPFControls.Enums;
using Dopamine.Core.Base;
using Dopamine.Common.Enums;
using Dopamine.Common.Prism;
using Dopamine.Views.FullPlayer.Collection;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace Dopamine.ViewModels.FullPlayer.Collection
{
    public class CollectionViewModel : BindableBase
    {
        private int slideInFrom;
        private IRegionManager regionManager;

        public int SlideInFrom
        {
            get { return this.slideInFrom; }
            set { SetProperty<int>(ref this.slideInFrom, value); }
        }

        public CollectionViewModel(IEventAggregator eventAggregator, IRegionManager regionManager)
        {
            this.regionManager = regionManager;

            eventAggregator.GetEvent<IsCollectionPageChanged>().Subscribe(tuple =>
            {
                this.NagivateToPage(tuple.Item1, tuple.Item2);
            });
        }

        private void NagivateToPage(SlideDirection direction, CollectionPage page)
        {
            this.SlideInFrom = direction == SlideDirection.RightToLeft ? Constants.SlideDistance  : -Constants.SlideDistance;

            switch (page)
            {
                case CollectionPage.Artists:
                    this.regionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionArtists).FullName);
                    break;
                case CollectionPage.Genres:
                    this.regionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionGenres).FullName);
                    break;
                case CollectionPage.Albums:
                    this.regionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionAlbums).FullName);
                    break;
                case CollectionPage.Songs:
                    this.regionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionTracks).FullName);
                    break;
                case CollectionPage.Playlists:
                    this.regionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionPlaylists).FullName);
                    break;
                case CollectionPage.Frequent:
                    this.regionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionFrequent).FullName);
                    break;
                default:
                    break;
            }
        }
    }
}
