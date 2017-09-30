using Digimezzo.Utilities.Settings;
using Dopamine.Common.Base;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace Dopamine.Common.Services.Notification
{
    public class NotificationService : LegacyNotificationService
    {
        #region Variables
        private bool systemNotificationIsEnabled;
        private SystemMediaTransportControls systemMediaControls;
        private SystemMediaTransportControlsDisplayUpdater displayUpdater;
        private MusicDisplayProperties musicProperties;
        private InMemoryRandomAccessStream artworkStream;
        #endregion

        #region Properties
        public override bool SystemNotificationIsEnabled
        {
            get => this.systemNotificationIsEnabled;
            set
            {
                this.systemNotificationIsEnabled = value;
                SettingsClient.Set("Behaviour", "EnableSystemNotification", value);
                Application.Current.Dispatcher.InvokeAsync(async () => await SwitchNotificationHandlerAsync(value));
            }
        }
        #endregion

        #region Construction
        public NotificationService(IPlaybackService playbackService, ICacheService cacheService, IMetadataService metadataService) : base(playbackService, cacheService, metadataService)
        {
            // Pay attention to UPPERCASE property
            this.SystemNotificationIsEnabled = SettingsClient.Get<bool>("Behaviour", "EnableSystemNotification");

            systemMediaControls = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
            displayUpdater = systemMediaControls.DisplayUpdater;
            displayUpdater.Type = MediaPlaybackType.Music;
            musicProperties = displayUpdater.MusicProperties;
            systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Closed;
            displayUpdater.Update();
        }
        #endregion

        #region Overrides
        protected override bool CanShowNotification()
        {
            if (this.systemNotificationIsEnabled) return false;
            return base.CanShowNotification();
        }
        #endregion

        #region Private
        private async void SMCButtonPressed(SystemMediaTransportControls sender,
            SystemMediaTransportControlsButtonPressedEventArgs e)
        {
            switch (e.Button)
            {
                case SystemMediaTransportControlsButton.Previous:
                    await this.PlaybackService.PlayPreviousAsync();
                    break;
                case SystemMediaTransportControlsButton.Next:
                    await this.PlaybackService.PlayNextAsync();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    await this.PlaybackService.PlayOrPauseAsync();
                    break;
                case SystemMediaTransportControlsButton.Play:
                    await this.PlaybackService.PlayOrPauseAsync();
                    break;
                default:
                    // Never happens
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PlaybackResumedSystemNotificationHandler(object _, EventArgs __)
        {
            systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Playing;
            displayUpdater.Update();
        }

        private void PlaybackPausedSystemNotificationHandler(object _, EventArgs __)
        {
            systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Paused;
            displayUpdater.Update();
        }

        private async void PlaybackSuccessSystemNotificationHandler(bool _)
        {
            systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Playing;
            var track = this.PlaybackService.CurrentTrack.Value;
            musicProperties.AlbumArtist = track.AlbumArtist;
            musicProperties.AlbumTitle = track.AlbumTitle;
            musicProperties.Artist = track.ArtistName;
            musicProperties.Title = track.TrackTitle;
            musicProperties.TrackNumber = Convert.ToUInt32(track.TrackNumber);
            await SetArtworkThumbnailAsync(await this.MetadataService.GetArtworkAsync(track.Path));
            displayUpdater.Update();
        }

        private async Task SwitchNotificationHandlerAsync(bool systemNotificationIsEnabled)
        {
            await Task.Run(() =>
            {
                if (systemNotificationIsEnabled)
                {
                    // We can safely unsubscribe event handlers before subscribing them
                    // See https://msdn.microsoft.com/en-us/library/system.delegate.remove.aspx
                    this.PlaybackService.PlaybackSuccess -= this.PlaybackSuccessHandler;
                    this.PlaybackService.PlaybackPaused -= this.PlaybackPausedHandler;
                    this.PlaybackService.PlaybackResumed -= this.PlaybackResumedHandler;

                    if (Constants.IsWindows10)
                    {
                        systemMediaControls.IsEnabled = true;
                        systemMediaControls.IsPlayEnabled = true;
                        systemMediaControls.IsPauseEnabled = true;
                        systemMediaControls.IsPreviousEnabled = true;
                        systemMediaControls.IsNextEnabled = true;
                        systemMediaControls.ShuffleEnabled = false;
                        systemMediaControls.IsRewindEnabled = false;
                        systemMediaControls.IsFastForwardEnabled = false;
                        systemMediaControls.IsRecordEnabled = false;
                        systemMediaControls.IsStopEnabled = false;
                        systemMediaControls.ButtonPressed += SMCButtonPressed;

                        this.PlaybackService.PlaybackSuccess += this.PlaybackSuccessSystemNotificationHandler;
                        this.PlaybackService.PlaybackPaused += this.PlaybackPausedSystemNotificationHandler;
                        this.PlaybackService.PlaybackResumed += this.PlaybackResumedSystemNotificationHandler;
                    }
                }
                else
                {
                    this.PlaybackService.PlaybackSuccess += this.PlaybackSuccessHandler;
                    this.PlaybackService.PlaybackPaused += this.PlaybackPausedHandler;
                    this.PlaybackService.PlaybackResumed += this.PlaybackResumedHandler;

                    if (Constants.IsWindows10)
                    {
                        systemMediaControls.IsEnabled = false;
                        systemMediaControls.ButtonPressed -= SMCButtonPressed;

                        this.PlaybackService.PlaybackSuccess -= this.PlaybackSuccessSystemNotificationHandler;
                        this.PlaybackService.PlaybackPaused -= this.PlaybackPausedSystemNotificationHandler;
                        this.PlaybackService.PlaybackResumed -= this.PlaybackResumedSystemNotificationHandler;
                    }
                }
            });
        }

        private async Task SetArtworkThumbnailAsync(byte[] data)
        {
            artworkStream?.Dispose();
            if (data == null)
            {
                artworkStream = null;
                displayUpdater.Thumbnail = null;
            }
            else
            {
                artworkStream = new InMemoryRandomAccessStream();
                await artworkStream.WriteAsync(data.AsBuffer());
                displayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromStream(artworkStream);
            }
        }
        #endregion
    }
}
