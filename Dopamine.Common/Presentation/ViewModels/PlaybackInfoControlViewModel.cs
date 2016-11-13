using Digimezzo.WPFControls.Enums;
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
        private ITrackRepository trackRepository;
        private SlideDirection slideDirection;
        private string previousFilename;
        private string filename;
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
        public PlaybackInfoControlViewModel(IPlaybackService playbackService, ITrackRepository trackRepository)
        {
            this.playbackService = playbackService;
            this.trackRepository = trackRepository;

            this.playbackService.PlaybackSuccess += (isPlayingPreviousTrack) =>
            {
                if (isPlayingPreviousTrack)
                {
                    this.SlideDirection = SlideDirection.UpToDown;
                }
                else
                {
                    this.SlideDirection = SlideDirection.DownToUp;
                }

                this.ShowPlaybackInfoAsync(this.playbackService.PlayingFile);
            };

            this.playbackService.PlaybackProgressChanged += (_, __) => this.UpdateTime();

            this.ShowPlaybackInfoAsync(this.playbackService.PlayingFile);

            // Default SlideDirection
            this.SlideDirection = SlideDirection.DownToUp;
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
            this.filename = null;
        }

        private async void ShowPlaybackInfoAsync(string filename)
        {
            this.previousFilename = this.filename;

            // No track selected: clear playback info.
            if (filename == null)
            {
                this.ClearPlaybackInformation();
                return;
            }

            this.filename = filename;

            // The track didn't change: leave the previous playback info.
            if (this.filename.Equals(this.previousFilename)) return;

            // get the track from the database
            TrackInfo dbTrack = await this.trackRepository.GetTrackInfoAsync(filename);

            if(dbTrack == null)
            {
                LogClient.Instance.Logger.Error("Track not found in the database for path: {0}", filename);
                this.ClearPlaybackInformation();
                return;
            }

            // The track changed: we need to show new playback info.
            try
            {
                string year = string.Empty;

                if (dbTrack.Year != null && dbTrack.Year > 0)
                {
                    year = dbTrack.Year.ToString();
                }

                this.PlaybackInfoViewModel = new PlaybackInfoViewModel
                {
                    Title = string.IsNullOrEmpty(dbTrack.TrackTitle) ? dbTrack.FileName : dbTrack.TrackTitle,
                    Artist = dbTrack.ArtistName,
                    Album = dbTrack.AlbumTitle,
                    Year = year,
                    CurrentTime = FormatUtils.FormatTime(new TimeSpan(0)),
                    TotalTime = FormatUtils.FormatTime(new TimeSpan(0))
                };
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not show playback information for Track {0}. Exception: {1}", filename, ex.Message);
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
