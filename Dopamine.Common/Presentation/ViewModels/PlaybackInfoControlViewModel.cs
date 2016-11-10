using Digimezzo.WPFControls.Enums;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Database;
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
        private SlideDirection slideDirection;
        private TrackInfo previousTrackInfo;
        private TrackInfo trackInfo;
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
        public PlaybackInfoControlViewModel(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;

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

                this.ShowPlaybackInfoAsync(this.playbackService.PlayingTrack);
            };

            this.playbackService.PlaybackProgressChanged += (_, __) => this.UpdateTime();

            this.ShowPlaybackInfoAsync(this.playbackService.PlayingTrack);

            // Default SlideDirection
            this.SlideDirection = SlideDirection.DownToUp;
        }
        #endregion

        #region Private
        private void ShowPlaybackInfoAsync(TrackInfo trackInfo)
        {
            this.previousTrackInfo = this.trackInfo;

            // No track selected: clear playback info.
            if (trackInfo == null)
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
                this.trackInfo = null;
                return;
            }

            this.trackInfo = trackInfo;

            // The track didn't change: leave the previous playback info.
            if (this.trackInfo.Equals(this.previousTrackInfo)) return;

            // The track changed: we need to show new playback info.
            try
            {
                string year = string.Empty;

                if (trackInfo.Year != null && trackInfo.Year > 0)
                {
                    year = trackInfo.Year.ToString();
                }

                this.PlaybackInfoViewModel = new PlaybackInfoViewModel
                {
                    Title = string.IsNullOrEmpty(trackInfo.TrackTitle) ? trackInfo.FileName : trackInfo.TrackTitle,
                    Artist = trackInfo.ArtistName,
                    Album = trackInfo.AlbumTitle,
                    Year = year,
                    CurrentTime = FormatUtils.FormatTime(new TimeSpan(0)),
                    TotalTime = FormatUtils.FormatTime(new TimeSpan(0))
                };
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not show playback information for Track {0}. Exception: {1}", trackInfo.Path, ex.Message);
                this.PlaybackInfoViewModel = new PlaybackInfoViewModel
                {
                    Title = string.Empty,
                    Artist = string.Empty,
                    Album = string.Empty,
                    Year = string.Empty,
                    CurrentTime = string.Empty,
                    TotalTime = string.Empty
                };
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
