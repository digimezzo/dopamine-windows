using Dopamine.Common.Controls;
using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Base;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Repositories.Interfaces;
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
        private ICacheService cacheService;
        private ITrackRepository trackRepository;
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
        public NotificationService(IUnityContainer container, IPlaybackService playbackService, ICacheService cacheService, ITrackRepository trackRepository)
        {
            this.container = container;
            this.playbackService = playbackService;
            this.cacheService = cacheService;
            this.trackRepository = trackRepository;

            this.playbackService.PlaybackSuccess += async (_) => await this.ShowNotificationIfAllowedAsync();
            this.playbackService.PlaybackPaused += async (_, __) => await this.ShowNotificationIfAllowedAsync();
            this.playbackService.PlaybackResumed += async (_, __) => await this.ShowNotificationIfAllowedAsync();
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

            if (!XmlSettingsClient.Instance.Get<bool>("Behaviour", "ShowNotificationWhenPlaying")
                & !XmlSettingsClient.Instance.Get<bool>("Behaviour", "ShowNotificationWhenPausing")
                & !XmlSettingsClient.Instance.Get<bool>("Behaviour", "ShowNotificationWhenResuming"))
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

            MergedTrackViewModel viewModel = null; // Create a dummy track

            MergedTrack mergedTrack = null;

            if (this.playbackService.PlayingFile != null)
            {
                mergedTrack = await this.trackRepository.GetMergedTrackAsync(this.playbackService.PlayingFile);
            }

            await Task.Run(() =>
        {
            try
            {
                if (mergedTrack != null)
                {
                    artworkPath = this.cacheService.GetCachedArtworkPath(mergedTrack.AlbumArtworkID);
                    viewModel = this.container.Resolve<MergedTrackViewModel>();
                    viewModel.MergedTrack = mergedTrack;
                }
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Error while trying to show the notification. Exception: {0}", ex.Message);
            }
        });

            Application.Current.Dispatcher.Invoke(() =>
            {
                this.notification = new NotificationWindow(viewModel,
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
