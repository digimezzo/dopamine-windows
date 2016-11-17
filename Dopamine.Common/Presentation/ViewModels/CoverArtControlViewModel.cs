using Digimezzo.WPFControls.Enums;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.IO;
using Dopamine.Core.Logging;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
                this.SlideDirection = isPlayingPreviousTrack ? SlideDirection.UpToDown : SlideDirection.DownToUp;
                this.RefreshCoverArtAsync(this.playbackService.PlayingTrack,false);
            };

            this.playbackService.PlayingTrackChanged += (_, __) => this.RefreshCoverArtAsync(this.playbackService.PlayingTrack, true);

            // Defaults
            this.SlideDirection = SlideDirection.DownToUp;
            this.RefreshCoverArtAsync(this.playbackService.PlayingTrack,false);
        }
        #endregion

        #region Virtual
        protected async virtual void RefreshCoverArtAsync(MergedTrack track, bool allowRefreshingCurrentTrack)
        {
            this.previousAlbum = this.album;

            // No track selected: clear cover art.
            if (track == null)
            {
                this.CoverArtViewModel = new CoverArtViewModel { CoverArt = null };
                this.album = null;
                return;
            }

            this.album = new Album
            {
                AlbumArtist = track.AlbumArtist,
                AlbumTitle = track.AlbumTitle,
                Year = track.AlbumYear,
                ArtworkID = track.AlbumArtworkID,
            };

            // The album didn't change: leave the previous covert art.
            if (!allowRefreshingCurrentTrack & this.album.Equals(this.previousAlbum)) return;

            // The album changed: we need to show new cover art.
            string artworkPath = string.Empty;

            await Task.Run(() =>
            {
                artworkPath = this.cacheService.GetCachedArtworkPath(track.AlbumArtworkID);
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
                LogClient.Instance.Logger.Error("Could not show cover art for Track {0}. Exception: {1}", track.Path, ex.Message);
                this.CoverArtViewModel = new CoverArtViewModel { CoverArt = null };
            }
        }
        #endregion
    }
}