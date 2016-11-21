using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Api.LyricWikia;
using Dopamine.Core.Database;
using Dopamine.Core.Logging;
using Dopamine.Core.Metadata;
using Dopamine.Core.Prism;
using Dopamine.Core.Settings;
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
        private IMetadataService metadataService;
        private IPlaybackService playbackService;
        private LyricsViewModel lyricsViewModel;
        private MergedTrack previousTrack;
        private int contentSlideInFrom;
        private Timer highlightTimer = new Timer();
        private int highlightTimerIntervalMilliseconds = 100;
        private EventAggregator eventAggregator;
        private Object lockObject = new Object();
        private Timer updateLyricsAfterEditingTimer = new Timer();
        private int updateLyricsAfterEditingTimerIntervalMilliseconds = 100;
        private bool isDownloadingLyrics;
        private bool canHighlight;
        private Timer refreshTimer = new Timer();
        private int refreshTimerIntervalMilliseconds = 500;
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

        public bool IsDownloadingLyrics
        {
            get { return this.isDownloadingLyrics; }
            set { SetProperty<bool>(ref this.isDownloadingLyrics, value); }
        }
        #endregion

        #region Construction
        public LyricsControlViewModel(IMetadataService metadataService, IPlaybackService playbackService, EventAggregator eventAggregator)
        {
            this.metadataService = metadataService;
            this.playbackService = playbackService;
            this.eventAggregator = eventAggregator;

            this.highlightTimer.Interval = this.highlightTimerIntervalMilliseconds;
            this.highlightTimer.Elapsed += HighlightTimer_Elapsed;

            this.updateLyricsAfterEditingTimer.Interval = this.updateLyricsAfterEditingTimerIntervalMilliseconds;
            this.updateLyricsAfterEditingTimer.Elapsed += UpdateLyricsAfterEditingTimer_Elapsed;

            this.refreshTimer.Interval = this.refreshTimerIntervalMilliseconds;
            this.refreshTimer.Elapsed += RefreshTimer_Elapsed;

            this.playbackService.PlaybackPaused += (_, __) => this.highlightTimer.Stop();
            this.playbackService.PlaybackResumed += (_, __) => this.highlightTimer.Start();

            this.metadataService.MetadataChanged += (_) => this.RefreshLyricsAsync(this.playbackService.PlayingTrack);

            this.eventAggregator.GetEvent<SettingDownloadLyricsChanged>().Subscribe(isDownloadLyricsEnabled => { if (isDownloadLyricsEnabled) this.RefreshLyricsAsync(this.playbackService.PlayingTrack); });

            this.playbackService.PlaybackSuccess += (isPlayingPreviousTrack) =>
            {
                this.ContentSlideInFrom = isPlayingPreviousTrack ? -30 : 30;

                if (this.previousTrack == null || !this.playbackService.PlayingTrack.Equals(this.previousTrack))
                {
                    this.refreshTimer.Stop();
                    this.refreshTimer.Start();
                    this.previousTrack = this.playbackService.PlayingTrack;
                }
            };

            this.RefreshLyricsAsync(this.playbackService.PlayingTrack);

            if (this.playbackService.PlayingTrack != null) this.previousTrack = this.playbackService.PlayingTrack;
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.refreshTimer.Stop();
            this.RefreshLyricsAsync(this.playbackService.PlayingTrack);
        }

        private void UpdateLyricsAfterEditingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.updateLyricsAfterEditingTimer.Stop();
            this.RefreshLyricsAsync(this.playbackService.PlayingTrack);
        }

        private async void HighlightTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.highlightTimer.Stop();
            await HighlightLyricsLineAsync();
            this.highlightTimer.Start();
        }
        #endregion

        #region Private
        private void StartHighlighting()
        {
            this.highlightTimer.Start();
            this.canHighlight = false;
        }

        private void StopHighlighting()
        {
            this.canHighlight = true;
            this.highlightTimer.Stop();
        }

        private void ClearLyrics()
        {
            this.LyricsViewModel = new LyricsViewModel();
        }

        private async void RefreshLyricsAsync(MergedTrack track)
        {
            this.StopHighlighting();

            FileMetadata fmd = await this.metadataService.GetFileMetadataAsync(track.Path);

            await Task.Run(() =>
            {
                // If we're in editing mode, delay changing the lyrics.
                if (this.LyricsViewModel != null && this.LyricsViewModel.IsEditing)
                {
                    this.updateLyricsAfterEditingTimer.Start();
                    return;
                }

                // No FileMetadata available: clear the lyrics.
                if (fmd == null)
                {
                    this.ClearLyrics();
                    return;
                }
            });

            try
            {
                string lyrics = string.Empty;
                string artist = string.Empty;
                string title = string.Empty;

                bool mustDownloadLyrics = false;

                // If the file has no lyrics, and the user enabled automatic download of lyrics, indicate that we need to try to download.
                await Task.Run(() =>
                {
                    if(XmlSettingsClient.Instance.Get<bool>("Lyrics", "DownloadLyrics")){
                        lyrics = fmd != null && fmd.Lyrics.Value != null ? fmd.Lyrics.Value : String.Empty;

                        if (string.IsNullOrWhiteSpace(lyrics))
                        {
                            artist = fmd.Artists != null && fmd.Artists.Values != null && fmd.Artists.Values.Length > 0 ? fmd.Artists.Values[0] : string.Empty;
                            title = fmd.Title != null && fmd.Title.Value != null ? fmd.Title.Value : string.Empty;

                            if (!string.IsNullOrWhiteSpace(artist) & !string.IsNullOrWhiteSpace(title)) mustDownloadLyrics = true;
                        }
                    }
                });

                // No lyrics were found in the file: try to download.
                if (mustDownloadLyrics)
                {
                    this.IsDownloadingLyrics = true;
                    lyrics = await LyricWikiaApi.GetLyricsAsync(fmd.Artists.Values[0], fmd.Title.Value);
                    this.IsDownloadingLyrics = false;
                }

                await Task.Run(() =>
                            {
                                this.LyricsViewModel = new LyricsViewModel(track.Path, metadataService);
                                this.LyricsViewModel.SetLyrics(lyrics);
                            });
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not show lyrics for Track {0}. Exception: {1}", track.Path, ex.Message);
                this.ClearLyrics();
                return;
            }

            this.StartHighlighting();
        }

        private async Task HighlightLyricsLineAsync()
        {
            if (!this.canHighlight) return;
            if (this.LyricsViewModel == null || this.LyricsViewModel.LyricsLines == null) return;

            await Task.Run(() =>
            {
                try
                {
                    for (int i = 0; i < this.LyricsViewModel.LyricsLines.Count; i++)
                    {
                        if (!this.canHighlight) break;
                        double progressTime = this.playbackService.GetCurrentTime.TotalMilliseconds;

                        double lyricsLineTime = this.LyricsViewModel.LyricsLines[i].Time.TotalMilliseconds;
                        double nextLyricsLineTime = 0;

                        int j = 1;

                        while (i + j < this.LyricsViewModel.LyricsLines.Count && nextLyricsLineTime <= lyricsLineTime)
                        {
                            if (!this.canHighlight) break;
                            nextLyricsLineTime = this.LyricsViewModel.LyricsLines[i + j].Time.TotalMilliseconds;
                            j++;
                        }

                        if (progressTime >= lyricsLineTime & (nextLyricsLineTime >= progressTime | nextLyricsLineTime == 0))
                        {
                            this.LyricsViewModel.LyricsLines[i].IsHighlighted = true;
                            if (this.LyricsViewModel.AutomaticScrolling & this.canHighlight) this.eventAggregator.GetEvent<ScrollToHighlightedLyricsLine>().Publish(null);
                        }
                        else
                        {
                            this.LyricsViewModel.LyricsLines[i].IsHighlighted = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not highlight the lyrics. Exception: {0}", ex.Message);
                }

            });
        }
        #endregion
    }
}