using Digimezzo.Utilities.Settings;
using Dopamine.Common.Base;
using Dopamine.Common.Controls;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Logging;
using Microsoft.Practices.Unity;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace Dopamine.Common.Services.Notification
{
    public class NotificationService : INotificationService
    {
        #region Variables
        private IUnityContainer container;
        private NotificationWindow notification;
        private IPlaybackService playbackService;
        private ICacheService cacheService;
        private IMetadataService metadataService;
        private DopamineWindow mainWindow;
        private DopamineWindow playlistWindow;
        private Window trayControlsWindow;
        private bool systemNotificationIsEnabled;
        private bool showNotificationWhenPlaying;
        private bool showNotificationWhenPausing;
        private bool showNotificationWhenResuming;
        private bool showNotificationControls;
        private SystemMediaTransportControls systemMediaControls;
        private SystemMediaTransportControlsDisplayUpdater displayUpdater;
        private MusicDisplayProperties musicProperties;
        private InMemoryRandomAccessStream artworkStream;
        #endregion

        #region Properties

        public bool ShowNotificationControls
        {
            get => this.showNotificationControls;
            set
            {
                this.showNotificationControls = value;
                SettingsClient.Set<bool>("Behaviour", "ShowNotificationControls", value);
            }
        }

        public bool ShowNotificationWhenResuming
        {
            get => this.showNotificationWhenResuming;
            set
            {
                this.showNotificationWhenResuming = value;
                SettingsClient.Set<bool>("Behaviour", "ShowNotificationWhenResuming", value);
            }
        }

        public bool ShowNotificationWhenPausing
        {
            get => this.showNotificationWhenPausing;
            set
            {
                this.showNotificationWhenPausing = value;
                SettingsClient.Set<bool>("Behaviour", "ShowNotificationWhenPausing", value);
            }
        }

        public bool ShowNotificationWhenPlaying
        {
            get => this.showNotificationWhenPlaying;
            set
            {
                this.showNotificationWhenPlaying = value;
                SettingsClient.Set<bool>("Behaviour", "ShowNotificationWhenPlaying", value);
            }
        }

        public bool SystemNotificationIsEnabled
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
        public NotificationService(IUnityContainer container, IPlaybackService playbackService, ICacheService cacheService, IMetadataService metadataService)
        {
            this.container = container;
            this.playbackService = playbackService;
            this.cacheService = cacheService;
            this.metadataService = metadataService;

            // Pay attention to UPPERCASE property
            this.SystemNotificationIsEnabled = SettingsClient.Get<bool>("Behaviour", "EnableSystemNotification");
            this.showNotificationControls = SettingsClient.Get<bool>("Behaviour", "ShowNotificationControls");
            this.showNotificationWhenResuming = SettingsClient.Get<bool>("Behaviour", "ShowNotificationWhenResuming");
            this.showNotificationWhenPausing = SettingsClient.Get<bool>("Behaviour", "ShowNotificationWhenPausing");
            this.showNotificationWhenPlaying = SettingsClient.Get<bool>("Behaviour", "ShowNotificationWhenPlaying");

            if (Constants.IsWindows10)
            {
                systemMediaControls = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
                displayUpdater = systemMediaControls.DisplayUpdater;
                displayUpdater.Type = MediaPlaybackType.Music;
                musicProperties = displayUpdater.MusicProperties;
                systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                displayUpdater.Update();
            }
        }
        #endregion

        #region Private
        private async void SMCButtonPressed(SystemMediaTransportControls sender,
            SystemMediaTransportControlsButtonPressedEventArgs e)
        {
            switch (e.Button)
            {
                case SystemMediaTransportControlsButton.Previous:
                    await this.playbackService.PlayPreviousAsync();
                    break;
                case SystemMediaTransportControlsButton.Next:
                    await this.playbackService.PlayNextAsync();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    await this.playbackService.PlayOrPauseAsync();
                    break;
                case SystemMediaTransportControlsButton.Play:
                    await this.playbackService.PlayOrPauseAsync();
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
            var track = this.playbackService.CurrentTrack.Value;
            musicProperties.AlbumArtist = track.AlbumArtist;
            musicProperties.AlbumTitle = track.AlbumTitle;
            musicProperties.Artist = track.ArtistName;
            musicProperties.Title = track.TrackTitle;
            musicProperties.TrackNumber = Convert.ToUInt32(track.TrackNumber);
            await SetArtworkThumbnailAsync(await this.metadataService.GetArtworkAsync(track.Path));
            displayUpdater.Update();
        }

        private async void PlaybackResumedHandler(object _, EventArgs __)
        {
            if (this.showNotificationWhenResuming) await this.ShowNotificationIfAllowedAsync();
        }

        private async void PlaybackPausedHandler(object _, EventArgs __)
        {
            if(this.showNotificationWhenPausing) await this.ShowNotificationIfAllowedAsync();
        }

        private async void PlaybackSuccessHandler(bool _)
        {
            if (this.showNotificationWhenPlaying) await this.ShowNotificationIfAllowedAsync();
        }

        private async Task SwitchNotificationHandlerAsync(bool systemNotificationIsEnabled)
        {
            await Task.Run(() =>
            {
                if (systemNotificationIsEnabled)
                {
                    // We can safely unsubscribe event handlers before subscribing them
                    // See https://msdn.microsoft.com/en-us/library/system.delegate.remove.aspx
                    this.playbackService.PlaybackSuccess -= this.PlaybackSuccessHandler;
                    this.playbackService.PlaybackPaused -= this.PlaybackPausedHandler;
                    this.playbackService.PlaybackResumed -= this.PlaybackResumedHandler;

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

                        this.playbackService.PlaybackSuccess += this.PlaybackSuccessSystemNotificationHandler;
                        this.playbackService.PlaybackPaused += this.PlaybackPausedSystemNotificationHandler;
                        this.playbackService.PlaybackResumed += this.PlaybackResumedSystemNotificationHandler;
                    }
                }
                else
                {
                    this.playbackService.PlaybackSuccess += this.PlaybackSuccessHandler;
                    this.playbackService.PlaybackPaused += this.PlaybackPausedHandler;
                    this.playbackService.PlaybackResumed += this.PlaybackResumedHandler;

                    if (Constants.IsWindows10)
                    {
                        systemMediaControls.IsEnabled = false;
                        systemMediaControls.ButtonPressed -= SMCButtonPressed;

                        this.playbackService.PlaybackSuccess -= this.PlaybackSuccessSystemNotificationHandler;
                        this.playbackService.PlaybackPaused -= this.PlaybackPausedSystemNotificationHandler;
                        this.playbackService.PlaybackResumed -= this.PlaybackResumedSystemNotificationHandler;
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

        private void ShowMainWindow(Object sender, EventArgs e)
        {
            if (this.mainWindow != null)
            {
                this.mainWindow.ActivateNow();
            }
        }

        private bool CanShowNotification()
        {
            if (this.systemNotificationIsEnabled) return false;
            var showNotificationOnlyWhenPlayerNotVisible = SettingsClient.Get<bool>("Behaviour", "ShowNotificationOnlyWhenPlayerNotVisible");
            if (this.trayControlsWindow != null && this.trayControlsWindow.IsActive) return false; // Never show a notification when the tray controls are visible.
            if (this.mainWindow != null && this.mainWindow.IsActive && showNotificationOnlyWhenPlayerNotVisible) return false;
            if (this.playlistWindow != null && this.playlistWindow.IsActive && showNotificationOnlyWhenPlayerNotVisible) return false;

            return true;
        }

        private async Task ShowNotificationIfAllowedAsync()
        {
            if (this.CanShowNotification())
            {
                await this.ShowNotificationAsync();
            }
        }
        #endregion

        #region INotificationService
        public async Task ShowNotificationAsync()
        {
            if (this.notification != null)
            {
                this.notification.DoubleClicked -= ShowMainWindow;
            }

            try
            {
                if (this.notification != null) this.notification.Disable();
            }
            catch (Exception ex)
            {
                CoreLogger.Current.Error("Error while trying to disable the notification. Exception: {0}", ex.Message);
            }

            try
            {
                byte[] artworkData = null;

                if (this.playbackService.HasCurrentTrack)
                {
                    artworkData = await this.metadataService.GetArtworkAsync(this.playbackService.CurrentTrack.Value.Path);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.notification = new NotificationWindow(this.playbackService.CurrentTrack.Value,
                                                          artworkData,
                                                          (NotificationPosition)SettingsClient.Get<int>("Behaviour", "NotificationPosition"),
                                                          SettingsClient.Get<bool>("Behaviour", "ShowNotificationControls"),
                                                          SettingsClient.Get<int>("Behaviour", "NotificationAutoCloseSeconds"));

                    this.notification.DoubleClicked += ShowMainWindow;

                    this.notification.Show();
                });
            }
            catch (Exception ex)
            {
                CoreLogger.Current.Error("Error while trying to show the notification. Exception: {0}", ex.Message);
            }
        }

        public void HideNotification()
        {
            if (this.notification != null)
                this.notification.Disable();
        }

        public void SetApplicationWindows(DopamineWindow mainWindow, DopamineWindow playlistWindow, Window trayControlsWindow)
        {
            if (mainWindow != null)
            {
                this.mainWindow = mainWindow;
            }

            if (playlistWindow != null)
            {
                this.playlistWindow = playlistWindow;
            }

            if (trayControlsWindow != null)
            {
                this.trayControlsWindow = trayControlsWindow;
            }
        }
        #endregion
    }
}
