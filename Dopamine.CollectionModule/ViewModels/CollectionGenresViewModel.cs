using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.CollectionModule.Views;
using Dopamine.Common.Presentation.Interfaces;
using Dopamine.Common.Presentation.Utils;
using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Base;
using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.Helpers;
using Digimezzo.Utilities.Log;
using Dopamine.Common.Prism;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Dopamine.CollectionModule.ViewModels
{
    public class CollectionGenresViewModel : CommonAlbumsViewModel, ISemanticZoomViewModel
    {
        #region Variables
        // Repositories
        private IGenreRepository genreRepository;

        // Lists
        private ObservableCollection<ISemanticZoomable> genres;
        private CollectionViewSource genresCvs;
        private IList<Genre> selectedGenres;
        private ObservableCollection<ISemanticZoomSelector> genresZoomSelectors;

        // Flags
        private bool isGenresZoomVisible;

        // Other
        private long genresCount;
        private SubscriptionToken shellMouseUpToken;
        private double leftPaneWidthPercent;
        private double rightPaneWidthPercent;
        private GenreOrder genreOrder;
        private string genreOrderText;
        #endregion

        #region Commands
        public DelegateCommand<string> AddGenresToPlaylistCommand { get; set; }
        public DelegateCommand<object> SelectedGenresCommand { get; set; }
        public DelegateCommand ShowGenresZoomCommand { get; set; }
        public DelegateCommand SemanticJumpCommand { get; set; }
        public DelegateCommand AddGenresToNowPlayingCommand { get; set; }
        public DelegateCommand ToggleGenreOrderCommand { get; set; }
        #endregion

        #region Properties
        public string GenreOrderText
        {
            get { return this.genreOrderText; }
        }

        public GenreOrder GenreOrder
        {
            get { return this.genreOrder; }
            set
            {
                SetProperty<GenreOrder>(ref this.genreOrder, value);
                this.UpdateGenreOrderText(value);
            }
        }

        public double LeftPaneWidthPercent
        {
            get { return this.leftPaneWidthPercent; }
            set
            {
                SetProperty<double>(ref this.leftPaneWidthPercent, value);
                SettingsClient.Set<int>("ColumnWidths", "GenresLeftPaneWidthPercent", Convert.ToInt32(value));
            }
        }

        public double RightPaneWidthPercent
        {
            get { return this.rightPaneWidthPercent; }
            set
            {
                SetProperty<double>(ref this.rightPaneWidthPercent, value);
                SettingsClient.Set<int>("ColumnWidths", "GenresRightPaneWidthPercent", Convert.ToInt32(value));
            }
        }

        public ObservableCollection<ISemanticZoomable> Genres
        {
            get { return this.genres; }
            set { SetProperty<ObservableCollection<ISemanticZoomable>>(ref this.genres, value); }
        }
        ObservableCollection<ISemanticZoomable> ISemanticZoomViewModel.SemanticZoomables
        {
            get { return Genres; }
            set { Genres = value; }
        }

        public CollectionViewSource GenresCvs
        {
            get { return this.genresCvs; }
            set { SetProperty<CollectionViewSource>(ref this.genresCvs, value); }
        }

        public IList<Genre> SelectedGenres
        {
            get { return this.selectedGenres; }
            set { SetProperty<IList<Genre>>(ref this.selectedGenres, value); }
        }

        public long GenresCount
        {
            get { return this.genresCount; }
            set { SetProperty<long>(ref this.genresCount, value); }
        }

        public bool IsGenresZoomVisible
        {
            get { return this.isGenresZoomVisible; }
            set { SetProperty<bool>(ref this.isGenresZoomVisible, value); }
        }

        public ObservableCollection<ISemanticZoomSelector> GenresZoomSelectors
        {
            get { return this.genresZoomSelectors; }
            set { SetProperty<ObservableCollection<ISemanticZoomSelector>>(ref this.genresZoomSelectors, value); }
        }
        ObservableCollection<ISemanticZoomSelector> ISemanticZoomViewModel.SemanticZoomSelectors
        {
            get { return GenresZoomSelectors; }
            set { GenresZoomSelectors = value; }
        }

        public override bool CanOrderByAlbum
        {
            get
            {
                return (this.SelectedGenres != null && this.SelectedGenres.Count > 0) |
                       (this.SelectedAlbums != null && this.SelectedAlbums.Count > 0);
            }
        }
        #endregion

        #region Construction
        public CollectionGenresViewModel(IUnityContainer container, IGenreRepository genreRepository) : base(container)
        {
            // Repositories
            this.genreRepository = genreRepository;

            // Commands
            this.ToggleGenreOrderCommand = new DelegateCommand(async () => await this.ToggleGenreOrderAsync());
            this.ToggleTrackOrderCommand = new DelegateCommand(async () => await this.ToggleTrackOrderAsync());
            this.ToggleAlbumOrderCommand = new DelegateCommand(async () => await this.ToggleAlbumOrderAsync());
            this.RemoveSelectedTracksCommand = new DelegateCommand(async () => await this.RemoveTracksFromCollectionAsync(this.SelectedTracks), () => !this.IsIndexing);
            this.RemoveSelectedTracksFromDiskCommand = new DelegateCommand(async () => await this.RemoveTracksFromDiskAsync(this.SelectedTracks), () => !this.IsIndexing);
            this.AddGenresToPlaylistCommand = new DelegateCommand<string>(async (playlistName) => await this.AddGenresToPlaylistAsync(this.SelectedGenres, playlistName));
            this.SelectedGenresCommand = new DelegateCommand<object>(async (parameter) => await this.SelectedGenresHandlerAsync(parameter));
            this.ShowGenresZoomCommand = new DelegateCommand(async () => await this.ShowSemanticZoomAsync());
            this.SemanticJumpCommand = new DelegateCommand(() => this.IsGenresZoomVisible = false);
            this.AddGenresToNowPlayingCommand = new DelegateCommand(async () => await this.AddGenresToNowPlayingAsync(this.SelectedGenres));

            // Events
            this.eventAggregator.GetEvent<RemoveSelectedTracksWithKeyDelete>().Subscribe((screenName) =>
            {
                if (screenName == typeof(CollectionGenres).FullName) this.RemoveSelectedTracksCommand.Execute();
            });

            this.eventAggregator.GetEvent<SettingEnableRatingChanged>().Subscribe(async (enableRating) =>
            {
                this.EnableRating = enableRating;
                this.SetTrackOrder("GenresTrackOrder");
                await this.GetTracksAsync(null, this.SelectedGenres, this.SelectedAlbums, this.TrackOrder);
            });

            this.eventAggregator.GetEvent<SettingEnableLoveChanged>().Subscribe(async (enableLove) =>
            {
                this.EnableLove = enableLove;
                this.SetTrackOrder("GenresTrackOrder");
                await this.GetTracksAsync(null, this.SelectedGenres, this.SelectedAlbums, this.TrackOrder);
            });

            // MetadataService
            this.metadataService.MetadataChanged += MetadataChangedHandlerAsync;

            // IndexingService
            this.indexingService.RefreshArtwork += async (_, __) => await this.collectionService.RefreshArtworkAsync(this.Albums);

            // Set the initial GenreOrder
            this.SetGenreOrder("GenresGenreOrder");

            // Set the initial AlbumOrder
            this.AlbumOrder = (AlbumOrder)SettingsClient.Get<int>("Ordering", "GenresAlbumOrder");

            // Set the initial TrackOrder
            this.SetTrackOrder("GenresTrackOrder");

            // Subscribe to Events and Commands on creation
            this.Subscribe();

            // Set width of the panels
            this.LeftPaneWidthPercent = SettingsClient.Get<int>("ColumnWidths", "GenresLeftPaneWidthPercent");
            this.RightPaneWidthPercent = SettingsClient.Get<int>("ColumnWidths", "GenresRightPaneWidthPercent");

            // Cover size
            this.SetCoversizeAsync((CoverSizeType)SettingsClient.Get<int>("CoverSizes", "GenresCoverSize"));
        }
        #endregion

        #region ISemanticZoomViewModel
        public async Task ShowSemanticZoomAsync()
        {
            this.GenresZoomSelectors = await SemanticZoomUtils.UpdateSemanticZoomSelectors(this.GenresCvs.View);

            this.IsGenresZoomVisible = true;
        }

        public void HideSemanticZoom()
        {
            this.IsGenresZoomVisible = false;
        }

        public void UpdateSemanticZoomHeaders()
        {
            string previousHeader = string.Empty;

            foreach (GenreViewModel gvm in this.GenresCvs.View)
            {
                if (string.IsNullOrEmpty(previousHeader) || !gvm.Header.Equals(previousHeader))
                {
                    previousHeader = gvm.Header;
                    gvm.IsHeader = true;
                }
                else
                {
                    gvm.IsHeader = false;
                }
            }
        }
        #endregion

        #region Private
        private async Task ToggleGenreOrderAsync()
        {
            switch (this.GenreOrder)
            {
                case GenreOrder.Alphabetical:
                    this.GenreOrder = GenreOrder.ReverseAlphabetical;
                    break;
                case GenreOrder.ReverseAlphabetical:
                    this.GenreOrder = GenreOrder.Alphabetical;
                    break;
                default:
                    // Cannot happen, but just in case.
                    this.GenreOrder = GenreOrder.Alphabetical;
                    break;
            }

            SettingsClient.Set<int>("Ordering", "GenresGenreOrder", (int)this.GenreOrder);
            await this.GetGenresCommonAsync(this.Genres.Select((g) => ((GenreViewModel)g).Genre).ToList(), this.GenreOrder);
        }

        private void SetGenreOrder(string settingName)
        {
            this.GenreOrder = (GenreOrder)SettingsClient.Get<int>("Ordering", settingName);
        }

        protected void UpdateGenreOrderText(GenreOrder genreOrder)
        {
            switch (genreOrder)
            {
                case GenreOrder.Alphabetical:
                    this.genreOrderText = ResourceUtils.GetStringResource("Language_A_Z");
                    break;
                case GenreOrder.ReverseAlphabetical:
                    this.genreOrderText = ResourceUtils.GetStringResource("Language_Z_A");
                    break;
                default:
                    // Cannot happen, but just in case.
                    this.genreOrderText = ResourceUtils.GetStringResource("Language_A_Z");
                    break;
            }

            OnPropertyChanged(() => this.GenreOrderText);
        }

        private async void MetadataChangedHandlerAsync(MetadataChangedEventArgs e)
        {
            if (e.IsArtworkChanged) await this.collectionService.RefreshArtworkAsync(this.Albums);
            if (e.IsGenreChanged) await this.GetGenresAsync(this.GenreOrder);
            if (e.IsGenreChanged | e.IsAlbumChanged) await this.GetAlbumsAsync(null, this.SelectedGenres, this.AlbumOrder);
            if (e.IsGenreChanged | e.IsAlbumChanged | e.IsTrackChanged) await this.GetTracksAsync(null, this.SelectedGenres, this.SelectedAlbums, this.TrackOrder);
        }

        private async Task GetGenresAsync(GenreOrder genreOrder)
        {
            await this.GetGenresCommonAsync(await this.genreRepository.GetGenresAsync(), genreOrder);
        }

        private async Task GetGenresCommonAsync(IList<Genre> genres, GenreOrder genreOrder)
        {
            try
            {
                // Order the incoming Genres
                List<Genre> orderedGenres = await Common.Database.Utils.OrderGenresAsync(genres, genreOrder);

                // Create new ObservableCollection
                ObservableCollection<GenreViewModel> genreViewModels = new ObservableCollection<GenreViewModel>();

                await Task.Run(() =>
                {
                    var tempGenreViewModels = new List<GenreViewModel>();

                    // Workaround to make sure the "#" GroupHeader is shown at the top of the list
                    if(genreOrder == GenreOrder.Alphabetical) tempGenreViewModels.AddRange(orderedGenres.Select((gen) => new GenreViewModel { Genre = gen, IsHeader = false }).Where((gvm) => gvm.Header.Equals("#")));
                    tempGenreViewModels.AddRange(orderedGenres.Select((gen) => new GenreViewModel { Genre = gen, IsHeader = false }).Where((gvm) => !gvm.Header.Equals("#")));
                    if (genreOrder == GenreOrder.ReverseAlphabetical) tempGenreViewModels.AddRange(orderedGenres.Select((gen) => new GenreViewModel { Genre = gen, IsHeader = false }).Where((gvm) => gvm.Header.Equals("#")));

                    foreach (GenreViewModel gvm in tempGenreViewModels)
                    {
                        genreViewModels.Add(gvm);
                    }
                });

                // Unbind to improve UI performance
                if (this.GenresCvs != null) this.GenresCvs.Filter -= new FilterEventHandler(GenresCvs_Filter);
                this.Genres = null;
                this.GenresCvs = null;

                // Populate ObservableCollection
                this.Genres = new ObservableCollection<ISemanticZoomable>(genreViewModels);
            }
            catch (Exception ex)
            {
                LogClient.Error("An error occurred while getting Genres. Exception: {0}", ex.Message);

                // Failed getting Genres. Create empty ObservableCollection.
                this.Genres = new ObservableCollection<ISemanticZoomable>();
            }

            // Populate CollectionViewSource
            this.GenresCvs = new CollectionViewSource { Source = this.Genres };
            this.GenresCvs.Filter += new FilterEventHandler(GenresCvs_Filter);

            // Update count
            this.GenresCount = this.GenresCvs.View.Cast<ISemanticZoomable>().Count();

            // Update Semantic Zoom Headers
            this.UpdateSemanticZoomHeaders();
        }

        private async Task SelectedGenresHandlerAsync(object parameter)
        {
            if (parameter != null)
            {
                this.SelectedGenres = new List<Genre>();

                foreach (GenreViewModel item in (IList)parameter)
                {
                    this.SelectedGenres.Add(item.Genre);
                }
            }

            // Don't reload the lists when updating Metadata. MetadataChangedHandlerAsync handles that.
            if (this.metadataService.IsUpdatingDatabaseMetadata) return;

            await this.GetAlbumsAsync(null, this.SelectedGenres, (AlbumOrder)SettingsClient.Get<int>("Ordering", "GenresAlbumOrder"));
            this.SetTrackOrder("GenresTrackOrder");
            await this.GetTracksAsync(null, this.SelectedGenres, this.SelectedAlbums, this.TrackOrder);
        }

        private async Task AddGenresToPlaylistAsync(IList<Genre> genres, string playlistName)
        {
            AddPlaylistResult addPlaylistResult = AddPlaylistResult.Success; // Default Success

            // If no playlist is provided, first create one.
            if (playlistName == null)
            {
                var responseText = ResourceUtils.GetStringResource("Language_New_Playlist");

                if (this.dialogService.ShowInputDialog(
                    0xea37,
                    16,
                    ResourceUtils.GetStringResource("Language_New_Playlist"),
                    ResourceUtils.GetStringResource("Language_Enter_Name_For_New_Playlist"),
                    ResourceUtils.GetStringResource("Language_Ok"),
                    ResourceUtils.GetStringResource("Language_Cancel"),
                    ref responseText))
                {
                    playlistName = responseText;
                    addPlaylistResult = await this.collectionService.AddPlaylistAsync(playlistName);
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
                    AddToPlaylistResult result = await this.collectionService.AddGenresToPlaylistAsync(genres, playlistName);

                    if (!result.IsSuccess)
                    {
                        this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetStringResource("Language_Error"), ResourceUtils.GetStringResource("Language_Error_Adding_Songs_To_Playlist").Replace("%playlistname%", "\"" + playlistName + "\""), ResourceUtils.GetStringResource("Language_Ok"), true, ResourceUtils.GetStringResource("Language_Log_File"));
                    }
                    break;
                case AddPlaylistResult.Error:
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetStringResource("Language_Error"),
                        ResourceUtils.GetStringResource("Language_Error_Adding_Playlist"),
                        ResourceUtils.GetStringResource("Language_Ok"),
                        true,
                        ResourceUtils.GetStringResource("Language_Log_File"));
                    break;
                case AddPlaylistResult.Blank:
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetStringResource("Language_Error"),
                        ResourceUtils.GetStringResource("Language_Provide_Playlist_Name"),
                        ResourceUtils.GetStringResource("Language_Ok"),
                        false,
                        string.Empty);
                    break;
                default:
                    // Never happens
                    break;
            }
        }

        private async Task AddGenresToNowPlayingAsync(IList<Genre> genres)
        {
            AddToQueueResult result = await this.playbackService.AddToQueue(genres);

            if (!result.IsSuccess)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetStringResource("Language_Error"), ResourceUtils.GetStringResource("Language_Error_Adding_Genres_To_Now_Playing"), ResourceUtils.GetStringResource("Language_Ok"), true, ResourceUtils.GetStringResource("Language_Log_File"));
            }
        }

        private void GenresCvs_Filter(object sender, FilterEventArgs e)
        {
            GenreViewModel gvm = e.Item as GenreViewModel;

            e.Accepted = Dopamine.Common.Database.Utils.FilterGenres(gvm.Genre, this.searchService.SearchText);
        }
        #endregion

        #region Protected
        protected async Task ToggleTrackOrderAsync()
        {
            base.ToggleTrackOrder();

            SettingsClient.Set<int>("Ordering", "GenresTrackOrder", (int)this.TrackOrder);
            await this.GetTracksCommonAsync(this.Tracks.Select((t) => t.Track).ToList(), this.TrackOrder);
        }

        protected async Task ToggleAlbumOrderAsync()
        {
            base.ToggleAlbumOrder();

            SettingsClient.Set<int>("Ordering", "GenresAlbumOrder", (int)this.AlbumOrder);
            await this.GetAlbumsCommonAsync(this.Albums.Select((a) => a.Album).ToList(), this.AlbumOrder);
        }
        #endregion

        #region Overrides
        protected async override Task SetCoversizeAsync(CoverSizeType coverSize)
        {
            await base.SetCoversizeAsync(coverSize);
            SettingsClient.Set<int>("CoverSizes", "GenresCoverSize", (int)coverSize);
        }

        protected async override Task FillListsAsync()
        {
            await this.GetGenresAsync(this.GenreOrder);
            await this.GetAlbumsAsync(null, null, this.AlbumOrder);
            await this.GetTracksAsync(null, null, null, this.TrackOrder);
        }

        protected override void FilterLists()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Genres
                if (this.GenresCvs != null)
                {
                    this.GenresCvs.View.Refresh();
                    this.GenresCount = this.GenresCvs.View.Cast<ISemanticZoomable>().Count();
                    this.UpdateSemanticZoomHeaders();
                }
            });

            base.FilterLists();
        }

        protected async override Task SelectedAlbumsHandlerAsync(object parameter)
        {
            await base.SelectedAlbumsHandlerAsync(parameter);

            this.SetTrackOrder("GenresTrackOrder");
            await this.GetTracksAsync(null, this.SelectedGenres, this.SelectedAlbums, this.TrackOrder);
        }

        protected override void Unsubscribe()
        {
            // Commands
            ApplicationCommands.SemanticJumpCommand.UnregisterCommand(this.SemanticJumpCommand);
            ApplicationCommands.AddTracksToPlaylistCommand.UnregisterCommand(this.AddTracksToPlaylistCommand);
            ApplicationCommands.AddAlbumsToPlaylistCommand.UnregisterCommand(this.AddAlbumsToPlaylistCommand);
            ApplicationCommands.AddGenresToPlaylistCommand.UnregisterCommand(this.AddGenresToPlaylistCommand);

            // Events
            this.eventAggregator.GetEvent<ShellMouseUp>().Unsubscribe(this.shellMouseUpToken);

            // Other
            this.IsGenresZoomVisible = false;
        }

        protected override void Subscribe()
        {
            // Prevents subscribing twice
            this.Unsubscribe();

            // Commands
            ApplicationCommands.SemanticJumpCommand.RegisterCommand(this.SemanticJumpCommand);
            ApplicationCommands.AddTracksToPlaylistCommand.RegisterCommand(this.AddTracksToPlaylistCommand);
            ApplicationCommands.AddAlbumsToPlaylistCommand.RegisterCommand(this.AddAlbumsToPlaylistCommand);
            ApplicationCommands.AddGenresToPlaylistCommand.RegisterCommand(this.AddGenresToPlaylistCommand);

            // Events
            this.shellMouseUpToken = this.eventAggregator.GetEvent<ShellMouseUp>().Subscribe((_) => this.IsGenresZoomVisible = false);
        }

        protected override void RefreshLanguage()
        {
            this.UpdateGenreOrderText(this.GenreOrder);
            this.UpdateAlbumOrderText(this.AlbumOrder);
            this.UpdateTrackOrderText(this.TrackOrder);
        }
        #endregion
    }
}
