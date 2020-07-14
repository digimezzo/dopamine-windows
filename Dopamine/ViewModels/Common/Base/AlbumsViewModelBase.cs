using Digimezzo.Foundation.Core.Logging;
using Digimezzo.Foundation.Core.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Data;
using Dopamine.Data.Entities;
using Dopamine.Data.Repositories;
using Dopamine.Services.Cache;
using Dopamine.Services.Collection;
using Dopamine.Services.Dialog;
using Dopamine.Services.Entities;
using Dopamine.Services.Indexing;
using Dopamine.Services.Playback;
using Dopamine.Services.Playlist;
using Dopamine.Services.Search;
using Dopamine.Views.Common;
using Prism.Commands;
using Prism.Ioc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Dopamine.ViewModels.Common.Base
{
    public abstract class AlbumsViewModelBase : TracksViewModelBase
    {
        private IContainerProvider container;
        private ICollectionService collectionService;
        private IPlaybackService playbackService;
        private IDialogService dialogService;
        private ISearchService searchService;
        private IPlaylistService playlistService;
        private ICacheService cacheService;
        private IIndexingService indexingService;
        private IAlbumArtworkRepository albumArtworkRepository;
        private ObservableCollection<AlbumViewModel> albums;
        private CollectionViewSource albumsCvs;
        private IList<AlbumViewModel> selectedAlbums;
        private bool delaySelectedAlbums;
        private long albumsCount;
        private AlbumOrder albumOrder;
        private string albumOrderText;
        private double coverSize;
        private double albumWidth;
        private double albumHeight;
        private CoverSizeType selectedCoverSize;

        public DelegateCommand ToggleAlbumOrderCommand { get; set; }

        public DelegateCommand<string> AddAlbumsToPlaylistCommand { get; set; }

        public DelegateCommand<object> SelectedAlbumsCommand { get; set; }

        public DelegateCommand EditAlbumCommand { get; set; }

        public DelegateCommand AddAlbumsToNowPlayingCommand { get; set; }

        public DelegateCommand<string> SetCoverSizeCommand { get; set; }

        public DelegateCommand DelaySelectedAlbumsCommand { get; set; }

        public DelegateCommand ShuffleSelectedAlbumsCommand { get; set; }

        public double UpscaledCoverSize => this.CoverSize * Constants.CoverUpscaleFactor;

        public bool IsSmallCoverSizeSelected => this.selectedCoverSize == CoverSizeType.Small;

        public bool IsMediumCoverSizeSelected => this.selectedCoverSize == CoverSizeType.Medium;

        public bool IsLargeCoverSizeSelected => this.selectedCoverSize == CoverSizeType.Large;

        public string AlbumOrderText => this.albumOrderText;

        public double CoverSize
        {
            get { return this.coverSize; }
            set { SetProperty<double>(ref this.coverSize, value); }
        }

        public double AlbumWidth
        {
            get { return this.albumWidth; }
            set { SetProperty<double>(ref this.albumWidth, value); }
        }

        public double AlbumHeight
        {
            get { return this.albumHeight; }
            set { SetProperty<double>(ref this.albumHeight, value); }
        }

        public ObservableCollection<AlbumViewModel> Albums
        {
            get { return this.albums; }
            set { SetProperty<ObservableCollection<AlbumViewModel>>(ref this.albums, value); }
        }

        public CollectionViewSource AlbumsCvs
        {
            get { return this.albumsCvs; }
            set { SetProperty<CollectionViewSource>(ref this.albumsCvs, value); }
        }

        public IList<AlbumViewModel> SelectedAlbums
        {
            get { return this.selectedAlbums; }
            set
            {
                SetProperty<IList<AlbumViewModel>>(ref this.selectedAlbums, value);
            }
        }

        public long AlbumsCount
        {
            get { return this.albumsCount; }
            set { SetProperty<long>(ref this.albumsCount, value); }
        }

        public AlbumOrder AlbumOrder
        {
            get { return this.albumOrder; }
            set
            {
                SetProperty<AlbumOrder>(ref this.albumOrder, value);

                this.UpdateAlbumOrderText(value);
            }
        }

        public AlbumsViewModelBase(IContainerProvider container) : base(container)
        {
            // Dependency injection
            this.container = container;
            this.collectionService = container.Resolve<ICollectionService>();
            this.playbackService = container.Resolve<IPlaybackService>();
            this.dialogService = container.Resolve<IDialogService>();
            this.searchService = container.Resolve<ISearchService>();
            this.playlistService = container.Resolve<IPlaylistService>();
            this.cacheService = container.Resolve<ICacheService>();
            this.indexingService = container.Resolve<IIndexingService>();
            this.albumArtworkRepository = container.Resolve<IAlbumArtworkRepository>();

            // Commands
            this.ToggleAlbumOrderCommand = new DelegateCommand(() => this.ToggleAlbumOrder());
            this.ShuffleSelectedAlbumsCommand = new DelegateCommand(async () => await this.playbackService.EnqueueAlbumsAsync(this.SelectedAlbums, true, false));
            this.AddAlbumsToPlaylistCommand = new DelegateCommand<string>(async (playlistName) => await this.AddAlbumsToPlaylistAsync(this.SelectedAlbums, playlistName));
            this.EditAlbumCommand = new DelegateCommand(() => this.EditSelectedAlbum(), () => !this.IsIndexing);
            this.AddAlbumsToNowPlayingCommand = new DelegateCommand(async () => await this.AddAlbumsToNowPlayingAsync(this.SelectedAlbums));
            this.DelaySelectedAlbumsCommand = new DelegateCommand(() => this.delaySelectedAlbums = true);

            // Events
            this.indexingService.AlbumArtworkAdded += async (_, e) => await this.RefreshAlbumArtworkAsync(e.AlbumKeys);

            this.SelectedAlbumsCommand = new DelegateCommand<object>(async (parameter) =>
            {
                if (this.delaySelectedAlbums)
                {
                    await Task.Delay(Constants.DelaySelectedAlbumsDelay);
                }

                this.delaySelectedAlbums = false;
                await this.SelectedAlbumsHandlerAsync(parameter);
            });

            this.SetCoverSizeCommand = new DelegateCommand<string>(async (coverSize) =>
            {
                if (int.TryParse(coverSize, out int selectedCoverSize))
                {
                    await this.SetCoversizeAsync((CoverSizeType)selectedCoverSize);
                }
            });
        }

        public async Task LoadAlbumArtworkAsync(int delayMilliSeconds)
        {
            await Task.Delay(delayMilliSeconds);

            IList<AlbumArtwork> allAlbumArtwork = await this.albumArtworkRepository.GetAlbumArtworkAsync();

            await this.SetAlbumArtwork(allAlbumArtwork);
        }

        public async Task RefreshAlbumArtworkAsync(IList<string> albumsKeys = null)
        {
            IList<AlbumArtwork> allAlbumArtwork = await this.albumArtworkRepository.GetAlbumArtworkAsync();

            await this.SetAlbumArtwork(allAlbumArtwork, albumsKeys);
        }

        private async Task SetAlbumArtwork(IList<AlbumArtwork> allAlbumArtwork, IList<string> albumsKeys = null)
        {
            if (this.albums != null && this.albums.Count > 0)
            {
                await Task.Run(() =>
                {
                    foreach (AlbumViewModel alb in this.albums)
                    {
                        try
                        {
                            if (allAlbumArtwork != null && allAlbumArtwork.Count > 0 && albumsKeys != null ? albumsKeys.Contains(alb.AlbumKey) : true)
                            {
                                AlbumArtwork albumArtwork = allAlbumArtwork.Where(a => a.AlbumKey.Equals(alb.AlbumKey)).FirstOrDefault();

                                if (albumArtwork != null)
                                {
                                    alb.ArtworkPath = this.cacheService.GetCachedArtworkPath(albumArtwork.ArtworkID);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Error while refreshing artwork for Album {0}/{1}. Exception: {2}", alb.AlbumTitle, alb.AlbumArtist, ex.Message);
                        }
                    }
                });
            }
        }

        private void EditSelectedAlbum()
        {
            if (this.SelectedAlbums == null || this.SelectedAlbums.Count == 0)
            {
                return;
            }

            EditAlbum view = this.container.Resolve<EditAlbum>();
            view.DataContext = this.container.Resolve<Func<AlbumViewModel, EditAlbumViewModel>>()(this.SelectedAlbums.First());

            this.dialogService.ShowCustomDialog(
                0xe104,
                14,
                ResourceUtils.GetString("Language_Edit_Album"),
                view,
                405,
                450,
                false,
                true,
                true,
                true,
                ResourceUtils.GetString("Language_Ok"),
                ResourceUtils.GetString("Language_Cancel"),
                ((EditAlbumViewModel)view.DataContext).SaveAlbumAsync);
        }

        private void AlbumsCvs_Filter(object sender, FilterEventArgs e)
        {
            AlbumViewModel avm = e.Item as AlbumViewModel;
            e.Accepted = Services.Utils.EntityUtils.FilterAlbums(avm, this.searchService.SearchText);
        }

        protected void UpdateAlbumOrderText(AlbumOrder albumOrder)
        {
            switch (albumOrder)
            {
                case AlbumOrder.Alphabetical:
                    this.albumOrderText = ResourceUtils.GetString("Language_A_Z");
                    break;
                case AlbumOrder.ByDateAdded:
                    this.albumOrderText = ResourceUtils.GetString("Language_By_Date_Added");
                    break;
                case AlbumOrder.ByDateCreated:
                    this.albumOrderText = ResourceUtils.GetString("Language_By_Date_Created");
                    break;
                case AlbumOrder.ByAlbumArtist:
                    this.albumOrderText = ResourceUtils.GetString("Language_By_Album_Artist");
                    break;
                case AlbumOrder.ByYearDescending:
                    this.albumOrderText = ResourceUtils.GetString("Language_By_Year_Descending");
                    break;
                case AlbumOrder.ByYearAscending:
                    this.albumOrderText = ResourceUtils.GetString("Language_By_Year_Ascending");
                    break;
                default:
                    // Cannot happen, but just in case.
                    this.albumOrderText = ResourceUtils.GetString("Language_A_Z");
                    break;
            }

            RaisePropertyChanged(nameof(this.AlbumOrderText));
        }

        protected async Task GetArtistAlbumsAsync(IList<string> selectedArtists, ArtistType artistType, AlbumOrder albumOrder)
        {
            if (!selectedArtists.IsNullOrEmpty())
            {
                await this.GetAlbumsCommonAsync(await this.collectionService.GetArtistAlbumsAsync(selectedArtists, artistType), albumOrder);

                return;
            }

            await this.GetAlbumsCommonAsync(await this.collectionService.GetAllAlbumsAsync(), albumOrder);
        }

        protected async Task GetGenreAlbumsAsync(IList<string> selectedGenres, AlbumOrder albumOrder)
        {
            if (!selectedGenres.IsNullOrEmpty())
            {
                await this.GetAlbumsCommonAsync(await this.collectionService.GetGenreAlbumsAsync(selectedGenres), albumOrder);

                return;
            }

            await this.GetAlbumsCommonAsync(await this.collectionService.GetAllAlbumsAsync(), albumOrder);
        }

        protected async Task GetAllAlbumsAsync(AlbumOrder albumOrder)
        {
            await this.GetAlbumsCommonAsync(await this.collectionService.GetAllAlbumsAsync(), albumOrder);
        }

        protected void ClearAlbums()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (this.AlbumsCvs != null)
                {
                    this.AlbumsCvs.Filter -= new FilterEventHandler(AlbumsCvs_Filter);
                }

                this.AlbumsCvs = null;
            });

            this.Albums = null;
        }

        protected async Task GetAlbumsCommonAsync(IList<AlbumViewModel> albums, AlbumOrder albumOrder)
        {
            try
            {
                // Order the incoming Albums
                IList<AlbumViewModel> orderedAlbums = await this.collectionService.OrderAlbumsAsync(albums, albumOrder);

                // Create new ObservableCollection
                var albumViewModels = new ObservableCollection<AlbumViewModel>(orderedAlbums);

                // Unbind to improve UI performance
                this.ClearAlbums();

                // Populate ObservableCollection
                this.Albums = albumViewModels;
            }
            catch (Exception ex)
            {
                LogClient.Error("An error occurred while getting Albums. Exception: {0}", ex.Message);

                // Failed getting Albums. Create empty ObservableCollection.
                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.Albums = new ObservableCollection<AlbumViewModel>();
                });
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Populate CollectionViewSource
                this.AlbumsCvs = new CollectionViewSource { Source = this.Albums };
                this.AlbumsCvs.Filter += new FilterEventHandler(AlbumsCvs_Filter);

                // Update count
                this.AlbumsCount = this.AlbumsCvs.View.Cast<AlbumViewModel>().Count();
            });

            // Set Album artwork
            this.LoadAlbumArtworkAsync(Constants.ArtworkLoadDelay);
        }

        protected async Task AddAlbumsToPlaylistAsync(IList<AlbumViewModel> albumViewModels, string playlistName)
        {
            CreateNewPlaylistResult addPlaylistResult = CreateNewPlaylistResult.Success; // Default Success

            // If no playlist is provided, first create one.
            if (playlistName == null)
            {
                var responseText = ResourceUtils.GetString("Language_New_Playlist");

                if (this.dialogService.ShowInputDialog(
                    0xea37,
                    16,
                    ResourceUtils.GetString("Language_New_Playlist"),
                    ResourceUtils.GetString("Language_Enter_Name_For_Playlist"),
                    ResourceUtils.GetString("Language_Ok"),
                    ResourceUtils.GetString("Language_Cancel"),
                    ref responseText))
                {
                    playlistName = responseText;
                    addPlaylistResult = await this.playlistService.CreateNewPlaylistAsync(new EditablePlaylistViewModel(playlistName, PlaylistType.Static));
                }
            }

            // If playlist name is still null, the user clicked cancel on the previous dialog. Stop here.
            if (playlistName == null)
            {
                return;
            }

            // Verify if the playlist was added
            switch (addPlaylistResult)
            {
                case CreateNewPlaylistResult.Success:
                case CreateNewPlaylistResult.Duplicate:
                    // Add items to playlist
                    AddTracksToPlaylistResult result = await this.playlistService.AddAlbumsToStaticPlaylistAsync(albumViewModels, playlistName);

                    if (result == AddTracksToPlaylistResult.Error)
                    {
                        this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Adding_Songs_To_Playlist").Replace("{playlistname}", "\"" + playlistName + "\""), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
                    }
                    break;
                case CreateNewPlaylistResult.Error:
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetString("Language_Error"),
                        ResourceUtils.GetString("Language_Error_Adding_Playlist"),
                        ResourceUtils.GetString("Language_Ok"),
                        true,
                        ResourceUtils.GetString("Language_Log_File"));
                    break;
                case CreateNewPlaylistResult.Blank:
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

        protected async Task AddAlbumsToNowPlayingAsync(IList<AlbumViewModel> albumViewModel)
        {
            EnqueueResult result = await this.playbackService.AddAlbumsToQueueAsync(albumViewModel);

            if (!result.IsSuccess)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Adding_Albums_To_Now_Playing"), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
            }
        }

        protected async virtual Task SelectedAlbumsHandlerAsync(object parameter)
        {
            // This method needs to be awaitable for use in child classes

            if (parameter != null)
            {
                this.SelectedAlbums = new List<AlbumViewModel>();

                foreach (AlbumViewModel item in (IList)parameter)
                {
                    this.SelectedAlbums.Add(item);
                }
            }
        }

        protected override void SetEditCommands()
        {
            base.SetEditCommands();

            if (this.EditAlbumCommand != null)
            {
                this.EditAlbumCommand.RaiseCanExecuteChanged();
            }
        }

        protected override void FilterLists()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Albums
                if (this.AlbumsCvs != null)
                {
                    this.AlbumsCvs.View.Refresh();
                    this.AlbumsCount = this.AlbumsCvs.View.Cast<AlbumViewModel>().Count();
                }
            });

            base.FilterLists();
        }

        protected virtual async Task SetCoversizeAsync(CoverSizeType coverSize)
        {
            await Task.Run(() =>
            {
                this.selectedCoverSize = coverSize;

                switch (coverSize)
                {
                    case CoverSizeType.Small:
                        this.CoverSize = Constants.CoverSmallSize;
                        break;
                    case CoverSizeType.Medium:
                        this.CoverSize = Constants.CoverMediumSize;
                        break;
                    case CoverSizeType.Large:
                        this.CoverSize = Constants.CoverLargeSize;
                        break;
                    default:
                        this.CoverSize = Constants.CoverMediumSize;
                        this.selectedCoverSize = CoverSizeType.Medium;
                        break;
                }

                // this.AlbumWidth = this.CoverSize + Constants.AlbumTilePadding.Left + Constants.AlbumTilePadding.Right + Constants.AlbumTileMargin.Left + Constants.AlbumTileMargin.Right;
                this.AlbumWidth = this.CoverSize + Constants.AlbumTileMargin.Left + Constants.AlbumTileMargin.Right;
                this.AlbumHeight = this.AlbumWidth + Constants.AlbumTileAlbumInfoHeight + Constants.AlbumSelectionBorderSize;

                RaisePropertyChanged(nameof(this.CoverSize));
                RaisePropertyChanged(nameof(this.AlbumWidth));
                RaisePropertyChanged(nameof(this.AlbumHeight));
                RaisePropertyChanged(nameof(this.UpscaledCoverSize));
                RaisePropertyChanged(nameof(this.IsSmallCoverSizeSelected));
                RaisePropertyChanged(nameof(this.IsMediumCoverSizeSelected));
                RaisePropertyChanged(nameof(this.IsLargeCoverSizeSelected));
            });
        }

        protected virtual void ToggleAlbumOrder()
        {
            switch (this.AlbumOrder)
            {
                case AlbumOrder.Alphabetical:
                    this.AlbumOrder = AlbumOrder.ByDateAdded;
                    break;
                case AlbumOrder.ByDateAdded:
                    this.AlbumOrder = AlbumOrder.ByDateCreated;
                    break;
                case AlbumOrder.ByDateCreated:
                    this.AlbumOrder = AlbumOrder.ByAlbumArtist;
                    break;
                case AlbumOrder.ByAlbumArtist:
                    this.AlbumOrder = AlbumOrder.ByYearAscending;
                    break;
                case AlbumOrder.ByYearAscending:
                    this.AlbumOrder = AlbumOrder.ByYearDescending;
                    break;
                case AlbumOrder.ByYearDescending:
                    this.AlbumOrder = AlbumOrder.Alphabetical;
                    break;
                default:
                    // Cannot happen, but just in case.
                    this.AlbumOrder = AlbumOrder.Alphabetical;
                    break;
            }
        }
    }
}
