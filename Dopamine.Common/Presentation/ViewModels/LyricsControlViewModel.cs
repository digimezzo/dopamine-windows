using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Database;
using Dopamine.Core.Logging;
using Dopamine.Core.Metadata;
using Dopamine.Core.Prism;
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
        private TrackInfo previousTrack;
        private int contentSlideInFrom;
        private Timer highlightTimer = new Timer();
        private int highlightTimerIntervalMilliseconds = 100;
        private EventAggregator eventAggregator;
        private Object lockObject = new Object();
        private Timer updateLyricsAfterEditingTimer = new Timer();
        private int updateLyricsAfterEditingTimerIntervalMilliseconds = 100;
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
        public LyricsControlViewModel(IMetadataService metadataService, IPlaybackService playbackService, EventAggregator eventAggregator)
        {
            this.metadataService = metadataService;
            this.playbackService = playbackService;
            this.eventAggregator = eventAggregator;

            this.highlightTimer.Interval = this.highlightTimerIntervalMilliseconds;
            this.highlightTimer.Elapsed += HighlightTimer_Elapsed;

            this.updateLyricsAfterEditingTimer.Interval = this.updateLyricsAfterEditingTimerIntervalMilliseconds;
            this.updateLyricsAfterEditingTimer.Elapsed += UpdateLyricsAfterEditingTimer_Elapsed;

            this.playbackService.PlaybackPaused += (_, __) => this.highlightTimer.Stop();
            this.playbackService.PlaybackResumed += (_, __) => this.highlightTimer.Start();

            this.metadataService.MetadataChanged += (_) => this.ShowLyricsAsync(this.playbackService.PlayingTrack);

            this.playbackService.PlaybackSuccess += (isPlayingPreviousTrack) =>
            {
                this.ContentSlideInFrom = isPlayingPreviousTrack ? -30 : 30;

                if (this.previousTrack == null || !this.playbackService.PlayingTrack.Equals(this.previousTrack))
                {
                    this.ShowLyricsAsync(this.playbackService.PlayingTrack);
                    this.previousTrack = this.playbackService.PlayingTrack;
                }
            };

            this.ShowLyricsAsync(this.playbackService.FirstQueuedTrack);

            if (this.playbackService.PlayingTrack != null) this.previousTrack = this.playbackService.PlayingTrack;
        }

        private void UpdateLyricsAfterEditingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.updateLyricsAfterEditingTimer.Stop();
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

            FileMetadata fmd = null;
            if (trackInfo != null) fmd = await this.metadataService.GetFileMetadataAsync(trackInfo.Path);

            await Task.Run(() =>
            {
                lock (lockObject)
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
                        this.LyricsViewModel = new LyricsViewModel();
                        return;
                    }

                    // Show the new lyrics
                    try
                    {
                        this.LyricsViewModel = new LyricsViewModel(trackInfo.Path, metadataService);
                        this.LyricsViewModel.SetLyrics(string.IsNullOrWhiteSpace(fmd.Lyrics.Value) ? string.Empty : fmd.Lyrics.Value);
                        this.highlightTimer.Start();
                    }
                    catch (Exception ex)
                    {
                        LogClient.Instance.Logger.Error("Could not show lyrics for Track {0}. Exception: {1}", trackInfo.Path, ex.Message);
                        this.LyricsViewModel = new LyricsViewModel();
                    }
                }
            });
        }

        private async Task HighlightLyricsLineAsync()
        {
            if (this.LyricsViewModel == null || this.LyricsViewModel.LyricsLines == null) return;

            await Task.Run(() =>
            {
                lock (lockObject)
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
                            if (this.LyricsViewModel.AutomaticScrolling) this.eventAggregator.GetEvent<ScrollToHighlightedLyricsLine>().Publish(null);
                        }
                        else
                        {
                            this.LyricsViewModel.LyricsLines[i].IsHighlighted = false;
                        }
                    }
                }
            });
        }
        #endregion
    }
}