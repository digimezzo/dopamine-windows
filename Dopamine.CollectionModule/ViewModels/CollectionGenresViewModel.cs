using Dopamine.Common.Presentation.Interfaces;
using Dopamine.Common.Presentation.Utils;
using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Base;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Helpers;
using Dopamine.Core.Logging;
using Dopamine.Core.Prism;
using Dopamine.Core.Settings;
using Dopamine.Core.Utils;
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
        #endregion

        #region Commands
        public DelegateCommand<string> AddGenresToPlaylistCommand { get; set; }
        public DelegateCommand<object> SelectedGenresCommand { get; set; }
        public DelegateCommand ShowGenresZoomCommand { get; set; }
        public DelegateCommand SemanticJumpCommand { get; set; }
        public DelegateCommand AddGenresToNowPlayingCommand { get; set; }
        #endregion

        #region Properties
        public double LeftPaneWidthPercent
        {
            get { return this.leftPaneWidthPercent; }
            set
            {
                SetProperty<double>(ref this.leftPaneWidthPercent, value);
                XmlSettingsClient.Instance.Set<int>("ColumnWidths", "GenresLeftPaneWidthPercent", Convert.ToInt32(value));
            }
        }

        public double RightPaneWidthPercent
        {
            get { return this.rightPaneWidthPercent; }
            set
            {
                SetProperty<double>(ref this.rightPaneWidthPercent, value);
                XmlSettingsClient.Instance.Set<int>("ColumnWidths", "GenresRightPaneWidthPercent", Convert.ToInt32(value));
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
        public CollectionGenresViewModel(IGenreRepository genreRepository) : base()
        {
            // Repositories
            this.genreRepository = genreRepository;

            // Commands
            this.ToggleTrackOrderCommand = new DelegateCommand(async () => await this.ToggleTrackOrderAsync());
            this.ToggleAlbumOrderCommand = new DelegateCommand(async () => await this.ToggleAlbumOrderAsync());
            this.RemoveSelectedTracksCommand = new DelegateCommand(async () => await this.RemoveTracksFromCollectionAsync(this.SelectedTracks), () => !this.IsIndexing);
            this.AddGenresToPlaylistCommand = new DelegateCommand<string>(async (playlistName) => await this.AddGenresToPlaylistAsync(this.SelectedGenres, playlistName));
            this.SelectedGenresCommand = new DelegateCommand<object>(async (parameter) => await this.SelectedGenresHandlerAsync(parameter));
            this.ShowGenresZoomCommand = new DelegateCommand(async () => await this.ShowSemanticZoomAsync());
            this.SemanticJumpCommand = new DelegateCommand(() => this.IsGenresZoomVisible = false);
            this.AddGenresToNowPlayingCommand = new DelegateCommand(async () => await this.AddGenresToNowPlayingAsync(this.SelectedGenres));

            // Events
            this.eventAggregator.GetEvent<SettingEnableRatingChanged>().Subscribe(async (enableRating) =>
            {
                this.EnableRating = enableRating;
                this.SetTrackOrder("GenresTrackOrder");
                await this.GetTracksAsync(null, this.SelectedGenres, this.SelectedAlbums, this.TrackOrder);
            });

            this.eventAggregator.GetEvent<SettingUseStarRatingChanged>().Subscribe((useStarRating) => this.UseStarRating = useStarRating);

            // MetadataService
            this.metadataService.MetadataChanged += MetadataChangedHandlerAsync;

            // IndexingService
            this.indexingService.RefreshArtwork += async (_, __) => await this.collectionService.RefreshArtworkAsync(this.Albums, this.Tracks);

            // Set the initial AlbumOrder
            this.AlbumOrder = (AlbumOrder)XmlSettingsClient.Instance.Get<int>("Ordering", "GenresAlbumOrder");

            // Set the initial TrackOrder
            this.SetTrackOrder("GenresTrackOrder");

            // Subscribe to Events and Commands on creation
            this.Subscribe();

            // Set width of the panels
            this.LeftPaneWidthPercent = XmlSettingsClient.Instance.Get<int>("ColumnWidths", "GenresLeftPaneWidthPercent");
            this.RightPaneWidthPercent = XmlSettingsClient.Instance.Get<int>("ColumnWidths", "GenresRightPaneWidthPercent");

            // Cover size
            this.SetCoversizeAsync((CoverSizeType)XmlSettingsClient.Instance.Get<int>("CoverSizes", "GenresCoverSize"));
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
            }
        }
        #endregion

        #region Private
        private async void MetadataChangedHandlerAsync(MetadataChangedEventArgs e)
        {
            if (e.IsAlbumArtworkMetadataChanged)
            {
                await this.collectionService.RefreshArtworkAsync(this.Albums, this.Tracks);
            }

            if (e.IsGenreMetadataChanged)
            {
                await this.GetGenresAsync();
            }

            if (e.IsGenreMetadataChanged | e.IsAlbumTitleMetadataChanged | e.IsAlbumArtistMetadataChanged | e.IsAlbumYearMetadataChanged)
            {
                await this.GetAlbumsAsync(null, this.SelectedGenres, this.AlbumOrder);
            }

            if (e.IsGenreMetadataChanged | e.IsAlbumTitleMetadataChanged | e.IsAlbumArtistMetadataChanged | e.IsTrackMetadataChanged)
            {
                await this.GetTracksAsync(null, this.SelectedGenres, this.SelectedAlbums, this.TrackOrder);
            }
        }

        private async Task GetGenresAsync()
        {
            try
            {
                // Get Genres from database
                List<Genre> genres = await this.genreRepository.GetGenresAsync();

                // Create new ObservableCollection
                ObservableCollection<GenreViewModel> genreViewModels = new ObservableCollection<GenreViewModel>();

                await Task.Run(() =>
                {
                    var tempGenreViewModels = new List<GenreViewModel>();

                    // Workaround to make sure the "#" GroupHeader is shown at the top of the list
                    tempGenreViewModels.AddRange(genres.Select((gen) => new GenreViewModel { Genre = gen, IsHeader = false }).Where((gvm) => gvm.Header.Equals("#")));
                    tempGenreViewModels.AddRange(genres.Select((gen) => new GenreViewModel { Genre = gen, IsHeader = false }).Where((gvm) => !gvm.Header.Equals("#")));

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
                LogClient.Instance.Logger.Error("An error occurred while getting Genres. Exception: {0}", ex.Message);

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

            await this.GetAlbumsAsync(null, this.SelectedGenres, (AlbumOrder)XmlSettingsClient.Instance.Get<int>("Ordering", "GenresAlbumOrder"));
            this.SetTrackOrder("GenresTrackOrder");
            await this.GetTracksAsync(null, this.SelectedGenres, this.SelectedAlbums, this.TrackOrder);
        }

        private async Task AddGenresToPlaylistAsync(IList<Genre> genres, string playlistName)
        {
            AddToPlaylistResult result = await this.collectionService.AddGenresToPlaylistAsync(genres, playlistName);

            if (!result.IsSuccess)
            {
                this.dialogService.ShowNotification(
                    0xe711, 
                    16, 
                    ResourceUtils.GetStringResource("Language_Error"),
                    ResourceUtils.GetStringResource("Language_Error_Adding_Genres_To_Playlist").Replace("%playlistname%", "\"" + playlistName + "\""), 
                    ResourceUtils.GetStringResource("Language_Ok"), 
                    true, 
                    ResourceUtils.GetStringResource("Language_Log_File"));
            }
        }

        private async Task AddGenresToNowPlayingAsync(IList<Genre> genres)
        {
            AddToQueueResult result  = await this.playbackService.AddToQueue(genres);

            if (!result.IsSuccess)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetStringResource("Language_Error"), ResourceUtils.GetStringResource("Language_Error_Adding_Genres_To_Now_Playing"), ResourceUtils.GetStringResource("Language_Ok"), true, ResourceUtils.GetStringResource("Language_Log_File"));
            }
        }

        private void GenresCvs_Filter(object sender, FilterEventArgs e)
        {
            GenreViewModel gvm = e.Item as GenreViewModel;

            e.Accepted = Dopamine.Core.Database.Utils.FilterGenres(gvm.Genre, this.searchService.SearchText);
        }
        #endregion

        #region Protected
        protected async Task ToggleTrackOrderAsync()
        {
            base.ToggleTrackOrder();

            XmlSettingsClient.Instance.Set<int>("Ordering", "GenresTrackOrder", (int)this.TrackOrder);
            await this.GetTracksCommonAsync(this.Tracks.Select((t) => t.TrackInfo).ToList(), this.TrackOrder);
        }

        protected async Task ToggleAlbumOrderAsync()
        {
            base.ToggleAlbumOrder();

            XmlSettingsClient.Instance.Set<int>("Ordering", "GenresAlbumOrder", (int)this.AlbumOrder);
            await this.GetAlbumsCommonAsync(this.Albums.Select((a) => a.Album).ToList(), this.AlbumOrder);
        }
        #endregion

        #region Overrides
        protected async override Task SetCoversizeAsync(CoverSizeType coverSize)
        {
            await base.SetCoversizeAsync(coverSize);
            XmlSettingsClient.Instance.Set<int>("CoverSizes", "GenresCoverSize", (int)coverSize);
        }

        protected async override Task FillListsAsync()
        {

            await this.GetGenresAsync();
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
            ApplicationCommands.RemoveSelectedTracksCommand.UnregisterCommand(this.RemoveSelectedTracksCommand);
            ApplicationCommands.SemanticJumpCommand.UnregisterCommand(this.SemanticJumpCommand);

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
            ApplicationCommands.RemoveSelectedTracksCommand.RegisterCommand(this.RemoveSelectedTracksCommand);
            ApplicationCommands.SemanticJumpCommand.RegisterCommand(this.SemanticJumpCommand);

            // Events
            this.shellMouseUpToken = this.eventAggregator.GetEvent<ShellMouseUp>().Subscribe((_) => this.IsGenresZoomVisible = false);
        }

        protected override void RefreshLanguage()
        {
            this.UpdateAlbumOrderText(this.AlbumOrder);
            this.UpdateTrackOrderText(this.TrackOrder);
        }
        #endregion
    }
}
