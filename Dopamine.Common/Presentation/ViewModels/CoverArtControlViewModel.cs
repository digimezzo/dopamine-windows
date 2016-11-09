using Digimezzo.WPFControls.Enums;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.IO;
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
        private ICacheService cacheService;
        private SlideDirection slideDirection;
        private Album previousAlbum;
        private Album album;
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
        public CoverArtControlViewModel(IPlaybackService playbackService, ICacheService cacheService)
        {
            this.playbackService = playbackService;
            this.cacheService = cacheService;

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

            if (this.playbackService.IsPlaying)
            {
                this.ShowCoverArtAsync(this.playbackService.PlayingTrack);
                
            }else
            {
                this.ShowCoverArtAsync(this.playbackService.FirstQueuedTrack);
            }

            // The default SlideDirection
            this.SlideDirection = SlideDirection.DownToUp;
        }
        #endregion

        #region Virtual
        protected async virtual void ShowCoverArtAsync(TrackInfo trackInfo)
        {
            this.previousAlbum = this.album;

            // No track selected: clear cover art.
            if (trackInfo == null)
            {
                this.CoverArtViewModel = new CoverArtViewModel { CoverArt = null };
                this.album = null;
                return;
            }

            this.album = new Album
            {
                AlbumArtist = trackInfo.AlbumArtist,
                AlbumTitle = trackInfo.AlbumTitle,
                Year = trackInfo.AlbumYear,
                ArtworkID = trackInfo.AlbumArtworkID,
            };

            // The album didn't change: leave the previous covert art.
            if (this.album.Equals(this.previousAlbum)) return;

            // The album changed: we need to show new cover art.
            string artworkPath = string.Empty;

            await Task.Run(() =>
            {
                artworkPath = this.cacheService.GetCachedArtworkPath(trackInfo.AlbumArtworkID);
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
                    proxyImage.Source = ImageOperations.PathToBitmapImage(artworkPath, 0, 0);
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