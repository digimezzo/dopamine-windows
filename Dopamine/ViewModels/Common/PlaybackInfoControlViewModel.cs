using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.WPFControls.Enums;
using Dopamine.Core.Utils;
using Dopamine.Data.Entities;
using Dopamine.Services.Metadata;
using Dopamine.Services.Playback;
using Dopamine.Services.Scrobbling;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace Dopamine.ViewModels.Common
{
    public class PlaybackInfoControlViewModel : BindableBase
    {
        private PlaybackInfoViewModel playbackInfoViewModel;
        private IPlaybackService playbackService;
        private IMetadataService metadataService;
        private IScrobblingService scrobblingService;
        private SlideDirection slideDirection;
        private PlayableTrack previousTrack;
        private PlayableTrack track;
        private Timer refreshTimer = new Timer();
        private int refreshTimerIntervalMilliseconds = 250;
        private bool enableRating;
        private bool enableLove;

        public int Rating
        {
            get
            {
                return this.track == null ? 0 : NumberUtils.ConvertToInt32(this.track.Rating);
            }
            set
            {
                if(this.track != null)
                {
                    this.track.Rating = (long?)value;
                    RaisePropertyChanged(nameof(this.Rating));
                    this.metadataService.UpdateTrackRatingAsync(this.track.Path, value);
                } 
            }
        }

        public bool Love
        {
            get {
                return this.track == null ? false : NumberUtils.ConvertToBoolean(this.track.Love);
            }
            set
            {
                if (this.track != null)
                {
                    // Update the UI
                    this.track.Love = value ? 1 : 0;
                    RaisePropertyChanged(nameof(this.Love));

                    // Update Love in the database
                    this.metadataService.UpdateTrackLoveAsync(this.track.Path, value);

                    // Send Love/Unlove to the scrobbling service
                    this.scrobblingService.SendTrackLoveAsync(this.track, value);
                }
            }
        }

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

        public PlaybackInfoControlViewModel(IPlaybackService playbackService, IMetadataService metadataService, IScrobblingService scrobblingService)
        {
            this.playbackService = playbackService;
            this.metadataService = metadataService;
            this.scrobblingService = scrobblingService;

            this.refreshTimer.Interval = this.refreshTimerIntervalMilliseconds;
            this.refreshTimer.Elapsed += RefreshTimer_Elapsed;

            this.playbackService.PlaybackSuccess += (_, e) =>
            {
                this.SlideDirection = e.IsPlayingPreviousTrack ? SlideDirection.UpToDown : SlideDirection.DownToUp;
                this.refreshTimer.Stop();
                this.refreshTimer.Start();
            };

            this.playbackService.PlaybackProgressChanged += (_, __) => this.UpdateTime();
            this.playbackService.PlayingTrackPlaybackInfoChanged += (_, __) => this.RefreshPlaybackInfoAsync(this.playbackService.CurrentTrack.Value, true);

            // Settings
            SettingsClient.SettingChanged += async (_, e) =>
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

            // Defaults
            this.SlideDirection = SlideDirection.DownToUp;
            this.RefreshPlaybackInfoAsync(this.playbackService.CurrentTrack.Value, false);
            this.EnableRating = SettingsClient.Get<bool>("Behaviour", "EnableRating");
            this.EnableLove = SettingsClient.Get<bool>("Behaviour", "EnableLove");
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.refreshTimer.Stop();
            this.RefreshPlaybackInfoAsync(this.playbackService.CurrentTrack.Value, false);
        }

        private void ClearPlaybackInfo()
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

            this.track = null;
        }

        private async void RefreshPlaybackInfoAsync(PlayableTrack track, bool allowRefreshingCurrentTrack)
        {
            await Task.Run(() =>
            {
                this.previousTrack = this.track;

                // No track selected: clear playback info.
                if (track == null)
                {
                    this.ClearPlaybackInfo();
                    return;
                }

                this.track = track;

                // The track didn't change: leave the previous playback info.
                if (!allowRefreshingCurrentTrack & this.track.Equals(this.previousTrack)) return;

                // The track changed: we need to show new playback info.
                try
                {
                    string year = string.Empty;

                    if (track.Year != null && track.Year > 0)
                    {
                        year = track.Year.ToString();
                    }

                    this.PlaybackInfoViewModel = new PlaybackInfoViewModel
                    {
                        Title = string.IsNullOrEmpty(track.TrackTitle) ? track.FileName : track.TrackTitle,
                        Artist = track.ArtistName,
                        Album = track.AlbumTitle,
                        Year = year,
                        CurrentTime = FormatUtils.FormatTime(new TimeSpan(0)),
                        TotalTime = FormatUtils.FormatTime(new TimeSpan(0))
                    };
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not show playback information for Track {0}. Exception: {1}", track.Path, ex.Message);
                    this.ClearPlaybackInfo();
                }

                this.UpdateTime();
            });
        }

        private void UpdateTime()
        {
            this.PlaybackInfoViewModel.CurrentTime = FormatUtils.FormatTime(this.playbackService.GetCurrentTime);
            this.PlaybackInfoViewModel.TotalTime = " / " + FormatUtils.FormatTime(this.playbackService.GetTotalTime);
        }
    }
}
