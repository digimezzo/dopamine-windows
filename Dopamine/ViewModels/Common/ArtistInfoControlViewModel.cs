using Digimezzo.Foundation.Core.IO;
using Digimezzo.Foundation.Core.Logging;
using Dopamine.Core.Alex;  //Digimezzo.Foundation.Core.Settings
using Digimezzo.Foundation.Core.Utils;
using Digimezzo.Foundation.WPF.Controls;
using Dopamine.Core.Api.Fanart;
using Dopamine.Core.Api.Lastfm;
using Dopamine.Services.Entities;
using Dopamine.Services.I18n;
using Dopamine.Services.Playback;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.Common
{
    public class ArtistInfoControlViewModel : BindableBase
    {
        private IContainerProvider container;
        private ArtistInfoViewModel artistInfoViewModel;
        private IPlaybackService playbackService;
        private II18nService i18nService;
        private string previousArtistName;
        private string artistName;
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

        public ArtistInfoControlViewModel(IContainerProvider container, IPlaybackService playbackService, II18nService i18nService)
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
                await this.ShowArtistInfoAsync(this.playbackService.CurrentTrack, false);
            };

            this.i18nService.LanguageChanged += async (_, __) =>
            {
                if (this.playbackService.HasCurrentTrack) await this.ShowArtistInfoAsync(this.playbackService.CurrentTrack, true);
            };

            // Defaults
            this.SlideDirection = SlideDirection.LeftToRight;
            this.ShowArtistInfoAsync(this.playbackService.CurrentTrack, true);
        }

        private async Task ShowArtistInfoAsync(TrackViewModel track, bool forceReload)
        {
            this.previousArtistName = this.artistName;

            // User doesn't want to download artist info, or no track is selected.
            if (!SettingsClient.Get<bool>("Lastfm", "DownloadArtistInformation") || track == null)
            {
                this.ArtistInfoViewModel = this.container.Resolve<ArtistInfoViewModel>();
                this.artistName = string.Empty;
                return;
            }

            // Artist name is unknown
            if (string.IsNullOrEmpty(track.ArtistName))
            {
                ArtistInfoViewModel localArtistInfoViewModel = this.container.Resolve<ArtistInfoViewModel>();
                await localArtistInfoViewModel.SetArtistInformation(new LastFmArtist { Name = string.Empty }, string.Empty);
                this.ArtistInfoViewModel = localArtistInfoViewModel;
                this.artistName = string.Empty;
                return;
            }

            this.artistName = track.ArtistName;

            // The artist didn't change: leave the previous artist info.
            if (this.artistName.Equals(this.previousArtistName) & !forceReload)
            {
                return;
            }

            // The artist changed: we need to show new artist info.
            string artworkPath = string.Empty;

            this.IsBusy = true;

            try
            {
                LastFmArtist lfmArtist = await LastfmApi.ArtistGetInfo(track.ArtistName, true, ResourceUtils.GetString("Language_ISO639-1"));

                if (lfmArtist != null)
                {
                    if (string.IsNullOrEmpty(lfmArtist.Biography.Content))
                    {
                        // In case there is no localized Biography, get the English one.
                        lfmArtist = await LastfmApi.ArtistGetInfo(track.ArtistName, true, "EN");
                    }

                    if (lfmArtist != null)
                    {
                        string artistImageUrl = string.Empty;

                        try
                        {
                            // Last.fm was so nice to break their artist image API. So we need to get images from elsewhere.  
                            artistImageUrl = await FanartApi.GetArtistThumbnailAsync(lfmArtist.MusicBrainzId);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Warning($"Could not get artist image from Fanart for artist {track.ArtistName}. Exception: {ex}");
                        }

                        ArtistInfoViewModel localArtistInfoViewModel = this.container.Resolve<ArtistInfoViewModel>();
                        await localArtistInfoViewModel.SetArtistInformation(lfmArtist, artistImageUrl);
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
                this.artistName = string.Empty;
            }

            this.IsBusy = false;
        }
    }
}
