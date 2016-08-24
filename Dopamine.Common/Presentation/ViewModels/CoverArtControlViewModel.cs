using Digimezzo.WPFControls.Enums;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Database;
using Dopamine.Core.Logging;
using Dopamine.Core.Utils;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class CoverArtControlViewModel : BindableBase
    {
        #region Variables
        protected CoverArtViewModel coverArtViewModel;
        protected IPlaybackService playbackService;
        private SlideDirection slideDirection;
        #endregion

        #region Properties
        public CoverArtViewModel CoverArtViewModel
        {
            get { return this.coverArtViewModel; }
            set { SetProperty<CoverArtViewModel>(ref this.coverArtViewModel, value); }
        }

        public SlideDirection SlideDirection
        {
            get { return this.slideDirection; }
            set { SetProperty<SlideDirection>(ref this.slideDirection, value); }
        }
        #endregion

        #region Construction
        public CoverArtControlViewModel(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;

            this.playbackService.PlaybackFailed += (_, __) => this.ShowCoverArtAsync(null);
            this.playbackService.PlaybackStopped += (_, __) => this.ShowCoverArtAsync(null);


            this.playbackService.PlaybackSuccess += (isPlayingPreviousTrack) =>
            {
                if (isPlayingPreviousTrack)
                {
                    this.SlideDirection = SlideDirection.UpToDown;
                }
                else
                {
                    this.SlideDirection = SlideDirection.DownToUp;
                }

                this.ShowCoverArtAsync(this.playbackService.PlayingTrack);
            };

            // If PlaybackService.PlayingTrackInfo is Nothing, nothing is shown. This is handled in ShowCoverArtAsync.
            // If it is not nothing, the cover for the currently playing track is shown when this screen is created.
            // If we didn't call this function here, we would have to wait until the next PlaybackService.PlaybackSuccess 
            // before seeing any cover.
            this.ShowCoverArtAsync(this.playbackService.PlayingTrack);

            // The default SlideDirection
            this.SlideDirection = SlideDirection.DownToUp;
        }
        #endregion

        #region Virtual
        protected async virtual void ShowCoverArtAsync(TrackInfo trackInfo)
        {
            if (trackInfo == null)
            {
                this.CoverArtViewModel = new CoverArtViewModel { CoverArt = null };
                return;
            }

            string artworkPath = string.Empty;

            await Task.Run(() =>
            {
                artworkPath = ArtworkUtils.GetArtworkPath(trackInfo.AlbumArtworkID);
            });

            if (string.IsNullOrEmpty(artworkPath))
            {
                this.CoverArtViewModel = new CoverArtViewModel { CoverArt = null };
                return;
            }

            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var proxyImage = new Image();
                    proxyImage.Stretch = Stretch.Fill;

                    BitmapImage bmpImage = new BitmapImage();
                    bmpImage.BeginInit();
                    bmpImage.UriSource = new Uri(artworkPath);
                    bmpImage.EndInit();

                    proxyImage.Source = bmpImage;
                    this.CoverArtViewModel = new CoverArtViewModel { CoverArt = proxyImage };
                });
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not show cover art for Track {0}. Exception: {1}", trackInfo.Path, ex.Message);
                this.CoverArtViewModel = new CoverArtViewModel { CoverArt = null };
            }
        }
        #endregion
    }
}