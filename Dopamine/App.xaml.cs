using CommonServiceLocator;
using Digimezzo.Foundation.Core.IO;
using Digimezzo.Foundation.Core.Logging;
using Digimezzo.Foundation.Core.Settings;
using Digimezzo.Foundation.Core.Utils;
using Digimezzo.Foundation.WPF.Controls;
using Dopamine.Core.Base;
using Dopamine.Core.Helpers;
using Dopamine.Core.IO;
using Dopamine.Data;
using Dopamine.Data.Repositories;
using Dopamine.Services.Appearance;
using Dopamine.Services.Blacklist;
using Dopamine.Services.Cache;
using Dopamine.Services.Collection;
using Dopamine.Services.Command;
using Dopamine.Services.Dialog;
using Dopamine.Services.Discord;
using Dopamine.Services.Equalizer;
using Dopamine.Services.ExternalControl;
using Dopamine.Services.File;
using Dopamine.Services.Folders;
using Dopamine.Services.I18n;
using Dopamine.Services.Indexing;
using Dopamine.Services.InfoDownload;
using Dopamine.Services.JumpList;
using Dopamine.Services.Lifetime;
using Dopamine.Services.Lyrics;
using Dopamine.Services.Metadata;
using Dopamine.Services.Notification;
using Dopamine.Services.Playback;
using Dopamine.Services.Playlist;
using Dopamine.Services.Provider;
using Dopamine.Services.Scrobbling;
using Dopamine.Services.Search;
using Dopamine.Services.Shell;
using Dopamine.Services.Statistics;
using Dopamine.Services.Taskbar;
using Dopamine.Services.Update;
using Dopamine.Services.Win32Input;
using Dopamine.Services.WindowsIntegration;
using Dopamine.Utils;
using Dopamine.Views;
using Dopamine.Views.Common;
using Dopamine.Views.FullPlayer;
using Dopamine.Views.FullPlayer.Collection;
using Dopamine.Views.FullPlayer.Information;
using Dopamine.Views.FullPlayer.Settings;
using Dopamine.Views.MiniPlayer;
using Dopamine.Views.NowPlaying;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Shell;
using System.Windows.Threading;

namespace Dopamine
{
    public partial class App : PrismApplication
    {
        private Mutex instanceMutex = null;
        private DateTime lastUnhandledExceptionLoggedTime = DateTime.MinValue;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Create a jump-list and assign it to the current application
            JumpList.SetJumpList(Current, new JumpList());

            // Check that there is only one instance of the application running
            this.instanceMutex = new Mutex(true, $"{ProductInformation.ApplicationGuid}-{ProcessExecutable.AssemblyVersion()}", out bool isNewInstance);

            // Process the command-line arguments
            this.ProcessCommandLineArguments(isNewInstance);

            if (isNewInstance)
            {
                this.instanceMutex.ReleaseMutex();
                this.LaunchInitializer();
                base.OnStartup(e);
            }
            else
            {
                // HACK: because shutdown is too fast, some logging might be missing in the log file.
                LogClient.Warning("{0} is already running. Shutting down.", ProductInformation.ApplicationName);
                this.Shutdown();
            }
        }

        private void LaunchInitializer()
        {
            var initializer = new Initializer();

            if (initializer.IsMigrationNeeded())
            {
                // Show the Update Window
                var initWin = new Initialize();

                // Disable shutdown when the dialogs close
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                // Show as a dialog. This prevents further code execution until the dialog is closed.
                initWin.ShowDialog();
                initWin.ForceActivate();
            }
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        protected override void InitializeShell(Window shell)
        {
            LogClient.Info($"### STARTING {ProductInformation.ApplicationName}, version {ProcessExecutable.AssemblyVersion()}, IsPortable = {SettingsClient.BaseGet<bool>("Configuration", "IsPortable")}, Windows version = {EnvironmentUtils.GetFriendlyWindowsVersion()} ({EnvironmentUtils.GetInternalWindowsVersion()}), IsWindows10 = {Core.Base.Constants.IsWindows10} ###");

            // Handler for unhandled AppDomain exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            this.InitializeWcfServices();

            Current.MainWindow = shell;

            bool showOobe = SettingsClient.Get<bool>("General", "ShowOobe");

            if (showOobe)
            {
                var oobeWin = Container.Resolve<Oobe>();

                // These 2 lines are required to set the RegionManager of the child window.
                // If we don't do this, regions on child windows are never known by the Shell 
                // RegionManager and navigation doesn't work
                RegionManager.SetRegionManager(oobeWin, Container.Resolve<IRegionManager>());
                RegionManager.UpdateRegions();

                // Show the OOBE window. Don't tell the Indexer to start. 
                // It will get a signal to start when the OOBE window closes.
                LogClient.Info("Showing Oobe screen");

                // Disable shutdown when the dialogs close
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                // Show as a dialog. This prevents further code execution until the dialog is closed.
                oobeWin.ShowDialog();
                oobeWin.ForceActivate();
            }

            // Re-enable normal shutdown mode
            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            // Show the main window
            LogClient.Info("Showing Main screen");
            shell.Show();

            // We're not showing the OOBE screen, tell the IndexingService to start.
            if (!showOobe)
            {
                Container.Resolve<IIndexingService>().RefreshCollectionAsync();
            }
        }

        protected void InitializeWcfServices()
        {
            // CommandService
            // --------------
            var commandServicehost = new ServiceHost(Container.Resolve<ICommandService>(), new Uri[] { new Uri(string.Format("net.pipe://localhost/{0}/CommandService", ProductInformation.ApplicationName)) });
            commandServicehost.AddServiceEndpoint(typeof(ICommandService), new StrongNetNamedPipeBinding(), "CommandServiceEndpoint");

            try
            {
                commandServicehost.Open();
                LogClient.Info("CommandService was started successfully");
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not start CommandService. Exception: {0}", ex.Message);
            }

            // FileService
            // -----------
            var fileServicehost = new ServiceHost(Container.Resolve<IFileService>(), new Uri[] { new Uri(string.Format("net.pipe://localhost/{0}/FileService", ProductInformation.ApplicationName)) });
            fileServicehost.AddServiceEndpoint(typeof(IFileService), new StrongNetNamedPipeBinding(), "FileServiceEndpoint");

            try
            {
                fileServicehost.Open();
                LogClient.Info("FileService was started successfully");
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not start FileService. Exception: {0}", ex.Message);
            }
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            RegisterCoreComponents();
            RegisterFactories();
            RegisterRepositories();
            RegisterServices();
            InitializeServices();
            RegisterViews();
            RegisterViewModels();

            void RegisterCoreComponents()
            {
                containerRegistry.RegisterInstance<IContainerProvider>(Container);
                containerRegistry.RegisterInstance<ILocalizationInfo>(new LocalizationInfo());
            }

            void RegisterFactories()
            {
                containerRegistry.RegisterSingleton<ISQLiteConnectionFactory, SQLiteConnectionFactory>();
            }

            void RegisterRepositories()
            {
                containerRegistry.RegisterSingleton<IFolderRepository, FolderRepository>();
                containerRegistry.RegisterSingleton<ITrackRepository, TrackRepository>();
                containerRegistry.RegisterSingleton<IAlbumArtworkRepository, AlbumArtworkRepository>();
                containerRegistry.RegisterSingleton<IQueuedTrackRepository, QueuedTrackRepository>();
                containerRegistry.RegisterSingleton<IBlacklistTrackRepository, BlacklistTrackRepository>();
            }

            void RegisterServices()
            {
                containerRegistry.RegisterSingleton<ICacheService, CacheService>();
                containerRegistry.RegisterSingleton<IUpdateService, UpdateService>();
                containerRegistry.RegisterSingleton<IAppearanceService, AppearanceService>();
                containerRegistry.RegisterSingleton<II18nService, I18nService>();
                containerRegistry.RegisterSingleton<IDialogService, DialogService>();
                containerRegistry.RegisterSingleton<IIndexingService, IndexingService>();
                containerRegistry.RegisterSingleton<IStatisticsService, StatisticsService>();
                containerRegistry.RegisterSingleton<IPlaybackService, PlaybackService>();
                containerRegistry.RegisterSingleton<IWin32InputService, Win32InputService>();
                containerRegistry.RegisterSingleton<ISearchService, SearchService>();
                containerRegistry.RegisterSingleton<ITaskbarService, TaskbarService>();
                containerRegistry.RegisterSingleton<ICollectionService, CollectionService>();
                containerRegistry.RegisterSingleton<IFoldersService, FoldersService>();
                containerRegistry.RegisterSingleton<IJumpListService, JumpListService>();
                containerRegistry.RegisterSingleton<IFileService, FileService>();
                containerRegistry.RegisterSingleton<ICommandService, CommandService>();
                containerRegistry.RegisterSingleton<IMetadataService, MetadataService>();
                containerRegistry.RegisterSingleton<IEqualizerService, EqualizerService>();
                containerRegistry.RegisterSingleton<IProviderService, ProviderService>();
                containerRegistry.RegisterSingleton<IScrobblingService, ScrobblingService>();
                containerRegistry.RegisterSingleton<IPlaylistService, PlaylistService>();
                containerRegistry.RegisterSingleton<IExternalControlService, ExternalControlService>();
                containerRegistry.RegisterSingleton<IWindowsIntegrationService, WindowsIntegrationService>();
                containerRegistry.RegisterSingleton<ILyricsService, LyricsService>();
                containerRegistry.RegisterSingleton<IShellService, ShellService>();
                containerRegistry.RegisterSingleton<ILifetimeService, LifetimeService>();
                containerRegistry.RegisterSingleton<IInfoDownloadService, InfoDownloadService>();
                containerRegistry.RegisterSingleton<IRichPresenceService, RichPresenceService>();
                containerRegistry.RegisterSingleton<IBlacklistService, BlacklistService>();

                INotificationService notificationService;

                // NotificationService contains code that is only supported on Windows 10
                if (Core.Base.Constants.IsWindows10 && MediaFoundationHelper.HasMediaFoundationSupport(true))
                {
                    // On some editions of Windows 10, constructing NotificationService still fails
                    // (e.g. Windows 10 Enterprise N 2016 LTSB). Hence the try/catch block.
                    try
                    {
                        notificationService = new NotificationService(
                        Container.Resolve<IPlaybackService>(),
                        Container.Resolve<ICacheService>(),
                        Container.Resolve<IMetadataService>());
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Constructing NotificationService failed. Falling back to LegacyNotificationService. Exception: {0}", ex.Message);
                        notificationService = new LegacyNotificationService(
                        Container.Resolve<IPlaybackService>(),
                        Container.Resolve<ICacheService>(),
                        Container.Resolve<IMetadataService>());
                    }
                }
                else
                {
                    notificationService = new LegacyNotificationService(
                        Container.Resolve<IPlaybackService>(),
                        Container.Resolve<ICacheService>(),
                        Container.Resolve<IMetadataService>());
                }

                containerRegistry.RegisterInstance(notificationService);
            }

            void InitializeServices()
            {
                // Making sure resources are set before we need them
                Container.Resolve<II18nService>().ApplyLanguageAsync(SettingsClient.Get<string>("Appearance", "Language"));
                Container.Resolve<IAppearanceService>().ApplyTheme(SettingsClient.Get<bool>("Appearance", "EnableLightTheme"));

                Container.Resolve<IAppearanceService>().ApplyColorSchemeAsync(
                    SettingsClient.Get<string>("Appearance", "ColorScheme"),
                    SettingsClient.Get<bool>("Appearance", "FollowWindowsColor"),
                    SettingsClient.Get<bool>("Appearance", "FollowAlbumCoverColor")
                );
                Container.Resolve<IExternalControlService>();
                Container.Resolve<IRichPresenceService>();
            }

            void RegisterViews()
            {
                // Misc.
                containerRegistry.Register<object, Oobe>(typeof(Oobe).FullName);
                containerRegistry.Register<object, TrayControls>(typeof(TrayControls).FullName);
                containerRegistry.Register<object, Shell>(typeof(Shell).FullName);
                containerRegistry.Register<object, Empty>(typeof(Empty).FullName);
                containerRegistry.Register<object, FullPlayer>(typeof(FullPlayer).FullName);
                containerRegistry.Register<object, CoverPlayer>(typeof(CoverPlayer).FullName);
                containerRegistry.Register<object, MicroPlayer>(typeof(MicroPlayer).FullName);
                containerRegistry.Register<object, NanoPlayer>(typeof(NanoPlayer).FullName);
                containerRegistry.Register<object, NowPlaying>(typeof(NowPlaying).FullName);
                containerRegistry.Register<object, WindowControls>(typeof(WindowControls).FullName);

                // Collection
                containerRegistry.Register<object, CollectionMenu>(typeof(CollectionMenu).FullName);
                containerRegistry.Register<object, Collection>(typeof(Collection).FullName);
                containerRegistry.Register<object, CollectionAlbums>(typeof(CollectionAlbums).FullName);
                containerRegistry.Register<object, CollectionArtists>(typeof(CollectionArtists).FullName);
                containerRegistry.Register<object, CollectionPlaylists>(typeof(CollectionPlaylists).FullName);
                containerRegistry.Register<object, CollectionFolders>(typeof(CollectionFolders).FullName);
                containerRegistry.Register<object, CollectionGenres>(typeof(CollectionGenres).FullName);
                containerRegistry.Register<object, CollectionTracks>(typeof(CollectionTracks).FullName);

                // Settings
                containerRegistry.Register<object, SettingsMenu>(typeof(SettingsMenu).FullName);
                containerRegistry.Register<object, Settings>(typeof(Settings).FullName);
                containerRegistry.Register<object, SettingsAppearance>(typeof(SettingsAppearance).FullName);
                containerRegistry.Register<object, SettingsBehaviour>(typeof(SettingsBehaviour).FullName);
                containerRegistry.Register<object, SettingsOnline>(typeof(SettingsOnline).FullName);
                containerRegistry.Register<object, SettingsPlayback>(typeof(SettingsPlayback).FullName);
                containerRegistry.Register<object, SettingsStartup>(typeof(SettingsStartup).FullName);
                containerRegistry.Register<object, SettingsBlacklist>(typeof(SettingsBlacklist).FullName);

                // Information
                containerRegistry.Register<object, InformationMenu>(typeof(InformationMenu).FullName);
                containerRegistry.Register<object, Information>(typeof(Information).FullName);
                containerRegistry.Register<object, InformationHelp>(typeof(InformationHelp).FullName);
                containerRegistry.Register<object, InformationAbout>(typeof(InformationAbout).FullName);

                // Now playing
                containerRegistry.Register<object, NowPlayingArtistInformation>(typeof(NowPlayingArtistInformation).FullName);
                containerRegistry.Register<object, NowPlayingLyrics>(typeof(NowPlayingLyrics).FullName);
                containerRegistry.Register<object, NowPlayingPlaylist>(typeof(NowPlayingPlaylist).FullName);
                containerRegistry.Register<object, NowPlayingShowcase>(typeof(NowPlayingShowcase).FullName);
            }

            void RegisterViewModels()
            {
            }
        }

        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings mappings)
        {
            base.ConfigureRegionAdapterMappings(mappings);
            mappings.RegisterMapping(typeof(SlidingContentControl), Container.Resolve<SlidingContentControlRegionAdapter>());
        }

        private void ProcessCommandLineArguments(bool isNewInstance)
        {
            // Get the command-line arguments
            var args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                LogClient.Info("Found command-line arguments.");

                switch (args[1])
                {
                    case "/donate":
                        LogClient.Info("Detected DonateCommand from JumpList.");

                        try
                        {
                            Actions.TryOpenLink(args[2]);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not open the link {0} in Internet Explorer. Exception: {1}", args[2], ex.Message);
                        }
                        this.Shutdown();
                        break;
                    default:

                        LogClient.Info("Processing Non-JumpList command-line arguments.");


                        if (!isNewInstance)
                        {
                            // Send the command-line arguments to the running instance
                            this.TrySendCommandlineArguments(args);
                        }
                        else
                        {
                            // Do nothing. The command-line arguments of a single instance will be processed,
                            // in the ShellViewModel because over there we have access to the FileService.
                        }
                        break;
                }
            }
            else
            {
                // When started without command line arguments, and when not the first instance: try to show the running instance.
                if (!isNewInstance) this.TryShowRunningInstance();
            }
        }

        private void TryShowRunningInstance()
        {
            var commandServiceFactory = new ChannelFactory<ICommandService>(new StrongNetNamedPipeBinding(), new EndpointAddress(string.Format("net.pipe://localhost/{0}/CommandService/CommandServiceEndpoint", ProductInformation.ApplicationName)));

            try
            {
                var commandServiceProxy = commandServiceFactory.CreateChannel();
                commandServiceProxy.ShowMainWindowCommand();
                LogClient.Info("Trying to show the running instance");
            }
            catch (Exception ex)
            {
                LogClient.Error("A problem occurred while trying to show the running instance. Exception: {0}", ex.Message);
            }
        }

        private void TrySendCommandlineArguments(string[] args)
        {
            LogClient.Info("Trying to send {0} command-line arguments to the running instance", args.Count());

            var needsSending = true;
            var startTime = DateTime.Now;

            var fileServiceFactory = new ChannelFactory<IFileService>(new StrongNetNamedPipeBinding(), new EndpointAddress(string.Format("net.pipe://localhost/{0}/FileService/FileServiceEndpoint", ProductInformation.ApplicationName)));


            while (needsSending)
            {
                try
                {
                    // Try to send the command-line arguments to the running instance
                    var fileServiceProxy = fileServiceFactory.CreateChannel();
                    fileServiceProxy.ProcessArguments(args);
                    LogClient.Info("Sent {0} command-line arguments to the running instance", args.Count());

                    needsSending = false;
                }
                catch (Exception ex)
                {

                    if (ex is EndpointNotFoundException)
                    {
                        // When selecting multiple files, the first file is opened by the first instance.
                        // This instance takes some time to start. To avoid an EndpointNotFoundException
                        // when sending the second file to the first instance, we wait 10 ms repetitively,
                        // until there is an endpoint to talk to.
                        System.Threading.Thread.Sleep(10);
                    }
                    else
                    {
                        // Log any other Exception and stop trying to send the file to the running instance
                        needsSending = false;
                        LogClient.Info("A problem occurred while trying to send {0} command-line arguments to the running instance. Exception: {1}", args.Count().ToString(), ex.Message);
                    }
                }

                // This makes sure we don't try to send for longer than 30 seconds, 
                // so this instance won't stay open forever.
                if (Convert.ToInt64(DateTime.Now.Subtract(startTime).TotalSeconds) > 30)
                {
                    needsSending = false;
                }
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            // Log the exception and stop the application
            this.ExecuteEmergencyStop(ex);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Prevent default unhandled exception processing
            e.Handled = true;

            // Log the exception and stop the application
            this.ExecuteEmergencyStop(e.Exception);
        }

        private bool CanLogUnhandledException()
        {
            DateTime now = DateTime.Now;
            double differenceInSeconds = now.Subtract(this.lastUnhandledExceptionLoggedTime).TotalSeconds;

            // We only allow logging unhandled exceptions once every 5 seconds to avoid filling the logs too much.
            if (differenceInSeconds >= 5)
            {
                this.lastUnhandledExceptionLoggedTime = now;
                return true;
            }

            return false;
        }

        private void ExecuteEmergencyStop(Exception ex)
        {
            // This is a workaround for a bug in the .Net framework, which randomly causes a System.ArgumentNullException when
            // scrolling through a Virtualizing StackPanel. Scroll to playing song sometimes triggers this bug. We catch the
            // Exception here, and do nothing with it. The application can just proceed. This prevents a complete crash.
            // This might be fixed in .Net 4.5.2. See here: https://connect.microsoft.com/VisualStudio/feedback/details/789438/scrolling-in-virtualized-wpf-treeview-is-very-unstable
            if (ex.GetType().ToString().Equals("System.ArgumentNullException") & ex.Source.ToString().Equals("PresentationCore"))
            {
                if (this.CanLogUnhandledException())
                {
                    LogClient.Warning($"Ignored Unhandled Exception: {ex.Message}");
                }

                return;
            }

            // This is a workaround for an inexplicable issue which occurs on 1 user's computer (as far as I know).
            // Exception "System.ComponentModel.Win32Exception (0x80004005): Access is denied" is thrown when performing
            // function "MS.Win32.UnsafeNativeMethods.GetWindowText(HandleRef hWnd, StringBuilder lpString, Int32 nMaxCount)"
            if (ex.GetType().ToString().Equals("System.ComponentModel.Win32Exception") & ex.Source.ToString().Equals("WindowsBase"))
            {
                if (this.CanLogUnhandledException())
                {
                    LogClient.Warning($"Ignored Unhandled Exception: {ex.Message}");
                }

                return;
            }

            // This is a workaround for an exception which occurs when using Dopamine together with WinDock (https://www.ivanyu.ca/windock)
            // Unhandled Exception. Exception: System.OverflowException: Arithmetic operation resulted in an overflow.
            // at System.Windows.Shell.WindowChromeWorker._HandleNCHitTest(WM uMsg, IntPtr wParam, IntPtr lParam, Boolean & handled)
            // at System.Windows.Shell.WindowChromeWorker._WndProc(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, Boolean & handled)
            if (ex.GetType().ToString().Equals("System.OverflowException") & ex.Source.ToString().Equals("PresentationFramework"))
            {
                if (this.CanLogUnhandledException())
                {
                    LogClient.Warning($"Ignored Unhandled Exception: {ex.Message}");
                }

                return;
            }

            // LogClient.Warning($"Ignored Unhandled Exception: Message=<<<<{ex.Message}>>>>");
            // LogClient.Warning($"Ignored Unhandled Exception: Type=<<<<{ex.GetType().ToString()}>>>>");
            // LogClient.Warning($"Ignored Unhandled Exception: Source=<<<<{ex.Source.ToString()}>>>>");
            // return;

            LogClient.Error("Unhandled Exception. {0}", LogClient.GetAllExceptions(ex));

            // Close the application to prevent further problems
            LogClient.Info("### FORCED STOP of {0}, version {1} ###", ProductInformation.ApplicationName, ProcessExecutable.AssemblyVersion());

            // Stop playing (This avoids remaining processes in Task Manager)
            var playbackService = ServiceLocator.Current.GetInstance<IPlaybackService>();
            playbackService.Stop();

            // Emergency save of the settings
            SettingsClient.Write();

            Current.Shutdown();
        }
    }
}
