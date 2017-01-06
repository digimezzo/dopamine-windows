using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Controls;
using Dopamine.Common.Enums;
using Dopamine.Common.Presentation.Views;
using Dopamine.Common.Services.Appearance;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Notification;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Services.Win32Input;
using Dopamine.Common.Base;
using Dopamine.Common.Extensions;
using Dopamine.Common.IO;
using Digimezzo.Utilities.Log;
using Dopamine.Common.Prism;
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
        private bool allowSaveWindowGeometry = false;
        private Storyboard backgroundAnimation;
        private IUnityContainer container;
        private readonly IRegionManager regionManager;
        private IAppearanceService appearanceService;
        private IPlaybackService playbackService;
        private IWin32InputService win32InputService;
        private INotificationService notificationService;
        private MetadataService metadataService;
        private System.Windows.Forms.NotifyIcon trayIcon;
        private ContextMenu trayIconContextMenu;
        private bool isMiniPlayer;
        private MiniPlayerType selectedMiniPlayerType;
        private bool isCoverPlayerListExpanded;
        private bool isMicroPlayerListExpanded;
        private bool isNanoPlayerListExpanded;
        private bool isMiniPlayerPositionLocked;
        private bool isMiniPlayerAlwaysOnTop;
        private IEventAggregator eventAggregator;
        private TrayControls trayControls;
        private Storyboard enableWindowTransparencyStoryboard;
        private Storyboard disableWindowTransparencyStoryboard;
        private Playlist miniPlayerPlaylist;
        private bool isShuttingDown;
        private bool mustPerformClosingTasks;
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
        public Shell(IUnityContainer container, IRegionManager regionManager, IAppearanceService appearanceService, IPlaybackService playbackService, IWin32InputService win32InputService, IEventAggregator eventAggregator, INotificationService notificationService, MetadataService metadataService)
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

            // Flags
            this.mustPerformClosingTasks = true;

            // Window
            this.InitializeWindow();

            // Tray icon
            this.InitializeTrayIcon();

            // Services
            this.InitializeServicesAsync();

            // PubSub Events
            this.InitializePubSubEvents();

            // Commands
            this.InitializeCommands();
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
        }

        private void InitializePubSubEvents()
        {
            // Window border
            // -------------
            this.eventAggregator.GetEvent<SettingShowWindowBorderChanged>().Subscribe(showWindowBorder => this.SetWindowBorder(showWindowBorder));

            // Cover Player
            // ------------
            this.eventAggregator.GetEvent<CoverPlayerPlaylistButtonClicked>().Subscribe(isPlaylistButtonChecked => this.ToggleMiniPlayerPlaylist(MiniPlayerType.CoverPlayer, isPlaylistButtonChecked));

            // Micro Player
            // ------------
            this.eventAggregator.GetEvent<MicroPlayerPlaylistButtonClicked>().Subscribe(isPlaylistButtonChecked => this.ToggleMiniPlayerPlaylist(MiniPlayerType.MicroPlayer, isPlaylistButtonChecked));

            // Nano Player
            // -----------
            this.eventAggregator.GetEvent<NanoPlayerPlaylistButtonClicked>().Subscribe(isPlaylistButtonChecked => this.ToggleMiniPlayerPlaylist(MiniPlayerType.NanoPlayer, isPlaylistButtonChecked));

            // Tray icon
            // ---------
            this.eventAggregator.GetEvent<SettingShowTrayIconChanged>().Subscribe(showTrayIcon => this.trayIcon.Visible = showTrayIcon);
        }

        private void InitializeCommands()
        {
            // TaskbarItemInfo
            // ---------------
            TaskbarItemInfoPlayCommand = new DelegateCommand(async () => await this.playbackService.PlayOrPauseAsync());
            Common.Prism.ApplicationCommands.TaskbarItemInfoPlayCommand.RegisterCommand(this.TaskbarItemInfoPlayCommand);

            // Window State
            // ------------
            this.MinimizeWindowCommand = new DelegateCommand(() => this.WindowState = WindowState.Minimized);
            Common.Prism.ApplicationCommands.MinimizeWindowCommand.RegisterCommand(this.MinimizeWindowCommand);

            this.RestoreWindowCommand = new DelegateCommand(() => this.SetPlayer(false, MiniPlayerType.CoverPlayer));
            Common.Prism.ApplicationCommands.RestoreWindowCommand.RegisterCommand(this.RestoreWindowCommand);

            this.MaximizeRestoreWindowCommand = new DelegateCommand(() =>
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                }
            });

            Common.Prism.ApplicationCommands.MaximizeRestoreWindowCommand.RegisterCommand(this.MaximizeRestoreWindowCommand);

            this.CloseWindowCommand = new DelegateCommand(() => this.Close());
            Common.Prism.ApplicationCommands.CloseWindowCommand.RegisterCommand(this.CloseWindowCommand);

            // Player type
            // -----------
            this.ChangePlayerTypeCommand = new DelegateCommand<string>((miniPlayerType) => this.SetPlayer(true, (MiniPlayerType)Convert.ToInt32(miniPlayerType)));
            Common.Prism.ApplicationCommands.ChangePlayerTypeCommand.RegisterCommand(this.ChangePlayerTypeCommand);

            this.TogglePlayerCommand = new DelegateCommand(() => this.TogglePlayer());
            Common.Prism.ApplicationCommands.TogglePlayerCommand.RegisterCommand(this.TogglePlayerCommand);

            // Mini Player
            // -----------
            this.isMiniPlayerPositionLocked = SettingsClient.Get<bool>("Behaviour", "MiniPlayerPositionLocked");
            this.isMiniPlayerAlwaysOnTop = SettingsClient.Get<bool>("Behaviour", "MiniPlayerOnTop");

            this.ToggleMiniPlayerPositionLockedCommand = new DelegateCommand(() =>
            {
                this.isMiniPlayerPositionLocked = !isMiniPlayerPositionLocked;
                this.SetWindowPositionLocked(isMiniPlayer);
            });

            Common.Prism.ApplicationCommands.ToggleMiniPlayerPositionLockedCommand.RegisterCommand(this.ToggleMiniPlayerPositionLockedCommand);

            this.ToggleMiniPlayerAlwaysOnTopCommand = new DelegateCommand(() =>
            {
                this.isMiniPlayerAlwaysOnTop = !this.isMiniPlayerAlwaysOnTop;
                this.SetWindowAlwaysOnTop(this.isMiniPlayer);
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

            this.ShowMainWindowCommand = new DelegateCommand(() => this.ActivateNow());
            Common.Prism.ApplicationCommands.ShowMainWindowCommand.RegisterCommand(this.ShowMainWindowCommand);
        }

        private void InitializeTrayIcon()
        {
            this.trayIcon = new System.Windows.Forms.NotifyIcon();
            this.trayIcon.Text = ProductInformation.ApplicationAssemblyName;

            // Reflection is needed to get the full path of the executable. Because when starting the application from the start menu
            // without specifying the full path, the application fails to find the Tray icon and crashes here
            string iconFile = EnvironmentUtils.IsWindows10() ? "Tray.ico" : "Legacy tray.ico";

            string iconPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ApplicationPaths.IconsSubDirectory, iconFile);
            this.trayIcon.Icon = new System.Drawing.Icon(iconPath, System.Windows.Forms.SystemInformation.SmallIconSize);

            this.trayIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(TrayIcon_MouseClick);
            this.trayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(TrayIcon_MouseDoubleClick);

            this.trayIconContextMenu = (ContextMenu)this.FindResource("TrayIconContextMenu");

            if (SettingsClient.Get<bool>("Behaviour", "ShowTrayIcon"))
            {
                this.trayIcon.Visible = true;
            }
        }

        private void InitializeWindow()
        {
            this.trayControls = this.container.Resolve<Views.TrayControls>();

            this.miniPlayerPlaylist = this.container.Resolve<Views.Playlist>(new DependencyOverride(typeof(DopamineWindow), this));
            RegionManager.SetRegionManager(this.miniPlayerPlaylist, this.regionManager);
            RegionManager.UpdateRegions();

            this.notificationService.SetApplicationWindows(this, this.miniPlayerPlaylist, this.trayControls);

            // Handler
            this.Restored += Shell_Restored;

            // Workaround to make sure the PART_MiniPlayerButton ToolTip also gets updated on a language change
            this.CloseToolTipChanged += Shell_CloseToolTipChanged;

            // This makes sure the position and size of the window is correct and avoids jumping of 
            // the window to the correct position when SetPlayer() is called later while starting up
            this.SetPlayerType(SettingsClient.Get<bool>("General", "IsMiniPlayer"), (MiniPlayerType)SettingsClient.Get<int>("General", "MiniPlayerType"));
        }

        private void TogglePlayer()
        {
            if (this.isMiniPlayer)
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

        private void SetWindowPositionLocked(bool isMiniPlayer)
        {
            // Only lock position when the mini player is active
            if (isMiniPlayer)
            {
                this.IsMovable = !this.isMiniPlayerPositionLocked;
            }
            else
            {
                this.IsMovable = true;
            }

        }

        private void SetWindowAlwaysOnTop(bool isMiniPlayer)
        {
            if (isMiniPlayer)
            {
                this.Topmost = this.isMiniPlayerAlwaysOnTop;
            }
            else
            {
                this.Topmost = false;
            }
        }

        private void SetPlayer(bool isMiniPlayer, MiniPlayerType miniPlayerType)
        {
            // Clear the player's content for smoother Window resizing
            this.ClearPlayerContent();

            // Determine if the player position is locked
            this.SetWindowPositionLocked(isMiniPlayer);

            // Set the player type
            this.SetPlayerType(isMiniPlayer, miniPlayerType);

            // Set the content of the player window
            this.SetPlayerContent(150);
        }

        private void SetPlayerType(bool isMiniPlayer, MiniPlayerType miniPlayerType)
        {
            // Save the player type in the settings
            SettingsClient.Set<bool>("General", "IsMiniPlayer", isMiniPlayer);

            // Only save the Mini Player Type in the settings if the current player is set to the Mini Player
            if (isMiniPlayer) SettingsClient.Set<int>("General", "MiniPlayerType", (int)miniPlayerType);

            // Set the current player type
            this.isMiniPlayer = isMiniPlayer;
            this.selectedMiniPlayerType = miniPlayerType;

            // Prevents saving window state and size to the Settings XML while switching players
            this.allowSaveWindowGeometry = false;

            // Sets the geometry of the player

            if (isMiniPlayer)
            {
                PART_MiniPlayerButton.ToolTip = ResourceUtils.GetStringResource("Language_Restore");

                switch (miniPlayerType)
                {
                    case MiniPlayerType.CoverPlayer:
                        this.ClosingText.FontSize = Constants.MediumBackgroundFontSize;
                        this.SetMiniPlayer(MiniPlayerType.CoverPlayer, Constants.CoverPlayerWidth, Constants.CoverPlayerHeight, this.isCoverPlayerListExpanded);
                        break;
                    case MiniPlayerType.MicroPlayer:
                        this.ClosingText.FontSize = Constants.MediumBackgroundFontSize;
                        this.SetMiniPlayer(MiniPlayerType.MicroPlayer, Constants.MicroPlayerWidth, Constants.MicroPlayerHeight, this.isMicroPlayerListExpanded);
                        break;
                    case MiniPlayerType.NanoPlayer:
                        this.ClosingText.FontSize = Constants.SmallBackgroundFontSize;
                        this.SetMiniPlayer(MiniPlayerType.NanoPlayer, Constants.NanoPlayerWidth, Constants.NanoPlayerHeight, this.isNanoPlayerListExpanded);
                        break;
                    default:
                        break;
                        // Doesn't happen
                }
            }
            else
            {
                this.ClosingText.FontSize = Constants.LargeBackgroundFontSize;
                PART_MiniPlayerButton.ToolTip = ResourceUtils.GetStringResource("Language_Mini_Player");
                this.SetFullPlayer();
            }

            this.allowSaveWindowGeometry = true;
        }

        private void ClearPlayerContent()
        {
            this.regionManager.RequestNavigate(RegionNames.PlayerTypeRegion, typeof(Empty).FullName);
        }

        private async void SetPlayerContent(int delayMilliseconds = 0)
        {
            // This delay makes sure the content of the window is shown only after the specified delay
            await Task.Delay(delayMilliseconds);

            if (this.isMiniPlayer)
            {
                switch (this.selectedMiniPlayerType)
                {
                    case MiniPlayerType.CoverPlayer:
                        this.regionManager.RequestNavigate(RegionNames.PlayerTypeRegion, typeof(CoverPlayer).FullName);
                        break;
                    case MiniPlayerType.MicroPlayer:
                        this.regionManager.RequestNavigate(RegionNames.PlayerTypeRegion, typeof(MicroPlayer).FullName);
                        break;
                    case MiniPlayerType.NanoPlayer:
                        this.regionManager.RequestNavigate(RegionNames.PlayerTypeRegion, typeof(NanoPlayer).FullName);
                        break;
                    default:
                        break;
                        // Doesn't happen
                }
            }
            else
            {
                this.regionManager.RequestNavigate(RegionNames.PlayerTypeRegion, typeof(FullPlayer).FullName);
            }
        }

        private void SaveWindowState()
        {
            if (this.allowSaveWindowGeometry)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    SettingsClient.Set<bool>("FullPlayer", "IsMaximized", true);
                }
                else
                {
                    SettingsClient.Set<bool>("FullPlayer", "IsMaximized", false);
                }
            }
        }

        private void SaveWindowSize()
        {
            if (this.allowSaveWindowGeometry)
            {
                if (!this.isMiniPlayer & !(this.WindowState == WindowState.Maximized))
                {
                    SettingsClient.Set<int>("FullPlayer", "Width", Convert.ToInt32(this.ActualWidth));
                    SettingsClient.Set<int>("FullPlayer", "Height", Convert.ToInt32(this.ActualHeight));
                }
            }
        }

        private void SaveWindowLocation()
        {
            if (this.allowSaveWindowGeometry)
            {
                if (this.isMiniPlayer)
                {
                    SettingsClient.Set<int>("MiniPlayer", "Top", Convert.ToInt32(this.Top));
                    SettingsClient.Set<int>("MiniPlayer", "Left", Convert.ToInt32(this.Left));
                }
                else if (!this.isMiniPlayer & !(this.WindowState == WindowState.Maximized))
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

            // The FullPlayer window is never TopMost
            this.Topmost = false;
        }

        private void SetMiniPlayerDimensions()
        {
            this.SetGeometry(
                SettingsClient.Get<int>("MiniPlayer", "Top"),
                SettingsClient.Get<int>("MiniPlayer", "Left"),
                Convert.ToInt32(this.MinWidth),
                Convert.ToInt32(this.MinHeight),
                Constants.DefaultShellTop,
                Constants.DefaultShellLeft);

            this.SetWindowAlwaysOnTop(true);
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

            this.SetMiniPlayerDimensions();

            // Show the playlist AFTER changing window dimensions to avoid strange behaviour
            if (isMiniPlayerListExpanded) this.miniPlayerPlaylist.Show(miniPlayerType);
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
        #endregion

        #region Event Handlers
        private void Shell_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.eventAggregator.GetEvent<ShellMouseUp>().Publish(null);
        }

        private void Shell_ContentRendered(object sender, EventArgs e)
        {
            // Corrects size of the window (taking into account the HasBorders Property which is now set) and sets the content of the player
            this.SetPlayer(SettingsClient.Get<bool>("General", "IsMiniPlayer"), (MiniPlayerType)SettingsClient.Get<int>("General", "MiniPlayerType"));
        }

        private void ThemeChangedHandler(object sender, EventArgs e)
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

        private void TrayIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.ActivateNow();
        }

        private void TrayIconContextMenuAppName_Click(object sender, RoutedEventArgs e)
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
            LogClient.Info("### STOPPING {0}, version {1} ###", ProductInformation.ApplicationDisplayName, ProcessExecutable.AssemblyVersion().ToString());
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
            if (this.playbackService.IsSavingTrackStatistics)
            {
                while (this.playbackService.IsSavingTrackStatistics)
                {
                    await Task.Delay(50);
                }
            }
            else
            {
                await this.playbackService.SaveTrackStatisticsAsync();
            }

            LogClient.Info("### STOPPING {0}, version {1} ###", ProductInformation.ApplicationDisplayName, ProcessExecutable.AssemblyVersion().ToString());

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

        private void Shell_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemPlus | e.Key == Key.Add)
            {
                // [Ctrl] allows fine-tuning of the volume
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    this.playbackService.Volume = Convert.ToSingle(this.playbackService.Volume + 0.01);
                }
                else
                {
                    this.playbackService.Volume = Convert.ToSingle(this.playbackService.Volume + 0.05);
                }
            }
            else if (e.Key == Key.OemMinus | e.Key == Key.Subtract)
            {
                // [Ctrl] allows fine-tuning of the volume
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    this.playbackService.Volume = Convert.ToSingle(this.playbackService.Volume - 0.01);
                }
                else
                {
                    this.playbackService.Volume = Convert.ToSingle(this.playbackService.Volume - 0.05);
                }

            }
            else if (Keyboard.Modifiers == ModifierKeys.Control & e.Key == Key.L)
            {
                // View the log file
                try
                {
                    Actions.TryViewInExplorer(LogClient.Logfile());
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not view the log file {0} in explorer. Exception: {1}", LogClient.Logfile(), ex.Message);
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
            this.PART_MiniPlayerButton.ToolTip = ResourceUtils.GetStringResource("Language_Mini_Player");
        }
        #endregion
    }
}
