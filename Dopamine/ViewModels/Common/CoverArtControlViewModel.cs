using Digimezzo.Utilities.Log;
using Digimezzo.WPFControls.Enums;
using Dopamine.Data.Entities;
using Dopamine.ViewModels;
using Dopamine.Services.Cache;
using Dopamine.Services.Metadata;
using Dopamine.Services.Playback;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using System.Timers;
using Dopamine.Services.Entities;

namespace Dopamine.ViewModels.Common
{
    public class CoverArtControlViewModel : BindableBase
    {
        protected CoverArtViewModel coverArtViewModel;
        protected IPlaybackService playbackService;
        private ICacheService cacheService;
        private IMetadataService metadataService;
        private SlideDirection slideDirection;
        private byte[] previousArtwork;
        private byte[] artwork;
        private Timer refreshTimer = new Timer();
        private int refreshTimerIntervalMilliseconds = 250;

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

        private void ClearArtwork()
        {
            this.CoverArtViewModel = new CoverArtViewModel { CoverArt = null };
            this.artwork = null;
        }

        public CoverArtControlViewModel(IPlaybackService playbackService, ICacheService cacheService, IMetadataService metadataService)
        {
            this.playbackService = playbackService;
            this.cacheService = cacheService;
            this.metadataService = metadataService;

            this.refreshTimer.Interval = this.refreshTimerIntervalMilliseconds;
            this.refreshTimer.Elapsed += RefreshTimer_Elapsed;

            this.playbackService.PlaybackSuccess += (_, e) =>
            {
                this.SlideDirection = e.IsPlayingPreviousTrack ? SlideDirection.UpToDown : SlideDirection.DownToUp;
                this.refreshTimer.Stop();
                this.refreshTimer.Start();
            };

            this.playbackService.PlayingTrackArtworkChanged += (_, __) => this.RefreshCoverArtAsync(this.playbackService.CurrentTrack);

            // Defaults
            this.SlideDirection = SlideDirection.DownToUp;
            this.RefreshCoverArtAsync(this.playbackService.CurrentTrack);
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.refreshTimer.Stop();
            this.RefreshCoverArtAsync(this.playbackService.CurrentTrack);
        }

        protected async virtual void RefreshCoverArtAsync(TrackViewModel track)
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
                    LogClient.Error("Could not get artwork for Track {0}. Exception: {1}", track.Path, ex.Message);
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
                        LogClient.Error("Could not show file artwork for Track {0}. Exception: {1}", track.Path, ex.Message);
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
}