using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Digimezzo.WPFControls.Enums;
using Dopamine.Common.Api.Lastfm;
using Dopamine.Common.Presentation.ViewModels.Entities;
using Dopamine.Common.Services.I18n;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Base;
using Dopamine.Data;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class ArtistInfoControlViewModel : BindableBase
    {
        private IUnityContainer container;
        private ArtistInfoViewModel artistInfoViewModel;
        private IPlaybackService playbackService;
        private II18nService i18nService;
        private Data.Entities.Artist previousArtist;
        private Data.Entities.Artist artist;
        private SlideDirection slideDirection;
        private bool isBusy;

        public DelegateCommand<string> OpenLinkCommand { get; set; }

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

        public ArtistInfoControlViewModel(IUnityContainer container, IPlaybackService playbackService, II18nService i18nService)
        {
            this.container = container;
            this.playbackService = playbackService;
            this.i18nService = i18nService;

            this.OpenLinkCommand = new DelegateCommand<string>((url) =>
            {
                try
                {
                    Actions.TryOpenLink(url);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not open link {0}. Exception: {1}", url, ex.Message);
                }
            });

            this.playbackService.PlaybackSuccess += async (_, e) =>
            {
                this.SlideDirection = e.IsPlayingPreviousTrack ? SlideDirection.RightToLeft : SlideDirection.LeftToRight;
                await this.ShowArtistInfoAsync(this.playbackService.CurrentTrack.Value, false);
            };

            this.i18nService.LanguageChanged += async (_, __) =>
            {
                if (this.playbackService.HasCurrentTrack) await this.ShowArtistInfoAsync(this.playbackService.CurrentTrack.Value, true);
            };

            // Defaults
            this.SlideDirection = SlideDirection.LeftToRight;
            this.ShowArtistInfoAsync(this.playbackService.CurrentTrack.Value, true);
        }

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
            if (track.ArtistName == Defaults.UnknownArtistText)
            {
                ArtistInfoViewModel localArtistInfoViewModel = this.container.Resolve<ArtistInfoViewModel>();
                await localArtistInfoViewModel.SetLastFmArtistAsync(new Common.Api.Lastfm.Artist { Name = Defaults.UnknownArtistText });
                this.ArtistInfoViewModel = localArtistInfoViewModel;
                this.artist = null;
                return;
            }

            this.artist = new Data.Entities.Artist
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
                Common.Api.Lastfm.Artist lfmArtist = await LastfmApi.ArtistGetInfo(track.ArtistName, true, ResourceUtils.GetString("Language_ISO639-1"));

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
    }
}
