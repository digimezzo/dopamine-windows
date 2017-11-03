using Digimezzo.Utilities.Settings;
using Dopamine.Common.Base;
using Dopamine.Common.Enums;
using Dopamine.Common.Presentation.ViewModels.Base;
using Dopamine.Common.Prism;
using Dopamine.Views.FullPlayer.Collection;
using Prism.Commands;
using Prism.Regions;

namespace Dopamine.ViewModels.FullPlayer.Collection
{
    public class CollectionViewModel : NavigationViewModelBase
    {
        private CollectionPage previousSelectedCollectionPage;
        private CollectionPage selectedCollectionPage;

        public CollectionPage SelectedCollectionPage
        {
            get { return this.selectedCollectionPage; }
            set
            {
                SetProperty<CollectionPage>(ref this.selectedCollectionPage, value);
                SettingsClient.Set<int>("FullPlayer", "SelectedCollectionPage", (int)value);
                RaisePropertyChanged(nameof(this.CanSearch));
                this.NagivateToSelectedPage();
            }
        }

        public bool CanSearch
        {
            get { return this.selectedCollectionPage != CollectionPage.Frequent; }
        }

        public CollectionViewModel(IRegionManager regionManager) : base(regionManager)
        {
            this.LoadedCommand = new DelegateCommand(() =>
            {
                if (SettingsClient.Get<bool>("Startup", "ShowLastSelectedPage"))
                {
                    this.SelectedCollectionPage = (CollectionPage)SettingsClient.Get<int>("FullPlayer", "SelectedCollectionPage");
                }
                else
                {
                    this.SelectedCollectionPage = CollectionPage.Artists;
                }

                this.NagivateToSelectedPage();
            });
        }

        private void NagivateToSelectedPage()
        {
            this.SlideInFrom = this.selectedCollectionPage <= this.previousSelectedCollectionPage ? -Constants.SlideDistance : Constants.SlideDistance;
            this.previousSelectedCollectionPage = this.selectedCollectionPage;

            switch (this.selectedCollectionPage)
            {
                case CollectionPage.Artists:
                    this.RegionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionArtists).FullName);
                    break;
                case CollectionPage.Genres:
                    this.RegionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionGenres).FullName);
                    break;
                case CollectionPage.Albums:
                    this.RegionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionAlbums).FullName);
                    break;
                case CollectionPage.Songs:
                    this.RegionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionTracks).FullName);
                    break;
                case CollectionPage.Playlists:
                    this.RegionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionPlaylists).FullName);
                    break;
                case CollectionPage.Frequent:
                    this.RegionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionFrequent).FullName);
                    break;
                default:
                    break;
            }
        }
    }
}
