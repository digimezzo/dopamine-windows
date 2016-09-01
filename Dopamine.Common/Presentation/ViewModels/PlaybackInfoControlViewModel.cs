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

            this.playbackService.PlaybackFailed += (_, __) => this.ShowPlaybackInfoAsync(null);
            this.playbackService.PlaybackStopped += (_, __) => this.ShowPlaybackInfoAsync(null);

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

            // If PlaybackService.PlayingTrack is Nothing, nothing is shown. This is handled in ShowPlaybackInfoAsync.
            // If it is not nothing, the Plackback information for the currently playing track is shown when this screen is created.
            // If we didn't call this function here, we would have to wait until the next playbackService.PlaybackSuccess 
            // before seeing any Plackback information.
            this.ShowPlaybackInfoAsync(this.playbackService.PlayingTrack);

            // Default SlideDirection
            this.SlideDirection = SlideDirection.DownToUp;
        }
        #endregion

        #region Private
        private void ShowPlaybackInfoAsync(TrackInfo iTrackInfo)
        {
            if (iTrackInfo == null)
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
                return;
            }

            try
            {
                string year = string.Empty;

                if (iTrackInfo.Year != null && iTrackInfo.Year > 0)
                {
                    year = iTrackInfo.Year.ToString();
                }

                this.PlaybackInfoViewModel = new PlaybackInfoViewModel
                {
                    Title = string.IsNullOrEmpty(iTrackInfo.TrackTitle) ? iTrackInfo.FileName : iTrackInfo.TrackTitle,
                    Artist = iTrackInfo.ArtistName,
                    Album = iTrackInfo.AlbumTitle,
                    Year = year,
                    CurrentTime = FormatUtils.FormatTime(new TimeSpan(0)),
                    TotalTime = FormatUtils.FormatTime(new TimeSpan(0))
                };
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not show playback information for Track {0}. Exception: {1}", iTrackInfo.Path, ex.Message);
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
