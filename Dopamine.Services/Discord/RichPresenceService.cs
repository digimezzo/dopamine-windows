using DiscordRPC;
using Dopamine.Services.Playback;
using System;

namespace Dopamine.Services.Discord
{
    public class RichPresenceService : IRichPresenceService
    {
        protected const string ClientApplicationId = "825803781551030314";

        protected DiscordRpcClient client;
        protected IPlaybackService playbackService;

        public RichPresenceService(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;
            this.Initialize();
        }

        /// <summary>
        /// Registers the Discord client and the event handlers.
        /// </summary>
        private void Initialize()
        {
            this.client = new DiscordRpcClient(RichPresenceService.ClientApplicationId);
            this.client.Initialize();
            
            this.playbackService.PlaybackResumed += this.HandleDetailsChanged;
            this.playbackService.PlaybackSkipped += this.HandleDetailsChanged;
            this.playbackService.PlaybackPaused += this.HandleDetailsChanged;
            this.playbackService.PlayingTrackChanged += this.HandleDetailsChanged;
            this.playbackService.PlaybackSuccess += this.HandleDetailsChanged;
            this.playbackService.PlaybackStopped += this.HandleStop;
        }

        /// <summary>
        /// Shows the current track's details.
        /// </summary>
        private void ShowTrackDetails()
        {
            RichPresence presence = new RichPresence()
            {
                Details = this.playbackService.CurrentTrack.TrackTitle,
                State = $"by {this.playbackService.CurrentTrack.ArtistName}",
                Assets = new Assets()
                {
                    LargeImageKey = "icon",
                    LargeImageText = "Playing with Dopamine"
                },
            };

            // If the track is playing, we show its remaining duration
            // and an icon
            if (this.playbackService.IsPlaying)
            {
                // Sets the duration
                presence.WithTimestamps(new Timestamps()
                {
                    Start = DateTime.UtcNow,
                    End = DateTime.UtcNow.AddMilliseconds(this.playbackService.CurrentTrack.Track.Duration.Value - this.playbackService.GetCurrentTime.TotalMilliseconds)
                });

                // Sets the "playing" image
                presence.Assets.SmallImageKey = "play";
                presence.Assets.SmallImageText = "Playing";
            }

            // If the track is paused, we don't set the timestamps and
            // we show the appropriate icon
            else
            {
                presence.Assets.SmallImageKey = "pause";
                presence.Assets.SmallImageText = "Paused";
            }


            this.client.SetPresence(presence);
        }

        /// <summary>
        /// Handles events that change the track's details.
        /// </summary>
        private void HandleDetailsChanged(object sender, EventArgs e)
        {
            this.ShowTrackDetails();
        }

        /// <summary>
        /// Handles events that stop the track.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleStop(object sender, EventArgs e)
        {
            this.client.ClearPresence();
        }

        /// <summary>
        /// Dispose of the Discord client.
        /// </summary>
        ~RichPresenceService()
        {
            this.client.Dispose();
        }
    }
}
