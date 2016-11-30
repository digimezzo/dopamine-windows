using Dopamine.Common.Presentation.Views;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Base;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Extensions;
using Dopamine.Core.Helpers;
using Dopamine.Core.Logging;
using Dopamine.Core.Utils;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Dopamine.Common.Presentation.ViewModels
{
    public abstract class CommonAlbumsViewModel : CommonTracksViewModel
    {
        #region Variables
        // Repositories
        private IAlbumRepository albumRepository;

        // Lists
        private ObservableCollection<AlbumViewModel> albums;
        private CollectionViewSource albumsCvs;
        private IList<Album> selectedAlbums;

        // Other
        private long albumsCount;
        private AlbumOrder albumOrder;
        private string albumOrderText;
        private double coverSize;
        private double albumWidth;
        private double albumHeight;
        private CoverSizeType selectedCoverSize;

        // Flags
        private bool delaySelectedAlbums;
        #endregion

        #region Commands
        public DelegateCommand ToggleAlbumOrderCommand { get; set; }
        public DelegateCommand<string> AddAlbumsToPlaylistCommand { get; set; }
        public DelegateCommand<object> SelectedAlbumsCommand { get; set; }
        public DelegateCommand EditAlbumCommand { get; set; }
        public DelegateCommand AddAlbumsToNowPlayingCommand { get; set; }
        public DelegateCommand<string> SetCoverSizeCommand { get; set; }
        public DelegateCommand DelaySelectedAlbumsCommand { get; set; }
        #endregion

        #region Properties
        public bool IsMultipleAlbumsSelected
        {
            get
            {
                if (this.selectedAlbums != null && this.selectedAlbums.Count > 1) return true;
                return false;
            }
        }
        public bool OrderedByYear
        {
            get { return this.AlbumOrder == AlbumOrder.ByYear; }
        }

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

        public double UpscaledCoverSize
        {
            get { return this.CoverSize * Constants.CoverUpscaleFactor; }
        }

        public bool IsSmallCoverSizeSelected
        {
            get { return this.selectedCoverSize == CoverSizeType.Small; }
        }

        public bool IsMediumCoverSizeSelected
        {
            get { return this.selectedCoverSize == CoverSizeType.Medium; }
        }

        public bool IsLargeCoverSizeSelected
        {
            get { return this.selectedCoverSize == CoverSizeType.Large; }
        }

        public double UpscaledToolTipCoverSize
        {
            get { return Constants.AlbumToolTipCoverSize * Constants.CoverUpscaleFactor; }
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

        public IList<Album> SelectedAlbums
        {
            get { return this.selectedAlbums; }
            set
            {
                SetProperty<IList<Album>>(ref this.selectedAlbums, value);
                OnPropertyChanged(() => this.CanOrderByAlbum);
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

        public string AlbumOrderText
        {
            get { return this.albumOrderText; }
        }
        #endregion

        #region Construction
        public CommonAlbumsViewModel(IUnityContainer container) : base(container)
        {
            this.albumRepository = ServiceLocator.Current.GetInstance<IAlbumRepository>();
            this.Initialize();
        }
        #endregion

        #region Private
        private void Initialize()
        {
            // Initialize Commands
            this.AddAlbumsToPlaylistCommand = new DelegateCommand<string>(async (playlistName) => await this.AddAlbumsToPlaylistAsync(this.SelectedAlbums, playlistName));
            this.SelectedAlbumsCommand = new DelegateCommand<object>(async (parameter) =>
            {
                if (this.delaySelectedAlbums) await Task.Delay(Constants.DelaySelectedAlbumsDelay);
                this.delaySelectedAlbums = false;
                await this.SelectedAlbumsHandlerAsync(parameter);
            });

            this.EditAlbumCommand = new DelegateCommand(() => this.EditSelectedAlbum(), () => !this.IsIndexing);
            this.AddAlbumsToNowPlayingCommand = new DelegateCommand(async () => await this.AddAlbumsToNowPlayingAsync(this.SelectedAlbums));
            this.SetCoverSizeCommand = new DelegateCommand<string>(async (coverSize) =>
            {
                int selectedCoverSize = 0;

                if (int.TryParse(coverSize, out selectedCoverSize))
                {
                    await this.SetCoversizeAsync((CoverSizeType)selectedCoverSize);
                }
            });

            this.DelaySelectedAlbumsCommand = new DelegateCommand(() => this.delaySelectedAlbums = true);
        }

        private void EditSelectedAlbum()
        {
            if (this.SelectedAlbums == null || this.SelectedAlbums.Count == 0) return;

            EditAlbum view = this.container.Resolve<EditAlbum>();
            view.DataContext = this.container.Resolve<EditAlbumViewModel>(new DependencyOverride(typeof(Album), this.SelectedAlbums.First()));

            this.dialogService.ShowCustomDialog(
                0xe104,
                14,
                ResourceUtils.GetStringResource("Language_Edit_Album"),
                view,
                405,
                450,
                false,
                true,
                true,
                true,
                ResourceUtils.GetStringResource("Language_Ok"),
                ResourceUtils.GetStringResource("Language_Cancel"),
                ((EditAlbumViewModel)view.DataContext).SaveAlbumAsync);
        }

        private void AlbumsCvs_Filter(object sender, FilterEventArgs e)
        {
            AlbumViewModel avm = e.Item as AlbumViewModel;
            e.Accepted = Dopamine.Core.Database.Utils.FilterAlbums(avm.Album, this.searchService.SearchText);
        }
        #endregion

        #region Protected
        protected void UpdateAlbumOrderText(AlbumOrder albumOrder)
        {
            switch (albumOrder)
            {
                case AlbumOrder.Alphabetical:
                    this.albumOrderText = ResourceUtils.GetStringResource("Language_A_Z");
                    break;
                case AlbumOrder.ByDateAdded:
                    this.albumOrderText = ResourceUtils.GetStringResource("Language_By_Date_Added");
                    break;
                case AlbumOrder.ByAlbumArtist:
                    this.albumOrderText = ResourceUtils.GetStringResource("Language_By_Album_Artist");
                    break;
                case AlbumOrder.ByYear:
                    this.albumOrderText = ResourceUtils.GetStringResource("Language_By_Year");
                    break;
                default:
                    // Cannot happen, but just in case.
                    this.albumOrderText = ResourceUtils.GetStringResource("Language_A_Z");
                    break;
            }

            OnPropertyChanged(() => this.AlbumOrderText);
        }

        protected override void SetEditCommands()
        {
            base.SetEditCommands();

            if (this.EditAlbumCommand != null)
            {
                this.EditAlbumCommand.RaiseCanExecuteChanged();
            }
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

                this.AlbumHeight = this.CoverSize + Constants.AlbumTileAlbumInfoHeight + 4; // 4 = margin of 2px at top and bottom
                this.AlbumWidth = this.CoverSize + 4; // 4 = margin of 2px at left and right

                OnPropertyChanged(() => this.CoverSize);
                OnPropertyChanged(() => this.AlbumWidth);
                OnPropertyChanged(() => this.AlbumHeight);
                OnPropertyChanged(() => this.UpscaledCoverSize);
                OnPropertyChanged(() => this.IsSmallCoverSizeSelected);
                OnPropertyChanged(() => this.IsMediumCoverSizeSelected);
                OnPropertyChanged(() => this.IsLargeCoverSizeSelected);
            });
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

        protected async Task GetAlbumsAsync(IList<Artist> selectedArtists, IList<Genre> selectedGenres, AlbumOrder albumOrder)
        {
            if (selectedArtists.IsNullOrEmpty() & selectedGenres.IsNullOrEmpty())
            {
                await this.GetAlbumsCommonAsync(await this.albumRepository.GetAlbumsAsync(), albumOrder);
            }
            else
            {
                if (!selectedArtists.IsNullOrEmpty())
                {
                    await this.GetAlbumsCommonAsync(await this.albumRepository.GetAlbumsAsync(selectedArtists), albumOrder);
                    return;
                }

                if (!selectedGenres.IsNullOrEmpty())
                {
                    await this.GetAlbumsCommonAsync(await this.albumRepository.GetAlbumsAsync(selectedGenres), albumOrder);
                    return;
                }
            }
        }

        protected async Task GetAlbumsCommonAsync(IList<Album> albums, AlbumOrder albumOrder)
        {
            try
            {
                // Create new ObservableCollection
                var albumViewModels = new ObservableCollection<AlbumViewModel>();

                // Order the incoming Albums
                List<Album> orderedAlbums = await Core.Database.Utils.OrderAlbumsAsync(albums, albumOrder);

                await Task.Run(() =>
                {
                    foreach (Album alb in orderedAlbums)
                    {
                        string mainHeader = alb.AlbumTitle;
                        string subHeader = alb.AlbumArtist;

                        switch (albumOrder)
                        {
                            case AlbumOrder.Alphabetical:
                                break;
                            // Do nothing
                            case AlbumOrder.ByDateAdded:
                                break;
                            // Do nothing
                            case AlbumOrder.ByAlbumArtist:
                                mainHeader = alb.AlbumArtist;
                                subHeader = alb.AlbumTitle;
                                break;
                            case AlbumOrder.ByYear:
                                mainHeader = alb.Year.HasValue && alb.Year.Value > 0 ? alb.Year.ToString() : string.Empty;
                                subHeader = alb.AlbumTitle;
                                break;
                            default:
                                break;
                                // Do nothing
                        }

                        albumViewModels.Add(new AlbumViewModel
                        {
                            Album = alb,
                            MainHeader = mainHeader,
                            SubHeader = subHeader
                        });
                    }
                });

                // Unbind to improve UI performance
                if (this.AlbumsCvs != null) this.AlbumsCvs.Filter -= new FilterEventHandler(AlbumsCvs_Filter);
                this.Albums = null;
                this.AlbumsCvs = null;

                // Populate ObservableCollection
                this.Albums = albumViewModels;
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("An error occurred while getting Albums. Exception: {0}", ex.Message);

                // Failed getting Albums. Create empty ObservableCollection.
                this.Albums = new ObservableCollection<AlbumViewModel>();
            }

            // Populate CollectionViewSource
            this.AlbumsCvs = new CollectionViewSource { Source = this.Albums };
            this.AlbumsCvs.Filter += new FilterEventHandler(AlbumsCvs_Filter);

            // Update count
            this.AlbumsCount = this.AlbumsCvs.View.Cast<AlbumViewModel>().Count();

            // Set Album artwork
            this.collectionService.SetAlbumArtworkAsync(this.Albums, Constants.ArtworkLoadDelay);
        }

        protected void ToggleAlbumOrder()
        {
            switch (this.AlbumOrder)
            {
                case AlbumOrder.Alphabetical:
                    this.AlbumOrder = AlbumOrder.ByDateAdded;
                    break;
                case AlbumOrder.ByDateAdded:
                    this.AlbumOrder = AlbumOrder.ByAlbumArtist;
                    break;
                case AlbumOrder.ByAlbumArtist:
                    this.AlbumOrder = AlbumOrder.ByYear;
                    break;
                case AlbumOrder.ByYear:
                    this.AlbumOrder = AlbumOrder.Alphabetical;
                    break;
                default:
                    // Cannot happen, but just in case.
                    this.AlbumOrder = AlbumOrder.Alphabetical;
                    break;
            }

            OnPropertyChanged(() => this.OrderedByYear);
        }

        protected async Task AddAlbumsToPlaylistAsync(IList<Album> albums, string playlistName)
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
                    AddToPlaylistResult result = await this.collectionService.AddAlbumsToPlaylistAsync(albums, playlistName);

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

        protected async Task AddAlbumsToNowPlayingAsync(IList<Album> albums)
        {
            AddToQueueResult result = await this.playbackService.AddToQueue(albums);

            if (!result.IsSuccess)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetStringResource("Language_Error"), ResourceUtils.GetStringResource("Language_Error_Adding_Albums_To_Now_Playing"), ResourceUtils.GetStringResource("Language_Ok"), true, ResourceUtils.GetStringResource("Language_Log_File"));
            }
        }

        protected async virtual Task SelectedAlbumsHandlerAsync(object parameter)
        {
            if (parameter != null)
            {
                this.SelectedAlbums = new List<Album>();

                foreach (AlbumViewModel item in (IList)parameter)
                {
                    this.SelectedAlbums.Add(item.Album);
                }

                OnPropertyChanged(() => this.IsMultipleAlbumsSelected);
            }
        }
        #endregion
    }
}
