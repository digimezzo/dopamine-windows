using Digimezzo.Foundation.Core.IO;
using Digimezzo.Foundation.Core.Logging;
using Digimezzo.Foundation.Core.Settings;
using Digimezzo.Foundation.Core.Utils;
using Digimezzo.Foundation.WPF.Controls;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Core.IO;
using Dopamine.Core.Prism;
using Dopamine.Services.Appearance;
using Dopamine.Services.I18n;
using Dopamine.Services.Lifetime;
using Dopamine.Services.Metadata;
using Dopamine.Services.Notification;
using Dopamine.Services.Playback;
using Dopamine.Services.Shell;
using Dopamine.Services.Win32Input;
using Dopamine.Services.WindowsIntegration;
using Dopamine.Views.Common;
using Dopamine.Views.MiniPlayer;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace Dopamine.Views
{
    public partial class Shell : Windows10BorderlessWindow
    {
        private IContainerProvider container;
        private IWindowsIntegrationService windowsIntegrationService;
        private INotificationService notificationService;
        private IWin32InputService win32InputService;
        private IPlaybackService playbackService;
        private IMetadataService metadataService;
        private IAppearanceService appearanceService;
        private IShellService shellService;
        private II18nService i18nService;
        private ILifetimeService lifetimeService;
        private IEventAggregator eventAggregator;
        private System.Windows.Forms.NotifyIcon trayIcon;
        private ContextMenu trayIconContextMenu;
        private TrayControls trayControls;
        private MiniPlayerPlaylist miniPlayerPlaylist;
        private bool isShuttingDown = false;

        public DelegateCommand MinimizeWindowCommand { get; set; }
        public DelegateCommand MaximizeRestoreWindowCommand { get; set; }
        public DelegateCommand CloseWindowCommand { get; set; }
        public DelegateCommand ShowMainWindowCommand { get; set; }

        public Shell(IContainerProvider container, IWindowsIntegrationService windowsIntegrationService, II18nService i18nService,
            INotificationService notificationService, IWin32InputService win32InputService, IAppearanceService appearanceService,
            IPlaybackService playbackService, IMetadataService metadataService, ILifetimeService lifetimeService,
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
            this.i18nService = i18nService;
            this.lifetimeService = lifetimeService;
            this.eventAggregator = eventAggregator;

            this.shellService = container.Resolve<Func<string, string, string, string, string, IShellService>>()(
                typeof(NowPlaying.NowPlaying).FullName, typeof(FullPlayer.FullPlayer).FullName, typeof(CoverPlayer).FullName,
                typeof(MicroPlayer).FullName, typeof(NanoPlayer).FullName);

            this.InitializeServices();
            this.InitializeWindows();
            this.InitializeTrayIcon();
            this.InitializeCommands();
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

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
                LogClient.Info("### STOPPING {0}, version {1} ###", ProductInformation.ApplicationName, ProcessExecutable.AssemblyVersion().ToString());

                if (this.lifetimeService.MustPerformClosingTasks)
                {
                    e.Cancel = true;
                    await this.lifetimeService.PerformClosingTasksAsync();
                    this.Close();
                }
            }
        }

        private void ShowClosingAnimation()
        {
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

        private void SetTrayIcon()
        {
            // Reflection is needed to get the full path of the executable. Because when starting the application from the start menu
            // without specifying the full path, the application fails to find the Tray icon and crashes here
            string iconFile = "Legacy tray.ico";

            if (EnvironmentUtils.IsWindows10())
            {
                if (this.windowsIntegrationService.IsSystemUsingLightTheme)
                {
                    iconFile = "Tray_black.ico";
                }
                else
                {
                    iconFile = "Tray_white.ico";
                }
            }

            string iconPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ApplicationPaths.IconsSubDirectory, iconFile);
            this.trayIcon.Icon = new System.Drawing.Icon(iconPath, System.Windows.Forms.SystemInformation.SmallIconSize);
        }

        private void InitializeTrayIcon()
        {
            this.trayIcon = new System.Windows.Forms.NotifyIcon();
            this.trayIcon.Visible = false;
            this.trayIcon.Text = ProductInformation.ApplicationName;

            this.SetTrayIcon();

            this.trayIcon.MouseClick += TrayIcon_MouseClick;
            this.trayIcon.MouseDoubleClick += (_, __) => this.ShowWindowInForeground();

            this.trayIconContextMenu = (ContextMenu)this.FindResource("TrayIconContextMenu");
        }

        private void InitializeWindows()
        {
            // Start monitoring tablet mode
            this.windowsIntegrationService.StartMonitoringTabletMode();

            // Start monitoring system uses light theme
            this.windowsIntegrationService.StartMonitoringSystemUsesLightTheme();

            // Tray controls
            this.trayControls = this.container.Resolve<TrayControls>();

            // Create the Mini Player playlist
            this.miniPlayerPlaylist = this.container.Resolve<Func<Windows10BorderlessWindow, MiniPlayerPlaylist>>()(this);

            // NotificationService needs to know about the application windows
            this.notificationService.SetApplicationWindows(this, this.miniPlayerPlaylist, this.trayControls);

            // Settings changed
            SettingsClient.SettingChanged += (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "Appearance", "ShowWindowBorder"))
                {
                    this.WindowBorder.BorderThickness = new Thickness((bool)e.Entry.Value ? 1 : 0);
                }

                if (SettingsClient.IsSettingChanged(e, "Behaviour", "ShowTrayIcon"))
                {
                    this.trayIcon.Visible = (bool)e.Entry.Value;
                }
            };

            this.shellService.WindowStateChangeRequested += (_, e) => this.WindowState = e.WindowState;
            this.shellService.IsMovableChangeRequested += (_, e) => this.IsMovable = e.IsMovable;
            this.shellService.ResizeModeChangeRequested += (_, e) => this.ResizeMode = e.ResizeMode;
            this.shellService.TopmostChangeRequested += (_, e) => this.Topmost = e.IsTopmost;

            this.shellService.GeometryChangeRequested += (_, e) => this.SetGeometry(
                e.Top, e.Left, e.Size.Width, e.Size.Height,
                Constants.DefaultShellTop,
                Constants.DefaultShellLeft);

            this.shellService.MinimumSizeChangeRequested += (_, e) =>
            {
                this.MinWidth = e.MinimumSize.Width;
                this.MinHeight = e.MinimumSize.Height;
            };

            this.shellService.PlaylistVisibilityChangeRequested += (_, e) =>
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

            this.MaximizeRestoreWindowCommand = new DelegateCommand(() => this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized);
            Core.Prism.ApplicationCommands.MaximizeRestoreWindowCommand.RegisterCommand(this.MaximizeRestoreWindowCommand);

            this.CloseWindowCommand = new DelegateCommand(() => this.Close());
            Core.Prism.ApplicationCommands.CloseWindowCommand.RegisterCommand(this.CloseWindowCommand);

            this.ShowMainWindowCommand = new DelegateCommand(() => this.ShowWindowInForeground());
            Core.Prism.ApplicationCommands.ShowMainWindowCommand.RegisterCommand(this.ShowMainWindowCommand);
        }

        private void InitializeServices()
        {
            // IWin32InputService
            this.win32InputService.SetKeyboardHook(new WindowInteropHelper(this).EnsureHandle()); // Listen to media keys

            // IWindowsIntegrationService
            this.windowsIntegrationService.TabletModeChanged += (_, __) =>
            {
                Application.Current.Dispatcher.Invoke(() => this.shellService.CheckIfTabletMode(false));
            };

            this.windowsIntegrationService.SystemUsesLightThemeChanged += (_, __) =>
            {
                Application.Current.Dispatcher.Invoke(() => this.SetTrayIcon());
            };
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

            // Stop monitoring system uses light theme
            this.windowsIntegrationService.StopMonitoringSystemUsesLightTheme();

            // Make sure the Tray icon is removed from the tray
            this.trayIcon.Visible = false;

            // Stop listening to keyboard outside the application
            this.win32InputService.UnhookKeyboard();

            // This makes sure the application doesn't keep running when the main window is closed.
            // Extra windows created by the main window can keep a WPF application running even when
            // the main window is closed, because the default ShutDownMode of a WPF application is
            // OnLastWindowClose. This was happening here because of the Mini Player Playlist.
            Application.Current.Shutdown();

            LogClient.Info("### STOPPED {0}, version {1} ###", ProductInformation.ApplicationName, ProcessExecutable.AssemblyVersion().ToString());

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
                else if (e.Key == Key.F)
                {
                    this.eventAggregator.GetEvent<FocusSearchBox>().Publish(null);
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
