using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Core.Prism;
using Dopamine.Data;
using Dopamine.Data.Contracts;
using Dopamine.Data.Contracts.Entities;
using Dopamine.Data.Contracts.Repositories;
using Dopamine.Presentation.ViewModels;
using Dopamine.Services.Contracts.Collection;
using Dopamine.Services.Contracts.Dialog;
using Dopamine.Services.Contracts.I18n;
using Dopamine.Services.Contracts.Metadata;
using Dopamine.Services.Contracts.Playback;
using Dopamine.Services.Contracts.Playlist;
using Dopamine.Services.Contracts.Provider;
using Dopamine.Services.Contracts.Search;
using Dopamine.Services.Utils;
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

namespace Dopamine.ViewModels.Common.Base
{
    public abstract class TracksViewModelBase : CommonViewModelBase
    {
        private IUnityContainer container;
        private IDialogService dialogService;
        private ITrackRepository trackRepository;
        private ISearchService searchService;
        private IPlaybackService playbackService;
        private ICollectionService collectionService;
        private II18nService i18nService;
        private IEventAggregator eventAggregator;
        private IProviderService providerService;
        private IPlaylistService playlistService;
        private ObservableCollection<TrackViewModel> tracks;
        private CollectionViewSource tracksCvs;
        private IList<PlayableTrack> selectedTracks;
        private TrackViewModel lastPlayingTrackVm;

        public abstract bool CanOrderByAlbum { get; }

        public bool ShowRemoveFromDisk => SettingsClient.Get<bool>("Behaviour", "ShowRemoveFromDisk");

        public ObservableCollection<TrackViewModel> Tracks
        {
            get { return this.tracks; }
            set { SetProperty<ObservableCollection<TrackViewModel>>(ref this.tracks, value); }
        }

        public CollectionViewSource TracksCvs
        {
            get { return this.tracksCvs; }
            set { SetProperty<CollectionViewSource>(ref this.tracksCvs, value); }
        }

        public IList<PlayableTrack> SelectedTracks
        {
            get { return this.selectedTracks; }
            set { SetProperty<IList<PlayableTrack>>(ref this.selectedTracks, value); }
        }

        public TracksViewModelBase(IUnityContainer container) : base(container)
        {
            // Dependency injection
            this.container = container;
            this.trackRepository = container.Resolve<ITrackRepository>();
            this.dialogService = container.Resolve<IDialogService>();
            this.searchService = container.Resolve<ISearchService>();
            this.playbackService = container.Resolve<IPlaybackService>();
            this.collectionService = container.Resolve<ICollectionService>();
            this.i18nService = container.Resolve<II18nService>();
            this.eventAggregator = container.Resolve<IEventAggregator>();
            this.providerService = container.Resolve<IProviderService>();
            this.playlistService = container.Resolve<IPlaylistService>();

            // Commands
            this.ToggleTrackOrderCommand = new DelegateCommand(() => this.ToggleTrackOrder());
            this.AddTracksToPlaylistCommand = new DelegateCommand<string>(async (playlistName) => await this.AddTracksToPlaylistAsync(playlistName, this.SelectedTracks));
            this.PlaySelectedCommand = new DelegateCommand(async () => await this.PlaySelectedAsync());
            this.PlayNextCommand = new DelegateCommand(async () => await this.PlayNextAsync());
            this.AddTracksToNowPlayingCommand = new DelegateCommand(async () => await this.AddTracksToNowPlayingAsync());

            // Settings
            SettingsClient.SettingChanged += (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "Behaviour", "ShowRemoveFromDisk"))
                {
                    RaisePropertyChanged(nameof(this.ShowRemoveFromDisk));
                }
            };

            // Events
            this.i18nService.LanguageChanged += (_, __) =>
            {
                RaisePropertyChanged(nameof(this.TotalDurationInformation));
                RaisePropertyChanged(nameof(this.TotalSizeInformation));
                this.RefreshLanguage();
            };

            this.playbackService.TrackStatisticsChanged += PlaybackService_TrackStatisticsChanged;
        }

        private async void PlaybackService_TrackStatisticsChanged(IList<TrackStatistic> trackStatistics)
        {
            if (this.Tracks == null)
            {
                return;
            }

            if (trackStatistics == null)
            {
                return;
            }

            if (trackStatistics.Count == 0)
            {
                return;
            }

            await Task.Run(() =>
            {
                foreach (TrackViewModel vm in this.Tracks)
                {
                    if (trackStatistics.Select(t => t.SafePath).Contains(vm.Track.SafePath))
                    {
                        // The UI is only updated if PropertyChanged is fired on the UI thread
                        TrackStatistic statistic = trackStatistics.Where(c => c.SafePath.Equals(vm.Track.SafePath)).FirstOrDefault();
                        Application.Current.Dispatcher.Invoke(() => vm.UpdateVisibleCounters(statistic));
                    }
                }
            });
        }

        protected void SetTrackOrder(string settingName)
        {
            TrackOrder savedTrackOrder = (TrackOrder)SettingsClient.Get<int>("Ordering", settingName);

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

        protected void TracksCvs_Filter(object sender, FilterEventArgs e)
        {
            TrackViewModel vm = e.Item as TrackViewModel;
            e.Accepted = DataUtils.FilterTracks(vm.Track, this.searchService.SearchText);
        }

        protected async Task GetTracksAsync(IList<Artist> selectedArtists, IList<long> selectedGenreIds, IList<long> selectedAlbumIds, TrackOrder trackOrder)
        {

            if (selectedArtists.IsNullOrEmpty() & selectedGenreIds.IsNullOrEmpty() & selectedAlbumIds.IsNullOrEmpty())
            {
                await this.GetTracksCommonAsync(await this.trackRepository.GetTracksAsync(), trackOrder);
            }
            else
            {
                if (!selectedAlbumIds.IsNullOrEmpty())
                {
                    await this.GetTracksCommonAsync(await this.trackRepository.GetAlbumTracksAsync(selectedAlbumIds), trackOrder);
                    return;
                }

                if (!selectedArtists.IsNullOrEmpty())
                {
                    await this.GetTracksCommonAsync(await this.trackRepository.GetArtistTracksAsync(selectedArtists), trackOrder);
                    return;
                }

                if (!selectedGenreIds.IsNullOrEmpty())
                {
                    await this.GetTracksCommonAsync(await this.trackRepository.GetGenreTracksAsync(selectedGenreIds), trackOrder);
                    return;
                }
            }
        }

        protected async Task GetTracksCommonAsync(IList<PlayableTrack> tracks, TrackOrder trackOrder)
        {
            try
            {
                // Do we need to show the TrackNumber?
                bool showTracknumber = this.TrackOrder == TrackOrder.ByAlbum;

                // Create new ObservableCollection
                ObservableCollection<TrackViewModel> viewModels = new ObservableCollection<TrackViewModel>();

                // Order the incoming Tracks
                List<PlayableTrack> orderedTracks = await DataUtils.OrderTracksAsync(tracks, trackOrder);

                await Task.Run(() =>
                {
                    foreach (PlayableTrack t in orderedTracks)
                    {
                        TrackViewModel vm = this.container.Resolve<TrackViewModel>();
                        vm.Track = t;
                        vm.ShowTrackNumber = showTracknumber;
                        viewModels.Add(vm);
                    }
                });

                // Unbind to improve UI performance
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (this.TracksCvs != null) this.TracksCvs.Filter -= new FilterEventHandler(TracksCvs_Filter);
                    this.TracksCvs = null;
                    this.Tracks = null;
                });

                // Populate ObservableCollection
                Application.Current.Dispatcher.Invoke(() => this.Tracks = viewModels);
            }
            catch (Exception ex)
            {
                LogClient.Error("An error occurred while getting Tracks. Exception: {0}", ex.Message);

                // Failed getting Tracks. Create empty ObservableCollection.
                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.Tracks = new ObservableCollection<TrackViewModel>();
                });
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Populate CollectionViewSource
                this.TracksCvs = new CollectionViewSource { Source = this.Tracks };
                this.TracksCvs.Filter += new FilterEventHandler(TracksCvs_Filter);

                // Update count
                this.TracksCount = this.TracksCvs.View.Cast<TrackViewModel>().Count();

                // Group by Album if needed
                if (this.TrackOrder == TrackOrder.ByAlbum) this.TracksCvs.GroupDescriptions.Add(new PropertyGroupDescription("GroupHeader"));
            });

            // Update duration and size
            this.CalculateSizeInformationAsync(this.TracksCvs);

            // Show playing Track
            this.ShowPlayingTrackAsync();
        }

        protected async Task RemoveTracksFromCollectionAsync(IList<PlayableTrack> selectedTracks)
        {
            string title = ResourceUtils.GetString("Language_Remove");
            string body = ResourceUtils.GetString("Language_Are_You_Sure_To_Remove_Song");

            if (selectedTracks != null && selectedTracks.Count > 1)
            {
                body = ResourceUtils.GetString("Language_Are_You_Sure_To_Remove_Songs");
            }

            if (this.dialogService.ShowConfirmation(0xe11b, 16, title, body, ResourceUtils.GetString("Language_Yes"), ResourceUtils.GetString("Language_No")))
            {
                RemoveTracksResult result = await this.collectionService.RemoveTracksFromCollectionAsync(selectedTracks);

                if (result == RemoveTracksResult.Error)
                {
                    this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Removing_Songs"), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
                }
                else
                {
                    await this.playbackService.DequeueAsync(selectedTracks);
                }
            }
        }

        protected async Task RemoveTracksFromDiskAsync(IList<PlayableTrack> selectedTracks)
        {
            string title = ResourceUtils.GetString("Language_Remove_From_Disk");
            string body = ResourceUtils.GetString("Language_Are_You_Sure_To_Remove_Song_From_Disk");

            if (selectedTracks != null && selectedTracks.Count > 1)
            {
                body = ResourceUtils.GetString("Language_Are_You_Sure_To_Remove_Songs_From_Disk");
            }

            if (this.dialogService.ShowConfirmation(0xe11b, 16, title, body, ResourceUtils.GetString("Language_Yes"), ResourceUtils.GetString("Language_No")))
            {
                RemoveTracksResult result = await this.collectionService.RemoveTracksFromDiskAsync(selectedTracks);

                if (result == RemoveTracksResult.Error)
                {
                    this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Removing_Songs_From_Disk"), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
                }
                else
                {
                    await this.playbackService.DequeueAsync(selectedTracks);
                }
            }
        }

        protected async void CalculateSizeInformationAsync(CollectionViewSource source)
        {
            // Reset duration and size
            this.SetSizeInformation(0, 0);

            CollectionView viewCopy = null;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (source != null)
                {
                    // Create copy of CollectionViewSource because only STA can access it
                    viewCopy = new CollectionView(source.View);
                }
            });

            if (viewCopy != null)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        long totalDuration = 0;
                        long totalSize = 0;

                        foreach (TrackViewModel vm in viewCopy)
                        {
                            totalDuration += vm.Track.Duration.Value;
                            totalSize += vm.Track.FileSize.Value;
                        }

                        this.SetSizeInformation(totalDuration, totalSize);
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("An error occurred while setting size information. Exception: {0}", ex.Message);
                    }

                });
            }

            RaisePropertyChanged(nameof(this.TotalDurationInformation));
            RaisePropertyChanged(nameof(this.TotalSizeInformation));
        }

        protected async Task PlaySelectedAsync()
        {
            var result = await this.playbackService.PlaySelectedAsync(this.selectedTracks);

            if (!result)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Playing_Selected_Songs"), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
            }
        }

        protected async Task PlayNextAsync()
        {
            IList<PlayableTrack> selectedTracks = this.SelectedTracks;

            EnqueueResult result = await this.playbackService.AddToQueueNextAsync(selectedTracks);

            if (!result.IsSuccess)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Adding_Songs_To_Now_Playing"), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
            }
        }

        protected async Task AddTracksToNowPlayingAsync()
        {
            IList<PlayableTrack> selectedTracks = this.SelectedTracks;

            EnqueueResult result = await this.playbackService.AddToQueueAsync(selectedTracks);

            if (!result.IsSuccess)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Adding_Songs_To_Now_Playing"), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
            }
        }

        protected override void FilterLists()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Tracks
                if (this.TracksCvs != null)
                {
                    this.TracksCvs.View.Refresh();
                    this.TracksCount = this.TracksCvs.View.Cast<TrackViewModel>().Count();
                }
            });

            this.CalculateSizeInformationAsync(this.TracksCvs);
            this.ShowPlayingTrackAsync();
        }

        protected override void ConditionalScrollToPlayingTrack()
        {
            // Trigger ScrollToPlayingTrack only if set in the settings
            if (SettingsClient.Get<bool>("Behaviour", "FollowTrack"))
            {
                if (this.Tracks != null && this.Tracks.Count > 0)
                {
                    this.eventAggregator.GetEvent<ScrollToPlayingTrack>().Publish(null);
                }
            }
        }

        protected override async Task ShowPlayingTrackAsync()
        {
            await Task.Run(() =>
            {
                if (lastPlayingTrackVm != null)
                {
                    lastPlayingTrackVm.IsPlaying = false;
                    lastPlayingTrackVm.IsPaused = true;
                }

                if (!this.playbackService.HasCurrentTrack) return;

                if (this.Tracks == null) return;
                {
                    var safePath = this.playbackService.CurrentTrack.Value.SafePath;

                    TrackViewModel trackVm = this.Tracks.FirstOrDefault(vm => vm.Track.SafePath.Equals(safePath));

                    if (!this.playbackService.IsStopped && trackVm != null)
                    {
                        trackVm.IsPlaying = true;
                        trackVm.IsPaused = !this.playbackService.IsPlaying;
                    }
                    lastPlayingTrackVm = trackVm;
                }
            });

            this.ConditionalScrollToPlayingTrack();
        }

        protected async override void MetadataService_RatingChangedAsync(RatingChangedEventArgs e)
        {
            if (this.Tracks == null) return;

            await Task.Run(() =>
            {
                foreach (TrackViewModel vm in this.Tracks)
                {
                    if (vm.Track.Path.Equals(e.Path))
                    {
                        // The UI is only updated if PropertyChanged is fired on the UI thread
                        Application.Current.Dispatcher.Invoke(() => vm.UpdateVisibleRating(e.Rating));
                    }
                }
            });
        }

        protected async override void MetadataService_LoveChangedAsync(LoveChangedEventArgs e)
        {
            if (this.Tracks == null) return;

            await Task.Run(() =>
            {
                foreach (TrackViewModel vm in this.Tracks)
                {
                    if (vm.Track.Path.Equals(e.Path))
                    {
                        // The UI is only updated if PropertyChanged is fired on the UI thread
                        Application.Current.Dispatcher.Invoke(() => vm.UpdateVisibleLove(e.Love));
                    }
                }
            });
        }

        protected override void ShowSelectedTrackInformation()
        {
            // Don't try to show the file information when nothing is selected
            if (this.SelectedTracks == null || this.SelectedTracks.Count == 0) return;

            this.ShowFileInformation(this.SelectedTracks.Select(t => t.Path).ToList());
        }

        protected async override Task LoadedCommandAsync()
        {
            if (!this.IsFirstLoad()) return;
            await Task.Delay(Constants.CommonListLoadDelay);  // Wait for the UI to slide in
            await this.FillListsAsync(); // Fill all the lists
        }

        protected override void EditSelectedTracks()
        {
            if (this.SelectedTracks == null || this.SelectedTracks.Count == 0) return;

            this.EditFiles(this.SelectedTracks.Select(t => t.Path).ToList());
        }

        protected override void SelectedTracksHandler(object parameter)
        {
            if (parameter != null)
            {
                this.SelectedTracks = new List<PlayableTrack>();

                foreach (TrackViewModel item in (IList)parameter)
                {
                    this.SelectedTracks.Add(item.Track);
                }
            }
        }

        protected override void SearchOnline(string id)
        {
            if (this.SelectedTracks != null && this.SelectedTracks.Count > 0)
            {
                this.providerService.SearchOnline(id, new string[] { this.SelectedTracks.First().ArtistName, this.SelectedTracks.First().TrackTitle });
            }
        }

        protected virtual void ToggleTrackOrder()
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
                    if (SettingsClient.Get<bool>("Behaviour", "EnableRating"))
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

        protected virtual void RefreshLanguage()
        {
            // Make sure that unknown artist, genre and album are translated correctly.
            this.FillListsAsync();
        }
    }
}
