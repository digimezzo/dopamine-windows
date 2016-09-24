using Dopamine.Common.Services.I18n;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.API.Lastfm;
using Dopamine.Core.Base;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Logging;
using Dopamine.Core.Utils;
using Prism.Mvvm;
using System;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class ArtistInfoControlViewModel : BindableBase
    {
        #region Variables
        private ArtistInfoViewModel artistInfoViewModel;
        private IPlaybackService playbackService;
        private II18nService i18nService;
        private Artist previousArtist;
        private Artist artist;
        #endregion

        #region Properties
        public ArtistInfoViewModel ArtistInfoViewModel
        {
            get { return this.artistInfoViewModel; }
            set { SetProperty<ArtistInfoViewModel>(ref this.artistInfoViewModel, value); }
        }
        #endregion

        #region Construction
        public ArtistInfoControlViewModel(IPlaybackService playbackService, II18nService i18nService)
        {
            this.playbackService = playbackService;
            this.i18nService = i18nService;

            this.playbackService.PlaybackFailed += (_, __) => this.ShowArtistInfoAsync(null, false);
            this.playbackService.PlaybackStopped += (_, __) => this.ShowArtistInfoAsync(null, false);
            this.playbackService.PlaybackSuccess += (_) => this.ShowArtistInfoAsync(this.playbackService.PlayingTrack, false);

            this.i18nService.LanguageChanged += (_, __) =>
            {
                if (this.playbackService.PlayingTrack != null) this.ShowArtistInfoAsync(this.playbackService.PlayingTrack, true);
            };

            // If PlaybackService.PlayingTrack is null, nothing is shown. This is handled in ShowArtistInfoAsync.
            // If it is not nothing, the cover for the currently playing track is shown when this screen is created.
            // If we didn't call this function here, we would have to wait until the next PlaybackService.PlaybackSuccess 
            // before seeing any artist info.
            this.ShowArtistInfoAsync(this.playbackService.PlayingTrack, false);
        }
        #endregion

        #region Private
        protected async virtual void ShowArtistInfoAsync(TrackInfo trackInfo, bool forceReload)
        {
            this.previousArtist = this.artist;

            // No track selected: clear artist info.
            if (trackInfo == null || trackInfo.ArtistName == Defaults.UnknownArtistString)
            {
                this.ArtistInfoViewModel = new ArtistInfoViewModel { LfmArtist = null };
                this.artist = null;
                return;
            }

            this.artist = new Artist
            {
                ArtistName = this.playbackService.PlayingTrack.ArtistName
            };

            // The artist didn't change: leave the previous artist info.
            if (this.artist.Equals(this.previousArtist) & !forceReload) return;

            // The artist changed: we need to show new artist info.
            string artworkPath = string.Empty;

            try
            {
                LastFmArtist lfmArtist = await LastfmAPI.ArtistGetInfo(this.playbackService.PlayingTrack.ArtistName, ResourceUtils.GetStringResource("Language_ISO639-1"));

                if (lfmArtist != null)
                {
                    if (string.IsNullOrEmpty(lfmArtist.Biography.Content))
                    {
                        // In case there is no localized Biography, get the English one.
                        lfmArtist = await LastfmAPI.ArtistGetInfo(this.playbackService.PlayingTrack.ArtistName, "EN");
                    }

                    if (lfmArtist != null)
                    {
                        this.ArtistInfoViewModel = new ArtistInfoViewModel { LfmArtist = lfmArtist };
                    }

                }
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not show artist information for Track {0}. Exception: {1}", trackInfo.Path, ex.Message);
                this.ArtistInfoViewModel = new ArtistInfoViewModel { LfmArtist = null };
            }
        }
        #endregion
    }
}
