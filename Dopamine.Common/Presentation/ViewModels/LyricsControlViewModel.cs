using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Dopamine.Common.Api.Lyrics;
using Dopamine.Common.Base;
using Dopamine.Common.Enums;
using Dopamine.Common.Metadata;
using Dopamine.Common.Presentation.ViewModels.Base;
using Dopamine.Common.Prism;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Helpers;
using Dopamine.Data;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class LyricsControlViewModel : ContextMenuViewModelBase
    {
        private IUnityContainer container;
        private ILocalizationInfo info;
        private IMetadataService metadataService;
        private IPlaybackService playbackService;
        private LyricsViewModel lyricsViewModel;
        private PlayableTrack previousTrack;
        private int contentSlideInFrom;
        private Timer highlightTimer = new Timer();
        private int highlightTimerIntervalMilliseconds = 100;
        private IEventAggregator eventAggregator;
        private Object lockObject = new Object();
        private Timer updateLyricsAfterEditingTimer = new Timer();
        private int updateLyricsAfterEditingTimerIntervalMilliseconds = 100;
        private bool isDownloadingLyrics;
        private bool canHighlight;
        private Timer refreshTimer = new Timer();
        private int refreshTimerIntervalMilliseconds = 500;
        private bool isNowPlayingPageActive;
        private bool isNowPlayingLyricsPageActive;

        public DelegateCommand RefreshLyricsCommand { get; set; }

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

        public LyricsControlViewModel(IUnityContainer container) : base(container)
        {
            this.container = container;
            this.info = container.Resolve<ILocalizationInfo>();
            this.metadataService = container.Resolve<IMetadataService>();
            this.playbackService = container.Resolve<IPlaybackService>();
            this.eventAggregator = container.Resolve<IEventAggregator>();

            this.highlightTimer.Interval = this.highlightTimerIntervalMilliseconds;
            this.highlightTimer.Elapsed += HighlightTimer_Elapsed;

            this.updateLyricsAfterEditingTimer.Interval = this.updateLyricsAfterEditingTimerIntervalMilliseconds;
            this.updateLyricsAfterEditingTimer.Elapsed += UpdateLyricsAfterEditingTimer_Elapsed;

            this.refreshTimer.Interval = this.refreshTimerIntervalMilliseconds;
            this.refreshTimer.Elapsed += RefreshTimer_Elapsed;

            this.playbackService.PlaybackPaused += (_, __) => this.highlightTimer.Stop();
            this.playbackService.PlaybackResumed += (_, __) => this.highlightTimer.Start();

            this.metadataService.MetadataChanged += (_) => this.RestartRefreshTimer();

            SettingsClient.SettingChanged += (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "Lyrics", "DownloadLyrics"))
                {
                    if ((bool)e.SettingValue)
                    {
                        this.RestartRefreshTimer();
                    }
                }
            };

            this.isNowPlayingPageActive = SettingsClient.Get<bool>("FullPlayer", "IsNowPlayingSelected");
            this.isNowPlayingLyricsPageActive = ((NowPlayingSubPage)SettingsClient.Get<int>("FullPlayer", "SelectedNowPlayingSubPage")) == NowPlayingSubPage.Lyrics;

            this.eventAggregator.GetEvent<IsNowPlayingPageActiveChanged>().Subscribe(isNowPlayingPageActive =>
            {
                this.isNowPlayingPageActive = isNowPlayingPageActive;
                this.RestartRefreshTimer();
            });

            this.eventAggregator.GetEvent<IsNowPlayingSubPageChanged>().Subscribe(tuple =>
            {
                this.isNowPlayingLyricsPageActive = tuple.Item2 == NowPlayingSubPage.Lyrics;
                this.RestartRefreshTimer();
            });

            this.RefreshLyricsCommand = new DelegateCommand(() => this.RestartRefreshTimer(), () => !this.IsDownloadingLyrics);
            ApplicationCommands.RefreshLyricsCommand.RegisterCommand(this.RefreshLyricsCommand);

            this.playbackService.PlaybackSuccess += (_, e) =>
            {
                this.ContentSlideInFrom = e.IsPlayingPreviousTrack ? -30 : 30;
                this.RestartRefreshTimer();
            };

            this.ClearLyrics(); // Makes sure the loading animation can be shown even at first start
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

        private void RestartRefreshTimer()
        {
            this.refreshTimer.Stop();
            this.refreshTimer.Start();
        }

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
            if (!this.isNowPlayingPageActive || !this.isNowPlayingLyricsPageActive) return;
            if (track == null) return;
            if (this.previousTrack != null && this.previousTrack.Equals(track)) return;

            this.previousTrack = track;

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

                await Task.Run(async () =>
                {
                    // Try to get lyrics from the audio file
                    lyrics = new Lyrics(fmd != null && fmd.Lyrics.Value != null ? fmd.Lyrics.Value : String.Empty, string.Empty);
                    lyrics.SourceType = SourceTypeEnum.Audio;

                    // If the audio file has no lyrics, try to find lyrics in a local lyrics file.
                    if (!lyrics.HasText)
                    {
                        var lrcFile = Path.Combine(Path.GetDirectoryName(fmd.Path), Path.GetFileNameWithoutExtension(fmd.Path) + FileFormats.LRC);

                        if (File.Exists(lrcFile))
                        {
                            using (var fs = new FileStream(lrcFile, FileMode.Open, FileAccess.Read))
                            {
                                using (var sr = new StreamReader(fs, Encoding.Default))
                                {
                                    lyrics = new Lyrics(await sr.ReadToEndAsync(), String.Empty);
                                    if (lyrics.HasText)
                                    {
                                        lyrics.SourceType = SourceTypeEnum.Lrc;
                                        return;
                                    }
                                }
                            }
                        }

                        // If we still don't have lyrics and the user enabled automatic download of lyrics: try to download them online.
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
                        var factory = new LyricsFactory(SettingsClient.Get<int>("Lyrics", "TimeoutSeconds"), SettingsClient.Get<string>("Lyrics", "Providers"), this.info);
                        lyrics = await factory.GetLyricsAsync(fmd.Artists.Values[0], fmd.Title.Value);
                        lyrics.SourceType = SourceTypeEnum.Online;
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Could not get lyrics online {0}. Exception: {1}", track.Path, ex.Message);
                    }

                    this.IsDownloadingLyrics = false;
                }

                await Task.Run(() =>
                            {
                                this.LyricsViewModel = new LyricsViewModel(container, track);
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

        protected override void SearchOnline(string id)
        {
            // No implementation required here
        }
    }
}