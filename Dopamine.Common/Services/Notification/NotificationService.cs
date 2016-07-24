using Dopamine.Common.Controls;
using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Base;
using Dopamine.Core.Logging;
using Dopamine.Core.Settings;
using Dopamine.Core.Utils;
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
        public NotificationService(IUnityContainer container, IPlaybackService playbackService)
        {
            this.container = container;
            this.playbackService = playbackService;

            this.playbackService.PlaybackSuccess += async (x) => await this.ShowNotificationIfAllowedAsync();
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

            if (!XmlSettingsClient.Instance.Get<bool>("Behaviour", "ShowNotification"))
            {
                return;
            }

            try
            {
                if (this.notification != null)
                {
                    this.notification.Disable();
                }
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Error while trying to disable the notification. Exception: {0}", ex.Message);
            }

            string artworkPath = string.Empty;

            TrackInfoViewModel playingTrackinfoVm = null; // Create a dummy track

            await Task.Run(() =>
            {
                try
                {
                    if (this.playbackService.PlayingTrack != null)
                    {
                        artworkPath = ArtworkUtils.GetArtworkPath(this.playbackService.PlayingTrack.Album);
                        playingTrackinfoVm = this.container.Resolve<TrackInfoViewModel>();
                        playingTrackinfoVm.TrackInfo = this.playbackService.PlayingTrack;
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Error while trying to show the notification. Exception: {0}", ex.Message);
                }
            });

            Application.Current.Dispatcher.Invoke(() =>
            {
                this.notification = new NotificationWindow(playingTrackinfoVm,
                                                      artworkPath,
                                                      (NotificationPosition)XmlSettingsClient.Instance.Get<int>("Behaviour", "NotificationPosition"),
                                                      XmlSettingsClient.Instance.Get<bool>("Behaviour", "ShowNotificationControls"),
                                                      XmlSettingsClient.Instance.Get<int>("Behaviour", "NotificationAutoCloseSeconds"));

                                                     this.notification.DoubleClicked += ShowMainWindow;

                this.notification.Show();
            });
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
