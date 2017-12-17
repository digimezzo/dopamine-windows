using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.WPFControls;
using Dopamine.Core.Base;
using Dopamine.Services.Contracts.Cache;
using Dopamine.Services.Contracts.Metadata;
using Dopamine.Services.Contracts.Playback;
using System;
using System.Threading.Tasks;
using System.Windows;
using Windows.Media;

namespace Dopamine.Services.Notification
{
    public class LegacyNotificationService : INotificationService
    {
        private NotificationWindow notification;
        private IPlaybackService playbackService;
        private ICacheService cacheService;
        private IMetadataService metadataService;
        private BorderlessWindows10Window mainWindow;
        private BorderlessWindows10Window playlistWindow;
        private Window trayControlsWindow;
        private bool showNotificationWhenPlaying;
        private bool showNotificationWhenPausing;
        private bool showNotificationWhenResuming;
        private bool showNotificationControls;
    
        public IPlaybackService PlaybackService => this.playbackService;
        public IMetadataService MetadataService => this.metadataService;

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

        public virtual bool SystemNotificationIsEnabled
        {
            get => false;
            set
            {
            }
        }
   
        public LegacyNotificationService(IPlaybackService playbackService, ICacheService cacheService, IMetadataService metadataService)
        {
            this.playbackService = playbackService;
            this.cacheService = cacheService;
            this.metadataService = metadataService;

            this.showNotificationControls = SettingsClient.Get<bool>("Behaviour", "ShowNotificationControls");
            this.showNotificationWhenResuming = SettingsClient.Get<bool>("Behaviour", "ShowNotificationWhenResuming");
            this.showNotificationWhenPausing = SettingsClient.Get<bool>("Behaviour", "ShowNotificationWhenPausing");
            this.showNotificationWhenPlaying = SettingsClient.Get<bool>("Behaviour", "ShowNotificationWhenPlaying");

            this.playbackService.PlaybackSuccess += this.PlaybackSuccessHandler;
            this.playbackService.PlaybackPaused += this.PlaybackPausedHandler;
            this.playbackService.PlaybackResumed += this.PlaybackResumedHandler;
        }
    
        protected async void PlaybackResumedHandler(object sender, EventArgs e)
        {
            if (this.showNotificationWhenResuming)
            {
                await this.ShowNotificationIfAllowedAsync();
            }
        }

        protected async void PlaybackPausedHandler(object sender, PlaybackPausedEventArgs e)
        {
            if (this.showNotificationWhenPausing && !e.IsSilent)
            {
                await this.ShowNotificationIfAllowedAsync();
            }
        }

        protected async void PlaybackSuccessHandler(object sender, PlaybackSuccessEventArgs e)
        {
            if (this.showNotificationWhenPlaying && !e.IsSilent)
            {
                await this.ShowNotificationIfAllowedAsync();
            }
        }

        protected virtual bool CanShowNotification()
        {
            var showNotificationOnlyWhenPlayerNotVisible = SettingsClient.Get<bool>("Behaviour", "ShowNotificationOnlyWhenPlayerNotVisible");
            if (this.trayControlsWindow != null && this.trayControlsWindow.IsActive) return false; // Never show a notification when the tray controls are visible.
            if (this.mainWindow != null && this.mainWindow.IsActive && showNotificationOnlyWhenPlayerNotVisible) return false;
            if (this.playlistWindow != null && this.playlistWindow.IsActive && showNotificationOnlyWhenPlayerNotVisible) return false;

            return true;
        }
     
        private void ShowMainWindow(Object sender, EventArgs e)
        {
            if (this.mainWindow != null)
            {
                this.mainWindow.ForceActivate();
            }
        }

        private async Task ShowNotificationIfAllowedAsync()
        {
            if (this.CanShowNotification())
            {
                await this.ShowNotificationAsync();
            }
        }

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
                LogClient.Error("Error while trying to disable the notification. Exception: {0}", ex.Message);
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
                LogClient.Error("Error while trying to show the notification. Exception: {0}", ex.Message);
            }
        }

        public void HideNotification()
        {
            if (this.notification != null)
                this.notification.Disable();
        }

        public void SetApplicationWindows(BorderlessWindows10Window mainWindow, BorderlessWindows10Window playlistWindow, Window trayControlsWindow)
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
    }
}
