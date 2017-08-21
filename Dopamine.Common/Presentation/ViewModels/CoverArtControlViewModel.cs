using Digimezzo.WPFControls.Enums;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Database;
using Dopamine.Core.Logging;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using System.Timers;

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
        private byte[] previousArtwork;
        private byte[] artwork;
        private Timer refreshTimer = new Timer();
        private int refreshTimerIntervalMilliseconds = 250;
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
            this.artwork = null;
        }
        #endregion

        #region Construction
        public CoverArtControlViewModel(IPlaybackService playbackService, ICacheService cacheService, IMetadataService metadataService)
        {
            this.playbackService = playbackService;
            this.cacheService = cacheService;
            this.metadataService = metadataService;

            this.refreshTimer.Interval = this.refreshTimerIntervalMilliseconds;
            this.refreshTimer.Elapsed += RefreshTimer_Elapsed;

            this.playbackService.PlaybackSuccess += (isPlayingPreviousTrack) =>
            {
                this.SlideDirection = isPlayingPreviousTrack ? SlideDirection.UpToDown : SlideDirection.DownToUp;
                this.refreshTimer.Stop();
                this.refreshTimer.Start();
            };

            this.playbackService.PlayingTrackArtworkChanged += (_, __) => this.RefreshCoverArtAsync(this.playbackService.CurrentTrack.Value);

            // Defaults
            this.SlideDirection = SlideDirection.DownToUp;
            this.RefreshCoverArtAsync(this.playbackService.CurrentTrack.Value);
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.refreshTimer.Stop();
            this.RefreshCoverArtAsync(this.playbackService.CurrentTrack.Value);
        }
        #endregion

        #region Virtual
        protected async virtual void RefreshCoverArtAsync(PlayableTrack track)
        {
            await Task.Run(async () =>
            {
                this.previousArtwork = this.artwork;

                // No track selected: clear cover art.
                if (track == null)
                {
                    this.ClearArtwork();
                    return;
                }

                // Try to find artwork
                byte[] artwork = null;

                try
                {
                    artwork = await this.metadataService.GetArtworkAsync(track.Path);
                }
                catch (Exception ex)
                {
                    CoreLogger.Current.Error("Could not get artwork for Track {0}. Exception: {1}", track.Path, ex.Message);
                }

                this.artwork = artwork;

                // Verify if the artwork changed
                if ((this.artwork != null & this.previousArtwork != null) && (this.artwork.LongLength == this.previousArtwork.LongLength))
                {
                    return;
                }
                else if (this.artwork == null & this.previousArtwork == null & this.CoverArtViewModel != null)
                {
                    return;
                }

                if (artwork != null)
                {
                    try
                    {
                        this.CoverArtViewModel = new CoverArtViewModel { CoverArt = artwork };
                    }
                    catch (Exception ex)
                    {
                        CoreLogger.Current.Error("Could not show file artwork for Track {0}. Exception: {1}", track.Path, ex.Message);
                        this.ClearArtwork();
                    }

                    return;
                }
                else
                {
                    this.ClearArtwork();
                    return;
                }
            });
        }
    }
    #endregion
}