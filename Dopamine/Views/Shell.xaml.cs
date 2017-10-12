using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Base;
using Dopamine.Common.Controls;
using Dopamine.Common.Enums;
using Dopamine.Common.Extensions;
using Dopamine.Common.IO;
using Dopamine.Common.Presentation.Views;
using Dopamine.Common.Prism;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Notification;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Services.Win32Input;
using Dopamine.Common.Services.WindowsIntegration;
using Digimezzo.Utilities.Log;
using Dopamine.Common.Services.Appearance;
using Dopamine.FullPlayerModule.Views;
using Dopamine.MiniPlayerModule.Views;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace Dopamine.Views
{
    public partial class Shell : DopamineWindow
    {
        #region Variables
        private IUnityContainer container;
        private readonly IRegionManager regionManager;
        private IAppearanceService appearanceService;
        private IPlaybackService playbackService;
        private IWin32InputService win32InputService;
        private INotificationService notificationService;
        private IMetadataService metadataService;
        private IEventAggregator eventAggregator;
        private IWindowsIntegrationService windowsIntegrationService;

        private bool canSaveWindowGeometry = false;
        private bool isShuttingDown = false;
        private bool mustPerformClosingTasks = true;

        private Storyboard backgroundAnimation;
        private System.Windows.Forms.NotifyIcon trayIcon;
        private ContextMenu trayIconContextMenu;
        private TrayControls trayControls;
        private Playlist miniPlayerPlaylist;

        private bool isCoverPlayerListExpanded; // TODO: remove this bool
        private bool isMicroPlayerListExpanded; // TODO: remove this bool
        private bool isNanoPlayerListExpanded; // TODO: remove this bool
        #endregion

        #region Commands
        public DelegateCommand RestoreWindowCommand { get; set; }
        public DelegateCommand MinimizeWindowCommand { get; set; }
        public DelegateCommand MaximizeRestoreWindowCommand { get; set; }
        public DelegateCommand CloseWindowCommand { get; set; }
        public DelegateCommand<string> ChangePlayerTypeCommand { get; set; }
        public DelegateCommand ToggleMiniPlayerPositionLockedCommand { get; set; }
        public DelegateCommand ToggleMiniPlayerAlwaysOnTopCommand { get; set; }
        public DelegateCommand TaskbarItemInfoPlayCommand { get; set; }
        public DelegateCommand NavigateToMainScreenCommand { get; set; }
        public DelegateCommand NavigateToNowPlayingScreenCommand { get; set; }
        public DelegateCommand TogglePlayerCommand { get; set; }
        public DelegateCommand ShowMainWindowCommand { get; set; }
        #endregion

        #region Construction
        public Shell(IUnityContainer container, IRegionManager regionManager, IAppearanceService appearanceService,
            IPlaybackService playbackService, IWin32InputService win32InputService, IEventAggregator eventAggregator,
            INotificationService notificationService, IMetadataService metadataService, IWindowsIntegrationService windowsIntegrationService)
        {
            InitializeComponent();

            // Dependency injection
            this.container = container;
            this.regionManager = regionManager;
            this.appearanceService = appearanceService;
            this.playbackService = playbackService;
            this.win32InputService = win32InputService;
            this.eventAggregator = eventAggregator;
            this.notificationService = notificationService;
            this.metadataService = metadataService;
            this.windowsIntegrationService = windowsIntegrationService;

            // Window
            this.InitializeWindow();

            // Services
            this.InitializeServicesAsync();

            // PubSub Events
            this.InitializePubSubEvents();

            // Commands
            this.InitializeCommands();

            // Tray icon
            this.InitializeTrayIcon();

        }
        #endregion

        #region Private
        private async void InitializeServicesAsync()
        {
            // IWin32InputService
            // ------------------
            this.win32InputService.SetKeyboardHook(new WindowInteropHelper(this).EnsureHandle()); // listen to media keys
            this.win32InputService.MediaKeyNextPressed += async (_, __) => await this.playbackService.PlayNextAsync();
            this.win32InputService.MediaKeyPreviousPressed += async (_, __) => await this.playbackService.PlayPreviousAsync();
            this.win32InputService.MediaKeyPlayPressed += async (_, __) => await this.playbackService.PlayOrPauseAsync();

            // IAppearanceService
            // ------------------
            this.appearanceService.ThemeChanged += this.ThemeChangedHandler;

            // IWindowsIntegrationService
            // --------------------------
            this.windowsIntegrationService.TabletModeChanged += (_, __) =>
            {
                Application.Current.Dispatcher.Invoke(() => this.CheckIfTabletMode());
            };
        }

        private void InitializePubSubEvents()
        {
            // Cover Player
            // ------------
            this.eventAggregator.GetEvent<CoverPlayerPlaylistButtonClicked>().Subscribe(isPlaylistButtonChecked => this.ToggleMiniPlayerPlaylist(MiniPlayerType.CoverPlayer, isPlaylistButtonChecked));

            // Micro Player
            // ------------
            this.eventAggregator.GetEvent<MicroPlayerPlaylistButtonClicked>().Subscribe(isPlaylistButtonChecked => this.ToggleMiniPlayerPlaylist(MiniPlayerType.MicroPlayer, isPlaylistButtonChecked));

            // Nano Player
            // -----------
            this.eventAggregator.GetEvent<NanoPlayerPlaylistButtonClicked>().Subscribe(isPlaylistButtonChecked => this.ToggleMiniPlayerPlaylist(MiniPlayerType.NanoPlayer, isPlaylistButtonChecked));
        }

        private void InitializeCommands()
        {
            // TaskbarItemInfo
            // ---------------
            this.TaskbarItemInfoPlayCommand = new DelegateCommand(async () => await this.playbackService.PlayOrPauseAsync());
            Common.Prism.ApplicationCommands.TaskbarItemInfoPlayCommand.RegisterCommand(this.TaskbarItemInfoPlayCommand);

            // Window State
            // ------------
            this.MinimizeWindowCommand = new DelegateCommand(() => this.WindowState = WindowState.Minimized);
            Common.Prism.ApplicationCommands.MinimizeWindowCommand.RegisterCommand(this.MinimizeWindowCommand);

            this.RestoreWindowCommand = new DelegateCommand(() => this.SetPlayer(false, MiniPlayerType.CoverPlayer));
            Common.Prism.ApplicationCommands.RestoreWindowCommand.RegisterCommand(this.RestoreWindowCommand);

            this.MaximizeRestoreWindowCommand = new DelegateCommand(() =>
            {
                this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            });

            Common.Prism.ApplicationCommands.MaximizeRestoreWindowCommand.RegisterCommand(this.MaximizeRestoreWindowCommand);

            this.CloseWindowCommand = new DelegateCommand(() => this.Close());
            Common.Prism.ApplicationCommands.CloseWindowCommand.RegisterCommand(this.CloseWindowCommand);

            // Player type
            // -----------
            this.ChangePlayerTypeCommand = new DelegateCommand<string>((miniPlayerType) => this.SetPlayer(true, (MiniPlayerType)Convert.ToInt32(miniPlayerType)));
            Common.Prism.ApplicationCommands.ChangePlayerTypeCommand.RegisterCommand(this.ChangePlayerTypeCommand);

            this.TogglePlayerCommand = new DelegateCommand(() =>
            {
                // If tablet mode is enabled, we should not be able to toggle the player.
                if (!this.windowsIntegrationService.IsTabletModeEnabled) this.TogglePlayer();
            });
            Common.Prism.ApplicationCommands.TogglePlayerCommand.RegisterCommand(this.TogglePlayerCommand);

            // Mini Player
            // -----------
            this.ToggleMiniPlayerPositionLockedCommand = new DelegateCommand(() =>
            {
                bool isMiniPlayerPositionLocked = SettingsClient.Get<bool>("Behaviour", "MiniPlayerPositionLocked");
                SettingsClient.Set<bool>("Behaviour", "MiniPlayerPositionLocked", !isMiniPlayerPositionLocked);
                this.SetWindowPositionLockedFromSettings();
            });

            Common.Prism.ApplicationCommands.ToggleMiniPlayerPositionLockedCommand.RegisterCommand(this.ToggleMiniPlayerPositionLockedCommand);

            this.ToggleMiniPlayerAlwaysOnTopCommand = new DelegateCommand(() =>
            {
                bool topmost = SettingsClient.Get<bool>("Behaviour", "MiniPlayerOnTop");
                SettingsClient.Set<bool>("Behaviour", "MiniPlayerOnTop", !topmost);
                this.SetWindowTopmostFromSettings();
            });

            Common.Prism.ApplicationCommands.ToggleMiniPlayerAlwaysOnTopCommand.RegisterCommand(this.ToggleMiniPlayerAlwaysOnTopCommand);

            // Screens
            // -------
            this.NavigateToMainScreenCommand = new DelegateCommand(() =>
            {
                this.regionManager.RequestNavigate(RegionNames.ScreenTypeRegion, typeof(MainScreen).FullName);
                SettingsClient.Set<bool>("FullPlayer", "IsNowPlayingSelected", false);
            });

            Common.Prism.ApplicationCommands.NavigateToMainScreenCommand.RegisterCommand(this.NavigateToMainScreenCommand);

            this.NavigateToNowPlayingScreenCommand = new DelegateCommand(() =>
            {
                this.regionManager.RequestNavigate(RegionNames.ScreenTypeRegion, typeof(NowPlayingScreen).FullName);
                SettingsClient.Set<bool>("FullPlayer", "IsNowPlayingSelected", true);
            });

            Common.Prism.ApplicationCommands.NavigateToNowPlayingScreenCommand.RegisterCommand(this.NavigateToNowPlayingScreenCommand);

            // Application
            // -----------
            this.ShowMainWindowCommand = new DelegateCommand(() => this.ShowWindowInForeground());
            Common.Prism.ApplicationCommands.ShowMainWindowCommand.RegisterCommand(this.ShowMainWindowCommand);
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
        }

        private void InitializeWindow()
        {
            // Start monitoring tablet mode
            this.windowsIntegrationService.StartMonitoringTabletMode();

            // Tray controls
            this.trayControls = this.container.Resolve<Views.TrayControls>();

            this.miniPlayerPlaylist = this.container.Resolve<Views.Playlist>(new DependencyOverride(typeof(DopamineWindow), this));
            RegionManager.SetRegionManager(this.miniPlayerPlaylist, this.regionManager);
            RegionManager.UpdateRegions();

            this.notificationService.SetApplicationWindows(this, this.miniPlayerPlaylist, this.trayControls);

            // Restored handler
            this.Restored += Shell_Restored;

            // Workaround to make sure the PART_MiniPlayerButton ToolTip also gets updated on a language change
            this.CloseToolTipChanged += Shell_CloseToolTipChanged;

            // Settings changed
            SettingsClient.SettingChanged += (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "Appearance", "ShowWindowBorder"))
                {
                    this.SetWindowBorder((bool) e.SettingValue);
                }

                if (SettingsClient.IsSettingChanged(e, "Behaviour", "ShowTrayIcon"))
                {
                    this.trayIcon.Visible = (bool) e.SettingValue;
                }
            };

            // Make sure the window geometry respects tablet mode at startup
            this.CheckIfTabletMode();
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

        private void TogglePlayer()
        {
            if (SettingsClient.Get<bool>("General", "IsMiniPlayer"))
            {
                // Show the Full Player
                this.SetPlayer(false, MiniPlayerType.CoverPlayer);
            }
            else
            {
                // Show the Mini Player, with the player type which is saved in the settings
                this.SetPlayer(true, (MiniPlayerType)SettingsClient.Get<int>("General", "MiniPlayerType"));
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

        private async void SetPlayer(bool isMiniPlayer, MiniPlayerType miniPlayerType)
        {
            string screenName = typeof(Empty).FullName;

            // Clear player content
            this.regionManager.RequestNavigate(RegionNames.PlayerTypeRegion, typeof(Empty).FullName);

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
                        this.SetMiniPlayer(MiniPlayerType.CoverPlayer, Constants.CoverPlayerWidth, Constants.CoverPlayerHeight, this.isCoverPlayerListExpanded);
                        screenName = typeof(CoverPlayer).FullName;
                        break;
                    case MiniPlayerType.MicroPlayer:
                        this.ClosingText.FontSize = Constants.MediumBackgroundFontSize;
                        this.SetMiniPlayer(MiniPlayerType.MicroPlayer, Constants.MicroPlayerWidth, Constants.MicroPlayerHeight, this.isMicroPlayerListExpanded);
                        screenName = typeof(MicroPlayer).FullName;
                        break;
                    case MiniPlayerType.NanoPlayer:
                        this.ClosingText.FontSize = Constants.SmallBackgroundFontSize;
                        this.SetMiniPlayer(MiniPlayerType.NanoPlayer, Constants.NanoPlayerWidth, Constants.NanoPlayerHeight, this.isNanoPlayerListExpanded);
                        screenName = typeof(NanoPlayer).FullName;
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
                screenName = typeof(FullPlayer).FullName;
            }

            // Determine if the player position is locked
            this.SetWindowPositionLockedFromSettings();

            // Delay, otherwise content is never shown (probably because regions don't exist yet at startup)
            await Task.Delay(150);

            // Navigate to content
            this.regionManager.RequestNavigate(RegionNames.PlayerTypeRegion, screenName);

            this.canSaveWindowGeometry = true;
        }

        private void SaveWindowState()
        {
            // Only save window state when not in tablet mode. Tablet mode maximizes the screen. 
            // We don't want to save that, as we want to be able to restore to the original state when leaving tablet mode.
            if (this.canSaveWindowGeometry & !this.windowsIntegrationService.IsTabletModeEnabled)
            {
                SettingsClient.Set<bool>("FullPlayer", "IsMaximized", this.WindowState == WindowState.Maximized ? true : false);
            }
        }

        private void SaveWindowSize()
        {
            if (this.canSaveWindowGeometry)
            {
                if (!SettingsClient.Get<bool>("General", "IsMiniPlayer") & !(this.WindowState == WindowState.Maximized))
                {
                    SettingsClient.Set<int>("FullPlayer", "Width", Convert.ToInt32(this.ActualWidth));
                    SettingsClient.Set<int>("FullPlayer", "Height", Convert.ToInt32(this.ActualHeight));
                }
            }
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

        private void SetMiniPlayer(MiniPlayerType miniPlayerType, double playerWidth, double playerHeight, bool isMiniPlayerListExpanded)
        {
            // Hide the playlist BEFORE changing window dimensions to avoid strange behaviour
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

            // Show the playlist AFTER changing window dimensions to avoid strange behaviour
            if (isMiniPlayerListExpanded) this.miniPlayerPlaylist.Show(miniPlayerType);

            // Content

        }

        private void ToggleMiniPlayerPlaylist(MiniPlayerType miniPlayerType, bool isMiniPlayerListExpanded)
        {
            switch (miniPlayerType)
            {
                case MiniPlayerType.CoverPlayer:
                    this.isCoverPlayerListExpanded = isMiniPlayerListExpanded;
                    break;
                case MiniPlayerType.MicroPlayer:
                    this.isMicroPlayerListExpanded = isMiniPlayerListExpanded;
                    break;
                case MiniPlayerType.NanoPlayer:
                    this.isNanoPlayerListExpanded = isMiniPlayerListExpanded;
                    break;
                default:
                    break;
                    // Shouldn't happen
            }

            if (isMiniPlayerListExpanded)
            {
                this.miniPlayerPlaylist.Show(miniPlayerType);
            }
            else
            {
                this.miniPlayerPlaylist.Hide();
            }
        }

        private void Shell_Deactivated(object sender, EventArgs e)
        {
            this.trayIconContextMenu.IsOpen = false;
        }

        private void Shell_Loaded(object sender, RoutedEventArgs e)
        {
            // This call is not in the constructor, because we want to show the tray icon only
            // when the main window has been shown by explicitly calling Show(). This prevents 
            // showing the tray icon when the OOBE window is displayed.
            if (SettingsClient.Get<bool>("Behaviour", "ShowTrayIcon")) this.trayIcon.Visible = true;
        }
        #endregion

        #region Event Handlers
        private void Shell_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.eventAggregator.GetEvent<ShellMouseUp>().Publish(null);
        }

        private void ThemeChangedHandler(bool useLightTheme)
        {
            Application.Current.Dispatcher.Invoke(() => { if (this.backgroundAnimation != null) this.backgroundAnimation.Begin(); });
        }

        private void TrayIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                this.trayControls.Topmost = true; // Make sure this appears above the Windows Tray popup
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

        private void TrayIconContextMenuAppName_Click(object sender, RoutedEventArgs e)
        {
            this.ShowWindowInForeground();
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

        private void TrayIconContextMenuExit_Click(object sender, RoutedEventArgs e)
        {
            LogClient.Info("### STOPPING {0}, version {1} ###", ProductInformation.ApplicationName, ProcessExecutable.AssemblyVersion().ToString());
            this.isShuttingDown = true;
            this.Close();
        }

        private void Shell_SourceInitialized(object sender, EventArgs e)
        {
            this.appearanceService.WatchWindowsColor(this);
        }

        private void Shell_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                if (SettingsClient.Get<bool>("Behaviour", "ShowTrayIcon") &
                    SettingsClient.Get<bool>("Behaviour", "MinimizeToTray"))
                {
                    // When minimizing to tray, hide this window from Taskbar and ALT-TAB menu.
                    this.ShowInTaskbar = false;

                    try
                    {
                        WindowUtils.HideWindowFromAltTab(this);
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Could not hide main window from ALT-TAB menu. Exception: {0}", ex.Message);
                    }
                }
            }
            else
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    try
                    {
                        WindowUtils.RemoveWindowCaption(this);
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Could not remove window caption. Exception: {0}", ex.Message);
                    }
                }

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
            }

            this.SaveWindowState();
        }

        private void Shell_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (SettingsClient.Get<bool>("Behaviour", "ShowTrayIcon") &
                                  SettingsClient.Get<bool>("Behaviour", "CloseToTray") &
                                  !this.isShuttingDown)
            {
                e.Cancel = true;

                // Minimize first, then hide from Taskbar. Otherwise a small window
                // remains visible in the lower left corner of the screen.
                this.WindowState = WindowState.Minimized;

                // When closing to tray, hide this window from Taskbar and ALT-TAB menu.
                this.ShowInTaskbar = false;

                try
                {
                    WindowUtils.HideWindowFromAltTab(this);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not hide main window from ALT-TAB menu. Exception: {0}", ex.Message);
                }
            }
            else
            {
                if (this.mustPerformClosingTasks)
                {
                    e.Cancel = true;
                    this.PerformClosingTasksAsync();
                }
            }
        }

        private async Task PerformClosingTasksAsync()
        {
            LogClient.Info("Performing closing tasks");

            this.ShowClosingAnimation();

            // Write the settings
            // ------------------
            SettingsClient.Write();

            // Save queued tracks
            // ------------------
            if (this.playbackService.IsSavingQueuedTracks)
            {
                while (this.playbackService.IsSavingQueuedTracks)
                {
                    await Task.Delay(50);
                }
            }
            else
            {
                await this.playbackService.SaveQueuedTracksAsync();
            }

            // Stop playing
            // ------------
            this.playbackService.Stop();

            // Update file metadata
            // --------------------
            await this.metadataService.SafeUpdateFileMetadataAsync();

            // Save track statistics
            // ---------------------
            if (this.playbackService.IsSavingPlaybackCounters)
            {
                while (this.playbackService.IsSavingPlaybackCounters)
                {
                    await Task.Delay(50);
                }
            }
            else
            {
                await this.playbackService.SavePlaybackCountersAsync();
            }

            LogClient.Info("### STOPPING {0}, version {1} ###", ProductInformation.ApplicationName, ProcessExecutable.AssemblyVersion().ToString());

            this.mustPerformClosingTasks = false;
            this.Close();
        }

        private void Shell_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.SaveWindowSize();
        }

        private void Shell_LocationChanged(object sender, EventArgs e)
        {
            // We need to put SaveWindowLocation() in the queue of the Dispatcher.
            // SaveWindowLocation() needs to be executed after LocationChanged was 
            // handled, when the WindowState has been updated otherwise we get 
            // incorrect values for Left and Top (both -7 last I checked).
            this.Dispatcher.BeginInvoke(new Action(() => this.SaveWindowLocation()));
        }

        private void Shell_Closed(object sender, EventArgs e)
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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Retrieve BackgroundAnimation storyboard
            // ---------------------------------------
            this.backgroundAnimation = this.WindowBorder.Resources["BackgroundAnimation"] as Storyboard;
            if (this.backgroundAnimation != null) this.backgroundAnimation.Begin();
        }

        private void ShowClosingAnimation()
        {
            this.ShowWindowControls = false;
            Storyboard closingAnimation = this.ClosingBorder.Resources["ClosingAnimation"] as Storyboard;

            this.ClosingBorder.Visibility = Visibility.Visible;
            closingAnimation.Begin();
        }

        private void Shell_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (e.OriginalSource is TextBox) return; // Don't interfere with typing in a TextBox
                e.Handled = true; // Prevents typing in the search box
                this.playbackService.PlayOrPauseAsync();
            }
        }

        private void Shell_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                // [Ctrl] is pressed
                if (e.Key == Key.L)
                {
                    e.Handled = true; // Prevents typing in the search box

                    try
                    {
                        Actions.TryViewInExplorer(LogClient.Logfile()); // View the log file
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Could not view the log file {0} in explorer. Exception: {1}", LogClient.Logfile(), ex.Message);
                    }
                }
                else if (e.Key == Key.OemPlus | e.Key == Key.Add)
                {
                    e.Handled = true; // Prevents typing in the search box
                    this.playbackService.Volume = Convert.ToSingle(this.playbackService.Volume + 0.01);
                }
                else if (e.Key == Key.OemMinus | e.Key == Key.Subtract)
                {
                    e.Handled = true; // Prevents typing in the search box
                    this.playbackService.Volume = Convert.ToSingle(this.playbackService.Volume - 0.01);
                }
                else if (e.Key == Key.Left)
                {
                    e.Handled = true; // Prevents typing in the search box
                    this.playbackService.SkipSeconds(Convert.ToInt32(-5));
                }
                else if (e.Key == Key.Right)
                {
                    e.Handled = true; // Prevents typing in the search box
                    this.playbackService.SkipSeconds(Convert.ToInt32(5));
                }
            }
            else
            {
                // [Ctrl] is not pressed
                if (e.Key == Key.OemPlus | e.Key == Key.Add)
                {
                    if (e.OriginalSource is TextBox) return; // Don't interfere with typing in a TextBox
                    e.Handled = true; // Prevents typing in the search box
                    this.playbackService.Volume = Convert.ToSingle(this.playbackService.Volume + 0.05);
                }
                else if (e.Key == Key.OemMinus | e.Key == Key.Subtract)
                {
                    if (e.OriginalSource is TextBox) return; // Don't interfere with typing in a TextBox
                    e.Handled = true; // Prevents typing in the search box
                    this.playbackService.Volume = Convert.ToSingle(this.playbackService.Volume - 0.05);
                }
                else if (e.Key == Key.Left)
                {
                    if (e.OriginalSource is TextBox) return; // Don't interfere with typing in a TextBox
                    e.Handled = true; // Prevents typing in the search box
                    this.playbackService.SkipSeconds(Convert.ToInt32(-15));
                }
                else if (e.Key == Key.Right)
                {
                    if (e.OriginalSource is TextBox) return; // Don't interfere with typing in a TextBox
                    e.Handled = true; // Prevents typing in the search box
                    this.playbackService.SkipSeconds(Convert.ToInt32(15));
                }
            }
        }

        private async void Shell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.XButton1)
            {
                await playbackService.PlayPreviousAsync();
            }
            else if (e.ChangedButton == MouseButton.XButton2)
            {
                await playbackService.PlayNextAsync();
            }
        }

        private void Shell_Restored(object sender, EventArgs e)
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

        private void Shell_CloseToolTipChanged(object sender, EventArgs e)
        {
            this.PART_MiniPlayerButton.ToolTip = ResourceUtils.GetString("Language_Mini_Player");
        }
        #endregion
    }
}
