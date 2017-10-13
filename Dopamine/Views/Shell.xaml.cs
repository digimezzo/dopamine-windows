using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Base;
using Dopamine.Common.Controls;
using Dopamine.Common.Enums;
using Dopamine.Common.Extensions;
using Dopamine.Common.IO;
using Dopamine.Common.Presentation.Views;
using Dopamine.Common.Services.Notification;
using Dopamine.Common.Services.Win32Input;
using Dopamine.Common.Services.WindowsIntegration;
using Microsoft.Practices.Unity;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Views
{
    public partial class Shell : DopamineWindow
    {
        private IUnityContainer container;
        private IWindowsIntegrationService windowsIntegrationService;
        private INotificationService notificationService;
        private IWin32InputService win32InputService;
        private System.Windows.Forms.NotifyIcon trayIcon;
        private ContextMenu trayIconContextMenu;
        private TrayControls trayControls;
        private Playlist miniPlayerPlaylist;
        private bool canSaveWindowGeometry = false;

        public Shell(IUnityContainer container, IWindowsIntegrationService windowsIntegrationService, INotificationService notificationService,
            IWin32InputService win32InputService)
        {
            InitializeComponent();

            this.container = container;
            this.windowsIntegrationService = windowsIntegrationService;
            this.notificationService = notificationService;
            this.win32InputService = win32InputService;

            this.InitializeShellWindow();
            this.InitializeTrayIcon();
        }

        private void TrayIconContextMenuAppName_Click(object sender, RoutedEventArgs e)
        {
            this.ShowWindowInForeground();
        }

        private void TrayIconContextMenuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ShellWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LogClient.Info("### STOPPING {0}, version {1} ###", ProductInformation.ApplicationName, ProcessExecutable.AssemblyVersion());
        }

        private void ShowWindowInForeground()
        {
            // When restored, show this window in Taskbar and ALT-TAB menu.
            this.ShowInTaskbar = true;

            try
            {
                WindowUtils.ShowWindowInAltTab(this);
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not show main window in ALT-TAB menu. Exception: {0}", ex.Message);
            }

            // By default, the window appears in the background when showing
            // from the tray menu. We force it on the foreground here.
            this.ActivateNow();
        }

        private void InitializeShellWindow()
        {
            // Start monitoring tablet mode
            this.windowsIntegrationService.StartMonitoringTabletMode();

            // Tray controls
            this.trayControls = this.container.Resolve<Views.TrayControls>();

            // Create the Mini Player playlist
            this.miniPlayerPlaylist = this.container.Resolve<Views.Playlist>(new DependencyOverride(typeof(DopamineWindow), this));

            // NotificationService needs to know about the application windows
            this.notificationService.SetApplicationWindows(this, this.miniPlayerPlaylist, this.trayControls);

            // Settings changed
            SettingsClient.SettingChanged += (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "Appearance", "ShowWindowBorder"))
                {
                    this.SetWindowBorder((bool)e.SettingValue);
                }

                if (SettingsClient.IsSettingChanged(e, "Behaviour", "ShowTrayIcon"))
                {
                    this.trayIcon.Visible = (bool)e.SettingValue;
                }
            };

            // Make sure the window geometry respects tablet mode at startup
            this.CheckIfTabletMode();
        }

        private void InitializeTrayIcon()
        {
            this.trayIcon = new System.Windows.Forms.NotifyIcon();
            this.trayIcon.Visible = false;
            this.trayIcon.Text = ProductInformation.ApplicationName;

            // Reflection is needed to get the full path of the executable. Because when starting the application from the start menu
            // without specifying the full path, the application fails to find the Tray icon and crashes here
            string iconFile = EnvironmentUtils.IsWindows10() ? "Tray.ico" : "Legacy tray.ico";

            string iconPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ApplicationPaths.IconsSubDirectory, iconFile);
            this.trayIcon.Icon = new System.Drawing.Icon(iconPath, System.Windows.Forms.SystemInformation.SmallIconSize);

            this.trayIcon.MouseClick += TrayIcon_MouseClick;
            this.trayIcon.MouseDoubleClick += (_, __) => this.ShowWindowInForeground();

            this.trayIconContextMenu = (ContextMenu)this.FindResource("TrayIconContextMenu");

            this.trayControls = this.container.Resolve<TrayControls>();
        }

        private void TrayIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                this.trayControls.Topmost = true; // Make sure this appears above the Windows Tray pop-up
                this.trayControls.Show();
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                // Open the Notify icon context menu
                this.trayIconContextMenu.IsOpen = true;

                // Required to close the Tray icon when Deactivated is called
                // See: http://copycodetheory.blogspot.be/2012/07/notify-icon-in-wpf-applications.html
                this.Activate();
            }
        }

        private void ShellWindow_Deactivated(object sender, EventArgs e)
        {
            this.trayIconContextMenu.IsOpen = false;
        }

        private void ShellWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // This call is not in the constructor, because we want to show the tray icon only
            // when the main window has been shown by explicitly calling Show(). This prevents 
            // showing the tray icon when the OOBE window is displayed.
            if (SettingsClient.Get<bool>("Behaviour", "ShowTrayIcon"))
            {
                this.trayIcon.Visible = true;
            }
        }

        private void ShellWindow_Closed(object sender, EventArgs e)
        {
            // Stop monitoring tablet mode
            this.windowsIntegrationService.StopMonitoringTabletMode();

            // Make sure the Tray icon is removed from the tray
            this.trayIcon.Visible = false;

            // Stop listening to keyboard outside the application
            this.win32InputService.UnhookKeyboard();

            // This makes sure the application doesn't keep running when the main window is closed.
            // Extra windows created by the main window can keep a WPF application running even when
            // the main window is closed, because the default ShutDownMode of a WPF application is
            // OnLastWindowClose. This was happening here because of the Mini Player Playlist.
            Application.Current.Shutdown();
        }

        private void ShellWindow_Restored(object sender, EventArgs e)
        {
            // This workaround is needed because when executing the following 
            // sequence, the window is restored to the Restore Position of 
            // the Mini Player: Maximize, Mini Player, Full Player, Restore.
            // That's because the property RestoreBounds of this window is updated
            // with the coordinates of the Mini Player when switching to the Mini
            // Player. Returning to the full player doesn't update RestoreBounds,
            // because the full player is still maximized at that point.
            this.SetGeometry(
                SettingsClient.Get<int>("FullPlayer", "Top"),
                SettingsClient.Get<int>("FullPlayer", "Left"),
                SettingsClient.Get<int>("FullPlayer", "Width"),
                SettingsClient.Get<int>("FullPlayer", "Height"),
                Constants.DefaultShellTop,
                Constants.DefaultShellLeft);
        }

        private void ShellWindow_CloseToolTipChanged(object sender, EventArgs e)
        {
            // Workaround to make sure the PART_MiniPlayerButton ToolTip also gets updated on a language change
            this.PART_MiniPlayerButton.ToolTip = ResourceUtils.GetString("Language_Mini_Player");
        }

        private void CheckIfTabletMode()
        {
            if (this.windowsIntegrationService.IsTabletModeEnabled)
            {
                // Always revert to full player when tablet mode is enabled. Maximizing will be done by Windows.
                this.SetPlayer(false, (MiniPlayerType)SettingsClient.Get<int>("General", "MiniPlayerType"));
            }
            else
            {
                bool isMiniPlayer = SettingsClient.Get<bool>("General", "IsMiniPlayer");
                bool isMaximized = SettingsClient.Get<bool>("FullPlayer", "IsMaximized");
                this.WindowState = isMaximized & !isMiniPlayer ? WindowState.Maximized : WindowState.Normal;

                this.SetPlayer(isMiniPlayer, (MiniPlayerType)SettingsClient.Get<int>("General", "MiniPlayerType"));
            }
        }

        private void ShellWindow_LocationChanged(object sender, EventArgs e)
        {
            // We need to put SaveWindowLocation() in the queue of the Dispatcher.
            // SaveWindowLocation() needs to be executed after LocationChanged was 
            // handled, when the WindowState has been updated otherwise we get 
            // incorrect values for Left and Top (both -7 last I checked).
            this.Dispatcher.BeginInvoke(new Action(() => this.SaveWindowLocation()));
        }

        private void SaveWindowLocation()
        {
            if (this.canSaveWindowGeometry)
            {
                if (SettingsClient.Get<bool>("General", "IsMiniPlayer"))
                {
                    SettingsClient.Set<int>("MiniPlayer", "Top", Convert.ToInt32(this.Top));
                    SettingsClient.Set<int>("MiniPlayer", "Left", Convert.ToInt32(this.Left));
                }
                else if (!SettingsClient.Get<bool>("General", "IsMiniPlayer") & !(this.WindowState == WindowState.Maximized))
                {
                    SettingsClient.Set<int>("FullPlayer", "Top", Convert.ToInt32(this.Top));
                    SettingsClient.Set<int>("FullPlayer", "Left", Convert.ToInt32(this.Left));
                }
            }
        }

        private async void SetPlayer(bool isMiniPlayer, MiniPlayerType miniPlayerType)
        {
            UserControl screen = null;

            // Clear player content
            this.ShellFrame.Navigate(this.container.Resolve<Empty>());

            // Save the player type in the settings
            SettingsClient.Set<bool>("General", "IsMiniPlayer", isMiniPlayer);

            // Only save the Mini Player Type in the settings if the current player is set to the Mini Player
            if (isMiniPlayer) SettingsClient.Set<int>("General", "MiniPlayerType", (int)miniPlayerType);

            // Prevents saving window state and size to the Settings XML while switching players
            this.canSaveWindowGeometry = false;

            // Sets the geometry of the player
            if (isMiniPlayer | (!this.windowsIntegrationService.IsTabletModeEnabled & this.windowsIntegrationService.IsStartedFromExplorer))
            {
                PART_MiniPlayerButton.ToolTip = ResourceUtils.GetString("Language_Restore");

                switch (miniPlayerType)
                {
                    case MiniPlayerType.CoverPlayer:
                        this.ClosingText.FontSize = Constants.MediumBackgroundFontSize;
                        this.SetMiniPlayer(MiniPlayerType.CoverPlayer, Constants.CoverPlayerWidth, Constants.CoverPlayerHeight);
                        screen = this.container.Resolve<MiniPlayer.CoverPlayer>();
                        break;
                    case MiniPlayerType.MicroPlayer:
                        this.ClosingText.FontSize = Constants.MediumBackgroundFontSize;
                        this.SetMiniPlayer(MiniPlayerType.MicroPlayer, Constants.MicroPlayerWidth, Constants.MicroPlayerHeight);
                        screen = this.container.Resolve<MiniPlayer.MicroPlayer>();
                        break;
                    case MiniPlayerType.NanoPlayer:
                        this.ClosingText.FontSize = Constants.SmallBackgroundFontSize;
                        this.SetMiniPlayer(MiniPlayerType.NanoPlayer, Constants.NanoPlayerWidth, Constants.NanoPlayerHeight);
                        screen = this.container.Resolve<MiniPlayer.NanoPlayer>();
                        break;
                    default:
                        break;
                        // Doesn't happen
                }
            }
            else
            {
                this.ClosingText.FontSize = Constants.LargeBackgroundFontSize;
                PART_MiniPlayerButton.ToolTip = ResourceUtils.GetString("Language_Mini_Player");
                this.SetFullPlayer();
                screen = this.container.Resolve<FullPlayer.FullPlayer>();
            }

            // Determine if the player position is locked
            this.SetWindowPositionLockedFromSettings();

            // Delay, otherwise content is never shown (probably because regions don't exist yet at startup)
            await Task.Delay(150);

            // Navigate to content
            this.ShellFrame.Navigate(screen);

            this.canSaveWindowGeometry = true;
        }

        private void SetFullPlayer()
        {
            this.miniPlayerPlaylist.Hide();

            this.ResizeMode = ResizeMode.CanResize;

            this.ShowWindowControls = true;

            if (SettingsClient.Get<bool>("FullPlayer", "IsMaximized"))
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;

                this.SetGeometry(
                    SettingsClient.Get<int>("FullPlayer", "Top"),
                    SettingsClient.Get<int>("FullPlayer", "Left"),
                    SettingsClient.Get<int>("FullPlayer", "Width"),
                    SettingsClient.Get<int>("FullPlayer", "Height"),
                    Constants.DefaultShellTop,
                    Constants.DefaultShellLeft);
            }

            // Set MinWidth and MinHeight AFTER SetGeometry(). This prevents flicker.
            this.MinWidth = Constants.MinShellWidth;
            this.MinHeight = Constants.MinShellHeight;

            this.SetWindowTopmostFromSettings();
        }

        private void SetMiniPlayer(MiniPlayerType miniPlayerType, double playerWidth, double playerHeight)
        {
            // Hide the playlist before changing window dimensions to avoid strange behaviour
            this.miniPlayerPlaylist.Hide();

            this.WindowState = WindowState.Normal;
            this.ResizeMode = ResizeMode.CanMinimize;
            this.ShowWindowControls = false;

            // Set MinWidth and MinHeight BEFORE SetMiniPlayerDimensions(). This prevents flicker.
            if (this.HasBorder)
            {
                // Correction to take into account the window border, otherwise the content 
                // misses 2px horizontally and vertically when displaying the window border
                this.MinWidth = playerWidth + 2;
                this.MinHeight = playerHeight + 2;
            }
            else
            {
                this.MinWidth = playerWidth;
                this.MinHeight = playerHeight;
            }

            this.SetGeometry(
               SettingsClient.Get<int>("MiniPlayer", "Top"),
               SettingsClient.Get<int>("MiniPlayer", "Left"),
               Convert.ToInt32(this.MinWidth),
               Convert.ToInt32(this.MinHeight),
               Constants.DefaultShellTop,
               Constants.DefaultShellLeft);

            this.SetWindowTopmostFromSettings();
        }

        private void SetWindowTopmostFromSettings()
        {
            if (SettingsClient.Get<bool>("General", "IsMiniPlayer"))
            {
                this.Topmost = SettingsClient.Get<bool>("Behaviour", "MiniPlayerOnTop");
            }
            else
            {
                // Full player is never topmost
                this.Topmost = false;
            }
        }

        private void SetWindowPositionLockedFromSettings()
        {
            // Only lock position when the mini player is active
            if (SettingsClient.Get<bool>("General", "IsMiniPlayer"))
            {
                this.IsMovable = !SettingsClient.Get<bool>("Behaviour", "MiniPlayerPositionLocked");
            }
            else
            {
                this.IsMovable = true;
            }
        }
    }
}
