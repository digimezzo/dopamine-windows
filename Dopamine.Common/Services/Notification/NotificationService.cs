using Digimezzo.Utilities.Settings;
using Dopamine.Common.Controls;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Base;
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
        private SystemMediaTransportControls systemMediaControls;
        private SystemMediaTransportControlsDisplayUpdater displayUpdater;
        private MusicDisplayProperties musicProperties;
        private InMemoryRandomAccessStream artworkStream;
        #endregion

        #region Properties
        public bool CanShowNotification
        {
            get
            {
                if (this.trayControlsWindow != null && this.trayControlsWindow.IsActive) return false; // Never show a notification when the tray controls are visible.
                if (this.mainWindow != null && this.mainWindow.IsActive && SettingsClient.Get<bool>("Behaviour", "ShowNotificationOnlyWhenPlayerNotVisible")) return false;
                if (this.playlistWindow != null && this.playlistWindow.IsActive && SettingsClient.Get<bool>("Behaviour", "ShowNotificationOnlyWhenPlayerNotVisible")) return false;

                return true;
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

            this.playbackService.PlaybackSuccess += async (_) =>
            {
                if (SettingsClient.Get<bool>("Behaviour", "ShowNotificationWhenPlaying")) await this.ShowNotificationIfAllowedAsync();
            };

            this.playbackService.PlaybackPaused += async (_, __) =>
            {
                if (SettingsClient.Get<bool>("Behaviour", "ShowNotificationWhenPausing")) await this.ShowNotificationIfAllowedAsync();
            };

            this.playbackService.PlaybackResumed += async (_, __) =>
            {
                if (SettingsClient.Get<bool>("Behaviour", "ShowNotificationWhenResuming")) await this.ShowNotificationIfAllowedAsync();
            };

            systemMediaControls = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
            systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Closed;
            systemMediaControls.IsEnabled = true;
            systemMediaControls.IsPlayEnabled = true;
            systemMediaControls.IsPauseEnabled = true;
            systemMediaControls.IsStopEnabled = true;
            systemMediaControls.IsPreviousEnabled = true;
            systemMediaControls.IsNextEnabled = true;
            systemMediaControls.IsRewindEnabled = false;
            systemMediaControls.IsFastForwardEnabled = false;

            displayUpdater = systemMediaControls.DisplayUpdater;
            displayUpdater.Type = MediaPlaybackType.Music;
            musicProperties = displayUpdater.MusicProperties;
            musicProperties.AlbumArtist = "Artist";
            musicProperties.AlbumTitle = "Title";
            musicProperties.Artist = "Artist";
            musicProperties.Title = "Title";
            musicProperties.TrackNumber = 1;
            displayUpdater.Update();
        }
        #endregion

        #region Private
        private async void SetArtworkThumbnail(byte[] data)
        {
            if (artworkStream != null)
                artworkStream.Dispose();
            if (data == null)
            {
                artworkStream = null;
                displayUpdater.Thumbnail = null;
            }
            else
            {
                artworkStream = new InMemoryRandomAccessStream();
                //await artworkStream.WriteAsync(data.AsBuffer());
                displayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromStream(artworkStream);
            }
        }

        private void SetDisplayValues()
        {
            displayUpdater.ClearAll();
            displayUpdater.Type = MediaPlaybackType.Music;
            SetArtworkThumbnail(null);

                musicProperties.AlbumArtist = "Artist";
                musicProperties.AlbumTitle = "Title";
                uint value;
                musicProperties.Artist = "Artist";
            musicProperties.Title = "Title";
                    musicProperties.TrackNumber = 1;
                byte[] imageData;

                //SetArtworkThumbnail(imageData);
            
            displayUpdater.Update();
        }

        private void ShowMainWindow(Object sender, EventArgs e)
        {
            if (this.mainWindow != null)
            {
                this.mainWindow.ActivateNow();
            }
        }

        private async Task ShowNotificationIfAllowedAsync()
        {
            if (this.CanShowNotification)
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
