using Digimezzo.Utilities.Settings;
using Dopamine.Core.Base;
using Dopamine.Data;
using Dopamine.Common.Presentation.ViewModels.Base;
using Dopamine.Services.Collection;
using Dopamine.Services.Indexing;
using Dopamine.Services.Metadata;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.FullPlayer.Collection
{
    public class CollectionAlbumsViewModel : AlbumsViewModelBase
    {
        private IIndexingService indexingService;
        private ICollectionService collectionService;
        private IMetadataService metadataService;
        private IEventAggregator eventAggregator;
        private double leftPaneWidthPercent;

        public double LeftPaneWidthPercent
        {
            get { return this.leftPaneWidthPercent; }
            set
            {
                SetProperty<double>(ref this.leftPaneWidthPercent, value);
                SettingsClient.Set<int>("ColumnWidths", "AlbumsLeftPaneWidthPercent", Convert.ToInt32(value));
            }
        }

        public override bool CanOrderByAlbum => this.SelectedAlbumIds != null && this.SelectedAlbumIds.Count > 0;

        public CollectionAlbumsViewModel(IUnityContainer container) : base(container)
        {
            // Dependency injection
            this.indexingService = container.Resolve<IIndexingService>();
            this.collectionService = container.Resolve<ICollectionService>();
            this.metadataService = container.Resolve<IMetadataService>();
            this.eventAggregator = container.Resolve<IEventAggregator>();

            // Settings
            SettingsClient.SettingChanged += async (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "Behaviour", "EnableRating"))
                {
                    this.EnableRating = (bool)e.SettingValue;
                    this.SetTrackOrder("AlbumsTrackOrder");
                    await this.GetTracksAsync(null, null, this.SelectedAlbumIds, this.TrackOrder);
                }

                if (SettingsClient.IsSettingChanged(e, "Behaviour", "EnableLove"))
                {
                    this.EnableLove = (bool)e.SettingValue;
                    this.SetTrackOrder("AlbumsTrackOrder");
                    await this.GetTracksAsync(null, null, this.SelectedAlbumIds, this.TrackOrder);
                }
            };

            // Events
            this.metadataService.MetadataChanged += MetadataChangedHandlerAsync;
            this.indexingService.AlbumArtworkAdded += async (_, e) => await this.collectionService.RefreshArtworkAsync(this.Albums, e.AlbumIds);

            //  Commands
            this.ToggleAlbumOrderCommand = new DelegateCommand(async () => await this.ToggleAlbumOrderAsync());
            this.ToggleTrackOrderCommand = new DelegateCommand(async () => await this.ToggleTrackOrderAsync());
            this.RemoveSelectedTracksCommand = new DelegateCommand(async () => await this.RemoveTracksFromCollectionAsync(this.SelectedTracks), () => !this.IsIndexing);
            this.RemoveSelectedTracksFromDiskCommand = new DelegateCommand(async () => await this.RemoveTracksFromDiskAsync(this.SelectedTracks), () => !this.IsIndexing);

            // Set the initial AlbumOrder
            this.AlbumOrder = (AlbumOrder)SettingsClient.Get<int>("Ordering", "AlbumsAlbumOrder");

            // Set the initial TrackOrder
            this.SetTrackOrder("AlbumsTrackOrder");

            // Set width of the panels
            this.LeftPaneWidthPercent = SettingsClient.Get<int>("ColumnWidths", "AlbumsLeftPaneWidthPercent");

            // Cover size
            this.SetCoversizeAsync((CoverSizeType)SettingsClient.Get<int>("CoverSizes", "AlbumsCoverSize"));
        }

        private async void MetadataChangedHandlerAsync(MetadataChangedEventArgs e)
        {
            if (e.IsArtworkChanged) await this.collectionService.RefreshArtworkAsync(this.Albums);
            if (e.IsAlbumChanged) await this.GetAlbumsAsync(null, null, this.AlbumOrder);
            if (e.IsAlbumChanged | e.IsTrackChanged) await this.GetTracksAsync(null, null, this.SelectedAlbumIds, this.TrackOrder);
        }

        private async Task ToggleTrackOrderAsync()
        {
            base.ToggleTrackOrder();

            SettingsClient.Set<int>("Ordering", "AlbumsTrackOrder", (int)this.TrackOrder);
            await this.GetTracksCommonAsync(this.Tracks.Select((t) => t.Track).ToList(), this.TrackOrder);
        }

        private async Task ToggleAlbumOrderAsync()
        {
            base.ToggleAlbumOrder();

            SettingsClient.Set<int>("Ordering", "AlbumsAlbumOrder", (int)this.AlbumOrder);
            await this.GetAlbumsCommonAsync(this.Albums.Select((a) => a.Album).ToList(), this.AlbumOrder);
        }

        protected async override Task SetCoversizeAsync(CoverSizeType iCoverSize)
        {

            await base.SetCoversizeAsync(iCoverSize);
            SettingsClient.Set<int>("CoverSizes", "AlbumsCoverSize", (int)iCoverSize);
        }

        protected async override Task FillListsAsync()
        {
            await this.GetAlbumsAsync(null, null, this.AlbumOrder);
            await this.GetTracksAsync(null, null, this.SelectedAlbumIds, this.TrackOrder);
        }

        protected async override Task SelectedAlbumsHandlerAsync(object parameter)
        {
            await base.SelectedAlbumsHandlerAsync(parameter);

            // Don't reload the lists when updating Metadata. MetadataChangedHandlerAsync handles that.
            if (this.metadataService.IsUpdatingDatabaseMetadata) return;

            this.SetTrackOrder("AlbumsTrackOrder");
            await this.GetTracksAsync(null, null, this.SelectedAlbumIds, this.TrackOrder);
        }

        protected override void RefreshLanguage()
        {
            this.UpdateAlbumOrderText(this.AlbumOrder);
            this.UpdateTrackOrderText(this.TrackOrder);
            base.RefreshLanguage();
        }
    }
}
