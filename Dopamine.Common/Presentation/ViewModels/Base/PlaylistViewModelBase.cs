using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Database;
using Dopamine.Common.Helpers;
using Dopamine.Common.Presentation.ViewModels.Base;
using Dopamine.Common.Presentation.ViewModels.Entities;
using Dopamine.Common.Prism;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;
using Microsoft.Practices.Unity;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Dopamine.Common.Presentation.ViewModels
{
    public abstract class PlaylistViewModelBase : CommonViewModelBase
    {
        #region Variables
        // Collections
        private ObservableCollection<KeyValuePair<string, TrackViewModel>> tracks;
        private CollectionViewSource tracksCvs;
        private IList<KeyValuePair<string, PlayableTrack>> selectedTracks;

        // Flags
        protected bool isDroppingTracks;
        #endregion

        #region Properties
        public ObservableCollection<KeyValuePair<string, TrackViewModel>> Tracks
        {
            get { return this.tracks; }
            set { SetProperty<ObservableCollection<KeyValuePair<string, TrackViewModel>>>(ref this.tracks, value); }
        }

        public CollectionViewSource TracksCvs
        {
            get { return this.tracksCvs; }
            set { SetProperty<CollectionViewSource>(ref this.tracksCvs, value); }
        }

        public IList<KeyValuePair<string, PlayableTrack>> SelectedTracks
        {
            get { return this.selectedTracks; }
            set { SetProperty<IList<KeyValuePair<string, PlayableTrack>>>(ref this.selectedTracks, value); }
        }
        #endregion

        #region Construction
        public PlaylistViewModelBase(IUnityContainer container)
           : base(container)
        {
            // Commands
            this.PlayNextCommand = new DelegateCommand(async () => await this.PlayNextAsync());
            this.AddTracksToNowPlayingCommand = new DelegateCommand(async () => await this.AddTracksToNowPlayingAsync());

            // Events
            this.eventAggregator.GetEvent<SettingEnableRatingChanged>().Subscribe((enableRating) => this.EnableRating = enableRating);
            this.eventAggregator.GetEvent<SettingEnableLoveChanged>().Subscribe((enableLove) => this.EnableLove = enableLove);
        }
        #endregion

        #region Private
        protected async Task GetTracksCommonAsync(OrderedDictionary<string, PlayableTrack> tracks)
        {
            try
            {
                // Create new ObservableCollection
                ObservableCollection<KeyValuePair<string, TrackViewModel>> viewModels = new ObservableCollection<KeyValuePair<string, TrackViewModel>>();

                await Task.Run(() =>
                {
                    foreach (KeyValuePair<string, PlayableTrack> track in tracks)
                    {
                        TrackViewModel vm = this.container.Resolve<TrackViewModel>();
                        vm.Track = track.Value;
                        viewModels.Add(new KeyValuePair<string, TrackViewModel>(track.Key, vm));
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
                    this.Tracks = new ObservableCollection<KeyValuePair<string, TrackViewModel>>();
                });
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
             // Populate CollectionViewSource
             this.TracksCvs = new CollectionViewSource { Source = this.Tracks };
                this.TracksCvs.Filter += new FilterEventHandler(TracksCvs_Filter);

             // Update count
             this.TracksCount = this.TracksCvs.View.Cast<KeyValuePair<string, TrackViewModel>>().Count();
            });

            // Update duration and size
            this.SetSizeInformationAsync(this.TracksCvs);


            // Show playing Track
            this.ShowPlayingTrackAsync();
        }

        private void TracksCvs_Filter(object sender, FilterEventArgs e)
        {
            KeyValuePair<string, TrackViewModel> vm = (KeyValuePair<string, TrackViewModel>)e.Item;
            e.Accepted = Database.Utils.FilterTracks(vm.Value.Track, this.searchService.SearchText);
        }
        #endregion

        #region Private
        private async void SetSizeInformationAsync(CollectionViewSource source)
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
                        foreach (KeyValuePair<string, TrackViewModel> vm in viewCopy)
                        {
                            this.totalDuration += vm.Value.Track.Duration.Value;
                            this.totalSize += vm.Value.Track.FileSize.Value;
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

        #region Overrides
        protected async Task PlayNextAsync()
        {
            IList<PlayableTrack> selectedTracks = this.SelectedTracks.Select(t => t.Value).ToList();

            EnqueueResult result = await this.playbackService.AddToQueueNextAsync(selectedTracks);

            if (!result.IsSuccess)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetStringResource("Language_Error"), ResourceUtils.GetStringResource("Language_Error_Adding_Songs_To_Now_Playing"), ResourceUtils.GetStringResource("Language_Ok"), true, ResourceUtils.GetStringResource("Language_Log_File"));
            }
        }

        protected async Task AddTracksToNowPlayingAsync()
        {
            IList<PlayableTrack> selectedTracks = this.SelectedTracks.Select(t => t.Value).ToList();

            EnqueueResult result = await this.playbackService.AddToQueueAsync(selectedTracks);

            if (!result.IsSuccess)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetStringResource("Language_Error"), ResourceUtils.GetStringResource("Language_Error_Adding_Songs_To_Now_Playing"), ResourceUtils.GetStringResource("Language_Ok"), true, ResourceUtils.GetStringResource("Language_Log_File"));
            }
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

        protected override void FilterLists()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
             // Tracks
             if (this.TracksCvs != null)
                {
                    this.TracksCvs.View.Refresh();
                    this.TracksCount = this.TracksCvs.View.Cast<KeyValuePair<string, TrackViewModel>>().Count();
                }
            });

            this.SetSizeInformationAsync(this.TracksCvs);
            this.ShowPlayingTrackAsync();
        }

        protected async override void MetadataService_RatingChangedAsync(RatingChangedEventArgs e)
        {
            if (this.Tracks == null) return;

            await Task.Run(() =>
            {
                foreach (KeyValuePair<string, TrackViewModel> vm in this.Tracks)
                {
                    if (vm.Value.Track.Path.Equals(e.Path))
                    {
                     // The UI is only updated if PropertyChanged is fired on the UI thread
                     Application.Current.Dispatcher.Invoke(() => vm.Value.UpdateVisibleRating(e.Rating));
                    }
                }
            });
        }

        protected async override void MetadataService_LoveChangedAsync(LoveChangedEventArgs e)
        {
            if (this.Tracks == null) return;

            await Task.Run(() =>
            {
                foreach (KeyValuePair<string, TrackViewModel> vm in this.Tracks)
                {
                    if (vm.Value.Track.Path.Equals(e.Path))
                    {
                     // The UI is only updated if PropertyChanged is fired on the UI thread
                     Application.Current.Dispatcher.Invoke(() => vm.Value.UpdateVisibleLove(e.Love));
                    }
                }
            });
        }

        protected override void ShowSelectedTrackInformation()
        {
            // Don't try to show the file information when nothing is selected
            if (this.SelectedTracks == null || this.SelectedTracks.Count == 0) return;

            this.ShowFileInformation(this.SelectedTracks.Select(t => t.Value.Path).ToList());
        }

        protected override void SearchOnline(string id)
        {
            if (this.SelectedTracks != null && this.SelectedTracks.Count > 0)
            {
                this.providerService.SearchOnline(id, new string[] { this.SelectedTracks.First().Value.ArtistName, this.SelectedTracks.First().Value.TrackTitle });
            }
        }

        protected override void EditSelectedTracks()
        {
            if (this.SelectedTracks == null || this.SelectedTracks.Count == 0) return;

            this.EditFiles(this.SelectedTracks.Select(t => t.Value.Path).ToList());
        }

        protected override void SelectedTracksHandler(object parameter)
        {
            if (parameter != null)
            {
                this.SelectedTracks = new List<KeyValuePair<string, PlayableTrack>>();

                System.Collections.IList items = (System.Collections.IList)parameter;
                var collection = items.Cast<KeyValuePair<string, TrackViewModel>>();

                foreach (KeyValuePair<string, TrackViewModel> item in collection)
                {
                    this.SelectedTracks.Add(new KeyValuePair<string, PlayableTrack>(item.Key, item.Value.Track));
                }
            }
        }

        protected async override Task ShowPlayingTrackAsync()
        {
            if (!this.playbackService.HasCurrentTrack) return;

            string trackGuid = this.playbackService.CurrentTrack.Key;
            string trackSafePath = this.playbackService.CurrentTrack.Value.SafePath;

            await Task.Run(() =>
            {
                if (this.Tracks != null)
                {
                    bool isGuidFound = false;

                    // 1st pass: try to find a matching Guid
                    foreach (KeyValuePair<string, TrackViewModel> vm in this.Tracks)
                    {
                        vm.Value.IsPlaying = false;
                        vm.Value.IsPaused = true;

                        if (vm.Key == trackGuid)
                        {
                            isGuidFound = true;

                            if (!this.playbackService.IsStopped)
                            {
                                vm.Value.IsPlaying = true;

                                if (this.playbackService.IsPlaying)
                                {
                                    vm.Value.IsPaused = false;
                                }
                            }

                            break;
                        }
                    }

                    // 2nd pass: if Guid is not found, try to find a matching path
                    if (!isGuidFound)
                    {
                        foreach (KeyValuePair<string, TrackViewModel> vm in this.Tracks)
                        {
                            vm.Value.IsPlaying = false;
                            vm.Value.IsPaused = true;

                            if (string.Equals(vm.Value.Track.SafePath,trackSafePath))
                            {
                                if (!this.playbackService.IsStopped)
                                {
                                    vm.Value.IsPlaying = true;

                                    if (this.playbackService.IsPlaying)
                                    {
                                        vm.Value.IsPaused = false;
                                    }
                                }

                                break;
                            }
                        }

                    }
                }
            });

            this.ConditionalScrollToPlayingTrack();
        }
        #endregion
    }
}
