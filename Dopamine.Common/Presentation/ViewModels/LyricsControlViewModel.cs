using Digimezzo.Utilities.Settings;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Api.Lyrics;
using Dopamine.Common.Database;
using Digimezzo.Utilities.Log;
using Dopamine.Common.Metadata;
using Dopamine.Common.Prism;
using Microsoft.Practices.Unity;
using Prism.Commands;
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
        private IUnityContainer container;
        private IMetadataService metadataService;
        private IPlaybackService playbackService;
        private LyricsViewModel lyricsViewModel;
        private PlayableTrack previousTrack;
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
        private bool lyricsScreenIsActive;
        #endregion

        #region Commands
        public DelegateCommand RefreshLyricsCommand { get; set; }
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
            set
            {
                SetProperty<bool>(ref this.isDownloadingLyrics, value);
                this.RefreshLyricsCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region Construction
        public LyricsControlViewModel(IUnityContainer container, IMetadataService metadataService, IPlaybackService playbackService, EventAggregator eventAggregator)
        {
            this.container = container;
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

            this.metadataService.MetadataChanged += (_) => this.RefreshLyricsAsync(this.playbackService.CurrentTrack.Value);

            this.eventAggregator.GetEvent<SettingDownloadLyricsChanged>().Subscribe(isDownloadLyricsEnabled => { if (isDownloadLyricsEnabled) this.RefreshLyricsAsync(this.playbackService.CurrentTrack.Value); });
            this.eventAggregator.GetEvent<LyricsScreenIsActiveChanged>().Subscribe(lyricsScreenIsActive =>
            {
                this.lyricsScreenIsActive = lyricsScreenIsActive;
                this.RefreshLyricsAsync(this.playbackService.CurrentTrack.Value);
            });

            this.RefreshLyricsCommand = new DelegateCommand(() => this.RefreshLyricsAsync(this.playbackService.CurrentTrack.Value), () => !this.IsDownloadingLyrics);
            ApplicationCommands.RefreshLyricsCommand.RegisterCommand(this.RefreshLyricsCommand);

            this.playbackService.PlaybackSuccess += (isPlayingPreviousTrack) =>
            {
                this.ContentSlideInFrom = isPlayingPreviousTrack ? -30 : 30;

                if (SettingsClient.Get<bool>("FullPlayer", "IsNowPlayingSelected"))
                {
                    if (this.previousTrack == null || !this.playbackService.CurrentTrack.Equals(this.previousTrack))
                    {
                        this.refreshTimer.Stop();
                        this.refreshTimer.Start();
                    }
                    this.previousTrack = this.playbackService.CurrentTrack.Value;
                }
            };

            this.ClearLyrics(); // Makes sure the loading animation can be shown even at first start
            this.RefreshLyricsAsync(this.playbackService.CurrentTrack.Value);
            if (this.playbackService.HasCurrentTrack) this.previousTrack = this.playbackService.CurrentTrack.Value;
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.refreshTimer.Stop();
            this.RefreshLyricsAsync(this.playbackService.CurrentTrack.Value);
        }

        private void UpdateLyricsAfterEditingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.updateLyricsAfterEditingTimer.Stop();
            this.RefreshLyricsAsync(this.playbackService.CurrentTrack.Value);
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
            this.canHighlight = true;
        }

        private void StopHighlighting()
        {
            this.canHighlight = false;
            this.highlightTimer.Stop();
        }

        private void ClearLyrics()
        {
            this.LyricsViewModel = new LyricsViewModel(container);
        }

        private async void RefreshLyricsAsync(PlayableTrack track)
        {
            if (!this.lyricsScreenIsActive) return;
            if (track == null) return;

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
                Lyrics lyrics = null;
                bool mustDownloadLyrics = false;

                await Task.Run(() =>
                {
                    lyrics = new Lyrics(fmd != null && fmd.Lyrics.Value != null ? fmd.Lyrics.Value : String.Empty, string.Empty);

                    // If the file has no lyrics, and the user enabled automatic download of lyrics, indicate that we need to try to download.
                    if (!lyrics.HasText)
                    {
                        if (SettingsClient.Get<bool>("Lyrics", "DownloadLyrics"))
                        {
                            string artist = fmd.Artists != null && fmd.Artists.Values != null && fmd.Artists.Values.Length > 0 ? fmd.Artists.Values[0] : string.Empty;
                            string title = fmd.Title != null && fmd.Title.Value != null ? fmd.Title.Value : string.Empty;

                            if (!string.IsNullOrWhiteSpace(artist) & !string.IsNullOrWhiteSpace(title)) mustDownloadLyrics = true;
                        }
                    }
                });

                // No lyrics were found in the file: try to download.
                if (mustDownloadLyrics)
                {
                    this.IsDownloadingLyrics = true;

                    try
                    {
                        var factory = new LyricsFactory();
                        lyrics = await factory.GetLyricsAsync(fmd.Artists.Values[0], fmd.Title.Value);
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Could not get lyrics online {0}. Exception: {1}", track.Path, ex.Message);
                    }

                    this.IsDownloadingLyrics = false;
                }

                await Task.Run(() =>
                            {
                                this.LyricsViewModel = new LyricsViewModel(container, track, metadataService);
                                this.LyricsViewModel.SetLyrics(lyrics);
                            });
            }
            catch (Exception ex)
            {
                this.IsDownloadingLyrics = false;
                LogClient.Error("Could not show lyrics for Track {0}. Exception: {1}", track.Path, ex.Message);
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
                    LogClient.Error("Could not highlight the lyrics. Exception: {0}", ex.Message);
                }

            });
        }
        #endregion
    }
}