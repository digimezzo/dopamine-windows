using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Core.Helpers;
using Dopamine.Core.Prism;
using Dopamine.Data.Entities;
using Dopamine.ViewModels;
using Dopamine.Services.Dialog;
using Dopamine.Services.I18n;
using Dopamine.Services.Metadata;
using Dopamine.Services.Playback;
using Dopamine.Services.Provider;
using Dopamine.Services.Search;
using Dopamine.Services.Utils;
using Dopamine.ViewModels.Common.Base;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Prism.Ioc;
using GongSolutions.Wpf.DragDrop;
using Dopamine.Core.Base;
using Dopamine.Services.Entities;

namespace Dopamine.ViewModels.Common
{
    public abstract class PlaylistViewModelBase : CommonViewModelBase
    {
        private IContainerProvider container;
        private IPlaybackService playbackService;
        private IEventAggregator eventAggregator;
        private ISearchService searchService;
        private IDialogService dialogService;
        private IProviderService providerService;
        private II18nService i18nService;
        private ObservableCollection<KeyValuePair<string, TrackViewModel>> tracks;
        private CollectionViewSource tracksCvs;
        private IList<KeyValuePair<string, TrackViewModel>> selectedTracks;
        protected bool isDroppingTracks;
        private KeyValuePair<string, TrackViewModel> lastPlayingTrackVm;
        private bool showTrackArt;

        public DelegateCommand<bool?> UpdateShowTrackArtCommand { get; set; }

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

        public IList<KeyValuePair<string, TrackViewModel>> SelectedTracks
        {
            get { return this.selectedTracks; }
            set { SetProperty<IList<KeyValuePair<string, TrackViewModel>>>(ref this.selectedTracks, value); }
        }

        public bool ShowTrackArt
        {
            get { return this.showTrackArt; }
            set { SetProperty(ref this.showTrackArt, value); }
        }

        public PlaylistViewModelBase(IContainerProvider container) : base(container)
        {
            // Dependency injection
            this.container = container;
            this.playbackService = container.Resolve<IPlaybackService>();
            this.eventAggregator = container.Resolve<IEventAggregator>();
            this.searchService = container.Resolve<ISearchService>();
            this.dialogService = container.Resolve<IDialogService>();
            this.providerService = container.Resolve<IProviderService>();
            this.i18nService = container.Resolve<II18nService>();

            // Commands
            this.PlaySelectedCommand = new DelegateCommand(async () => await this.PlaySelectedAsync());
            this.PlayNextCommand = new DelegateCommand(async () => await this.PlayNextAsync());
            this.AddTracksToNowPlayingCommand = new DelegateCommand(async () => await this.AddTracksToNowPlayingAsync());
            this.UpdateShowTrackArtCommand = new DelegateCommand<bool?>((showTrackArt) =>
            {
                SettingsClient.Set<bool>("Appearance", "ShowTrackArtOnPlaylists", showTrackArt.Value, true);
            });

            // Settings
            SettingsClient.SettingChanged += (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "Behaviour", "EnableRating"))
                {
                    this.EnableRating = (bool)e.SettingValue;
                }

                if (SettingsClient.IsSettingChanged(e, "Behaviour", "EnableLove"))
                {
                    this.EnableLove = (bool)e.SettingValue;
                }
            };

            // Events
            this.i18nService.LanguageChanged += (_, __) => this.RefreshLanguage();

            SettingsClient.SettingChanged += (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "Appearance", "ShowTrackArtOnPlaylists"))
                {
                    this.ShowTrackArt = (bool)e.SettingValue;
                    this.UpdateShowTrackArtAsync();
                }
            };

            // Settings
            this.ShowTrackArt = SettingsClient.Get<bool>("Appearance", "ShowTrackArtOnPlaylists");
        }

        private void TracksCvs_Filter(object sender, FilterEventArgs e)
        {
            KeyValuePair<string, TrackViewModel> vm = (KeyValuePair<string, TrackViewModel>)e.Item;
            e.Accepted = EntityUtils.FilterTracks(vm.Value, this.searchService.SearchText);
        }

        private async void CalculateSizeInformationAsync(CollectionViewSource source)
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

                        foreach (KeyValuePair<string, TrackViewModel> vm in viewCopy)
                        {
                            totalDuration += vm.Value.Track.Duration.Value;
                            totalSize += vm.Value.Track.FileSize.Value;
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

        private void RefreshLanguage()
        {
            // Make sure that unknown artist, genre and album are translated correctly.
            this.FillListsAsync();
        }

        protected async Task GetTracksCommonAsync(OrderedDictionary<string, TrackViewModel> tracks)
        {
            try
            {
                // Create new ObservableCollection
                ObservableCollection<KeyValuePair<string, TrackViewModel>> viewModels = new ObservableCollection<KeyValuePair<string, TrackViewModel>>();

                await Task.Run(() =>
                {
                    foreach (KeyValuePair<string, TrackViewModel> track in tracks)
                    {
                        TrackViewModel vm = this.container.Resolve<TrackViewModel>();
                        vm = track.Value;
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
            this.CalculateSizeInformationAsync(this.TracksCvs);

            // Show track art if required
            this.UpdateShowTrackArtAsync();

            // Show playing Track
            this.ShowPlayingTrackAsync();
        }

        protected async void UpdateShowTrackArtAsync()
        {
            if (this.Tracks == null || this.Tracks.Count == 0)
            {
                return;
            }

            await Task.Run(() =>
            {
                foreach (KeyValuePair<string, TrackViewModel> trackPair in this.Tracks)
                {
                    if (trackPair.Value != null)
                    {
                        trackPair.Value.ShowTrackArt = this.showTrackArt;
                    }
                }
            });
        }

        protected async Task PlaySelectedAsync()
        {
            var result = await this.playbackService.PlaySelectedAsync(this.selectedTracks.Select(t => t.Value).ToList());

            if (!result)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Playing_Selected_Songs"), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
            }
        }

        protected async Task PlayNextAsync()
        {
            IList<TrackViewModel> selectedTracks = this.SelectedTracks.Select(t => t.Value).ToList();

            EnqueueResult result = await this.playbackService.AddToQueueNextAsync(selectedTracks);

            if (!result.IsSuccess)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Adding_Songs_To_Now_Playing"), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
            }
        }

        protected async Task AddTracksToNowPlayingAsync()
        {
            IList<TrackViewModel> selectedTracks = this.SelectedTracks.Select(t => t.Value).ToList();

            EnqueueResult result = await this.playbackService.AddToQueueAsync(selectedTracks);

            if (!result.IsSuccess)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Adding_Songs_To_Now_Playing"), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
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

            this.CalculateSizeInformationAsync(this.TracksCvs);
            this.ShowPlayingTrackAsync();
        }

        protected async override void MetadataService_RatingChangedAsync(RatingChangedEventArgs e)
        {
            if (this.Tracks == null) return;

            await Task.Run(() =>
            {
                foreach (KeyValuePair<string, TrackViewModel> vm in this.Tracks)
                {
                    if (vm.Value.Track.SafePath.Equals(e.SafePath))
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
                    if (vm.Value.Track.SafePath.Equals(e.SafePath))
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
                this.SelectedTracks = new List<KeyValuePair<string, TrackViewModel>>();

                System.Collections.IList items = (System.Collections.IList)parameter;
                var collection = items.Cast<KeyValuePair<string, TrackViewModel>>();

                foreach (KeyValuePair<string, TrackViewModel> item in collection)
                {
                    this.SelectedTracks.Add(new KeyValuePair<string, TrackViewModel>(item.Key, item.Value));
                }
            }
        }

        protected override async Task ShowPlayingTrackAsync()
        {
            await Task.Run(() =>
            {
                if (!lastPlayingTrackVm.Equals(default(KeyValuePair<string, TrackViewModel>)))
                {
                    lastPlayingTrackVm.Value.IsPlaying = false;
                    lastPlayingTrackVm.Value.IsPaused = true;
                }

                if (!this.playbackService.HasCurrentTrack)
                {
                    return;
                }

                if (this.Tracks == null) return;
                {
                    var trackGuid = this.playbackService.CurrentTrack.Key;
                    var trackSafePath = this.playbackService.CurrentTrack.Value.SafePath;
                    var isTrackFound = false;

                    // First, try to find a matching Guid. This is the most exact.
                    var trackVm = this.Tracks.FirstOrDefault(vm => vm.Key.Equals(trackGuid));

                    if (!trackVm.Equals(default(KeyValuePair<string, TrackViewModel>)))
                    {
                        isTrackFound = true;
                    }
                    else
                    {
                        // If Guid is not found, try to find a matching path. Side effect: when the playlist contains multiple
                        // entries for the same track, the playlist was enqueued, and the application was stopped and started, entries the
                        // wrong track can be highlighted. That's because the Guids are not known by PlaybackService anymore and we need
                        // to rely on the path of the tracks.
                        trackVm = this.Tracks.FirstOrDefault(vm => vm.Value.Track.SafePath.Equals(trackSafePath));

                        if (!trackVm.Equals(default(KeyValuePair<string, TrackViewModel>)))
                        {
                            isTrackFound = true;
                        }
                    }

                    if (!isTrackFound)
                    {
                        return;
                    }

                    if (!this.playbackService.IsStopped)
                    {
                        trackVm.Value.IsPlaying = true;
                        trackVm.Value.IsPaused = !this.playbackService.IsPlaying;
                    }

                    lastPlayingTrackVm = trackVm;
                }
            });

            this.ConditionalScrollToPlayingTrack();
        }

        protected bool IsDraggingFiles(IDropInfo dropInfo)
        {
            try
            {
                var dataObject = dropInfo.Data as IDataObject;
                return dataObject != null && dataObject.GetDataPresent(DataFormats.FileDrop);
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not detect if we're dragging files. Exception: {0}", ex.Message);
            }

            return false;
        }

        protected bool IsDraggingMediaFiles(IDropInfo dropInfo)
        {
            try
            {
                var dataObject = dropInfo.Data as DataObject;

                var filenames = dataObject.GetFileDropList();
                var supportedExtensions = FileFormats.SupportedMediaExtensions.Concat(FileFormats.SupportedPlaylistExtensions).ToArray();

                foreach (string filename in filenames)
                {
                    if (supportedExtensions.Contains(System.IO.Path.GetExtension(filename.ToLower())))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not detect if we're dragging valid files. Exception: {0}", ex.Message);
            }

            return false;
        }

        protected List<string> GetDroppedFilenames(IDropInfo dropInfo)
        {
            var dataObject = dropInfo.Data as DataObject;

            List<string> filenames = new List<string>();

            try
            {
                filenames = dataObject.GetFileDropList().Cast<string>().ToList();
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not get the dropped filenames. Exception: {0}", ex.Message);
            }

            return filenames;
        }
    }
}
