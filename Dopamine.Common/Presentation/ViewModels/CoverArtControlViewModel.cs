using Digimezzo.WPFControls.Enums;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Database;
using Dopamine.Core.IO;
using Dopamine.Core.Logging;
using Dopamine.Core.Metadata;
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
        private int previousArtworkSize;
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

            // If PlaybackService.PlayingTrack is null, nothing is shown. This is handled in ShowCoverArtAsync.
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
            // No track selected: clear cover art.
            if (trackInfo == null)
            {
                this.CoverArtViewModel = new CoverArtViewModel { CoverArt = null };
                return;
            }

            // 1. Try to find File artwork
            byte[] fileArtWorkData = null;

            await Task.Run(() =>
            {
                var fmd = new FileMetadata(trackInfo.Path);
                fileArtWorkData = fmd.ArtworkData.Value;
            });

            if(fileArtWorkData != null)
            {
                // If the new artwork has the same byte size as the previous artwork, it is most likely the same.
                if (fileArtWorkData.Length == previousArtworkSize) return;

                previousArtworkSize = fileArtWorkData.Length;

                try
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var proxyImage = new Image();
                        proxyImage.Stretch = Stretch.Fill;
                        proxyImage.Source = ImageOperations.ByteToBitmapImage(fileArtWorkData, 0, 0);
                        this.CoverArtViewModel = new CoverArtViewModel { CoverArt = proxyImage };
                    });
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not show file artwork for Track {0}. Exception: {1}", trackInfo.Path, ex.Message);
                    this.CoverArtViewModel = new CoverArtViewModel { CoverArt = null };
                }

                return;
            }

            // 2. If no file artwork was found, try to find album artwork.
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
                byte[] albumArtWorkData = null;

                await Task.Run(() =>
                {
                    albumArtWorkData = ImageOperations.Image2ByteArray(artworkPath);
                });
                
                if (albumArtWorkData != null)
                {
                    // If the new artwork has the same byte size as the previous artwork, it is most likely the same.
                    if (albumArtWorkData.Length == previousArtworkSize) return;

                    previousArtworkSize = albumArtWorkData.Length;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var proxyImage = new Image();
                        proxyImage.Stretch = Stretch.Fill;
                        proxyImage.Source = ImageOperations.ByteToBitmapImage(albumArtWorkData, 0, 0);
                        this.CoverArtViewModel = new CoverArtViewModel { CoverArt = proxyImage };
                    });
                }
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not show album artwork for Track {0}. Exception: {1}", trackInfo.Path, ex.Message);
                this.CoverArtViewModel = new CoverArtViewModel { CoverArt = null };
            }
        }
        #endregion
    }
}