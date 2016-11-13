using Digimezzo.WPFControls.Enums;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
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
        private ITrackRepository trackRepository;
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
        public CoverArtControlViewModel(IPlaybackService playbackService, ICacheService cacheService, ITrackRepository trackRepository)
        {
            this.playbackService = playbackService;
            this.cacheService = cacheService;
            this.trackRepository = trackRepository;

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

                this.ShowCoverArtAsync(this.playbackService.PlayingPath);
            };

            this.ShowCoverArtAsync(this.playbackService.PlayingPath);

            // The default SlideDirection
            this.SlideDirection = SlideDirection.DownToUp;
        }
        #endregion

        #region Private
        private void ClearCoverArt()
        {
            this.CoverArtViewModel = new CoverArtViewModel { CoverArt = null };
            this.album = null;
        }
        #endregion

        #region Virtual
        protected async virtual void ShowCoverArtAsync(string filename)
        {
            this.previousAlbum = this.album;

            // No track selected: clear cover art.
            if (filename == null)
            {
                this.ClearCoverArt();
                return;
            }

            // Get the track from the database
            MergedTrack mergedTrack = await this.trackRepository.GetMergedTrackAsync(filename);

            if(mergedTrack == null)
            {
                LogClient.Instance.Logger.Error("Track not found in the database for path: {0}", filename);
                this.ClearCoverArt();
                return;
            }

            this.album = new Album
            {
                AlbumArtist = mergedTrack.AlbumArtist,
                AlbumTitle = mergedTrack.AlbumTitle,
                Year = mergedTrack.AlbumYear,
                ArtworkID = mergedTrack.AlbumArtworkID,
            };

            // The album didn't change: leave the previous covert art.
            if (this.album.Equals(this.previousAlbum)) return;

            // The album changed: we need to show new cover art.
            string artworkPath = string.Empty;

            await Task.Run(() =>
            {
                artworkPath = this.cacheService.GetCachedArtworkPath(mergedTrack.AlbumArtworkID);
            });

            if (string.IsNullOrEmpty(artworkPath))
            {
                this.ClearCoverArt();
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
                LogClient.Instance.Logger.Error("Could not show cover art for Track {0}. Exception: {1}", filename, ex.Message);
                this.ClearCoverArt();
            }
        }
        #endregion
    }
}