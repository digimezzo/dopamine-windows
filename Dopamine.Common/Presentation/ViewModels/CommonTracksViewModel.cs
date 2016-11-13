using Dopamine.Common.Presentation.Views;
using Dopamine.Common.Services.Collection;
using Dopamine.Common.Services.Dialog;
using Dopamine.Common.Services.I18n;
using Dopamine.Common.Services.Indexing;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Services.Search;
using Dopamine.Core.Base;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Extensions;
using Dopamine.Core.Helpers;
using Dopamine.Core.Logging;
using Dopamine.Core.Prism;
using Dopamine.Core.Settings;
using Dopamine.Core.Utils;
using Prism;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Dopamine.Common.Services.Provider;

namespace Dopamine.Common.Presentation.ViewModels
{
    public abstract class CommonTracksViewModel : BindableBase, INavigationAware, IActiveAware
    {
        #region Variables
        // Unity Container
        protected IUnityContainer container;

        // Services
        protected IIndexingService indexingService;
        protected IPlaybackService playbackService;
        protected ISearchService searchService;
        protected IDialogService dialogService;
        protected ICollectionService collectionService;
        protected IMetadataService metadataService;
        protected II18nService i18nService;
        protected IProviderService providerService;

        // Event aggregator
        protected IEventAggregator eventAggregator;

        // Repositories
        protected ITrackRepository trackRepository;

        // Lists
        private ObservableCollection<PlaylistViewModel> contextMenuPlaylists;
        private ObservableCollection<SearchProvider> contextMenuSearchProviders;
        private ObservableCollection<MergedTrackViewModel> tracks;
        private CollectionViewSource tracksCvs;
        private IList<MergedTrack> selectedTracks;

        // Flags
        protected bool isFirstLoad = true;
        private bool isIndexing;
        private bool enableRating;
        private bool enableLove;

        // IActiveAware
        private bool isActive;
        public event EventHandler IsActiveChanged;

        // Other
        private long tracksCount;
        private TrackOrder trackOrder;
        private string trackOrderText;
        protected long totalDuration;
        protected long totalSize;
        private string searchTextBeforeInactivate = string.Empty;
        #endregion

        #region Commands
        public DelegateCommand ToggleTrackOrderCommand { get; set; }
        public DelegateCommand RemoveSelectedTracksCommand { get; set; }
        public DelegateCommand<string> AddTracksToPlaylistCommand { get; set; }
        public DelegateCommand LoadedCommand { get; set; }
        public DelegateCommand ShowSelectedTrackInformationCommand { get; set; }
        public DelegateCommand<object> SelectedTracksCommand { get; set; }
        public DelegateCommand EditTracksCommand { get; set; }
        public DelegateCommand AddTracksToNowPlayingCommand { get; set; }
        public DelegateCommand<string> SearchOnlineCommand { get; set; }
        public DelegateCommand ShuffleAllCommand { get; set; }
        #endregion

        #region ReadOnly Properties
        public abstract bool CanOrderByAlbum { get; }

        public double UpscaledTrackCoverSize
        {
            get { return Constants.TrackCoverSize * Constants.CoverUpscaleFactor; }
        }

        public bool HasContextMenuPlaylists
        {
            get { return this.ContextMenuPlaylists != null && this.ContextMenuPlaylists.Count > 0; }
        }

        public bool HasContextMenuSearchProviders
        {
            get { return this.ContextMenuSearchProviders != null && this.ContextMenuSearchProviders.Count > 0; }
        }

        public string TrackOrderText
        {
            get { return this.trackOrderText; }
        }

        public string TotalSizeInformation
        {
            get { return this.totalSize > 0 ? FormatUtils.FormatFileSize(this.totalSize, false) : string.Empty; }
        }

        public string TotalDurationInformation
        {
            get { return this.totalDuration > 0 ? FormatUtils.FormatDuration(this.totalDuration) : string.Empty; }
        }
        #endregion

        #region Properties
        public bool EnableRating
        {
            get { return this.enableRating; }
            set { SetProperty<bool>(ref this.enableRating, value); }
        }

        public bool EnableLove
        {
            get { return this.enableLove; }
            set { SetProperty<bool>(ref this.enableLove, value); }
        }

        public ObservableCollection<PlaylistViewModel> ContextMenuPlaylists
        {
            get { return this.contextMenuPlaylists; }
            set
            {
                SetProperty<ObservableCollection<PlaylistViewModel>>(ref this.contextMenuPlaylists, value);
                OnPropertyChanged(() => this.HasContextMenuPlaylists);
            }
        }

        public ObservableCollection<SearchProvider> ContextMenuSearchProviders
        {
            get { return this.contextMenuSearchProviders; }
            set
            {
                SetProperty<ObservableCollection<SearchProvider>>(ref this.contextMenuSearchProviders, value);
                OnPropertyChanged(() => this.HasContextMenuSearchProviders);
            }
        }

        public ObservableCollection<MergedTrackViewModel> Tracks
        {
            get { return this.tracks; }
            set { SetProperty<ObservableCollection<MergedTrackViewModel>>(ref this.tracks, value); }
        }

        public CollectionViewSource TracksCvs
        {
            get { return this.tracksCvs; }
            set { SetProperty<CollectionViewSource>(ref this.tracksCvs, value); }
        }

        public IList<MergedTrack> SelectedTracks
        {
            get { return this.selectedTracks; }
            set { SetProperty<IList<MergedTrack>>(ref this.selectedTracks, value); }
        }

        public long TracksCount
        {
            get { return this.tracksCount; }
            set { SetProperty<long>(ref this.tracksCount, value); }
        }

        public bool IsIndexing
        {
            get { return this.isIndexing; }
            set { SetProperty<bool>(ref this.isIndexing, value); }
        }

        public TrackOrder TrackOrder
        {
            get { return this.trackOrder; }
            set
            {
                SetProperty<TrackOrder>(ref this.trackOrder, value);
                this.UpdateTrackOrderText(value);
            }
        }

        public bool IsActive
        {
            get { return this.isActive; }
            set { SetProperty<bool>(ref this.isActive, value); }
        }
        #endregion

        #region Construction
        /// <summary>
        /// Constructor which allows Dependency Injection. This can be useful for Unit Testing.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="indexingService"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="playbackService"></param>
        /// <param name="searchService"></param>
        /// <param name="dialogService"></param>
        /// <param name="collectionService"></param>
        /// <param name="trackRepository"></param>
        /// <param name="i18nService"></param>
        /// <remarks></remarks>
        public CommonTracksViewModel(IUnityContainer container, IIndexingService indexingService, IEventAggregator eventAggregator, IPlaybackService playbackService, ISearchService searchService, IDialogService dialogService, ICollectionService collectionService, ITrackRepository trackRepository, IMetadataService metadataService, II18nService i18nService, IProviderService providerService)
        {
            // Unity Container
            this.container = container;

            // EventAggregator
            this.eventAggregator = eventAggregator;

            // Services
            this.indexingService = indexingService;
            this.playbackService = playbackService;
            this.searchService = searchService;
            this.dialogService = dialogService;
            this.collectionService = collectionService;
            this.metadataService = metadataService;
            this.i18nService = i18nService;
            this.providerService = providerService;

            // Repositories
            this.trackRepository = trackRepository;

            // Initialize
            this.Initialize();
        }

        /// <summary>
        /// Parameterless Constructor used by child classes.
        /// </summary>
        /// <remarks></remarks>
        public CommonTracksViewModel()
        {
            // Unity Container
            this.container = ServiceLocator.Current.GetInstance<IUnityContainer>();

            // EventAggregator
            this.eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();

            // Services
            this.indexingService = ServiceLocator.Current.GetInstance<IIndexingService>();
            this.playbackService = ServiceLocator.Current.GetInstance<IPlaybackService>();
            this.searchService = ServiceLocator.Current.GetInstance<ISearchService>();
            this.dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
            this.collectionService = ServiceLocator.Current.GetInstance<ICollectionService>();
            this.metadataService = ServiceLocator.Current.GetInstance<IMetadataService>();
            this.i18nService = ServiceLocator.Current.GetInstance<II18nService>();
            this.providerService = ServiceLocator.Current.GetInstance<IProviderService>();

            // Repositories
            this.trackRepository = ServiceLocator.Current.GetInstance<ITrackRepository>();

            // Initialize
            this.Initialize();
        }
        #endregion

        #region Private
        private void Initialize()
        {
            // Initialize commands
            this.AddTracksToPlaylistCommand = new DelegateCommand<string>(async (playlistName) => await this.AddTracksToPlaylistAsync(this.SelectedTracks, playlistName));
            this.LoadedCommand = new DelegateCommand(async () => await this.LoadedCommandAsync());
            this.ShowSelectedTrackInformationCommand = new DelegateCommand(() => this.ShowSelectedTrackInformation());
            this.SelectedTracksCommand = new DelegateCommand<object>((parameter) => this.SelectedTracksHandler(parameter));
            this.EditTracksCommand = new DelegateCommand(() => this.EditSelectedTracks(), () => !this.IsIndexing);
            this.AddTracksToNowPlayingCommand = new DelegateCommand(async () => await this.AddTracksToNowPlayingAsync(this.SelectedTracks));

            this.SearchOnlineCommand = new DelegateCommand<string>((id) =>
            {
                if (this.selectedTracks != null && this.selectedTracks.Count > 0)
                {
                    this.providerService.SearchOnline(id, new string[] { this.SelectedTracks.First().ArtistName, this.SelectedTracks.First().TrackTitle });
                }
            });

            this.ShuffleAllCommand = new DelegateCommand(() =>
            {
                this.playbackService.SetShuffle(true);
                this.playbackService.Enqueue();
            });

            // Initialize Handlers
            this.playbackService.PlaybackFailed += (_, __) => this.ShowPlayingTrackAsync();
            this.playbackService.PlaybackPaused += (_, __) => this.ShowPlayingTrackAsync();
            this.playbackService.PlaybackResumed += (_, __) => this.ShowPlayingTrackAsync();
            this.playbackService.PlaybackStopped += (_, __) => this.ShowPlayingTrackAsync();
            this.playbackService.PlaybackSuccess += (_) => this.ShowPlayingTrackAsync();

            this.collectionService.CollectionChanged += async (_, __) => await this.FillListsAsync(); // Refreshes the lists when the Collection has changed
            this.collectionService.PlaylistsChanged += (_, __) => this.GetContextMenuPlaylistsAsync();

            this.indexingService.RefreshLists += async (_, __) => await this.FillListsAsync(); // Refreshes the lists when the indexer has finished indexing
            this.indexingService.IndexingStarted += (_, __) => this.SetEditCommands();
            this.indexingService.IndexingStopped += (_, __) => this.SetEditCommands();

            this.searchService.DoSearch += (searchText) => { if (this.IsActive) this.FilterLists(); };

            this.metadataService.RatingChanged += MetadataService_RatingChangedAsync;
            this.metadataService.LoveChanged += MetadataService_LoveChangedAsync;

            this.providerService.SearchProvidersChanged += (_, __) => { this.GetSearchProvidersAsync(); };

            this.i18nService.LanguageChanged += (_, __) =>
            {
                OnPropertyChanged(() => this.TotalDurationInformation);
                OnPropertyChanged(() => this.TotalSizeInformation);
                this.RefreshLanguage();
            };

            // Initialize flags
            this.EnableRating = XmlSettingsClient.Instance.Get<bool>("Behaviour", "EnableRating");
            this.EnableLove = XmlSettingsClient.Instance.Get<bool>("Behaviour", "EnableLove");

            // This makes sure the IsIndexing is correct even when this ViewModel is 
            // created after the Indexer is started, and thus after triggering the 
            // IndexingService.IndexerStarted event.
            this.SetEditCommands();

            // Initialize the playlists in the ContextMenu
            this.GetContextMenuPlaylistsAsync();

            // Initialize the search providers in the ContextMenu
            this.GetSearchProvidersAsync();
        }

        private void ShowSelectedTrackInformation()
        {
            // Don't try to show the file information when nothing is selected
            if (this.SelectedTracks == null || this.SelectedTracks.Count == 0) return;

            if (this.CheckAllSelectedTracksExist())
            {
                Views.FileInformation view = this.container.Resolve<Views.FileInformation>();
                view.DataContext = this.container.Resolve<FileInformationViewModel>(new DependencyOverride(typeof(MergedTrack), this.SelectedTracks.First()));

                this.dialogService.ShowCustomDialog(
                    0xe8d6,
                    16,
                    ResourceUtils.GetStringResource("Language_Information"),
                    view,
                    400,
                    620,
                    true,
                    true,
                    true,
                    false,
                    ResourceUtils.GetStringResource("Language_Ok"),
                    string.Empty,
                    null);
            }
        }

        private async void GetContextMenuPlaylistsAsync()
        {
            try
            {
                // Unbind to improve UI performance
                this.ContextMenuPlaylists = null;

                List<Playlist> playlists = await this.collectionService.GetPlaylistsAsync();

                // Populate an ObservableCollection
                var playlistViewModels = new ObservableCollection<PlaylistViewModel>();

                await Task.Run(() =>
                {
                    foreach (Playlist pl in playlists)
                    {
                        playlistViewModels.Add(new PlaylistViewModel
                        {
                            Playlist = pl
                        });
                    }

                });

                // Re-bind to update the UI
                this.ContextMenuPlaylists = playlistViewModels;

            }
            catch (Exception)
            {
                // If loading from the database failed, create and empty Collection.
                this.ContextMenuPlaylists = new ObservableCollection<PlaylistViewModel>();
            }
        }

        private async void GetSearchProvidersAsync()
        {
            this.ContextMenuSearchProviders = null;

            List<SearchProvider> providersList = await this.providerService.GetSearchProvidersAsync();
            var localProviders = new ObservableCollection<SearchProvider>();

            await Task.Run(() =>
            {
                foreach (SearchProvider vp in providersList)
                {
                    localProviders.Add(vp);
                }
            });

            this.ContextMenuSearchProviders = localProviders;
        }

        private bool CheckAllSelectedTracksExist()
        {
            bool allSelectedTracksExist = true;

            foreach (MergedTrack trk in this.SelectedTracks)
            {
                if (!System.IO.File.Exists(trk.Path))
                {
                    allSelectedTracksExist = false;
                    break;
                }
            }

            if (!allSelectedTracksExist)
            {
                string message = ResourceUtils.GetStringResource("Language_Song_Cannot_Be_Found_Refresh_Collection");
                if (this.SelectedTracks.Count > 1) message = ResourceUtils.GetStringResource("Language_Songs_Cannot_Be_Found_Refresh_Collection");

                if (this.dialogService.ShowConfirmation(0xe11b, 16, ResourceUtils.GetStringResource("Language_Refresh"), message, ResourceUtils.GetStringResource("Language_Yes"), ResourceUtils.GetStringResource("Language_No")))
                {
                    this.indexingService.NeedsIndexing = true;
                    this.indexingService.IndexCollectionAsync(XmlSettingsClient.Instance.Get<bool>("Indexing", "IgnoreRemovedFiles"), false);
                }
            }

            return allSelectedTracksExist;
        }
        #endregion

        #region Protected
        protected abstract void RefreshLanguage();

        protected void UpdateTrackOrderText(TrackOrder trackOrder)
        {
            switch (trackOrder)
            {
                case TrackOrder.Alphabetical:
                    this.trackOrderText = ResourceUtils.GetStringResource("Language_A_Z");
                    break;
                case TrackOrder.ReverseAlphabetical:
                    this.trackOrderText = ResourceUtils.GetStringResource("Language_Z_A");
                    break;
                case TrackOrder.ByAlbum:
                    this.trackOrderText = ResourceUtils.GetStringResource("Language_By_Album");
                    break;
                case TrackOrder.ByRating:
                    this.trackOrderText = ResourceUtils.GetStringResource("Language_By_Rating");
                    break;
                default:
                    // Cannot happen, but just in case.
                    this.trackOrderText = ResourceUtils.GetStringResource("Language_By_Album");
                    break;
            }

            OnPropertyChanged(() => this.TrackOrderText);
        }

        protected void SetTrackOrder(string settingName)
        {
            TrackOrder savedTrackOrder = (TrackOrder)XmlSettingsClient.Instance.Get<int>("Ordering", settingName);

            if ((!this.EnableRating & savedTrackOrder == TrackOrder.ByRating) | (!this.CanOrderByAlbum & savedTrackOrder == TrackOrder.ByAlbum))
            {
                this.TrackOrder = TrackOrder.Alphabetical;
            }
            else
            {
                // Only change the TrackOrder if it is not correct
                if (this.TrackOrder != savedTrackOrder) this.TrackOrder = savedTrackOrder;
            }
        }

        protected virtual void SetEditCommands()
        {
            this.IsIndexing = this.indexingService.IsIndexing;

            if (this.EditTracksCommand != null) this.EditTracksCommand.RaiseCanExecuteChanged();
            if (this.RemoveSelectedTracksCommand != null) this.RemoveSelectedTracksCommand.RaiseCanExecuteChanged();
        }

        protected async virtual Task LoadedCommandAsync()
        {
            if (this.isFirstLoad)
            {
                this.isFirstLoad = false;

                await Task.Delay(Constants.CommonListLoadDelay);  // Wait for the UI to slide in
                await this.FillListsAsync(); // Fill all the lists
            }
        }

        protected void TracksCvs_Filter(object sender, FilterEventArgs e)
        {
            MergedTrackViewModel vm = e.Item as MergedTrackViewModel;
            e.Accepted = Dopamine.Core.Database.Utils.FilterTracks(vm.Track, this.searchService.SearchText);
        }

        /// <summary>
        /// Abstract because each child ViewModel has its own set of Lists
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        protected abstract Task FillListsAsync();

        /// <summary>
        /// Virtual because each child ViewModel has its own set of Lists
        /// </summary>
        /// <remarks></remarks>
        protected virtual void FilterLists()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Tracks
                if (this.TracksCvs != null)
                {
                    this.TracksCvs.View.Refresh();
                    this.TracksCount = this.TracksCvs.View.Cast<MergedTrackViewModel>().Count();
                }

                this.ShowPlayingTrackAsync();
                this.SetSizeInformationAsync();
            });
        }

        /// <summary>
        /// Override this Method to add Subscriptions to Events and Commands
        /// </summary>
        /// <remarks></remarks>
        protected abstract void Subscribe();

        /// <summary>
        /// Override this Method to remove Subscriptions to Events and Commands
        /// </summary>
        /// <remarks></remarks>
        protected abstract void Unsubscribe();

        protected async Task GetTracksAsync(IList<Artist> selectedArtists, IList<Genre> selectedGenres, IList<Album> selectedAlbums, TrackOrder trackOrder)
        {

            if (selectedArtists.IsNullOrEmpty() & selectedGenres.IsNullOrEmpty() & selectedAlbums.IsNullOrEmpty())
            {
                await this.GetTracksCommonAsync(await this.trackRepository.GetTracksAsync(), trackOrder);
            }
            else
            {
                if (!selectedAlbums.IsNullOrEmpty())
                {
                    await this.GetTracksCommonAsync(await this.trackRepository.GetTracksAsync(selectedAlbums), trackOrder);
                    return;
                }

                if (!selectedArtists.IsNullOrEmpty())
                {
                    await this.GetTracksCommonAsync(await this.trackRepository.GetTracksAsync(selectedArtists), trackOrder);
                    return;
                }

                if (!selectedGenres.IsNullOrEmpty())
                {
                    await this.GetTracksCommonAsync(await this.trackRepository.GetTracksAsync(selectedGenres), trackOrder);
                    return;
                }
            }
        }

        protected async Task GetTracksCommonAsync(IList<MergedTrack> tracks, TrackOrder trackOrder)
        {
            try
            {
                // Do we need to show the TrackNumber?
                bool showTracknumber = this.TrackOrder == TrackOrder.ByAlbum;

                // Create new ObservableCollection
                ObservableCollection<MergedTrackViewModel> viewModels = new ObservableCollection<MergedTrackViewModel>();

                // Order the incoming Tracks
                List<MergedTrack> orderedTracks = await Core.Database.Utils.OrderTracksAsync(tracks, trackOrder);

                await Task.Run(() =>
                {
                    foreach (MergedTrack t in orderedTracks)
                    {
                        MergedTrackViewModel vm = this.container.Resolve<MergedTrackViewModel>();
                        vm.Track = t;
                        vm.ShowTrackNumber = showTracknumber;
                        viewModels.Add(vm);
                    }
                });

                // Unbind to improve UI performance
                if (this.TracksCvs != null)
                    this.TracksCvs.Filter -= new FilterEventHandler(TracksCvs_Filter);
                this.Tracks = null;
                this.TracksCvs = null;

                // Populate ObservableCollection
                this.Tracks = viewModels;
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("An error occurred while getting Tracks. Exception: {0}", ex.Message);

                // Failed getting Tracks. Create empty ObservableCollection.
                this.Tracks = new ObservableCollection<MergedTrackViewModel>();
            }

            // Populate CollectionViewSource
            this.TracksCvs = new CollectionViewSource { Source = this.Tracks };
            this.TracksCvs.Filter += new FilterEventHandler(TracksCvs_Filter);

            // Update count
            this.TracksCount = this.TracksCvs.View.Cast<MergedTrackViewModel>().Count();

            // Group by Album if needed
            if (this.TrackOrder == TrackOrder.ByAlbum)
                this.TracksCvs.GroupDescriptions.Add(new PropertyGroupDescription("GroupHeader"));

            // Update duration and size
            this.SetSizeInformationAsync();

            // Show playing Track
            this.ShowPlayingTrackAsync();

            // Set Track artwork
            this.collectionService.SetTrackArtworkAsync(this.Tracks, Constants.ArtworkLoadDelay);
        }

        private async Task SetSizeInformationAsync()
        {
            // Reset duration and size
            this.totalDuration = 0;
            this.totalSize = 0;

            if (this.TracksCvs != null)
            {
                // Create copy of CollectionViewSource because only STA can access it
                var viewCopy = new CollectionView(this.TracksCvs.View);

                await Task.Run(() =>
                {
                    try
                    {
                        foreach (MergedTrackViewModel vm in viewCopy)
                        {
                            this.totalDuration += vm.Track.Duration.Value;
                            this.totalSize += vm.Track.FileSize.Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogClient.Instance.Logger.Error("An error occured while setting size information. Exception: {0}", ex.Message);
                    }

                });
            }

            OnPropertyChanged(() => this.TotalDurationInformation);
            OnPropertyChanged(() => this.TotalSizeInformation);
        }

        protected void ToggleTrackOrder()
        {
            switch (this.TrackOrder)
            {
                case TrackOrder.Alphabetical:
                    this.TrackOrder = TrackOrder.ReverseAlphabetical;
                    break;
                case TrackOrder.ReverseAlphabetical:

                    if (this.CanOrderByAlbum)
                    {
                        this.TrackOrder = TrackOrder.ByAlbum;
                    }
                    else
                    {
                        this.TrackOrder = TrackOrder.ByRating;
                    }
                    break;
                case TrackOrder.ByAlbum:
                    if (XmlSettingsClient.Instance.Get<bool>("Behaviour", "EnableRating"))
                    {
                        this.TrackOrder = TrackOrder.ByRating;
                    }
                    else
                    {
                        this.TrackOrder = TrackOrder.Alphabetical;
                    }
                    break;
                case TrackOrder.ByRating:
                    this.TrackOrder = TrackOrder.Alphabetical;
                    break;
                default:
                    // Cannot happen, but just in case.
                    this.TrackOrder = TrackOrder.ByAlbum;
                    break;
            }
        }

        protected async virtual void ShowPlayingTrackAsync()
        {
            if (this.playbackService.PlayingTrack == null)
                return;

            string path = this.playbackService.PlayingTrack.Path;

            await Task.Run(() =>
            {
                if (this.Tracks != null)
                {
                    foreach (MergedTrackViewModel vm in this.Tracks)
                    {
                        vm.IsPlaying = false;
                        vm.IsPaused = true;

                        if (vm.Track.Path == path)
                        {
                            if (!this.playbackService.IsStopped)
                            {
                                vm.IsPlaying = true;

                                if (this.playbackService.IsPlaying)
                                {
                                    vm.IsPaused = false;
                                }
                            }
                        }
                    }
                }
            });

            this.ConditionalScrollToPlayingTrack();
        }

        protected async Task RemoveTracksFromCollectionAsync(IList<MergedTrack> selectedTracks)
        {
            string title = ResourceUtils.GetStringResource("Language_Remove");
            string body = ResourceUtils.GetStringResource("Language_Are_You_Sure_To_Remove_Song");

            if (selectedTracks.Count > 1)
            {
                body = ResourceUtils.GetStringResource("Language_Are_You_Sure_To_Remove_Songs");
            }

            if (this.dialogService.ShowConfirmation(0xe11b, 16, title, body, ResourceUtils.GetStringResource("Language_Yes"), ResourceUtils.GetStringResource("Language_No")))
            {
                RemoveTracksResult result = await this.collectionService.RemoveTracksFromCollectionAsync(selectedTracks);

                if (result == RemoveTracksResult.Error)
                {
                    this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetStringResource("Language_Error"), ResourceUtils.GetStringResource("Language_Error_Removing_Songs"), ResourceUtils.GetStringResource("Language_Ok"), true, ResourceUtils.GetStringResource("Language_Log_File"));
                }
                else
                {
                    await this.playbackService.Dequeue(selectedTracks);
                }
            }
        }

        protected async Task AddTracksToPlaylistAsync(IList<MergedTrack> tracks, string playlistName)
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
                    AddToPlaylistResult result = await this.collectionService.AddTracksToPlaylistAsync(tracks, playlistName);

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

        protected async Task AddTracksToNowPlayingAsync(IList<MergedTrack> tracks)
        {
            AddToQueueResult result = await this.playbackService.AddToQueue(tracks);

            if (!result.IsSuccess)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetStringResource("Language_Error"), ResourceUtils.GetStringResource("Language_Error_Adding_Songs_To_Now_Playing"), ResourceUtils.GetStringResource("Language_Ok"), true, ResourceUtils.GetStringResource("Language_Log_File"));
            }
        }

        private async void MetadataService_RatingChangedAsync(RatingChangedEventArgs e)
        {
            if (this.Tracks == null) return;

            await Task.Run(() =>
            {
                foreach (MergedTrackViewModel vm in this.Tracks)
                {
                    if (vm.Track.Path.Equals(e.Path))
                    {
                        // The UI is only updated if PropertyChanged is fired on the UI thread
                        Application.Current.Dispatcher.Invoke(() => vm.UpdateVisibleRating(e.Rating));
                    }
                }
            });
        }

        private async void MetadataService_LoveChangedAsync(LoveChangedEventArgs e)
        {
            if (this.Tracks == null) return;

            await Task.Run(() =>
            {
                foreach (MergedTrackViewModel vm in this.Tracks)
                {
                    if (vm.Track.Path.Equals(e.Path))
                    {
                        // The UI is only updated if PropertyChanged is fired on the UI thread
                        Application.Current.Dispatcher.Invoke(() => vm.UpdateVisibleLove(e.Love));
                    }
                }
            });
        }

        private void SelectedTracksHandler(object parameter)
        {
            if (parameter != null)
            {
                this.SelectedTracks = new List<MergedTrack>();

                foreach (MergedTrackViewModel item in (IList)parameter)
                {
                    this.SelectedTracks.Add(item.Track);
                }
            }
        }

        protected void ConditionalScrollToPlayingTrack()
        {
            // Trigger ScrollToPlayingTrack only if set in the settings
            if (XmlSettingsClient.Instance.Get<bool>("Behaviour", "FollowTrack"))
            {
                if (this.Tracks != null && this.Tracks.Count > 0)
                {
                    this.eventAggregator.GetEvent<ScrollToPlayingTrack>().Publish(null);
                }
            }
        }

        protected void EditSelectedTracks()
        {
            if (this.SelectedTracks == null || this.SelectedTracks.Count == 0) return;

            if (this.CheckAllSelectedTracksExist())
            {
                EditTrack view = this.container.Resolve<EditTrack>();
                view.DataContext = this.container.Resolve<EditTrackViewModel>(new DependencyOverride(typeof(IList<MergedTrack>), this.SelectedTracks));

                this.dialogService.ShowCustomDialog(
                    0xe104,
                    14,
                    ResourceUtils.GetStringResource("Language_Edit_Song"),
                    view,
                    620,
                    450,
                    false,
                    false,
                    false,
                    true,
                    ResourceUtils.GetStringResource("Language_Ok"),
                    ResourceUtils.GetStringResource("Language_Cancel"),
                ((EditTrackViewModel)view.DataContext).SaveTracksAsync);
            }
        }
        #endregion

        #region INavigationAware
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            this.Unsubscribe();
            this.searchTextBeforeInactivate = this.searchService.SearchText;
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.Subscribe();

            // Only refresh the Tracks if the search term was changed since the last time this screen was visited
            if (!this.searchTextBeforeInactivate.Equals(this.searchService.SearchText))
            {
                await Task.Delay(Constants.CommonListLoadDelay); // Wait for the UI to slide in
                this.FilterLists();
            }

            this.ConditionalScrollToPlayingTrack();
        }
        #endregion
    }
}
