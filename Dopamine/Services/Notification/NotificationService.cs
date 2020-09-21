using Digimezzo.Foundation.Core.Settings;
using Dopamine.Core.Base;
using Dopamine.Services.Cache;
using Dopamine.Services.Metadata;
using Dopamine.Services.Playback;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace Dopamine.Services.Notification
{
    public class NotificationService : LegacyNotificationService
    {
        private bool systemNotificationIsEnabled;
        private SystemMediaTransportControls systemMediaControls;
        private SystemMediaTransportControlsDisplayUpdater displayUpdater;
        private MusicDisplayProperties musicProperties;
        private InMemoryRandomAccessStream artworkStream;

        public override bool SupportsSystemNotification => true;

        public override bool SystemNotificationIsEnabled
        {
            get => this.systemNotificationIsEnabled;
            set
            {
                this.systemNotificationIsEnabled = value;
                SettingsClient.Set("Behaviour", "EnableSystemNotification", value, true);
                Application.Current.Dispatcher.InvokeAsync(async () => await SwitchNotificationHandlerAsync(value));
            }
        }

        public NotificationService(IPlaybackService playbackService, ICacheService cacheService, IMetadataService metadataService) : base(playbackService, cacheService, metadataService)
        {
            // Pay attention to UPPERCASE property
            this.SystemNotificationIsEnabled = SettingsClient.Get<bool>("Behaviour", "EnableSystemNotification");

            systemMediaControls = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
            BackgroundMediaPlayer.Current.IsLoopingEnabled = true;
            displayUpdater = systemMediaControls.DisplayUpdater;
            displayUpdater.Type = MediaPlaybackType.Music;
            musicProperties = displayUpdater.MusicProperties;
            systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Closed;
            displayUpdater.Update();
        }

        private async void SMCButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs e)
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
                case SystemMediaTransportControlsButton.Stop:
                    this.PlaybackService.Stop();
                    break;
                default:
                    // Never happens		
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SMCAutoRepeatModeChanged(SystemMediaTransportControls sender, AutoRepeatModeChangeRequestedEventArgs args)
        {
            switch (args.RequestedAutoRepeatMode)
            {
                case MediaPlaybackAutoRepeatMode.List:
                    this.PlaybackService.LoopMode = LoopMode.All;
                    break;
                case MediaPlaybackAutoRepeatMode.Track:
                    this.PlaybackService.LoopMode = LoopMode.One;
                    break;
                case MediaPlaybackAutoRepeatMode.None:
                    this.PlaybackService.LoopMode = LoopMode.None;
                    break;
            }
        }

        private async void SMCShuffleEnabledChanged(SystemMediaTransportControls sender, ShuffleEnabledChangeRequestedEventArgs args)
        {
            await this.PlaybackService.SetShuffleAsync(args.RequestedShuffleEnabled);
        }

        protected override bool CanShowNotification()
        {
            if (this.systemNotificationIsEnabled) return false;
            return base.CanShowNotification();
        }

        private void PlaybackStoppedHandler(object sender, EventArgs e)
        {
            systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
            displayUpdater.Update();
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

        private async void PlaybackSuccessSystemNotificationHandler(object sender, PlaybackSuccessEventArgs e)
        {
            systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Playing;
            var track = this.PlaybackService.CurrentTrack;
            musicProperties.AlbumTitle = track.AlbumTitle;
            musicProperties.Artist = track.ArtistName;
            musicProperties.Title = track.TrackTitle;
            uint.TryParse(track.TrackNumber, out var trackNumber);
            musicProperties.TrackNumber = trackNumber;
            await SetArtworkThumbnailAsync(await this.MetadataService.GetArtworkAsync(track.Path));
            displayUpdater.Update();
        }

        private void PlaybackLoopChangedHandler(object sender, EventArgs e)
        {
            UpdateSMCRepeatMode();
        }

        private void PlaybackShuffleChangedHandler(object sender, EventArgs e)
        {
            systemMediaControls.ShuffleEnabled = this.PlaybackService.Shuffle;
        }

        private async Task SwitchNotificationHandlerAsync(bool systemNotificationIsEnabled)
        {
            await Task.Run(() =>
            {
                // We can safely unsubscribe event handlers before subscribing them
                // See https://msdn.microsoft.com/en-us/library/system.delegate.remove.aspx
                // The constructor of LegacyNotificationService already hooks these handlers. 
                // Unhooking them here makes sure they don't get hooked a 2nd time further down.
                this.PlaybackService.PlaybackSuccess -= this.PlaybackSuccessHandler;
                this.PlaybackService.PlaybackPaused -= this.PlaybackPausedHandler;
                this.PlaybackService.PlaybackResumed -= this.PlaybackResumedHandler;
                this.PlaybackService.PlaybackStopped -= this.PlaybackStoppedHandler;
                this.PlaybackService.PlaybackLoopChanged -= this.PlaybackLoopChangedHandler;
                this.PlaybackService.PlaybackShuffleChanged -= this.PlaybackShuffleChangedHandler;

                // Do not add event handler to ButtonPressed, it has been dealt with Shell.xaml.cs
                if (systemNotificationIsEnabled)
                {
                    if (Constants.IsWindows10)
                    {
                        systemMediaControls.IsEnabled = true;
                        systemMediaControls.IsPlayEnabled = true;
                        systemMediaControls.IsPauseEnabled = true;
                        systemMediaControls.IsPreviousEnabled = true;
                        systemMediaControls.IsNextEnabled = true;
                        systemMediaControls.ShuffleEnabled = this.PlaybackService.Shuffle;
                        systemMediaControls.IsRewindEnabled = false;
                        systemMediaControls.IsFastForwardEnabled = false;
                        systemMediaControls.IsRecordEnabled = false;
                        systemMediaControls.IsStopEnabled = true;

                        UpdateSMCRepeatMode();

                        systemMediaControls.ButtonPressed += SMCButtonPressed;
                        systemMediaControls.AutoRepeatModeChangeRequested += SMCAutoRepeatModeChanged;
                        systemMediaControls.ShuffleEnabledChangeRequested += SMCShuffleEnabledChanged;

                        this.PlaybackService.PlaybackSuccess += this.PlaybackSuccessSystemNotificationHandler;
                        this.PlaybackService.PlaybackPaused += this.PlaybackPausedSystemNotificationHandler;
                        this.PlaybackService.PlaybackResumed += this.PlaybackResumedSystemNotificationHandler;
                        this.PlaybackService.PlaybackStopped += this.PlaybackStoppedHandler;
                        this.PlaybackService.PlaybackLoopChanged += this.PlaybackLoopChangedHandler;
                        this.PlaybackService.PlaybackShuffleChanged += this.PlaybackShuffleChangedHandler;
                    }
                }
                else
                {
                    this.PlaybackService.PlaybackSuccess += this.PlaybackSuccessHandler;
                    this.PlaybackService.PlaybackPaused += this.PlaybackPausedHandler;
                    this.PlaybackService.PlaybackResumed += this.PlaybackResumedHandler;
                    this.PlaybackService.PlaybackStopped += this.PlaybackStoppedHandler;
                    this.PlaybackService.PlaybackLoopChanged += this.PlaybackLoopChangedHandler;
                    this.PlaybackService.PlaybackShuffleChanged += this.PlaybackShuffleChangedHandler;

                    if (Constants.IsWindows10)
                    {
                        systemMediaControls.IsEnabled = false;
                        systemMediaControls.ButtonPressed -= SMCButtonPressed;
                        systemMediaControls.AutoRepeatModeChangeRequested -= SMCAutoRepeatModeChanged;
                        systemMediaControls.ShuffleEnabledChangeRequested -= SMCShuffleEnabledChanged;

                        this.PlaybackService.PlaybackSuccess -= this.PlaybackSuccessSystemNotificationHandler;
                        this.PlaybackService.PlaybackPaused -= this.PlaybackPausedSystemNotificationHandler;
                        this.PlaybackService.PlaybackResumed -= this.PlaybackResumedSystemNotificationHandler;
                        this.PlaybackService.PlaybackStopped -= this.PlaybackStoppedHandler;
                        this.PlaybackService.PlaybackLoopChanged -= this.PlaybackLoopChangedHandler;
                        this.PlaybackService.PlaybackShuffleChanged -= this.PlaybackShuffleChangedHandler;
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

        private void UpdateSMCRepeatMode()
        {
            switch (this.PlaybackService.LoopMode)
            {
                case LoopMode.All:
                    systemMediaControls.AutoRepeatMode = MediaPlaybackAutoRepeatMode.List;
                    break;
                case LoopMode.One:
                    systemMediaControls.AutoRepeatMode = MediaPlaybackAutoRepeatMode.Track;
                    break;
                case LoopMode.None:
                    systemMediaControls.AutoRepeatMode = MediaPlaybackAutoRepeatMode.None;
                    break;
            }

            displayUpdater.Update();
        }
    }
}
