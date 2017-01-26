using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Base;
using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Extensions;
using Dopamine.Common.Helpers;
using Dopamine.Common.Presentation.ViewModels.Entities;
using Dopamine.Common.Presentation.Views;
using Dopamine.Common.Prism;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;
using Microsoft.Practices.Unity;
using Prism;
using Prism.Regions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Dopamine.Common.Presentation.ViewModels.Base
{
    public abstract class TracksViewModelBase : CommonViewModelBase, INavigationAware, IActiveAware
    {
        #region Variables
        // Collections
        private ObservableCollection<TrackViewModel> tracks;
        private CollectionViewSource tracksCvs;
        private IList<PlayableTrack> selectedTracks;
        #endregion

        #region Properties
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
        #endregion

        #region Construction
        public TracksViewModelBase(IUnityContainer container) : base(container)
        {
        }
        #endregion

        #region Private
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

        private bool CheckAllSelectedTracksExist()
        {
            bool allSelectedTracksExist = true;

            foreach (PlayableTrack trk in this.SelectedTracks)
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
                    this.indexingService.IndexCollectionAsync(SettingsClient.Get<bool>("Indexing", "IgnoreRemovedFiles"), false);
                }
            }

            return allSelectedTracksExist;
        }
        #endregion

        #region Protected
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
            e.Accepted = Dopamine.Common.Database.Utils.FilterTracks(vm.Track, this.searchService.SearchText);
        }

        /// <summary>
        /// Virtual because each child ViewModel has its own set of Lists
        /// </summary>
        /// <remarks></remarks>
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

            this.SetSizeInformationAsync(this.TracksCvs);
            this.ShowPlayingTrackAsync();
        }

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

        protected async Task GetTracksCommonAsync(IList<PlayableTrack> tracks, TrackOrder trackOrder)
        {
            try
            {
                // Do we need to show the TrackNumber?
                bool showTracknumber = this.TrackOrder == TrackOrder.ByAlbum;

                // Create new ObservableCollection
                ObservableCollection<TrackViewModel> viewModels = new ObservableCollection<TrackViewModel>();

                // Order the incoming Tracks
                List<PlayableTrack> orderedTracks = await Database.Utils.OrderTracksAsync(tracks, trackOrder);

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
            this.SetSizeInformationAsync(this.TracksCvs);

            // Show playing Track
            this.ShowPlayingTrackAsync();
        }

        protected async Task RemoveTracksFromCollectionAsync(IList<PlayableTrack> selectedTracks)
        {
            string title = ResourceUtils.GetStringResource("Language_Remove");
            string body = ResourceUtils.GetStringResource("Language_Are_You_Sure_To_Remove_Song");

            if (selectedTracks != null && selectedTracks.Count > 1)
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

        protected async Task RemoveTracksFromDiskAsync(IList<PlayableTrack> selectedTracks)
        {
            string title = ResourceUtils.GetStringResource("Language_Remove_From_Disk");
            string body = ResourceUtils.GetStringResource("Language_Are_You_Sure_To_Remove_Song_From_Disk");

            if (selectedTracks != null && selectedTracks.Count > 1)
            {
                body = ResourceUtils.GetStringResource("Language_Are_You_Sure_To_Remove_Songs_From_Disk");
            }

            if (this.dialogService.ShowConfirmation(0xe11b, 16, title, body, ResourceUtils.GetStringResource("Language_Yes"), ResourceUtils.GetStringResource("Language_No")))
            {
                RemoveTracksResult result = await this.collectionService.RemoveTracksFromDiskAsync(selectedTracks);

                if (result == RemoveTracksResult.Error)
                {
                    this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetStringResource("Language_Error"), ResourceUtils.GetStringResource("Language_Error_Removing_Songs_From_Disk"), ResourceUtils.GetStringResource("Language_Ok"), true, ResourceUtils.GetStringResource("Language_Log_File"));
                }
                else
                {
                    await this.playbackService.Dequeue(selectedTracks);
                }
            }
        }

        protected async void SetSizeInformationAsync(CollectionViewSource source)
        {
            // Reset duration and size
            this.totalDuration = 0;
            this.totalSize = 0;

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
                        foreach (TrackViewModel vm in viewCopy)
                        {
                            this.totalDuration += vm.Track.Duration.Value;
                            this.totalSize += vm.Track.FileSize.Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("An error occured while setting size information. Exception: {0}", ex.Message);
                    }

                });
            }

            OnPropertyChanged(() => this.TotalDurationInformation);
            OnPropertyChanged(() => this.TotalSizeInformation);
        }
        #endregion

        #region Override
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

        protected async override Task ShowPlayingTrackAsync()
        {
            if (this.playbackService.PlayingTrack == null)
                return;

            string path = this.playbackService.PlayingTrack.Path;

            await Task.Run(() =>
            {
                if (this.Tracks != null)
                {
                    foreach (TrackViewModel vm in this.Tracks)
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

        protected async override Task AddTracksToPlaylistAsync(string playlistName)
        {
            IList<PlayableTrack> selectedTracks = this.SelectedTracks;

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
                    AddToPlaylistResult result = await this.collectionService.AddTracksToPlaylistAsync(selectedTracks, playlistName);

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

            if (this.CheckAllSelectedTracksExist())
            {
                Views.FileInformation view = this.container.Resolve<Views.FileInformation>();
                view.DataContext = this.container.Resolve<FileInformationViewModel>(new DependencyOverride(typeof(PlayableTrack), this.SelectedTracks.First()));

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

        protected async override Task LoadedCommandAsync()
        {
            if (this.isFirstLoad)
            {
                this.isFirstLoad = false;

                await Task.Delay(Constants.CommonListLoadDelay);  // Wait for the UI to slide in
                await this.FillListsAsync(); // Fill all the lists
            }
        }

        protected async override Task AddTracksToNowPlayingAsync()
        {
            IList<PlayableTrack> selectedTracks = this.SelectedTracks;

            EnqueueResult result = await this.playbackService.AddToQueue(selectedTracks);

            if (!result.IsSuccess)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetStringResource("Language_Error"), ResourceUtils.GetStringResource("Language_Error_Adding_Songs_To_Now_Playing"), ResourceUtils.GetStringResource("Language_Ok"), true, ResourceUtils.GetStringResource("Language_Log_File"));
            }
        }

        protected async override Task PlayNextAsync()
        {
            IList<PlayableTrack> selectedTracks = this.SelectedTracks;

            EnqueueResult result = await this.playbackService.AddToQueueNext(selectedTracks);

            if (!result.IsSuccess)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetStringResource("Language_Error"), ResourceUtils.GetStringResource("Language_Error_Adding_Songs_To_Now_Playing"), ResourceUtils.GetStringResource("Language_Ok"), true, ResourceUtils.GetStringResource("Language_Log_File"));
            }
        }

        protected override void EditSelectedTracks()
        {
            if (this.SelectedTracks == null || this.SelectedTracks.Count == 0) return;

            if (this.CheckAllSelectedTracksExist())
            {
                EditTrack view = this.container.Resolve<EditTrack>();
                view.DataContext = this.container.Resolve<EditTrackViewModel>(new DependencyOverride(typeof(IList<PlayableTrack>), this.SelectedTracks));

                this.dialogService.ShowCustomDialog(
                    0xe104,
                    14,
                    ResourceUtils.GetStringResource("Language_Edit_Song"),
                    view,
                    620,
                    660,
                    false,
                    false,
                    false,
                    true,
                    ResourceUtils.GetStringResource("Language_Ok"),
                    ResourceUtils.GetStringResource("Language_Cancel"),
                ((EditTrackViewModel)view.DataContext).SaveTracksAsync);
            }
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
                this.PerformSearchOnline(id, this.SelectedTracks.First().ArtistName, this.SelectedTracks.First().TrackTitle);
            }
        }
        #endregion
    }
}
