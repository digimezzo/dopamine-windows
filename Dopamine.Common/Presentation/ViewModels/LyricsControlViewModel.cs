using Dopamine.Common.Services.I18n;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Database;
using Dopamine.Core.Logging;
using Dopamine.Core.Metadata;
using Dopamine.Core.Utils;
using Prism.Mvvm;
using System;

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
        public LyricsControlViewModel(IPlaybackService playbackService, II18nService i18nService)
        {
            this.playbackService = playbackService;
            this.i18nService = i18nService;

            this.playbackService.PlaybackFailed += (_, __) => this.ShowLyricsAsync(null);
            this.playbackService.PlaybackStopped += (_, __) => this.ShowLyricsAsync(null);

            this.playbackService.PlaybackSuccess += (isPlayingPreviousTrack) =>
            {
                this.ContentSlideInFrom = isPlayingPreviousTrack ? 30 : -30;
                
                this.ShowLyricsAsync(this.playbackService.PlayingTrack);
            };

            this.i18nService.LanguageChanged += (_, __) => this.ShowLyricsAsync(this.playbackService.PlayingTrack);

            this.ShowLyricsAsync(this.playbackService.PlayingTrack);
        }
        #endregion

        #region Private
        private async void ShowLyricsAsync(TrackInfo trackInfo)
        {
            this.previousTrackInfo = this.trackInfo;

            // No track selected: clear playback info.
            if (trackInfo == null)
            {
                this.LyricsViewModel = new LyricsViewModel(string.Empty);
                this.trackInfo = null;
                return;
            }

            this.trackInfo = trackInfo;

            // The track didn't change: leave the previous lyrics.
            if (this.trackInfo.Equals(this.previousTrackInfo)) return;

            // The track changed: we need to show new lyrics.
            try
            {
                var fmd = new FileMetadata(trackInfo.Path);

                this.LyricsViewModel = new LyricsViewModel(string.IsNullOrWhiteSpace(trackInfo.TrackTitle) ? trackInfo.FileName : trackInfo.TrackTitle);
                await this.LyricsViewModel.SetLyricsAsync(string.IsNullOrWhiteSpace(fmd.Lyrics.Value) ? ResourceUtils.GetStringResource("Language_No_Lyrics") : fmd.Lyrics.Value);
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not show lyrics for Track {0}. Exception: {1}", trackInfo.Path, ex.Message);
                this.LyricsViewModel = new LyricsViewModel(string.Empty);
            }
        }
        #endregion
    }
}