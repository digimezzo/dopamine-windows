using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Digimezzo.WPFControls.Enums;
using Dopamine.Common.Services.I18n;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Api.Lastfm;
using Dopamine.Common.Base;
using Dopamine.Common.Database;
using Digimezzo.Utilities.Log;
using Microsoft.Practices.Unity;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class ArtistInfoControlViewModel : BindableBase
    {
        #region Variables
        private IUnityContainer container;
        private ArtistInfoViewModel artistInfoViewModel;
        private IPlaybackService playbackService;
        private II18nService i18nService;
        private Database.Entities.Artist previousArtist;
        private Database.Entities.Artist artist;
        private SlideDirection slideDirection;
        private bool isBusy;
        #endregion

        #region Properties
        public SlideDirection SlideDirection
        {
            get { return this.slideDirection; }
            set { SetProperty<SlideDirection>(ref this.slideDirection, value); }
        }

        public ArtistInfoViewModel ArtistInfoViewModel
        {
            get { return this.artistInfoViewModel; }
            set { SetProperty<ArtistInfoViewModel>(ref this.artistInfoViewModel, value); }
        }

        public bool IsBusy
        {
            get { return this.isBusy; }
            set { SetProperty<bool>(ref this.isBusy, value); }
        }

        #endregion

        #region Construction
        public ArtistInfoControlViewModel(IUnityContainer container, IPlaybackService playbackService, II18nService i18nService)
        {
            this.container = container;
            this.playbackService = playbackService;
            this.i18nService = i18nService;

            this.playbackService.PlaybackSuccess += async (isPlayingPreviousTrack) =>
            {
                this.SlideDirection = isPlayingPreviousTrack ? SlideDirection.RightToLeft : SlideDirection.LeftToRight;
                await this.ShowArtistInfoAsync(this.playbackService.PlayingTrack, false);
            };

            this.i18nService.LanguageChanged += async (_, __) =>
            {
                if (this.playbackService.PlayingTrack != null) await this.ShowArtistInfoAsync(this.playbackService.PlayingTrack, true);
            };

            // Defaults
            this.SlideDirection = SlideDirection.LeftToRight; 
            this.ShowArtistInfoAsync(this.playbackService.PlayingTrack, true);
        }
        #endregion

        #region Private
        private async Task ShowArtistInfoAsync(PlayableTrack track, bool forceReload)
        {
            this.previousArtist = this.artist;

            // User doesn't want to download artist info, or no track is selected.
            if (!SettingsClient.Get<bool>("Lastfm", "DownloadArtistInformation") || track == null)
            {
                this.ArtistInfoViewModel = this.container.Resolve<ArtistInfoViewModel>();
                this.artist = null;
                return;
            }

            // Artist name is unknown
            if (track.ArtistName == Defaults.UnknownArtistString)
            {
                ArtistInfoViewModel localArtistInfoViewModel = this.container.Resolve<ArtistInfoViewModel>();
                await localArtistInfoViewModel.SetLastFmArtistAsync(new Common.Api.Lastfm.Artist { Name = Defaults.UnknownArtistString });
                this.ArtistInfoViewModel = localArtistInfoViewModel;
                this.artist = null;
                return;
            }

            this.artist = new Common.Database.Entities.Artist
            {
                ArtistName = track.ArtistName
            };

            // The artist didn't change: leave the previous artist info.
            if (this.artist.Equals(this.previousArtist) & !forceReload) return;

            // The artist changed: we need to show new artist info.
            string artworkPath = string.Empty;

            this.IsBusy = true;

            try
            {
                Common.Api.Lastfm.Artist lfmArtist = await LastfmApi.ArtistGetInfo(track.ArtistName, true, ResourceUtils.GetStringResource("Language_ISO639-1"));

                if (lfmArtist != null)
                {
                    if (string.IsNullOrEmpty(lfmArtist.Biography.Content))
                    {
                        // In case there is no localized Biography, get the English one.
                        lfmArtist = await LastfmApi.ArtistGetInfo(track.ArtistName, true, "EN");
                    }

                    if (lfmArtist != null)
                    {
                        ArtistInfoViewModel localArtistInfoViewModel = this.container.Resolve<ArtistInfoViewModel>();
                        await localArtistInfoViewModel.SetLastFmArtistAsync(lfmArtist);
                        this.ArtistInfoViewModel = localArtistInfoViewModel;
                    }
                    else
                    {
                        throw new Exception("lfmArtist == null");
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not show artist information for Track {0}. Exception: {1}", track.Path, ex.Message);
                this.ArtistInfoViewModel = this.container.Resolve<ArtistInfoViewModel>();
                this.artist = null;
            }

            this.IsBusy = false;
        }
        #endregion
    }
}
