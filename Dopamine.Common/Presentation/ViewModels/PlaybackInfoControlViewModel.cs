using Digimezzo.WPFControls.Enums;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Logging;
using Dopamine.Core.Utils;
using Prism.Mvvm;
using System;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class PlaybackInfoControlViewModel : BindableBase
    {
        #region Variables
        private PlaybackInfoViewModel playbackInfoViewModel;
        private IPlaybackService playbackService;
        private IMetadataService metadataService;
        private ITrackRepository trackRepository;
        private SlideDirection slideDirection;
        private string previousPath;
        private string path;
        #endregion

        #region Properties
        public PlaybackInfoViewModel PlaybackInfoViewModel
        {
            get { return this.playbackInfoViewModel; }
            set { SetProperty<PlaybackInfoViewModel>(ref this.playbackInfoViewModel, value); }
        }

        public SlideDirection SlideDirection
        {
            get { return this.slideDirection; }
            set { SetProperty<SlideDirection>(ref this.slideDirection, value); }
        }
        #endregion

        #region Construction
        public PlaybackInfoControlViewModel(IPlaybackService playbackService,IMetadataService metadataService, ITrackRepository trackRepository)
        {
            this.playbackService = playbackService;
            this.metadataService = metadataService;
            this.trackRepository = trackRepository;

            this.playbackService.PlaybackSuccess += (isPlayingPreviousTrack) =>
            {
                this.SlideDirection = isPlayingPreviousTrack ? SlideDirection.UpToDown : SlideDirection.DownToUp;
                this.ShowPlaybackInfoAsync(this.playbackService.PlayingPath, false);
            };

            this.metadataService.MetadataChanged += (e) => {
                if (this.playbackService.PlayingPath != null && e.IsPlaybackInfoChanged && e.ChangedPaths.Contains(this.playbackService.PlayingPath))
                    this.ShowPlaybackInfoAsync(this.playbackService.PlayingPath, true);
            };

            this.playbackService.PlaybackProgressChanged += (_, __) => this.UpdateTime();

            // Default SlideDirection
            this.SlideDirection = SlideDirection.DownToUp;

            // Default playbak information
            this.ShowPlaybackInfoAsync(this.playbackService.PlayingPath, false);
        }
        #endregion

        #region Private
        private void ClearPlaybackInformation()
        {
            this.PlaybackInfoViewModel = new PlaybackInfoViewModel
            {
                Title = string.Empty,
                Artist = string.Empty,
                Album = string.Empty,
                Year = string.Empty,
                CurrentTime = string.Empty,
                TotalTime = string.Empty
            };
            this.path = null;
        }

        private async void ShowPlaybackInfoAsync(string path, bool allowShowingSamePath)
        {
            this.previousPath = this.path;

            // No track selected: clear playback info.
            if (path == null)
            {
                this.ClearPlaybackInformation();
                return;
            }

            this.path = path;

            // The track didn't change: leave the previous playback info.
            if (!allowShowingSamePath & this.path.Equals(this.previousPath)) return;

            // Get the track from the database
            MergedTrack mergedTrack = await this.trackRepository.GetMergedTrackAsync(path);

            if (mergedTrack == null)
            {
                LogClient.Instance.Logger.Error("Track not found in the database for path: {0}", path);
                this.ClearPlaybackInformation();
                return;
            }

            // The track changed: we need to show new playback info.
            try
            {
                string year = string.Empty;

                if (mergedTrack.Year != null && mergedTrack.Year > 0)
                {
                    year = mergedTrack.Year.ToString();
                }

                this.PlaybackInfoViewModel = new PlaybackInfoViewModel
                {
                    Title = string.IsNullOrEmpty(mergedTrack.TrackTitle) ? mergedTrack.FileName : mergedTrack.TrackTitle,
                    Artist = mergedTrack.ArtistName,
                    Album = mergedTrack.AlbumTitle,
                    Year = year,
                    CurrentTime = FormatUtils.FormatTime(new TimeSpan(0)),
                    TotalTime = FormatUtils.FormatTime(new TimeSpan(0))
                };
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not show playback information for Track {0}. Exception: {1}", path, ex.Message);
                this.ClearPlaybackInformation();
            }

            this.UpdateTime();
        }

        private void UpdateTime()
        {
            this.PlaybackInfoViewModel.CurrentTime = FormatUtils.FormatTime(this.playbackService.GetCurrentTime);
            this.PlaybackInfoViewModel.TotalTime = " / " + FormatUtils.FormatTime(this.playbackService.GetTotalTime);
        }
        #endregion
    }
}
