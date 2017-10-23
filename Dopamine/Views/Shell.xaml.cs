using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Base;
using Dopamine.Common.Controls;
using Dopamine.Common.Enums;
using Dopamine.Common.Extensions;
using Dopamine.Common.IO;
using Dopamine.Common.Prism;
using Dopamine.Common.Services.Appearance;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Notification;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Services.Win32Input;
using Dopamine.Common.Services.WindowsIntegration;
using Dopamine.Views.Common;
using Dopamine.Views.MiniPlayer;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace Dopamine.Views
{
    public partial class Shell : DopamineWindow
    {
        private IUnityContainer container;
        private IWindowsIntegrationService windowsIntegrationService;
        private INotificationService notificationService;
        private IWin32InputService win32InputService;
        private IPlaybackService playbackService;
        private IMetadataService metadataService;
        private IAppearanceService appearanceService;
        private IRegionManager regionManager;
        private IEventAggregator eventAggregator;
        private System.Windows.Forms.NotifyIcon trayIcon;
        private ContextMenu trayIconContextMenu;
        private TrayControls trayControls;
        private MiniPlayerPlaylist miniPlayerPlaylist;
        private bool canSaveWindowGeometry = false;
        private bool mustPerformClosingTasks = true;
        private bool isShuttingDown = false;
        private Storyboard backgroundAnimation;

        private ActiveMiniPlayerPlaylist activeMiniPlayerPlaylist = ActiveMiniPlayerPlaylist.None;

        public DelegateCommand ShowNowPlayingCommand { get; set; }
        public DelegateCommand ShowFullPlayerCommmand { get; set; }
        public DelegateCommand TogglePlayerCommand { get; set; }
        public DelegateCommand<string> ChangePlayerTypeCommand { get; set; }
        public DelegateCommand RestoreWindowCommand { get; set; }
        public DelegateCommand MinimizeWindowCommand { get; set; }
        public DelegateCommand MaximizeRestoreWindowCommand { get; set; }
        public DelegateCommand CloseWindowCommand { get; set; }
        public DelegateCommand<bool?> CoverPlayerPlaylistButtonCommand { get; set; }
        public DelegateCommand<bool?> MicroPlayerPlaylistButtonCommand { get; set; }
        public DelegateCommand<bool?> NanoPlayerPlaylistButtonCommand { get; set; }
        public DelegateCommand ToggleMiniPlayerPositionLockedCommand { get; set; }
        public DelegateCommand ToggleMiniPlayerAlwaysOnTopCommand { get; set; }

        public Shell(IUnityContainer container, IWindowsIntegrationService windowsIntegrationService,
            INotificationService notificationService, IWin32InputService win32InputService, IAppearanceService appearanceService,
            IPlaybackService playbackService, IMetadataService metadataService, IRegionManager regionManager,
            IEventAggregator eventAggregator)
        {
            InitializeComponent();

            this.container = container;
            this.windowsIntegrationService = windowsIntegrationService;
            this.notificationService = notificationService;
            this.win32InputService = win32InputService;
            this.playbackService = playbackService;
            this.metadataService = metadataService;
            this.appearanceService = appearanceService;
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;

            this.InitializeTrayIcon();
            this.InitializeShellWindow();
            this.InitializeCommands();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Retrieve BackgroundAnimation storyboard
            this.backgroundAnimation = this.WindowBorder.Resources["BackgroundAnimation"] as Storyboard;
            if (this.backgroundAnimation != null) this.backgroundAnimation.Begin();
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

        private void TrayIconContextMenuAppName_Click(object sender, RoutedEventArgs e)
        {
            this.ShowWindowInForeground();
        }

        private void TrayIconContextMenuExit_Click(object sender, RoutedEventArgs e)
        {
            this.isShuttingDown = true;
            this.Close();
        }

        private void ShellWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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

        private void ShowClosingAnimation()
        {
            this.ShowWindowControls = false;
            Storyboard closingAnimation = this.ClosingBorder.Resources["ClosingAnimation"] as Storyboard;

            this.ClosingBorder.Visibility = Visibility.Visible;
            closingAnimation.Begin();
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

        private void InitializeShellWindow()
        {
            // Start monitoring tablet mode
            this.windowsIntegrationService.StartMonitoringTabletMode();

            // Tray controls
            this.trayControls = this.container.Resolve<TrayControls>();

            // Create the Mini Player playlist
            this.miniPlayerPlaylist = this.container.Resolve<MiniPlayerPlaylist>(new DependencyOverride(typeof(DopamineWindow), this));

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
            this.CheckIfTabletMode(true);
        }

        private void InitializeCommands()
        {
            // Window State
            this.MinimizeWindowCommand = new DelegateCommand(() => this.WindowState = WindowState.Minimized);
            ApplicationCommands.MinimizeWindowCommand.RegisterCommand(this.MinimizeWindowCommand);

            this.RestoreWindowCommand = new DelegateCommand(() => this.SetPlayer(false, MiniPlayerType.CoverPlayer));
            ApplicationCommands.RestoreWindowCommand.RegisterCommand(this.RestoreWindowCommand);

            this.MaximizeRestoreWindowCommand = new DelegateCommand(() => this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized);
            ApplicationCommands.MaximizeRestoreWindowCommand.RegisterCommand(this.MaximizeRestoreWindowCommand);

            this.CloseWindowCommand = new DelegateCommand(() => this.Close());
            ApplicationCommands.CloseWindowCommand.RegisterCommand(this.CloseWindowCommand);

            this.ShowNowPlayingCommand = new DelegateCommand(() =>
            {
                this.regionManager.RequestNavigate(RegionNames.PlayerTypeRegion, typeof(NowPlaying.NowPlaying).FullName);
                SettingsClient.Set<bool>("FullPlayer", "IsNowPlayingSelected", true);
                this.eventAggregator.GetEvent<IsNowPlayingPageActiveChanged>().Publish(true);
            });
            ApplicationCommands.ShowNowPlayingCommand.RegisterCommand(this.ShowNowPlayingCommand);

            this.ShowFullPlayerCommmand = new DelegateCommand(() =>
            {
                this.regionManager.RequestNavigate(RegionNames.PlayerTypeRegion, typeof(FullPlayer.FullPlayer).FullName);
                SettingsClient.Set<bool>("FullPlayer", "IsNowPlayingSelected", false);
                this.eventAggregator.GetEvent<IsNowPlayingPageActiveChanged>().Publish(false);
            });
            ApplicationCommands.ShowFullPlayerCommand.RegisterCommand(this.ShowFullPlayerCommmand);

            // Player type
            this.ChangePlayerTypeCommand = new DelegateCommand<string>((miniPlayerType) => this.SetPlayer(true, (MiniPlayerType)Convert.ToInt32(miniPlayerType)));
            ApplicationCommands.ChangePlayerTypeCommand.RegisterCommand(this.ChangePlayerTypeCommand);

            this.TogglePlayerCommand = new DelegateCommand(() =>
            {
                // If tablet mode is enabled, we should not be able to toggle the player.
                if (!this.windowsIntegrationService.IsTabletModeEnabled)
                {
                    this.TogglePlayer();
                }
            });
            ApplicationCommands.TogglePlayerCommand.RegisterCommand(this.TogglePlayerCommand);

            // Mini Player Playlist
            this.CoverPlayerPlaylistButtonCommand = new DelegateCommand<bool?>(isPlaylistButtonChecked =>
            {
                this.ToggleMiniPlayerPlaylist(MiniPlayerType.CoverPlayer, isPlaylistButtonChecked.Value);
            });
            ApplicationCommands.CoverPlayerPlaylistButtonCommand.RegisterCommand(this.CoverPlayerPlaylistButtonCommand);

            this.MicroPlayerPlaylistButtonCommand = new DelegateCommand<bool?>(isPlaylistButtonChecked =>
            {
                this.ToggleMiniPlayerPlaylist(MiniPlayerType.MicroPlayer, isPlaylistButtonChecked.Value);
            });
            ApplicationCommands.MicroPlayerPlaylistButtonCommand.RegisterCommand(this.MicroPlayerPlaylistButtonCommand);

            this.NanoPlayerPlaylistButtonCommand = new DelegateCommand<bool?>(isPlaylistButtonChecked =>
            {
                this.ToggleMiniPlayerPlaylist(MiniPlayerType.NanoPlayer, isPlaylistButtonChecked.Value);
            });
            ApplicationCommands.NanoPlayerPlaylistButtonCommand.RegisterCommand(this.NanoPlayerPlaylistButtonCommand);

            // Mini Player
            this.ToggleMiniPlayerPositionLockedCommand = new DelegateCommand(() =>
            {
                bool isMiniPlayerPositionLocked = SettingsClient.Get<bool>("Behaviour", "MiniPlayerPositionLocked");
                SettingsClient.Set<bool>("Behaviour", "MiniPlayerPositionLocked", !isMiniPlayerPositionLocked);
                this.SetWindowPositionLockedFromSettings();
            });
            ApplicationCommands.ToggleMiniPlayerPositionLockedCommand.RegisterCommand(this.ToggleMiniPlayerPositionLockedCommand);

            this.ToggleMiniPlayerAlwaysOnTopCommand = new DelegateCommand(() =>
            {
                bool topmost = SettingsClient.Get<bool>("Behaviour", "MiniPlayerOnTop");
                SettingsClient.Set<bool>("Behaviour", "MiniPlayerOnTop", !topmost);
                this.SetWindowTopmostFromSettings();
            });
            ApplicationCommands.ToggleMiniPlayerAlwaysOnTopCommand.RegisterCommand(this.ToggleMiniPlayerAlwaysOnTopCommand);
        }

        private async void InitializeServicesAsync()
        {
            // IWin32InputService
            this.win32InputService.SetKeyboardHook(new WindowInteropHelper(this).EnsureHandle()); // listen to media keys
            this.win32InputService.MediaKeyNextPressed += async (_, __) => await this.playbackService.PlayNextAsync();
            this.win32InputService.MediaKeyPreviousPressed += async (_, __) => await this.playbackService.PlayPreviousAsync();
            this.win32InputService.MediaKeyPlayPressed += async (_, __) => await this.playbackService.PlayOrPauseAsync();

            // IAppearanceService
            this.appearanceService.ThemeChanged += this.ThemeChangedHandler;

            // IWindowsIntegrationService
            this.windowsIntegrationService.TabletModeChanged += (_, __) =>
            {
                Application.Current.Dispatcher.Invoke(() => this.CheckIfTabletMode(false));
            };
        }

        private void ThemeChangedHandler(bool useLightTheme)
        {
            Application.Current.Dispatcher.Invoke(() => { if (this.backgroundAnimation != null) this.backgroundAnimation.Begin(); });
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

        private void CheckIfTabletMode(bool isInitializing)
        {
            if (this.windowsIntegrationService.IsTabletModeEnabled)
            {
                // Always revert to full player when tablet mode is enabled. Maximizing will be done by Windows.
                this.SetPlayer(false, (MiniPlayerType)SettingsClient.Get<int>("General", "MiniPlayerType"), isInitializing);
            }
            else
            {
                bool isMiniPlayer = SettingsClient.Get<bool>("General", "IsMiniPlayer");
                bool isMaximized = SettingsClient.Get<bool>("FullPlayer", "IsMaximized");
                this.WindowState = isMaximized & !isMiniPlayer ? WindowState.Maximized : WindowState.Normal;

                this.SetPlayer(isMiniPlayer, (MiniPlayerType)SettingsClient.Get<int>("General", "MiniPlayerType"), isInitializing);
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

        private async void SetPlayer(bool isMiniPlayer, MiniPlayerType miniPlayerType, bool isInitializing = false)
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
                        this.SetMiniPlayer(MiniPlayerType.CoverPlayer, this.activeMiniPlayerPlaylist == ActiveMiniPlayerPlaylist.CoverPlayer);
                        screenName = typeof(CoverPlayer).FullName;
                        break;
                    case MiniPlayerType.MicroPlayer:
                        this.SetMiniPlayer(MiniPlayerType.MicroPlayer, this.activeMiniPlayerPlaylist == ActiveMiniPlayerPlaylist.MicroPlayer);
                        screenName = typeof(MicroPlayer).FullName;
                        break;
                    case MiniPlayerType.NanoPlayer:
                        this.SetMiniPlayer(MiniPlayerType.NanoPlayer, this.activeMiniPlayerPlaylist == ActiveMiniPlayerPlaylist.NanoPlayer);
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

                // Default case
                screenName = typeof(FullPlayer.FullPlayer).FullName;

                // Special cases
                if (SettingsClient.Get<bool>("FullPlayer", "IsNowPlayingSelected"))
                {
                    if (isInitializing)
                    {
                        if (SettingsClient.Get<bool>("Startup", "ShowLastSelectedPage"))
                        {
                            screenName = typeof(NowPlaying.NowPlaying).FullName;
                        }
                    }
                    else
                    {
                        screenName = typeof(NowPlaying.NowPlaying).FullName;
                    }
                }
            }

            // Determine if the player position is locked
            this.SetWindowPositionLockedFromSettings();

            // Delay, otherwise content is never shown (probably because regions don't exist yet at startup)
            await Task.Delay(150);

            // Navigate to content
            this.regionManager.RequestNavigate(RegionNames.PlayerTypeRegion, screenName);

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

        private void SetMiniPlayer(MiniPlayerType miniPlayerType, bool openPlaylist)
        {
            // Hide the playlist BEFORE changing window dimensions to avoid strange behaviour
            this.miniPlayerPlaylist.Hide();

            this.WindowState = WindowState.Normal;
            this.ResizeMode = ResizeMode.CanMinimize;
            this.ShowWindowControls = false;

            double width = 0;
            double height = 0;

            switch (miniPlayerType)
            {
                case MiniPlayerType.CoverPlayer:
                    this.ClosingText.FontSize = Constants.MediumBackgroundFontSize;
                    width = Constants.CoverPlayerWidth;
                    height = Constants.CoverPlayerHeight;
                    break;
                case MiniPlayerType.MicroPlayer:
                    this.ClosingText.FontSize = Constants.MediumBackgroundFontSize;
                    width = Constants.MicroPlayerWidth;
                    height = Constants.MicroPlayerHeight;
                    break;
                case MiniPlayerType.NanoPlayer:
                    this.ClosingText.FontSize = Constants.SmallBackgroundFontSize;
                    width = Constants.NanoPlayerWidth;
                    height = Constants.NanoPlayerHeight;
                    break;
                default:
                    // Can't happen
                    break;
            }

            // Set MinWidth and MinHeight BEFORE SetMiniPlayerDimensions(). This prevents flicker.
            if (this.HasBorder)
            {
                // Correction to take into account the window border, otherwise the content 
                // misses 2px horizontally and vertically when displaying the window border
                this.MinWidth = width + 2;
                this.MinHeight = height + 2;
            }
            else
            {
                this.MinWidth = width;
                this.MinHeight = height;
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
            if (openPlaylist)
            {
                this.miniPlayerPlaylist.Show(miniPlayerType);
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

        private void ShellWindow_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.eventAggregator.GetEvent<ShellMouseUp>().Publish(null);
        }

        private void ToggleMiniPlayerPlaylist(MiniPlayerType miniPlayerType, bool openPlaylist)
        {
            switch (miniPlayerType)
            {
                case MiniPlayerType.CoverPlayer:
                    this.activeMiniPlayerPlaylist = ActiveMiniPlayerPlaylist.CoverPlayer;
                    break;
                case MiniPlayerType.MicroPlayer:
                    this.activeMiniPlayerPlaylist = ActiveMiniPlayerPlaylist.MicroPlayer;
                    break;
                case MiniPlayerType.NanoPlayer:
                    this.activeMiniPlayerPlaylist = ActiveMiniPlayerPlaylist.NanoPlayer;
                    break;
                default:
                    break;
                    // Shouldn't happen
            }

            if (openPlaylist)
            {
                this.miniPlayerPlaylist.Show(miniPlayerType);
            }
            else
            {
                this.miniPlayerPlaylist.Hide();
            }
        }
    }
}
