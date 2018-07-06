using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.Prism;
using Dopamine.Data;
using Dopamine.Data.Entities;
using Dopamine.Data.Repositories;
using Dopamine.Services.Collection;
using Dopamine.Services.Dialog;
using Dopamine.Services.Entities;
using Dopamine.Services.Indexing;
using Dopamine.Services.Metadata;
using Dopamine.Services.Playback;
using Dopamine.Services.Playlist;
using Dopamine.Services.Search;
using Dopamine.Services.Utils;
using Dopamine.ViewModels.Common.Base;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Dopamine.ViewModels.FullPlayer.Collection
{
    public class CollectionArtistsViewModel : AlbumsViewModelBase, ISemanticZoomViewModel
    {
        private ICollectionService collectionService;
        private IMetadataService metadataService;
        private IPlaybackService playbackService;
        private IPlaylistService playlistService;
        private IIndexingService indexingService;
        private IDialogService dialogService;
        private ISearchService searchService;
        private IEventAggregator eventAggregator;
        private ObservableCollection<ISemanticZoomable> artists;
        private CollectionViewSource artistsCvs;
        private IList<string> selectedArtists;
        private ObservableCollection<ISemanticZoomSelector> artistsZoomSelectors;
        private bool isArtistsZoomVisible;
        private long artistsCount;
        private double leftPaneWidthPercent;
        private double rightPaneWidthPercent;
        private ArtistType artistType;
        private string artistTypeText;

        public DelegateCommand<string> AddArtistsToPlaylistCommand { get; set; }


        public DelegateCommand<object> SelectedArtistsCommand { get; set; }

        public DelegateCommand ShowArtistsZoomCommand { get; set; }

        public DelegateCommand<string> SemanticJumpCommand { get; set; }

        public DelegateCommand AddArtistsToNowPlayingCommand { get; set; }

        public DelegateCommand ShuffleSelectedArtistsCommand { get; set; }

        public ArtistType ArtistType
        {
            get { return this.artistType; }
            set
            {
                SetProperty<ArtistType>(ref this.artistType, value);
                this.UpdateArtistTypeText(value);
            }
        }

        public string ArtistTypeText
        {
            get { return this.artistTypeText; }
        }

        public double LeftPaneWidthPercent
        {
            get { return this.leftPaneWidthPercent; }
            set
            {
                SetProperty<double>(ref this.leftPaneWidthPercent, value);
                SettingsClient.Set<int>("ColumnWidths", "ArtistsLeftPaneWidthPercent", Convert.ToInt32(value));
            }
        }

        public double RightPaneWidthPercent
        {
            get { return this.rightPaneWidthPercent; }
            set
            {
                SetProperty<double>(ref this.rightPaneWidthPercent, value);
                SettingsClient.Set<int>("ColumnWidths", "ArtistsRightPaneWidthPercent", Convert.ToInt32(value));
            }
        }

        public ObservableCollection<ISemanticZoomable> Artists
        {
            get { return this.artists; }
            set { SetProperty<ObservableCollection<ISemanticZoomable>>(ref this.artists, value); }
        }

        ObservableCollection<ISemanticZoomable> ISemanticZoomViewModel.SemanticZoomables
        {
            get { return Artists; }
            set { Artists = value; }
        }

        public CollectionViewSource ArtistsCvs
        {
            get { return this.artistsCvs; }
            set { SetProperty<CollectionViewSource>(ref this.artistsCvs, value); }
        }

        public IList<string> SelectedArtists
        {
            get { return this.selectedArtists; }
            set { SetProperty<IList<string>>(ref this.selectedArtists, value); }
        }

        public long ArtistsCount
        {
            get { return this.artistsCount; }
            set { SetProperty<long>(ref this.artistsCount, value); }
        }

        public bool IsArtistsZoomVisible
        {
            get { return this.isArtistsZoomVisible; }
            set { SetProperty<bool>(ref this.isArtistsZoomVisible, value); }
        }

        public ObservableCollection<ISemanticZoomSelector> ArtistsZoomSelectors
        {
            get { return this.artistsZoomSelectors; }
            set { SetProperty<ObservableCollection<ISemanticZoomSelector>>(ref this.artistsZoomSelectors, value); }
        }
        ObservableCollection<ISemanticZoomSelector> ISemanticZoomViewModel.SemanticZoomSelectors
        {
            get { return ArtistsZoomSelectors; }
            set { ArtistsZoomSelectors = value; }
        }

        public override bool CanOrderByAlbum
        {
            get
            {
                return (this.SelectedArtists != null && this.SelectedArtists.Count > 0) |
                     (this.SelectedAlbumKeys != null && this.SelectedAlbumKeys.Count > 0);
            }
        }

        public bool HasSelectedArtists
        {
            get
            {
                return (this.SelectedArtists != null && this.SelectedArtists.Count > 0);
            }
        }

        public CollectionArtistsViewModel(IContainerProvider container) : base(container)
        {
            // Dependency injection
            this.collectionService = container.Resolve<ICollectionService>();
            this.metadataService = container.Resolve<IMetadataService>();
            this.playbackService = container.Resolve<IPlaybackService>();
            this.playlistService = container.Resolve<IPlaylistService>();
            this.indexingService = container.Resolve<IIndexingService>();
            this.dialogService = container.Resolve<IDialogService>();
            this.searchService = container.Resolve<ISearchService>();
            this.eventAggregator = container.Resolve<IEventAggregator>();

            // Commands
            this.ToggleTrackOrderCommand = new DelegateCommand(async () => await this.ToggleTrackOrderAsync());
            this.ToggleAlbumOrderCommand = new DelegateCommand(async () => await this.ToggleAlbumOrderAsync());
            this.RemoveSelectedTracksCommand = new DelegateCommand(async () => await this.RemoveTracksFromCollectionAsync(this.SelectedTracks), () => !this.IsIndexing);
            this.RemoveSelectedTracksFromDiskCommand = new DelegateCommand(async () => await this.RemoveTracksFromDiskAsync(this.SelectedTracks), () => !this.IsIndexing);
            this.AddArtistsToPlaylistCommand = new DelegateCommand<string>(async (playlistName) => await this.AddArtistsToPlaylistAsync(this.SelectedArtists, playlistName));
            this.SelectedArtistsCommand = new DelegateCommand<object>(async (parameter) => await this.SelectedArtistsHandlerAsync(parameter));
            this.ShowArtistsZoomCommand = new DelegateCommand(async () => await this.ShowSemanticZoomAsync());
            this.AddArtistsToNowPlayingCommand = new DelegateCommand(async () => await this.AddArtistsToNowPlayingAsync(this.SelectedArtists));
            this.ShuffleSelectedArtistsCommand = new DelegateCommand(async () => await this.playbackService.EnqueueArtistsAsync(this.SelectedArtists, true, false));

            this.SemanticJumpCommand = new DelegateCommand<string>((header) =>
            {
                this.HideSemanticZoom();
                this.eventAggregator.GetEvent<PerformSemanticJump>().Publish(new Tuple<string, string>("Artists", header));
            });

            // Settings
            SettingsClient.SettingChanged += async (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "Behaviour", "EnableRating"))
                {
                    this.EnableRating = (bool)e.SettingValue;
                    this.SetTrackOrder("ArtistsTrackOrder");
                    await this.GetTracksAsync(this.SelectedArtists, null, this.SelectedAlbumKeys, this.TrackOrder);
                }

                if (SettingsClient.IsSettingChanged(e, "Behaviour", "EnableLove"))
                {
                    this.EnableLove = (bool)e.SettingValue;
                    this.SetTrackOrder("ArtistsTrackOrder");
                    await this.GetTracksAsync(this.SelectedArtists, null, this.SelectedAlbumKeys, this.TrackOrder);
                }
            };

            // PubSub Events
            this.eventAggregator.GetEvent<ShellMouseUp>().Subscribe((_) => this.IsArtistsZoomVisible = false);
            this.eventAggregator.GetEvent<ToggleArtistOrderCommand>().Subscribe((_) => this.ToggleArtistTypeAsync());

            // Events
            this.metadataService.MetadataChanged += MetadataChangedHandlerAsync;
            this.indexingService.AlbumArtworkAdded += async (_, e) => await this.RefreshAlbumArtworkAsync(e.AlbumKeys);

            // Set the initial ArtistOrder		
            this.ArtistType = (ArtistType)SettingsClient.Get<int>("Ordering", "ArtistsArtistType");

            // Set the initial AlbumOrder
            this.AlbumOrder = (AlbumOrder)SettingsClient.Get<int>("Ordering", "ArtistsAlbumOrder");

            // Set the initial TrackOrder
            this.SetTrackOrder("ArtistsTrackOrder");

            // Set width of the panels
            this.LeftPaneWidthPercent = SettingsClient.Get<int>("ColumnWidths", "ArtistsLeftPaneWidthPercent");
            this.RightPaneWidthPercent = SettingsClient.Get<int>("ColumnWidths", "ArtistsRightPaneWidthPercent");

            // Cover size
            this.SetCoversizeAsync((CoverSizeType)SettingsClient.Get<int>("CoverSizes", "ArtistsCoverSize"));
        }

        public async Task ShowSemanticZoomAsync()
        {
            this.ArtistsZoomSelectors = await SemanticZoomUtils.UpdateSemanticZoomSelectors(this.ArtistsCvs.View);
            this.IsArtistsZoomVisible = true;
        }

        public void HideSemanticZoom()
        {
            this.IsArtistsZoomVisible = false;
        }

        public void UpdateSemanticZoomHeaders()
        {
            string previousHeader = string.Empty;

            foreach (ArtistViewModel avm in this.ArtistsCvs.View)
            {
                if (string.IsNullOrEmpty(previousHeader) || !avm.Header.Equals(previousHeader))
                {
                    previousHeader = avm.Header;
                    avm.IsHeader = true;
                }
                else
                {
                    avm.IsHeader = false;
                }
            }
        }

        private void SetArtistOrder(string settingName)
        {
            this.ArtistType = (ArtistType)SettingsClient.Get<int>("Ordering", settingName);
        }

        private async void MetadataChangedHandlerAsync(MetadataChangedEventArgs e)
        {
            if (e.IsArtworkChanged) await this.RefreshAlbumArtworkAsync();
            if (e.IsArtistChanged | (e.IsAlbumChanged & (this.ArtistType == ArtistType.Album | this.ArtistType == ArtistType.All))) await this.GetArtistsAsync(this.ArtistType);
            // TODO if (e.IsArtistChanged | e.IsAlbumChanged) await this.GetAlbumsAsync(this.SelectedArtists, null, this.AlbumOrder);
            // TODO if (e.IsArtistChanged | e.IsAlbumChanged | e.IsTrackChanged) await this.GetTracksAsync(this.SelectedArtists, null, this.SelectedAlbumIds, this.TrackOrder);
        }

        private async Task GetArtistsAsync(ArtistType artistType)
        {
            try
            {
                // Get the artists
                var artistViewModels = new ObservableCollection<ArtistViewModel>(await this.collectionService.GetAllArtistsAsync(artistType));

                // Unbind to improve UI performance
                if (this.ArtistsCvs != null) this.ArtistsCvs.Filter -= new FilterEventHandler(ArtistsCvs_Filter);
                this.Artists = null;
                this.ArtistsCvs = null;

                // Populate ObservableCollection
                this.Artists = new ObservableCollection<ISemanticZoomable>(artistViewModels);
            }
            catch (Exception ex)
            {
                LogClient.Error("An error occurred while getting Artists. Exception: {0}", ex.Message);

                // Failed getting Artists. Create empty ObservableCollection.
                this.Artists = new ObservableCollection<ISemanticZoomable>();
            }

            // Populate CollectionViewSource
            this.ArtistsCvs = new CollectionViewSource { Source = this.Artists };
            this.ArtistsCvs.Filter += new FilterEventHandler(ArtistsCvs_Filter);

            // Update count
            this.ArtistsCount = this.ArtistsCvs.View.Cast<ISemanticZoomable>().Count();

            // Update Semantic Zoom Headers
            this.UpdateSemanticZoomHeaders();
        }

        private async Task SelectedArtistsHandlerAsync(object parameter)
        {
            if (parameter != null)
            {
                this.SelectedArtists = new List<string>();

                foreach (ArtistViewModel item in (IList)parameter)
                {
                    this.SelectedArtists.Add(item.ArtistName);
                }
            }

            this.RaisePropertyChanged(nameof(this.HasSelectedArtists));

            // Don't reload the lists when updating Metadata. MetadataChangedHandlerAsync handles that.
            if (this.metadataService.IsUpdatingDatabaseMetadata)
            {
                return;
            }

            await this.GetAlbumsAsync(this.SelectedArtists, null, this.AlbumOrder);
            this.SetTrackOrder("ArtistsTrackOrder");
            await this.GetTracksAsync(this.SelectedArtists, null, this.SelectedAlbumKeys, this.TrackOrder);
        }

        private async Task AddArtistsToPlaylistAsync(IList<string> artists, string playlistName)
        {
            AddPlaylistResult addPlaylistResult = AddPlaylistResult.Success; // Default Success

            // If no playlist is provided, first create one.
            if (playlistName == null)
            {
                var responseText = ResourceUtils.GetString("Language_New_Playlist");

                if (this.dialogService.ShowInputDialog(
                    0xea37,
                    16,
                    ResourceUtils.GetString("Language_New_Playlist"),
                    ResourceUtils.GetString("Language_Enter_Name_For_New_Playlist"),
                    ResourceUtils.GetString("Language_Ok"),
                    ResourceUtils.GetString("Language_Cancel"),
                    ref responseText))
                {
                    playlistName = responseText;
                    addPlaylistResult = await this.playlistService.AddPlaylistAsync(playlistName);
                }
            }

            // If playlist name is still null, the user clicked cancel on the previous dialog. Stop here.
            if (playlistName == null) return;

            // Verify if the playlist was added
            switch (addPlaylistResult)
            {
                case AddPlaylistResult.Success:
                case AddPlaylistResult.Duplicate:
                    // Add items to playlist
                    AddTracksToPlaylistResult result = await this.playlistService.AddArtistsToPlaylistAsync(artists, playlistName);

                    if (result == AddTracksToPlaylistResult.Error)
                    {
                        this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Adding_Songs_To_Playlist").Replace("{playlistname}", "\"" + playlistName + "\""), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
                    }
                    break;
                case AddPlaylistResult.Error:
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetString("Language_Error"),
                        ResourceUtils.GetString("Language_Error_Adding_Playlist"),
                        ResourceUtils.GetString("Language_Ok"),
                        true,
                        ResourceUtils.GetString("Language_Log_File"));
                    break;
                case AddPlaylistResult.Blank:
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetString("Language_Error"),
                        ResourceUtils.GetString("Language_Provide_Playlist_Name"),
                        ResourceUtils.GetString("Language_Ok"),
                        false,
                        string.Empty);
                    break;
                default:
                    // Never happens
                    break;
            }
        }

        private void ArtistsCvs_Filter(object sender, FilterEventArgs e)
        {
            ArtistViewModel avm = e.Item as ArtistViewModel;

            e.Accepted = Services.Utils.EntityUtils.FilterArtists(avm, this.searchService.SearchText);
        }

        private async Task ToggleArtistTypeAsync()
        {
            this.HideSemanticZoom();

            switch (this.ArtistType)
            {
                case ArtistType.All:
                    this.ArtistType = ArtistType.Track;
                    break;
                case ArtistType.Track:
                    this.ArtistType = ArtistType.Album;
                    break;
                case ArtistType.Album:
                    this.ArtistType = ArtistType.All;
                    break;
                default:
                    // Cannot happen, but just in case.
                    this.ArtistType = ArtistType.All;
                    break;
            }

            SettingsClient.Set<int>("Ordering", "ArtistsArtistType", (int)this.ArtistType);
            await this.GetArtistsAsync(this.ArtistType);
        }

        private void UpdateArtistTypeText(ArtistType artistType)
        {
            switch (artistType)
            {
                case ArtistType.All:
                    this.artistTypeText = ResourceUtils.GetString("Language_All_Artists");
                    break;
                case ArtistType.Track:
                    this.artistTypeText = ResourceUtils.GetString("Language_Song_Artists");
                    break;
                case ArtistType.Album:
                    this.artistTypeText = ResourceUtils.GetString("Language_Album_Artists");
                    break;
                default:
                    // Cannot happen, but just in case.
                    this.artistTypeText = ResourceUtils.GetString("Language_All_Artists");
                    break;
            }

            RaisePropertyChanged(nameof(this.ArtistTypeText));
        }

        private async Task AddArtistsToNowPlayingAsync(IList<string> artists)
        {
            EnqueueResult result = await this.playbackService.AddArtistsToQueueAsync(artists);

            if (!result.IsSuccess)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Adding_Artists_To_Now_Playing"), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
            }
        }

        private async Task ToggleTrackOrderAsync()
        {
            base.ToggleTrackOrder();

            SettingsClient.Set<int>("Ordering", "ArtistsTrackOrder", (int)this.TrackOrder);
            await this.GetTracksCommonAsync(this.Tracks.Select((t) => t.Track).ToList(), this.TrackOrder);
        }

        private async Task ToggleAlbumOrderAsync()
        {

            base.ToggleAlbumOrder();

            SettingsClient.Set<int>("Ordering", "ArtistsAlbumOrder", (int)this.AlbumOrder);
            await this.GetAlbumsCommonAsync(this.Albums, this.AlbumOrder);
        }

        protected async override Task SetCoversizeAsync(CoverSizeType coverSize)
        {
            await base.SetCoversizeAsync(coverSize);
            SettingsClient.Set<int>("CoverSizes", "ArtistsCoverSize", (int)coverSize);
        }

        protected async override Task FillListsAsync()
        {
            await this.GetArtistsAsync(this.ArtistType);
            await this.GetAlbumsAsync(this.SelectedArtists, null, this.AlbumOrder);
            await this.GetTracksAsync(this.SelectedArtists, null, this.SelectedAlbumKeys, this.TrackOrder);
        }

        protected override void FilterLists()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Artists
                if (this.ArtistsCvs != null)
                {
                    this.ArtistsCvs.View.Refresh();
                    this.ArtistsCount = this.ArtistsCvs.View.Cast<ISemanticZoomable>().Count();
                    this.UpdateSemanticZoomHeaders();
                }
            });

            base.FilterLists();
        }

        protected async override Task SelectedAlbumsHandlerAsync(object parameter)
        {
            await base.SelectedAlbumsHandlerAsync(parameter);

            this.SetTrackOrder("ArtistsTrackOrder");
            await this.GetTracksAsync(this.SelectedArtists, null, this.SelectedAlbumKeys, this.TrackOrder);
        }

        protected override void RefreshLanguage()
        {
            this.UpdateArtistTypeText(this.ArtistType);
            this.UpdateAlbumOrderText(this.AlbumOrder);
            this.UpdateTrackOrderText(this.TrackOrder);
            base.RefreshLanguage();
        }
    }
}
