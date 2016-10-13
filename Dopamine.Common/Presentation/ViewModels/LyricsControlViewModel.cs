using System;
using Dopamine.Common.Services.Playback;
using Prism.Mvvm;
using Dopamine.Core.Database;
using System.Threading.Tasks;
using Dopamine.Core.Metadata;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class LyricsControlViewModel : BindableBase
    {
        #region Variables
        private IPlaybackService playbackService;
        private string title;
        private string staticLyrics;
        #endregion

        #region Properties
        public string Title
        {
            get { return this.title; }
            set { SetProperty<string>(ref this.title, value); }
        }

        public string StaticLyrics
        {
            get { return this.staticLyrics; }
            set { SetProperty<string>(ref this.staticLyrics, value); }
        }
        #endregion

        #region Construction
        public LyricsControlViewModel(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;

            this.playbackService.PlaybackFailed += (_, __) => this.ShowLyricsAsync(null);
            this.playbackService.PlaybackStopped += (_, __) => this.ShowLyricsAsync(null);

            this.playbackService.PlaybackSuccess += (isPlayingPreviousTrack) =>
            {
                this.ShowLyricsAsync(this.playbackService.PlayingTrack);
            };

            this.ShowLyricsAsync(this.playbackService.PlayingTrack);
        }
        #endregion

        #region Private
        private async void ShowLyricsAsync(TrackInfo trackInfo)
        {
            if (trackInfo == null) return;

            await Task.Run(() =>
            {
                var fmd = new FileMetadata(trackInfo.Path);

                this.Title = trackInfo.TrackTitle;
                this.StaticLyrics = string.IsNullOrWhiteSpace(fmd.Lyrics.Value) ? "Not available" : fmd.Lyrics.Value;
            });
        }
        #endregion
    }
}
