using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Digimezzo.WPFControls;
using Dopamine.Core.Base;
using Dopamine.Core.IO;
using Dopamine.Data;
using Dopamine.Services.Contracts.Command;
using Dopamine.Services.Contracts.File;
using Dopamine.Services.Contracts.Playback;
using Dopamine.Views;
using CommonServiceLocator;
using System;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Shell;
using System.Windows.Threading;
using Dopamine.Core.Helpers;
using Dopamine.Data.Contracts;
using Dopamine.Data.Contracts.Metadata;
using Dopamine.Data.Contracts.Repositories;
using Dopamine.Data.Metadata;
using Dopamine.Data.Repositories;
using Dopamine.Presentation.Utils;
using Dopamine.Services.Appearance;
using Dopamine.Services.Cache;
using Dopamine.Services.Collection;
using Dopamine.Services.Command;
using Dopamine.Services.Contracts.Appearance;
using Dopamine.Services.Contracts.Cache;
using Dopamine.Services.Contracts.Collection;
using Dopamine.Services.Contracts.Dialog;
using Dopamine.Services.Contracts.Equalizer;
using Dopamine.Services.Contracts.ExternalControl;
using Dopamine.Services.Contracts.I18n;
using Dopamine.Services.Contracts.Indexing;
using Dopamine.Services.Contracts.JumpList;
using Dopamine.Services.Contracts.Metadata;
using Dopamine.Services.Contracts.Notification;
using Dopamine.Services.Contracts.Playlist;
using Dopamine.Services.Contracts.Provider;
using Dopamine.Services.Contracts.Scrobbling;
using Dopamine.Services.Contracts.Search;
using Dopamine.Services.Contracts.Shell;
using Dopamine.Services.Contracts.Taskbar;
using Dopamine.Services.Contracts.Update;
using Dopamine.Services.Contracts.Win32Input;
using Dopamine.Services.Contracts.WindowsIntegration;
using Dopamine.Services.Dialog;
using Dopamine.Services.Equalizer;
using Dopamine.Services.ExternalControl;
using Dopamine.Services.File;
using Dopamine.Services.I18n;
using Dopamine.Services.Indexing;
using Dopamine.Services.JumpList;
using Dopamine.Services.Metadata;
using Dopamine.Services.Notification;
using Dopamine.Services.Playback;
using Dopamine.Services.Playlist;
using Dopamine.Services.Provider;
using Dopamine.Services.Scrobbling;
using Dopamine.Services.Search;
using Dopamine.Services.Shell;
using Dopamine.Services.Taskbar;
using Dopamine.Services.Update;
using Dopamine.Services.Win32Input;
using Dopamine.Services.WindowsIntegration;
using Dopamine.Views.Common;
using Dopamine.Views.FullPlayer;
using Dopamine.Views.FullPlayer.Collection;
using Dopamine.Views.FullPlayer.Information;
using Dopamine.Views.FullPlayer.Settings;
using Dopamine.Views.MiniPlayer;
using Dopamine.Views.NowPlaying;
using Prism.Ioc;
using Prism.Unity;
using Dopamine.Presentation.Views;
using Prism.Mvvm;
using Prism.Regions;
using Unity;
using Unity.Wcf;

namespace Dopamine
{
    public partial class App : PrismApplication
    {
        private Mutex instanceMutex = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Create a jump-list and assign it to the current application
            JumpList.SetJumpList(Application.Current, new JumpList());

            // Check that there is only one instance of the application running
            instanceMutex = new Mutex(true,
                $"{ProductInformation.ApplicationGuid}-{ProcessExecutable.AssemblyVersion()}", out var isNewInstance);

            // Process the command-line arguments
            this.ProcessCommandLineArguments(isNewInstance);

            if (isNewInstance)
            {
                instanceMutex.ReleaseMutex();
                this.ExecuteStartup();
            }
            else
            {
                LogClient.Warning("{0} is already running. Shutting down.", ProductInformation.ApplicationName);
                this.Shutdown();
            }
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            base.RegisterRequiredTypes(containerRegistry);
            var unityContainer = containerRegistry.GetContainer();

            RegisterCoreComponents();
            RegisterFactories();
            RegisterRepositories();
            RegisterServices();
            InitializeServices();
            RegisterViews();
            RegisterViewModels();

            void RegisterCoreComponents()
            {
                containerRegistry.RegisterInstance<ILocalizationInfo>(new LocalizationInfo());
            }

            void RegisterFactories()
            {
                containerRegistry.RegisterSingleton<ISQLiteConnectionFactory, SQLiteConnectionFactory>();
                containerRegistry.RegisterSingleton<IFileMetadataFactory, FileMetadataFactory>();
            }

            void RegisterRepositories()
            {
                containerRegistry.RegisterSingleton<IFolderRepository, FolderRepository>();
                containerRegistry.RegisterSingleton<IAlbumRepository, AlbumRepository>();
                containerRegistry.RegisterSingleton<IArtistRepository, ArtistRepository>();
                containerRegistry.RegisterSingleton<IGenreRepository, GenreRepository>();
                containerRegistry.RegisterSingleton<ITrackRepository, TrackRepository>();
                containerRegistry.RegisterSingleton<ITrackStatisticRepository, TrackStatisticRepository>();
                containerRegistry.RegisterSingleton<IQueuedTrackRepository, QueuedTrackRepository>();
            }

            void RegisterServices()
            {
                containerRegistry.RegisterSingleton<ICacheService, CacheService>();
                containerRegistry.RegisterSingleton<IUpdateService, UpdateService>();
                containerRegistry.RegisterSingleton<IAppearanceService, AppearanceService>();
                containerRegistry.RegisterSingleton<II18nService, I18nService>();
                containerRegistry.RegisterSingleton<IDialogService, DialogService>();
                containerRegistry.RegisterSingleton<IIndexingService, IndexingService>();
                containerRegistry.RegisterSingleton<IPlaybackService, PlaybackService>();
                containerRegistry.RegisterSingleton<IWin32InputService, Win32InputService>();
                containerRegistry.RegisterSingleton<ISearchService, SearchService>();
                containerRegistry.RegisterSingleton<ITaskbarService, TaskbarService>();
                containerRegistry.RegisterSingleton<ICollectionService, CollectionService>();
                containerRegistry.RegisterSingleton<IJumpListService, JumpListService>();
                containerRegistry.RegisterSingleton<IFileService, FileService>();
                containerRegistry.RegisterSingleton<ICommandService, CommandService>();
                containerRegistry.RegisterSingleton<IMetadataService, MetadataService>();
                containerRegistry.RegisterSingleton<IEqualizerService, EqualizerService>();
                containerRegistry.RegisterSingleton<IProviderService, ProviderService>();
                containerRegistry.RegisterSingleton<IScrobblingService, LastFmScrobblingService>();
                containerRegistry.RegisterSingleton<IPlaylistService, PlaylistService>();
                containerRegistry.RegisterSingleton<IExternalControlService, ExternalControlService>();
                containerRegistry.RegisterSingleton<IWindowsIntegrationService, WindowsIntegrationService>();
                containerRegistry.RegisterSingleton<IShellService, ShellService>();

                INotificationService notificationService;

                // NotificationService contains code that is only supported on Windows 10
                if (Core.Base.Constants.IsWindows10)
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
            }

            void RegisterViews()
            {
                // Misc.
                unityContainer.RegisterType<object, Oobe>(typeof(Oobe).FullName);
                unityContainer.RegisterType<object, TrayControls>(typeof(TrayControls).FullName);
                unityContainer.RegisterType<object, Shell>(typeof(Shell).FullName);
                unityContainer.RegisterType<object, Empty>(typeof(Empty).FullName);
                unityContainer.RegisterType<object, FullPlayer>(typeof(FullPlayer).FullName);
                unityContainer.RegisterType<object, CoverPlayer>(typeof(CoverPlayer).FullName);
                unityContainer.RegisterType<object, MicroPlayer>(typeof(MicroPlayer).FullName);
                unityContainer.RegisterType<object, NanoPlayer>(typeof(NanoPlayer).FullName);
                unityContainer.RegisterType<object, NowPlaying>(typeof(NowPlaying).FullName);

                // Collection
                unityContainer.RegisterType<object, CollectionMenu>(typeof(CollectionMenu).FullName);
                unityContainer.RegisterType<object, Collection>(typeof(Collection).FullName);
                unityContainer.RegisterType<object, CollectionAlbums>(typeof(CollectionAlbums).FullName);
                unityContainer.RegisterType<object, CollectionArtists>(typeof(CollectionArtists).FullName);
                unityContainer.RegisterType<object, CollectionFrequent>(typeof(CollectionFrequent).FullName);
                unityContainer.RegisterType<object, CollectionGenres>(typeof(CollectionGenres).FullName);
                unityContainer.RegisterType<object, CollectionPlaylists>(typeof(CollectionPlaylists).FullName);
                unityContainer.RegisterType<object, CollectionTracks>(typeof(CollectionTracks).FullName);

                // Settings
                unityContainer.RegisterType<object, SettingsMenu>(typeof(SettingsMenu).FullName);
                unityContainer.RegisterType<object, Settings>(typeof(Settings).FullName);
                unityContainer.RegisterType<object, SettingsAppearance>(typeof(SettingsAppearance).FullName);
                unityContainer.RegisterType<object, SettingsBehaviour>(typeof(SettingsBehaviour).FullName);
                unityContainer.RegisterType<object, SettingsCollection>(typeof(SettingsCollection).FullName);
                unityContainer.RegisterType<object, SettingsOnline>(typeof(SettingsOnline).FullName);
                unityContainer.RegisterType<object, SettingsPlayback>(typeof(SettingsPlayback).FullName);
                unityContainer.RegisterType<object, SettingsStartup>(typeof(SettingsStartup).FullName);

                // Information
                unityContainer.RegisterType<object, InformationMenu>(typeof(InformationMenu).FullName);
                unityContainer.RegisterType<object, Information>(typeof(Information).FullName);
                unityContainer.RegisterType<object, InformationHelp>(typeof(InformationHelp).FullName);
                unityContainer.RegisterType<object, InformationAbout>(typeof(InformationAbout).FullName);

                // Now playing
                unityContainer.RegisterType<object, NowPlayingArtistInformation>(typeof(NowPlayingArtistInformation).FullName);
                unityContainer.RegisterType<object, NowPlayingLyrics>(typeof(NowPlayingLyrics).FullName);
                unityContainer.RegisterType<object, NowPlayingPlaylist>(typeof(NowPlayingPlaylist).FullName);
                unityContainer.RegisterType<object, NowPlayingShowcase>(typeof(NowPlayingShowcase).FullName);
            }

            void RegisterViewModels()
            {
            }
        }

        protected override void ConfigureViewModelLocator()
        {
            ViewModelLocationProvider.SetDefaultViewModelFactory(type => Container.Resolve(type));
        }

        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings mappings)
        {
            base.ConfigureRegionAdapterMappings(mappings);
            mappings.RegisterMapping(typeof(SlidingContentControl), Container.Resolve<SlidingContentControlRegionAdapter>());
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        protected override void InitializeShell(Window shell)
        {
            this.InitializeWcfServices();

            Current.MainWindow = shell;

            if (SettingsClient.Get<bool>("General", "ShowOobe"))
            {
                BorderlessWindows10Window oobeWin = Container.Resolve<Oobe>();

                // These 2 lines are required to set the RegionManager of the child window.
                // If we don't do this, regions on child windows are never known by the Shell 
                // RegionManager and navigation doesn't work
                RegionManager.SetRegionManager(oobeWin, Container.Resolve<IRegionManager>());
                RegionManager.UpdateRegions();

                // Show the OOBE window. Don't tell the Indexer to start. 
                // It will get a signal to start when the OOBE window closes.
                LogClient.Info("Showing Oobe screen");
                oobeWin.Show();
                oobeWin.ForceActivate();
            }
            else
            {
                LogClient.Info("Showing Main screen");
                Current.MainWindow.Show();

                // We're not showing the OOBE screen, tell the IndexingService to start.
                Container.Resolve<IIndexingService>().RefreshCollectionAsync();
            }
        }

        protected void InitializeWcfServices()
        {
            var unityContainer = Container.GetContainer();

            // CommandService
            // --------------
            var commandServicehost = new UnityServiceHost(unityContainer, Container.Resolve<ICommandService>().GetType(), new Uri[] { new Uri(string.Format("net.pipe://localhost/{0}/CommandService", ProductInformation.ApplicationName)) });
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
            var fileServicehost = new UnityServiceHost(unityContainer, Container.Resolve<IFileService>().GetType(), new Uri[] { new Uri(string.Format("net.pipe://localhost/{0}/FileService", ProductInformation.ApplicationName)) });
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

        private void ExecuteStartup()
        {
            LogClient.Info($"### STARTING {ProductInformation.ApplicationName}, version {ProcessExecutable.AssemblyVersion()}, IsPortable = {SettingsClient.BaseGet<bool>("Configuration", "IsPortable")}, Windows version = {EnvironmentUtils.GetFriendlyWindowsVersion()} ({EnvironmentUtils.GetInternalWindowsVersion()}), IsWindows10 = {Core.Base.Constants.IsWindows10} ###");

            // Handler for unhandled AppDomain exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var initializer = new Initializer();

            if (initializer.IsMigrationNeeded())
            {
                // Show the Update Window
                BorderlessWindows10Window initWin = new Initialize();
                initWin.Show();
                initWin.ForceActivate();
            }
            else
            {
                // Start the bootstrapper
                //Bootstrapper bootstrapper = new Bootstrapper();
                //bootstrapper.Run();
            }
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

        private void ExecuteEmergencyStop(Exception ex)
        {
            // This is a workaround for a bug in the .Net framework, which randomly causes a System.ArgumentNullException when
            // scrolling through a Virtualizing StackPanel. Scroll to playing song sometimes triggers this bug. We catch the
            // Exception here, and do nothing with it. The application can just proceed. This prevents a complete crash.
            // This might be fixed in .Net 4.5.2. See here: https://connect.microsoft.com/VisualStudio/feedback/details/789438/scrolling-in-virtualized-wpf-treeview-is-very-unstable
            if (ex.GetType().ToString().Equals("System.ArgumentNullException") & ex.Source.ToString().Equals("PresentationCore"))
            {
                LogClient.Warning("Avoided Unhandled Exception: {0}", ex.Message);
                return;
            }

            LogClient.Error("Unhandled Exception. {0}", LogClient.GetAllExceptions(ex));

            // Close the application to prevent further problems
            LogClient.Info("### FORCED STOP of {0}, version {1} ###", ProductInformation.ApplicationName, ProcessExecutable.AssemblyVersion());

            // Stop playing (This avoids remaining processes in Task Manager)
            var playbackService = ServiceLocator.Current.GetInstance<IPlaybackService>();
            playbackService.Stop();

            // Emergency save of the settings
            SettingsClient.Write();

            Application.Current.Shutdown();
        }
    }
}
