using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.Prism;
using Dopamine.Data.Contracts;
using Dopamine.Data.Contracts.Entities;
using Dopamine.Data.Contracts.Repositories;
using Dopamine.Interfaces;
using Dopamine.Services.Contracts.Collection;
using Dopamine.Services.Contracts.Dialog;
using Dopamine.Services.Contracts.Indexing;
using Dopamine.Services.Contracts.Metadata;
using Dopamine.Services.Contracts.Playback;
using Dopamine.Services.Contracts.Playlist;
using Dopamine.Services.Contracts.Search;
using Dopamine.Services.Utils;
using Dopamine.Utils;
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
        private IArtistRepository artistRepository;
        private ObservableCollection<ISemanticZoomable> artists;
        private CollectionViewSource artistsCvs;
        private IList<Artist> selectedArtists;
        private ObservableCollection<ISemanticZoomSelector> artistsZoomSelectors;
        private bool isArtistsZoomVisible;
        private long artistsCount;
        private double leftPaneWidthPercent;
        private double rightPaneWidthPercent;
        private ArtistOrder artistOrder;
        private string artistOrderText;

        public DelegateCommand<string> AddArtistsToPlaylistCommand { get; set; }
        public DelegateCommand<object> SelectedArtistsCommand { get; set; }
        public DelegateCommand ShowArtistsZoomCommand { get; set; }
        public DelegateCommand<string> SemanticJumpCommand { get; set; }
        public DelegateCommand AddArtistsToNowPlayingCommand { get; set; }
        public DelegateCommand ToggleArtistOrderCommand { get; set; }
        public DelegateCommand ShuffleSelectedArtistsCommand { get; set; }

        public ArtistOrder ArtistOrder
        {
            get { return this.artistOrder; }
            set
            {
                SetProperty<ArtistOrder>(ref this.artistOrder, value);
                this.UpdateArtistOrderText(value);
            }
        }

        public string ArtistOrderText
        {
            get { return this.artistOrderText; }
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

        public IList<Artist> SelectedArtists
        {
            get { return this.selectedArtists; }
            set { SetProperty<IList<Artist>>(ref this.selectedArtists, value); }
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
                     (this.SelectedAlbumIds != null && this.SelectedAlbumIds.Count > 0);
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
            this.artistRepository = container.Resolve<IArtistRepository>();

            // Commands
            this.ToggleArtistOrderCommand = new DelegateCommand(async () => await this.ToggleArtistOrderAsync());
            this.ToggleTrackOrderCommand = new DelegateCommand(async () => await this.ToggleTrackOrderAsync());
            this.ToggleAlbumOrderCommand = new DelegateCommand(async () => await this.ToggleAlbumOrderAsync());
            this.RemoveSelectedTracksCommand = new DelegateCommand(async () => await this.RemoveTracksFromCollectionAsync(this.SelectedTracks), () => !this.IsIndexing);
            this.RemoveSelectedTracksFromDiskCommand = new DelegateCommand(async () => await this.RemoveTracksFromDiskAsync(this.SelectedTracks), () => !this.IsIndexing);
            this.AddArtistsToPlaylistCommand = new DelegateCommand<string>(async (iPlaylistName) => await this.AddArtistsToPlaylistAsync(this.SelectedArtists, iPlaylistName));
            this.SelectedArtistsCommand = new DelegateCommand<object>(async (iParameter) => await this.SelectedArtistsHandlerAsync(iParameter));
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
                    await this.GetTracksAsync(this.SelectedArtists, null, this.SelectedAlbumIds, this.TrackOrder);
                }

                if (SettingsClient.IsSettingChanged(e, "Behaviour", "EnableLove"))
                {
                    this.EnableLove = (bool)e.SettingValue;
                    this.SetTrackOrder("ArtistsTrackOrder");
                    await this.GetTracksAsync(this.SelectedArtists, null, this.SelectedAlbumIds, this.TrackOrder);
                }
            };

            // PubSub Events
            this.eventAggregator.GetEvent<ShellMouseUp>().Subscribe((_) => this.IsArtistsZoomVisible = false);

            // Events
            this.metadataService.MetadataChanged += MetadataChangedHandlerAsync;
            this.indexingService.AlbumArtworkAdded += async (_, e) => await this.RefreshArtworkAsync(e.AlbumIds);

            // Set the initial ArtistOrder		
            this.ArtistOrder = (ArtistOrder)SettingsClient.Get<int>("Ordering", "ArtistsArtistOrder");

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
            this.ArtistOrder = (ArtistOrder)SettingsClient.Get<int>("Ordering", settingName);
        }

        private async void MetadataChangedHandlerAsync(MetadataChangedEventArgs e)
        {
            if (e.IsArtworkChanged) await this.RefreshArtworkAsync();
            if (e.IsArtistChanged | (e.IsAlbumChanged & (this.ArtistOrder == ArtistOrder.Album | this.ArtistOrder == ArtistOrder.All))) await this.GetArtistsAsync(this.ArtistOrder);
            if (e.IsArtistChanged | e.IsAlbumChanged) await this.GetAlbumsAsync(this.SelectedArtists, null, this.AlbumOrder);
            if (e.IsArtistChanged | e.IsAlbumChanged | e.IsTrackChanged) await this.GetTracksAsync(this.SelectedArtists, null, this.SelectedAlbumIds, this.TrackOrder);
        }

        private async Task GetArtistsAsync(ArtistOrder artistOrder)
        {
            try
            {
                // Get all artists from the database
                IList<Artist> artists = await this.artistRepository.GetArtistsAsync(artistOrder);

                // Create new ObservableCollection
                ObservableCollection<ArtistViewModel> artistViewModels = new ObservableCollection<ArtistViewModel>();

                await Task.Run(() =>
                {
                    List<ArtistViewModel> tempArtistViewModels = new List<ArtistViewModel>();

                    // Workaround to make sure the "#" GroupHeader is shown at the top of the list
                    tempArtistViewModels.AddRange(artists.Select(art => new ArtistViewModel { Artist = art, IsHeader = false }).Where(avm => avm.Header.Equals("#")));
                    tempArtistViewModels.AddRange(artists.Select(art => new ArtistViewModel { Artist = art, IsHeader = false }).Where(avm => !avm.Header.Equals("#")));

                    foreach (ArtistViewModel avm in tempArtistViewModels)
                    {
                        artistViewModels.Add(avm);
                    }
                });

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
                this.SelectedArtists = new List<Artist>();

                foreach (ArtistViewModel item in (IList)parameter)
                {
                    this.SelectedArtists.Add(item.Artist);
                }
            }

            // Don't reload the lists when updating Metadata. MetadataChangedHandlerAsync handles that.
            if (this.metadataService.IsUpdatingDatabaseMetadata) return;

            await this.GetAlbumsAsync(this.SelectedArtists, null, this.AlbumOrder);
            this.SetTrackOrder("ArtistsTrackOrder");
            await this.GetTracksAsync(this.SelectedArtists, null, this.SelectedAlbumIds, this.TrackOrder);
        }

        private async Task AddArtistsToPlaylistAsync(IList<Artist> artists, string playlistName)
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

            e.Accepted = DataUtils.FilterArtists(avm.Artist, this.searchService.SearchText);
        }

        private async Task ToggleArtistOrderAsync()
        {
            this.HideSemanticZoom();

            switch (this.ArtistOrder)
            {
                case ArtistOrder.All:
                    this.ArtistOrder = ArtistOrder.Track;
                    break;
                case ArtistOrder.Track:
                    this.ArtistOrder = ArtistOrder.Album;
                    break;
                case ArtistOrder.Album:
                    this.ArtistOrder = ArtistOrder.All;
                    break;
                default:
                    // Cannot happen, but just in case.
                    this.ArtistOrder = ArtistOrder.All;
                    break;
            }

            SettingsClient.Set<int>("Ordering", "ArtistsArtistOrder", (int)this.ArtistOrder);
            await this.GetArtistsAsync(this.ArtistOrder);
        }

        private void UpdateArtistOrderText(ArtistOrder artistOrder)
        {
            switch (artistOrder)
            {
                case ArtistOrder.All:
                    this.artistOrderText = ResourceUtils.GetString("Language_All");
                    break;
                case ArtistOrder.Track:
                    this.artistOrderText = ResourceUtils.GetString("Language_Song");
                    break;
                case ArtistOrder.Album:
                    this.artistOrderText = ResourceUtils.GetString("Language_Album");
                    break;
                default:
                    // Cannot happen, but just in case.
                    this.artistOrderText = ResourceUtils.GetString("Language_All");
                    break;
            }

            RaisePropertyChanged(nameof(this.ArtistOrderText));
        }

        private async Task AddArtistsToNowPlayingAsync(IList<Artist> artists)
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
            await this.GetAlbumsCommonAsync(this.Albums.Select((a) => a.Album).ToList(), this.AlbumOrder);
        }

        protected async override Task SetCoversizeAsync(CoverSizeType coverSize)
        {
            await base.SetCoversizeAsync(coverSize);
            SettingsClient.Set<int>("CoverSizes", "ArtistsCoverSize", (int)coverSize);
        }

        protected async override Task FillListsAsync()
        {
            await this.GetArtistsAsync(this.ArtistOrder);
            await this.GetAlbumsAsync(this.SelectedArtists, null, this.AlbumOrder);
            await this.GetTracksAsync(this.SelectedArtists, null, this.SelectedAlbumIds, this.TrackOrder);
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
            await this.GetTracksAsync(this.SelectedArtists, null, this.SelectedAlbumIds, this.TrackOrder);
        }

        protected override void RefreshLanguage()
        {
            this.UpdateArtistOrderText(this.ArtistOrder);
            this.UpdateAlbumOrderText(this.AlbumOrder);
            this.UpdateTrackOrderText(this.TrackOrder);
            base.RefreshLanguage();
        }
    }
}
