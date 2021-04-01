using Digimezzo.Foundation.Core.Logging;
using Digimezzo.Foundation.Core.Settings;
using Digimezzo.Foundation.Core.Utils;
using DiscordRPC;
using Dopamine.Core.Base;
using Dopamine.Services.Playback;
using System;

namespace Dopamine.Services.Discord
{
    public class RichPresenceService : IRichPresenceService
    {
        private DiscordRpcClient client;
        private IPlaybackService playbackService;

        public RichPresenceService(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;

            if (SettingsClient.Get<bool>("Discord", "EnableDiscordRichPresence"))
            {
                this.RegisterClient();
            }

            SettingsClient.SettingChanged += (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "Discord", "EnableDiscordRichPresence"))
                {
                    bool enableDiscordRichPresence = (bool)e.Entry.Value;

                    if (enableDiscordRichPresence)
                    {
                        this.RegisterClient();
                        this.ShowTrackDetails();
                    }
                    else
                    {
                        this.UnregisterClient();
                    }
                }
            };
        }

        /// <summary>
        /// Registers the Discord client and the event handlers.
        /// </summary>
        private void RegisterClient()
        {
            try
            {
                if (this.client == null)
                {
                    this.client = new DiscordRpcClient(SensitiveInformation.DiscordClientId);
                    this.client.Initialize();
                }

                this.playbackService.PlaybackResumed += this.HandleDetailsChanged;
                this.playbackService.PlaybackSkipped += this.HandleDetailsChanged;
                this.playbackService.PlaybackPaused += this.HandleDetailsChanged;
                this.playbackService.PlayingTrackChanged += this.HandleDetailsChanged;
                this.playbackService.PlaybackSuccess += this.HandleDetailsChanged;
                this.playbackService.PlaybackStopped += this.HandleStop;
            }
            catch (Exception ex)
            {
                LogClient.Error($"Could not register cliente. Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Unregisters the Discord client and the event handlers.
        /// </summary>
        private void UnregisterClient()
        {
            try
            {
                this.playbackService.PlaybackResumed -= this.HandleDetailsChanged;
                this.playbackService.PlaybackSkipped -= this.HandleDetailsChanged;
                this.playbackService.PlaybackPaused -= this.HandleDetailsChanged;
                this.playbackService.PlayingTrackChanged -= this.HandleDetailsChanged;
                this.playbackService.PlaybackSuccess -= this.HandleDetailsChanged;
                this.playbackService.PlaybackStopped -= this.HandleStop;

                this.client.ClearPresence();
            }
            catch (Exception ex)
            {
                LogClient.Error($"Could not unregister client. Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows the current track's details.
        /// </summary>
        private void ShowTrackDetails()
        {
            try
            {
                if (this.playbackService.CurrentTrack == null)
                {
                    return;
                }

                RichPresence presence = new RichPresence()
                {
                    Details = this.playbackService.CurrentTrack.TrackTitle,
                    State = $"{ResourceUtils.GetString("Language_Discord_By")} {this.playbackService.CurrentTrack.ArtistName}",
                    Assets = new Assets()
                    {
                        LargeImageKey = "icon",
                        LargeImageText = ResourceUtils.GetString("Language_Discord_Playing_With_Dopamine")
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
                    presence.Assets.SmallImageText = ResourceUtils.GetString("Language_Discord_Playing");
                }

                // If the track is paused, we don't set the timestamps and
                // we show the appropriate icon
                else
                {
                    presence.Assets.SmallImageKey = "pause";
                    presence.Assets.SmallImageText = ResourceUtils.GetString("Language_Discord_Paused");
                }


                this.client.SetPresence(presence);
            }
            catch (Exception ex)
            {
                LogClient.Error($"Could not show track details. Exception: {ex.Message}");
            }

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
            try
            {
                this.client.ClearPresence();
            }
            catch (Exception ex)
            {
                LogClient.Error($"Could not handle stop. Exception: {ex.Message}");
            }
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
