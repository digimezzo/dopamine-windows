using Dopamine.Common.Services.I18n;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Database;
using Dopamine.Core.Logging;
using Dopamine.Core.Metadata;
using Dopamine.Core.Prism;
using Dopamine.Core.Utils;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class LyricsControlViewModel : BindableBase
    {
        #region Variables
        private IPlaybackService playbackService;
        private II18nService i18nService;
        private LyricsViewModel lyricsViewModel;
        private TrackInfo previousTrackInfo;
        private TrackInfo trackInfo;
        private int contentSlideInFrom;
        private Timer highlightTimer;
        private int highlightTimerIntervalMilliseconds = 100;
        private EventAggregator eventAggregator;
        #endregion

        #region Properties
        public int ContentSlideInFrom
        {
            get { return this.contentSlideInFrom; }
            set { SetProperty<int>(ref this.contentSlideInFrom, value); }
        }

        public LyricsViewModel LyricsViewModel
        {
            get { return this.lyricsViewModel; }
            set { SetProperty<LyricsViewModel>(ref this.lyricsViewModel, value); }
        }
        #endregion

        #region Construction
        public LyricsControlViewModel(IPlaybackService playbackService, II18nService i18nService, EventAggregator eventAggregator)
        {
            this.playbackService = playbackService;
            this.i18nService = i18nService;
            this.eventAggregator = eventAggregator;

            this.highlightTimer = new Timer();
            this.highlightTimer.Interval = this.highlightTimerIntervalMilliseconds;
            this.highlightTimer.Elapsed += HighlightTimer_Elapsed;

            this.playbackService.PlaybackFailed += (_, __) => this.ShowLyricsAsync(null);
            this.playbackService.PlaybackStopped += (_, __) => this.ShowLyricsAsync(null);

            this.playbackService.PlaybackPaused += (_, __) => this.highlightTimer.Stop();
            this.playbackService.PlaybackResumed += (_, __) => this.highlightTimer.Start();

            this.playbackService.PlaybackSuccess += (isPlayingPreviousTrack) =>
            {
                this.ContentSlideInFrom = isPlayingPreviousTrack ? 30 : -30;

                this.ShowLyricsAsync(this.playbackService.PlayingTrack);
            };

            this.i18nService.LanguageChanged += (_, __) => this.ShowLyricsAsync(this.playbackService.PlayingTrack);

            this.ShowLyricsAsync(this.playbackService.PlayingTrack);
        }

        private async void HighlightTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.highlightTimer.Stop();
            await HighlightLyricsLineAsync();
            this.highlightTimer.Start();
        }
        #endregion

        #region Private
        private async void ShowLyricsAsync(TrackInfo trackInfo)
        {
            this.highlightTimer.Stop();

            this.previousTrackInfo = this.trackInfo;

            // No track selected: clear lyrics.
            if (trackInfo == null)
            {
                this.LyricsViewModel = new LyricsViewModel();
                this.trackInfo = null;
                return;
            }

            this.trackInfo = trackInfo;

            // The track didn't change: leave the previous lyrics.
            if (this.trackInfo.Equals(this.previousTrackInfo))
            {
                this.highlightTimer.Start();
                return;
            }

            // The track changed: we need to show new lyrics.
            try
            {
                var fmd = new FileMetadata(trackInfo.Path);

                this.LyricsViewModel = new LyricsViewModel();
                await this.LyricsViewModel.SetLyricsAsync(string.IsNullOrWhiteSpace(fmd.Lyrics.Value) ? ResourceUtils.GetStringResource("Language_No_Lyrics") : fmd.Lyrics.Value);
                this.highlightTimer.Start();
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not show lyrics for Track {0}. Exception: {1}", trackInfo.Path, ex.Message);
                this.LyricsViewModel = new LyricsViewModel();
            }
        }

        private async Task HighlightLyricsLineAsync()
        {
            if (this.LyricsViewModel == null || this.LyricsViewModel.LyricsLines == null) return;

            await Task.Run(() =>
            {
                for (int i = 0; i < this.LyricsViewModel.LyricsLines.Count; i++)
                {
                    double progressTime = this.playbackService.GetCurrentTime.TotalMilliseconds;

                    double lyricsLineTime = this.LyricsViewModel.LyricsLines[i].Time.TotalMilliseconds;
                    double nextLyricsLineTime = 0;

                    int j = 1;

                    while (i + j < this.LyricsViewModel.LyricsLines.Count && nextLyricsLineTime <= lyricsLineTime)
                    {
                        nextLyricsLineTime = this.LyricsViewModel.LyricsLines[i + j].Time.TotalMilliseconds;
                        j++;
                    }

                    if (progressTime >= lyricsLineTime & (nextLyricsLineTime >= progressTime | nextLyricsLineTime == 0))
                    {
                        this.LyricsViewModel.LyricsLines[i].IsHighlighted = true;
                        this.eventAggregator.GetEvent<ScrollToHighlightedLyricsLine>().Publish(null);
                    }
                    else
                    {
                        this.LyricsViewModel.LyricsLines[i].IsHighlighted = false;
                    }
                }
            });
        }
        #endregion
    }
}