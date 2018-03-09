using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.WPFControls;
using Dopamine.Core.Base;
using Dopamine.Core.Helpers;
using Dopamine.Core.IO;
using Dopamine.Data;
using Dopamine.Data.Contracts;
using Dopamine.Data.Contracts.Metadata;
using Dopamine.Data.Contracts.Repositories;
using Dopamine.Data.Metadata;
using Dopamine.Data.Repositories;
using Dopamine.Presentation.Utils;
using Dopamine.Presentation.Views;
using Dopamine.Services.Appearance;
using Dopamine.Services.Cache;
using Dopamine.Services.Collection;
using Dopamine.Services.Command;
using Dopamine.Services.Contracts.Appearance;
using Dopamine.Services.Contracts.Cache;
using Dopamine.Services.Contracts.Collection;
using Dopamine.Services.Contracts.Command;
using Dopamine.Services.Contracts.Dialog;
using Dopamine.Services.Contracts.Equalizer;
using Dopamine.Services.Contracts.ExternalControl;
using Dopamine.Services.Contracts.File;
using Dopamine.Services.Contracts.I18n;
using Dopamine.Services.Contracts.Indexing;
using Dopamine.Services.Contracts.JumpList;
using Dopamine.Services.Contracts.Metadata;
using Dopamine.Services.Contracts.Notification;
using Dopamine.Services.Contracts.Playback;
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
using Dopamine.Views;
using Dopamine.Views.Common;
using Dopamine.Views.FullPlayer;
using Dopamine.Views.FullPlayer.Collection;
using Dopamine.Views.FullPlayer.Information;
using Dopamine.Views.FullPlayer.Settings;
using Dopamine.Views.MiniPlayer;
using Dopamine.Views.NowPlaying;
using Unity;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Windows;
using Unity.Wcf;
using Prism.Ioc;

namespace Dopamine
{
    public class Bootstrapper : PrismApplication
    {
        protected IContainerRegistry ContainerRegistry { get; set; }
        protected IUnityContainer Container { get; set; }

        public override void Initialize()
        {

        }

        protected override void ConfigureViewModelLocator()
        {
            ViewModelLocationProvider.SetDefaultViewModelFactory(type => Container.Resolve(type));
        }

        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings mappings)
        {
            mappings.RegisterMapping(typeof(SlidingContentControl), Container.Resolve<SlidingContentControlRegionAdapter>());
        }

        private void RegisterCoreComponents()
        {
            ContainerRegistry.RegisterInstance<ILocalizationInfo>(new LocalizationInfo());
        }

        private void RegisterServices()
        {
            ContainerRegistry.RegisterSingleton<ICacheService, CacheService>();
            ContainerRegistry.RegisterSingleton<IUpdateService, UpdateService>();
            ContainerRegistry.RegisterSingleton<IAppearanceService, AppearanceService>();
            ContainerRegistry.RegisterSingleton<II18nService, I18nService>();
            ContainerRegistry.RegisterSingleton<IDialogService, DialogService>();
            ContainerRegistry.RegisterSingleton<IIndexingService, IndexingService>();
            ContainerRegistry.RegisterSingleton<IPlaybackService, PlaybackService>();
            ContainerRegistry.RegisterSingleton<IWin32InputService, Win32InputService>();
            ContainerRegistry.RegisterSingleton<ISearchService, SearchService>();
            ContainerRegistry.RegisterSingleton<ITaskbarService, TaskbarService>();
            ContainerRegistry.RegisterSingleton<ICollectionService, CollectionService>();
            ContainerRegistry.RegisterSingleton<IJumpListService, JumpListService>();
            ContainerRegistry.RegisterSingleton<IFileService, FileService>();
            ContainerRegistry.RegisterSingleton<ICommandService, CommandService>();
            ContainerRegistry.RegisterSingleton<IMetadataService, MetadataService>();
            ContainerRegistry.RegisterSingleton<IEqualizerService, EqualizerService>();
            ContainerRegistry.RegisterSingleton<IProviderService, ProviderService>();
            ContainerRegistry.RegisterSingleton<IScrobblingService, LastFmScrobblingService>();
            ContainerRegistry.RegisterSingleton<IPlaylistService, PlaylistService>();
            ContainerRegistry.RegisterSingleton<IExternalControlService, ExternalControlService>();
            ContainerRegistry.RegisterSingleton<IWindowsIntegrationService, WindowsIntegrationService>();
            ContainerRegistry.RegisterSingleton<IShellService, ShellService>();

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

            ContainerRegistry.RegisterInstance<INotificationService>(notificationService);
        }

        private void InitializeServices()
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

        protected void RegisterRepositories()
        {
            ContainerRegistry.RegisterSingleton<IFolderRepository, FolderRepository>();
            ContainerRegistry.RegisterSingleton<IAlbumRepository, AlbumRepository>();
            ContainerRegistry.RegisterSingleton<IArtistRepository, ArtistRepository>();
            ContainerRegistry.RegisterSingleton<IGenreRepository, GenreRepository>();
            ContainerRegistry.RegisterSingleton<ITrackRepository, TrackRepository>();
            ContainerRegistry.RegisterSingleton<ITrackStatisticRepository, TrackStatisticRepository>();
            ContainerRegistry.RegisterSingleton<IQueuedTrackRepository, QueuedTrackRepository>();
        }

        protected void RegisterFactories()
        {
            ContainerRegistry.RegisterSingleton<ISQLiteConnectionFactory, SQLiteConnectionFactory>();
            ContainerRegistry.RegisterSingleton<IFileMetadataFactory, FileMetadataFactory>();
        }

        protected void RegisterViews()
        {
            // Misc.
            ContainerRegistry.RegisterSingleton<Oobe>();
            ContainerRegistry.RegisterSingleton<TrayControls>();
            ContainerRegistry.RegisterSingleton<Shell>();
            ContainerRegistry.RegisterSingleton<Empty>();
            ContainerRegistry.RegisterSingleton<FullPlayer>();
            ContainerRegistry.RegisterSingleton<CoverPlayer>();
            ContainerRegistry.RegisterSingleton<MicroPlayer>();
            ContainerRegistry.RegisterSingleton<NanoPlayer>();
            ContainerRegistry.RegisterSingleton<NowPlaying>();

            // Collection
            ContainerRegistry.RegisterSingleton<CollectionMenu>();
            ContainerRegistry.RegisterSingleton<Collection>();
            ContainerRegistry.RegisterSingleton<CollectionAlbums>();
            ContainerRegistry.RegisterSingleton<CollectionArtists>();
            ContainerRegistry.RegisterSingleton<CollectionFrequent>();
            ContainerRegistry.RegisterSingleton<CollectionGenres>();
            ContainerRegistry.RegisterSingleton<CollectionPlaylists>();
            ContainerRegistry.RegisterSingleton<CollectionTracks>();

            // Settings
            ContainerRegistry.RegisterSingleton<SettingsMenu>();
            ContainerRegistry.RegisterSingleton<Settings>();
            ContainerRegistry.RegisterSingleton<SettingsAppearance>();
            ContainerRegistry.RegisterSingleton<SettingsBehaviour>();
            ContainerRegistry.RegisterSingleton<SettingsCollection>();
            ContainerRegistry.RegisterSingleton<SettingsOnline>();
            ContainerRegistry.RegisterSingleton<SettingsPlayback>();
            ContainerRegistry.RegisterSingleton<SettingsStartup>();

            // Information
            ContainerRegistry.RegisterSingleton<InformationMenu>();
            ContainerRegistry.RegisterSingleton<Information>();
            ContainerRegistry.RegisterSingleton<InformationHelp>();
            ContainerRegistry.RegisterSingleton<InformationAbout>();

            // Now playing
            ContainerRegistry.RegisterSingleton<NowPlayingArtistInformation>();
            ContainerRegistry.RegisterSingleton<NowPlayingLyrics>();
            ContainerRegistry.RegisterSingleton<NowPlayingPlaylist>();
            ContainerRegistry.RegisterSingleton<NowPlayingShowcase>();
        }

        protected void RegisterViewModels()
        {
        }

        protected override Window CreateShell()
        {
            return CommonServiceLocator.ServiceLocator.Current.GetInstance<Shell>();
        }

        protected override void InitializeShell(Window shell)
        {
            this.InitializeWCFServices();

            Application.Current.MainWindow = (Window)shell;

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
                Application.Current.MainWindow.Show();

                // We're not showing the OOBE screen, tell the IndexingService to start.
                Container.Resolve<IIndexingService>().RefreshCollectionAsync();
            }
        }

        protected void InitializeWCFServices()
        {
            // CommandService
            // --------------
            UnityServiceHost commandServicehost = new UnityServiceHost(Container, Container.Resolve<ICommandService>().GetType(), new Uri[] { new Uri(string.Format("net.pipe://localhost/{0}/CommandService", ProductInformation.ApplicationName)) });
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
            UnityServiceHost fileServicehost = new UnityServiceHost(Container, Container.Resolve<IFileService>().GetType(), new Uri[] { new Uri(string.Format("net.pipe://localhost/{0}/FileService", ProductInformation.ApplicationName)) });
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
            base.RegisterRequiredTypes(containerRegistry);

            this.ContainerRegistry = containerRegistry;
            this.Container = containerRegistry.GetContainer();

            this.RegisterCoreComponents();
            this.RegisterFactories();
            this.RegisterRepositories();
            this.RegisterServices();
            this.InitializeServices();
            this.RegisterViews();
            this.RegisterViewModels();
        }
    }
}
