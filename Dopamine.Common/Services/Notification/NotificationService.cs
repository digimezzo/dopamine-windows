using Dopamine.Common.Controls;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Base;
using Dopamine.Core.Logging;
using Dopamine.Core.Settings;
using Microsoft.Practices.Unity;
using System;
using System.Threading.Tasks;
using System.Windows;

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
        #endregion

        #region Properties
        public bool CanShowNotification
        {
            get
            {
                bool returnValue = false;

                Application.Current.Dispatcher.Invoke(() => { returnValue = this.mainWindow == null || this.playlistWindow == null || this.trayControlsWindow == null ? true : !(XmlSettingsClient.Instance.Get<bool>("Behaviour", "ShowNotificationOnlyWhenPlayerNotVisible") && (this.mainWindow.IsActive | this.playlistWindow.IsActive | this.trayControlsWindow.IsActive)); });

                return returnValue;
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
                if (XmlSettingsClient.Instance.Get<bool>("Behaviour", "ShowNotificationWhenPlaying")) await this.ShowNotificationIfAllowedAsync();
            };

            this.playbackService.PlaybackPaused += async (_, __) =>
            {
                if (XmlSettingsClient.Instance.Get<bool>("Behaviour", "ShowNotificationWhenPausing")) await this.ShowNotificationIfAllowedAsync();
            };

            this.playbackService.PlaybackResumed += async (_, __) =>
            {
                if (XmlSettingsClient.Instance.Get<bool>("Behaviour", "ShowNotificationWhenResuming")) await this.ShowNotificationIfAllowedAsync();
            };
        }
        #endregion

        #region Private
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
                LogClient.Instance.Logger.Error("Error while trying to disable the notification. Exception: {0}", ex.Message);
            }

            try
            {
                byte[] artworkData = null;

                if (this.playbackService.PlayingTrack != null)
                {
                    artworkData = await this.metadataService.GetArtworkAsync(this.playbackService.PlayingTrack.Path);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.notification = new NotificationWindow(this.playbackService.PlayingTrack,
                                                          artworkData,
                                                          (NotificationPosition)XmlSettingsClient.Instance.Get<int>("Behaviour", "NotificationPosition"),
                                                          XmlSettingsClient.Instance.Get<bool>("Behaviour", "ShowNotificationControls"),
                                                          XmlSettingsClient.Instance.Get<int>("Behaviour", "NotificationAutoCloseSeconds"));

                    this.notification.DoubleClicked += ShowMainWindow;

                    this.notification.Show();
                });
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Error while trying to show the notification. Exception: {0}", ex.Message);
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
