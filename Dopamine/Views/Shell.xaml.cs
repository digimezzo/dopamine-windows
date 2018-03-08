using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Digimezzo.WPFControls;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Core.IO;
using Dopamine.Core.Prism;
using Dopamine.Services.Contracts.Appearance;
using Dopamine.Services.Contracts.I18n;
using Dopamine.Services.Contracts.Metadata;
using Dopamine.Services.Contracts.Notification;
using Dopamine.Services.Contracts.Playback;
using Dopamine.Services.Contracts.Shell;
using Dopamine.Services.Contracts.Win32Input;
using Dopamine.Services.Contracts.WindowsIntegration;
using Dopamine.Views.Common;
using Dopamine.Views.MiniPlayer;
using Prism.Commands;
using Prism.Events;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using Unity;
using Unity.Resolution;

namespace Dopamine.Views
{
    public partial class Shell : BorderlessWindows10Window
    {
        private IUnityContainer container;
        private IWindowsIntegrationService windowsIntegrationService;
        private INotificationService notificationService;
        private IWin32InputService win32InputService;
        private IPlaybackService playbackService;
        private IMetadataService metadataService;
        private IAppearanceService appearanceService;
        private IShellService shellService;
        private II18nService i18nService;
        private IEventAggregator eventAggregator;
        private System.Windows.Forms.NotifyIcon trayIcon;
        private ContextMenu trayIconContextMenu;
        private TrayControls trayControls;
        private MiniPlayerPlaylist miniPlayerPlaylist;
        private bool mustPerformClosingTasks = true;
        private bool isShuttingDown = false;
        private Storyboard backgroundAnimation;

        public DelegateCommand RestoreWindowCommand { get; set; }
        public DelegateCommand MinimizeWindowCommand { get; set; }
        public DelegateCommand MaximizeRestoreWindowCommand { get; set; }
        public DelegateCommand CloseWindowCommand { get; set; }
        public DelegateCommand ShowMainWindowCommand { get; set; }

        public Shell(IUnityContainer container, IWindowsIntegrationService windowsIntegrationService, II18nService i18nService,
            INotificationService notificationService, IWin32InputService win32InputService, IAppearanceService appearanceService,
            IPlaybackService playbackService, IMetadataService metadataService, IEventAggregator eventAggregator)
        {
            InitializeComponent();

            this.container = container;
            this.windowsIntegrationService = windowsIntegrationService;
            this.notificationService = notificationService;
            this.win32InputService = win32InputService;
            this.playbackService = playbackService;
            this.metadataService = metadataService;
            this.appearanceService = appearanceService;
            this.i18nService = i18nService;
            this.eventAggregator = eventAggregator;

            this.shellService = container.Resolve<IShellService>(
                new ParameterOverride("nowPlayingPage", typeof(NowPlaying.NowPlaying).FullName),
                new ParameterOverride("fullPlayerPage", typeof(FullPlayer.FullPlayer).FullName),
                new ParameterOverride("coverPlayerPage", typeof(CoverPlayer).FullName),
                new ParameterOverride("microplayerPage", typeof(MicroPlayer).FullName),
                new ParameterOverride("nanoPlayerPage", typeof(NanoPlayer).FullName)
                );

            this.InitializeServices();
            this.InitializeWindows();
            this.InitializeTrayIcon();
            this.InitializeCommands();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Retrieve BackgroundAnimation storyboard
            this.backgroundAnimation = this.BackgroundBorder.Resources["BackgroundAnimation"] as Storyboard;

            if (this.backgroundAnimation != null)
            {
                this.backgroundAnimation.Begin();
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
            LogClient.Info("Writing settings");
            SettingsClient.Write();

            // Save queued tracks
            // ------------------
            LogClient.Info("Saving queued tracks");
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
            LogClient.Info("Stopping playback");
            this.playbackService.Stop();

            // Update file metadata
            // --------------------
            LogClient.Info("Updating file metadata");
            await this.metadataService.SafeUpdateFileMetadataAsync();

            // Save track statistics
            // ---------------------
            LogClient.Info("Saving playback counters");
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
            this.ForceActivate();
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

        private void InitializeWindows()
        {
            // Start monitoring tablet mode
            this.windowsIntegrationService.StartMonitoringTabletMode();

            // Tray controls
            this.trayControls = this.container.Resolve<TrayControls>();

            // Create the Mini Player playlist
            this.miniPlayerPlaylist = this.container.Resolve<MiniPlayerPlaylist>(new DependencyOverride(typeof(BorderlessWindows10Window), this));

            // NotificationService needs to know about the application windows
            this.notificationService.SetApplicationWindows(this, this.miniPlayerPlaylist, this.trayControls);

            PART_MiniPlayerButton.ToolTip = SettingsClient.Get<bool>("General", "IsMiniPlayer") ? ResourceUtils.GetString("Language_Restore") : ResourceUtils.GetString("Language_Mini_Player");

            // Settings changed
            SettingsClient.SettingChanged += (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "Appearance", "ShowWindowBorder"))
                {
                    this.WindowBorder.BorderThickness = new Thickness((bool)e.SettingValue ? 1 : 0);
                }

                if (SettingsClient.IsSettingChanged(e, "Behaviour", "ShowTrayIcon"))
                {
                    this.trayIcon.Visible = (bool)e.SettingValue;
                }

                if (SettingsClient.IsSettingChanged(e, "General", "IsMiniPlayer"))
                {
                    PART_MiniPlayerButton.ToolTip = (bool)e.SettingValue ? ResourceUtils.GetString("Language_Restore") : ResourceUtils.GetString("Language_Mini_Player");
                }
            };

            this.shellService.WindowStateChanged += (_, e) => this.WindowState = e.WindowState;
            this.shellService.IsMovableChanged += (_, e) => this.IsMovable = e.IsMovable;
            this.shellService.ResizeModeChanged += (_, e) => this.ResizeMode = e.ResizeMode;
            this.shellService.TopmostChanged += (_, e) => this.Topmost = e.IsTopmost;
            this.shellService.ShowWindowControlsChanged += (_, e) => this.ShowWindowControls = e.ShowWindowControls;

            this.shellService.GeometryChanged += (_, e) => this.SetGeometry(
                e.Top, e.Left, e.Size.Width, e.Size.Height,
                Constants.DefaultShellTop,
                Constants.DefaultShellLeft);

            this.shellService.MinimumSizeChanged += (_, e) =>
            {
                this.MinWidth = e.MinimumSize.Width;
                this.MinHeight = e.MinimumSize.Height;
            };

            this.shellService.PlaylistVisibilityChanged += (_, e) =>
            {
                if (e.IsPlaylistVisible)
                {
                    this.miniPlayerPlaylist.Show(e.MiniPlayerType);
                }
                else
                {
                    this.miniPlayerPlaylist.Hide();
                }
            };

            this.shellService.CheckIfTabletMode(true); // Make sure the window geometry respects tablet mode at startup
            this.SetWindowBorder();
        }

        private void SetWindowBorder()
        {
            this.WindowBorder.BorderThickness = new Thickness(SettingsClient.Get<bool>("Appearance", "ShowWindowBorder") & this.WindowState != WindowState.Maximized ? 1 : 0);
        }

        private void InitializeCommands()
        {
            // Window State
            this.MinimizeWindowCommand = new DelegateCommand(() => this.WindowState = WindowState.Minimized);
            Core.Prism.ApplicationCommands.MinimizeWindowCommand.RegisterCommand(this.MinimizeWindowCommand);

            this.RestoreWindowCommand = new DelegateCommand(() => this.shellService.ForceFullPlayer());
            Core.Prism.ApplicationCommands.RestoreWindowCommand.RegisterCommand(this.RestoreWindowCommand);

            this.MaximizeRestoreWindowCommand = new DelegateCommand(() => this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized);
            Core.Prism.ApplicationCommands.MaximizeRestoreWindowCommand.RegisterCommand(this.MaximizeRestoreWindowCommand);

            this.CloseWindowCommand = new DelegateCommand(() => this.Close());
            Core.Prism.ApplicationCommands.CloseWindowCommand.RegisterCommand(this.CloseWindowCommand);

            this.ShowMainWindowCommand = new DelegateCommand(() => this.ShowWindowInForeground());
            Core.Prism.ApplicationCommands.ShowMainWindowCommand.RegisterCommand(this.ShowMainWindowCommand);
        }

        private void InitializeServices()
        {
            // I18nService
            this.i18nService.LanguageChanged += (_, __) =>
            {
                // TODO: the DynamicResource binding doesn't update the PART_MiniPlayerButton ToolTip on language change.
                // This is a workaround to make sure the PART_MiniPlayerButton ToolTip also gets updated on a language change.
                // Is there a better way to do this.
                if (this.PART_MiniPlayerButton != null)
                {
                    this.PART_MiniPlayerButton.ToolTip = ResourceUtils.GetString("Language_Mini_Player");
                }
            };

            // IWin32InputService
            this.win32InputService.SetKeyboardHook(new WindowInteropHelper(this).EnsureHandle()); // Listen to media keys

            // IAppearanceService
            this.appearanceService.ThemeChanged += this.ThemeChangedHandler;

            // IWindowsIntegrationService
            this.windowsIntegrationService.TabletModeChanged += (_, __) =>
            {
                Application.Current.Dispatcher.Invoke(() => this.shellService.CheckIfTabletMode(false));
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

        private void Window_Deactivated(object sender, System.EventArgs e)
        {
            this.trayIconContextMenu.IsOpen = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // This call is not in the constructor, because we want to show the tray icon only
            // when the main window has been shown by explicitly calling Show(). This prevents 
            // showing the tray icon when the OOBE window is displayed.
            if (SettingsClient.Get<bool>("Behaviour", "ShowTrayIcon"))
            {
                this.trayIcon.Visible = true;
            }
        }

        private void Window_Closed(object sender, System.EventArgs e)
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

        private void Window_Restored(object sender, System.EventArgs e)
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

        private void Window_LocationChanged(object sender, System.EventArgs e)
        {
            // We need to put SaveWindowLocation() in the queue of the Dispatcher.
            // SaveWindowLocation() needs to be executed after LocationChanged was 
            // handled, when the WindowState has been updated otherwise we get 
            // incorrect values for Left and Top (both -7 last I checked).
            this.Dispatcher.BeginInvoke(new Action(() => this.shellService.SaveWindowLocation(this.Top, this.Left, this.WindowState)));
        }

        private void Window_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.eventAggregator.GetEvent<ShellMouseUp>().Publish(null);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.shellService.SaveWindowSize(this.WindowState, new Size(this.ActualWidth, this.ActualHeight));
        }

        private void Window_SourceInitialized(object sender, System.EventArgs e)
        {
            this.appearanceService.WatchWindowsColor(this);
        }

        private void Window_StateChanged(object sender, System.EventArgs e)
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

            this.SetWindowBorder();
            this.shellService.SaveWindowState(this.WindowState);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
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

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (e.OriginalSource is TextBox) return; // Don't interfere with typing in a TextBox
                e.Handled = true; // Prevents typing in the search box
                this.playbackService.PlayOrPauseAsync();
            }
        }

        private async void Window_MouseDown(object sender, MouseButtonEventArgs e)
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
    }
}
