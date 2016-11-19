using Digimezzo.WPFControls.Enums;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.IO;
using Dopamine.Core.Logging;
using Prism.Mvvm;
using System;
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
        private IMetadataService metadataService;
        private SlideDirection slideDirection;
        private Track previousTrack;
        private Track track;
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

        #region Private
        private void ClearArtwork()
        {
            this.CoverArtViewModel = new CoverArtViewModel { CoverArt = null };
        }
        #endregion

        #region Construction
        public CoverArtControlViewModel(IPlaybackService playbackService, ICacheService cacheService, IMetadataService metadataService)
        {
            this.playbackService = playbackService;
            this.cacheService = cacheService;
            this.metadataService = metadataService;

            this.playbackService.PlaybackSuccess += (isPlayingPreviousTrack) =>
            {
                this.SlideDirection = isPlayingPreviousTrack ? SlideDirection.UpToDown : SlideDirection.DownToUp;
                this.RefreshCoverArtAsync(this.playbackService.PlayingTrack, false);
            };

            this.playbackService.PlayingTrackChanged += (_, __) => this.RefreshCoverArtAsync(this.playbackService.PlayingTrack, true);

            // Defaults
            this.SlideDirection = SlideDirection.DownToUp;
            this.RefreshCoverArtAsync(this.playbackService.PlayingTrack, false);
        }
        #endregion

        #region Virtual
        protected async virtual void RefreshCoverArtAsync(MergedTrack track, bool allowRefreshingCurrentTrack)
        {
            this.previousTrack = this.track;

            // No track selected: clear cover art.
            if (track == null)
            {
                this.ClearArtwork();
                return;
            }

            this.track = track;

            // The track didn't change: leave the previous playback info.
            if (!allowRefreshingCurrentTrack & this.track.Equals(this.previousTrack)) return;

            // 1. Try to find File artwork
            byte[] artWork = await this.metadataService.GetArtworkAsync(track.Path);

            if (artWork != null)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var proxyImage = new Image();
                        proxyImage.Stretch = Stretch.Fill;
                        proxyImage.Source = ImageOperations.ByteToBitmapImage(artWork, 0, 0);
                        this.CoverArtViewModel = new CoverArtViewModel { CoverArt = proxyImage };
                    });
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not show file artwork for Track {0}. Exception: {1}", track.Path, ex.Message);
                    this.ClearArtwork();
                }

                return;
            }
            else
            {
                this.ClearArtwork();
                return;
            }
        }
    }
    #endregion
}