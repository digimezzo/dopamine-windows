using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Services.Metadata;
using Dopamine.Core.Base;
using Dopamine.Core.Database;
using Dopamine.Core.Prism;
using Dopamine.Core.Settings;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.CollectionModule.ViewModels
{
    public class CollectionAlbumsViewModel : CommonAlbumsViewModel
    {
        #region Variables
        private double leftPaneWidthPercent;
        #endregion

        #region Properties
        public double LeftPaneWidthPercent
        {
            get { return this.leftPaneWidthPercent; }
            set
            {
                SetProperty<double>(ref this.leftPaneWidthPercent, value);
                XmlSettingsClient.Instance.Set<int>("ColumnWidths", "AlbumsLeftPaneWidthPercent", Convert.ToInt32(value));
            }
        }

        public override bool CanOrderByAlbum
        {
            get { return this.SelectedAlbums != null && this.SelectedAlbums.Count > 0; }
        }
        #endregion

        #region Construction
        public CollectionAlbumsViewModel()
        {
            // IndexingService
            this.indexingService.RefreshArtwork += async (sender, e) => await this.collectionService.RefreshArtworkAsync(this.Albums, this.Tracks);

            //  Commands
            this.ToggleTrackOrderCommand = new DelegateCommand(async () => await this.ToggleTrackOrderAsync());
            this.ToggleAlbumOrderCommand = new DelegateCommand(async () => await this.ToggleAlbumOrderAsync());
            this.RemoveSelectedTracksCommand = new DelegateCommand(async () => await this.RemoveTracksFromCollectionAsync(this.SelectedTracks), () => !this.IsIndexing);

            // Events
            this.eventAggregator.GetEvent<SettingEnableRatingChanged>().Subscribe(async (iEnableRating) =>
            {
                this.EnableRating = iEnableRating;
                this.SetTrackOrder("AlbumsTrackOrder");
                await this.GetTracksAsync(null, null, this.SelectedAlbums, this.TrackOrder);
            });

            this.eventAggregator.GetEvent<SettingUseStarRatingChanged>().Subscribe(iUseStarRating => this.UseStarRating = iUseStarRating);

            // MetadataService
            this.metadataService.MetadataChanged += MetadataChangedHandlerAsync;

            // Set the initial AlbumOrder
            this.AlbumOrder = (AlbumOrder)XmlSettingsClient.Instance.Get<int>("Ordering", "AlbumsAlbumOrder");

            // Set the initial TrackOrder
            this.SetTrackOrder("AlbumsTrackOrder");

            // Subscribe to Events and Commands on creation
            this.Subscribe();

            // Set width of the panels
            this.LeftPaneWidthPercent = XmlSettingsClient.Instance.Get<int>("ColumnWidths", "AlbumsLeftPaneWidthPercent");

            // Cover size
            this.SetCoversizeAsync((CoverSizeType)XmlSettingsClient.Instance.Get<int>("CoverSizes", "AlbumsCoverSize"));

        }
        #endregion

        #region Private
        private async void MetadataChangedHandlerAsync(MetadataChangedEventArgs e)
        {
            if (e.IsAlbumArtworkMetadataChanged)
            {
                await this.collectionService.RefreshArtworkAsync(this.Albums, this.Tracks);
            }

            if (e.IsAlbumTitleMetadataChanged | e.IsAlbumArtistMetadataChanged | e.IsAlbumYearMetadataChanged)
            {
                await this.GetAlbumsAsync(null, null, this.AlbumOrder);
            }

            if (e.IsAlbumTitleMetadataChanged | e.IsAlbumArtistMetadataChanged | e.IsTrackMetadataChanged)
            {
                await this.GetTracksAsync(null, null, this.SelectedAlbums, this.TrackOrder);
            }
        }
        #endregion

        #region Protected
        protected async Task ToggleTrackOrderAsync()
        {
            base.ToggleTrackOrder();

            XmlSettingsClient.Instance.Set<int>("Ordering", "AlbumsTrackOrder", (int)this.TrackOrder);
            await this.GetTracksCommonAsync(this.Tracks.Select((t) => t.TrackInfo).ToList(), this.TrackOrder);
        }

        protected async Task ToggleAlbumOrderAsync()
        {
            base.ToggleAlbumOrder();

            XmlSettingsClient.Instance.Set<int>("Ordering", "AlbumsAlbumOrder", (int)this.AlbumOrder);
            await this.GetAlbumsCommonAsync(this.Albums.Select((a) => a.Album).ToList(), this.AlbumOrder);
        }
        #endregion

        #region Overrides
        protected async override Task SetCoversizeAsync(CoverSizeType iCoverSize)
        {

            await base.SetCoversizeAsync(iCoverSize);
            XmlSettingsClient.Instance.Set<int>("CoverSizes", "AlbumsCoverSize", (int)iCoverSize);
        }

        protected async override Task FillListsAsync()
        {

            await this.GetAlbumsAsync(null, null, this.AlbumOrder);
            await this.GetTracksAsync(null, null, null, this.TrackOrder);
        }

        protected async override Task SelectedAlbumsHandlerAsync(object iParameter)
        {
            await base.SelectedAlbumsHandlerAsync(iParameter);

            // Don't reload the lists when updating Metadata. MetadataChangedHandlerAsync handles that.
            if (this.metadataService.IsUpdatingDatabaseMetadata) return;

            this.SetTrackOrder("AlbumsTrackOrder");
            await this.GetTracksAsync(null, null, this.SelectedAlbums, this.TrackOrder);
        }

        protected override void Unsubscribe()
        {
            // Commands
            ApplicationCommands.RemoveSelectedTracksCommand.UnregisterCommand(this.RemoveSelectedTracksCommand);
        }

        protected override void Subscribe()
        {
            // Prevents subscribing twice
            this.Unsubscribe();

            // Commands
            ApplicationCommands.RemoveSelectedTracksCommand.RegisterCommand(this.RemoveSelectedTracksCommand);
        }

        protected override void RefreshLanguage()
        {
            this.UpdateAlbumOrderText(this.AlbumOrder);
            this.UpdateTrackOrderText(this.TrackOrder);
        }
        #endregion
    }
}
