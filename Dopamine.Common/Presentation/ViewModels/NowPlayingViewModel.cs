using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Database;
using Dopamine.Common.Presentation.ViewModels.Base;
using Dopamine.Common.Presentation.ViewModels.Entities;
using Dopamine.Common.Prism;
using Dopamine.Common.Services.Playback;
using GongSolutions.Wpf.DragDrop;
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
    public class NowPlayingViewModel : ViewModelBase, IDropTarget
    {
        #region Variables
        private bool allowFillAllLists = true;
        private bool isDroppingTracks;
        private ObservableCollection<TrackViewModel> tracks;
        private CollectionViewSource tracksCvs;
        private IList<PlayableTrack> selectedTracks;
        #endregion

        #region Commands
        public DelegateCommand RemoveFromNowPlayingCommand { get; set; }
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
        public NowPlayingViewModel(IUnityContainer container) : base(container)
        {
            // Commands
            this.RemoveFromNowPlayingCommand = new DelegateCommand(async () => await RemoveSelectedTracksFromNowPlayingAsync());

            // Events
            this.eventAggregator.GetEvent<SettingEnableRatingChanged>().Subscribe((enableRating) => this.EnableRating = enableRating);
            this.eventAggregator.GetEvent<SettingEnableLoveChanged>().Subscribe((enableLove) => this.EnableLove = enableLove);

            // PlaybackService
            this.playbackService.QueueChanged += async (_, __) => { if (!isDroppingTracks) await this.FillListsAsync(); };
        }
        #endregion

        #region Private
        protected async Task GetTracksAsync()
        {
            try
            {
                // Create new ObservableCollection
                ObservableCollection<TrackViewModel> viewModels = new ObservableCollection<TrackViewModel>();

                // Order the incoming Tracks
                List<PlayableTrack> orderedTracks = this.playbackService.Queue;

                await Task.Run(() =>
                {
                    foreach (PlayableTrack t in orderedTracks)
                    {
                        TrackViewModel vm = this.container.Resolve<TrackViewModel>();
                        vm.Track = t;
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
            });

            // Update duration and size
            this.SetSizeInformationAsync(this.TracksCvs);


            // Show playing Track
            this.ShowPlayingTrackAsync();
        }

        private void TracksCvs_Filter(object sender, FilterEventArgs e)
        {
            TrackViewModel vm = e.Item as TrackViewModel;
            e.Accepted = Dopamine.Common.Database.Utils.FilterTracks(vm.Track, this.searchService.SearchText);
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

        private void ConditionalScrollToPlayingTrack()
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
        #endregion

        #region Protected
        protected async Task FillListsAsync()
        {
            if (!this.allowFillAllLists) return;
            await this.GetTracksAsync();
        }
        #endregion

        #region public
        public async Task RemoveSelectedTracksFromNowPlayingAsync()
        {
            this.allowFillAllLists = false;

            // Remove Tracks from PlaybackService (this dequeues the Tracks)
            DequeueResult dequeueResult = await this.playbackService.Dequeue(this.SelectedTracks);

            var viewModelsToRemove = new List<TrackViewModel>();

            await Task.Run(() =>
            {
                // Collect the ViewModels to remove
                foreach (TrackViewModel vm in this.Tracks)
                {
                    if (dequeueResult.DequeuedTracks.Select((t) => t.Path).ToList().Contains(vm.Track.Path))
                    {
                        viewModelsToRemove.Add(vm);
                    }
                }
            });

            // Remove the ViewModels from Tracks (this updates the UI)
            foreach (TrackViewModel vm in viewModelsToRemove)
            {
                this.Tracks.Remove(vm);
            }

            this.TracksCount = this.Tracks.Count;

            if (!dequeueResult.IsSuccess)
            {
                this.dialogService.ShowNotification(
                    0xe711,
                    16,
                    ResourceUtils.GetStringResource("Language_Error"),
                    ResourceUtils.GetStringResource("Language_Error_Removing_From_Now_Playing"),
                    ResourceUtils.GetStringResource("Language_Ok"),
                    true,
                    ResourceUtils.GetStringResource("Language_Log_File"));
            }

            this.allowFillAllLists = true;
        }
        #endregion

        #region IDropTarget
        public void DragOver(IDropInfo dropInfo)
        {
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.DragOver(dropInfo);

            try
            {
                dropInfo.NotHandled = true;
            }
            catch (Exception ex)
            {
                dropInfo.NotHandled = false;
                LogClient.Error("Could not drag tracks. Exception: {0}", ex.Message);
            }
        }

        public async void Drop(IDropInfo dropInfo)
        {
            isDroppingTracks = true;

            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);

            try
            {
                var tracks = new List<PlayableTrack>();

                foreach (var item in dropInfo.TargetCollection)
                {
                    tracks.Add(((TrackViewModel)item).Track);
                }

                await this.playbackService.UpdateQueueOrderAsync(tracks);
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not drop tracks. Exception: {0}", ex.Message);
            }

            isDroppingTracks = false;
        }

        protected async override Task LoadedCommandAsync()
        {
            // Not required here
        }
        #endregion
    }
}
