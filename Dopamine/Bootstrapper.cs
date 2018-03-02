using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.WPFControls;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
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
using Microsoft.Practices.Unity;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Windows;
using Unity.Wcf;

namespace Dopamine
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            this.RegisterCoreComponents();
            this.RegisterFactories();
            this.RegisterRepositories();
            this.RegisterServices();
            this.InitializeServices();
            this.RegisterViews();
            this.RegisterViewModels();

            ViewModelLocationProvider.SetDefaultViewModelFactory((type) => { return Container.Resolve(type); });
        }

        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {

            RegionAdapterMappings mappings = base.ConfigureRegionAdapterMappings();
            mappings.RegisterMapping(typeof(SlidingContentControl), Container.Resolve<SlidingContentControlRegionAdapter>());

            return mappings;
        }

        private void RegisterCoreComponents()
        {
            Container.RegisterInstance<ILocalizationInfo>(new LocalizationInfo());
        }

        private void RegisterServices()
        {
            Container.RegisterSingletonType<ICacheService, CacheService>();
            Container.RegisterSingletonType<IUpdateService, UpdateService>();
            Container.RegisterSingletonType<IAppearanceService, AppearanceService>();
            Container.RegisterSingletonType<II18nService, I18nService>();
            Container.RegisterSingletonType<IDialogService, DialogService>();
            Container.RegisterSingletonType<IIndexingService, IndexingService>();
            Container.RegisterSingletonType<IPlaybackService, PlaybackService>();
            Container.RegisterSingletonType<IWin32InputService, Win32InputService>();
            Container.RegisterSingletonType<ISearchService, SearchService>();
            Container.RegisterSingletonType<ITaskbarService, TaskbarService>();
            Container.RegisterSingletonType<ICollectionService, CollectionService>();
            Container.RegisterSingletonType<IJumpListService, JumpListService>();
            Container.RegisterSingletonType<IFileService, FileService>();
            Container.RegisterSingletonType<ICommandService, CommandService>();
            Container.RegisterSingletonType<IMetadataService, MetadataService>();
            Container.RegisterSingletonType<IEqualizerService, EqualizerService>();
            Container.RegisterSingletonType<IProviderService, ProviderService>();
            Container.RegisterSingletonType<IScrobblingService, LastFmScrobblingService>();
            Container.RegisterSingletonType<IPlaylistService, PlaylistService>();
            Container.RegisterSingletonType<IExternalControlService, ExternalControlService>();
            Container.RegisterSingletonType<IWindowsIntegrationService, WindowsIntegrationService>();
            Container.RegisterSingletonType<IShellService, ShellService>();

            INotificationService notificationService;

            // NotificationService contains code that is only supported on Windows 10
            if (Constants.IsWindows10)
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

            Container.RegisterInstance<INotificationService>(notificationService);
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
            Container.RegisterSingletonType<IFolderRepository, FolderRepository>();
            Container.RegisterSingletonType<IAlbumRepository, AlbumRepository>();
            Container.RegisterSingletonType<IArtistRepository, ArtistRepository>();
            Container.RegisterSingletonType<IGenreRepository, GenreRepository>();
            Container.RegisterSingletonType<ITrackRepository, TrackRepository>();
            Container.RegisterSingletonType<ITrackStatisticRepository, TrackStatisticRepository>();
            Container.RegisterSingletonType<IQueuedTrackRepository, QueuedTrackRepository>();
        }

        protected void RegisterFactories()
        {
            Container.RegisterSingletonType<ISQLiteConnectionFactory, SQLiteConnectionFactory>();
            Container.RegisterSingletonType<IFileMetadataFactory, FileMetadataFactory>();
        }

        protected void RegisterViews()
        {
            // Misc.
            Container.RegisterType<object, Oobe>(typeof(Oobe).FullName);
            Container.RegisterType<object, TrayControls>(typeof(TrayControls).FullName);
            Container.RegisterType<object, Shell>(typeof(Shell).FullName);
            Container.RegisterType<object, Empty>(typeof(Empty).FullName);
            Container.RegisterType<object, FullPlayer>(typeof(FullPlayer).FullName);
            Container.RegisterType<object, CoverPlayer>(typeof(CoverPlayer).FullName);
            Container.RegisterType<object, MicroPlayer>(typeof(MicroPlayer).FullName);
            Container.RegisterType<object, NanoPlayer>(typeof(NanoPlayer).FullName);
            Container.RegisterType<object, NowPlaying>(typeof(NowPlaying).FullName);

            // Collection
            Container.RegisterType<object, CollectionMenu>(typeof(CollectionMenu).FullName);
            Container.RegisterType<object, Collection>(typeof(Collection).FullName);
            Container.RegisterType<object, CollectionAlbums>(typeof(CollectionAlbums).FullName);
            Container.RegisterType<object, CollectionArtists>(typeof(CollectionArtists).FullName);
            Container.RegisterType<object, CollectionFrequent>(typeof(CollectionFrequent).FullName);
            Container.RegisterType<object, CollectionGenres>(typeof(CollectionGenres).FullName);
            Container.RegisterType<object, CollectionPlaylists>(typeof(CollectionPlaylists).FullName);
            Container.RegisterType<object, CollectionTracks>(typeof(CollectionTracks).FullName);

            // Settings
            Container.RegisterType<object, SettingsMenu>(typeof(SettingsMenu).FullName);
            Container.RegisterType<object, Settings>(typeof(Settings).FullName);
            Container.RegisterType<object, SettingsAppearance>(typeof(SettingsAppearance).FullName);
            Container.RegisterType<object, SettingsBehaviour>(typeof(SettingsBehaviour).FullName);
            Container.RegisterType<object, SettingsCollection>(typeof(SettingsCollection).FullName);
            Container.RegisterType<object, SettingsOnline>(typeof(SettingsOnline).FullName);
            Container.RegisterType<object, SettingsPlayback>(typeof(SettingsPlayback).FullName);
            Container.RegisterType<object, SettingsStartup>(typeof(SettingsStartup).FullName);

            // Information
            Container.RegisterType<object, InformationMenu>(typeof(InformationMenu).FullName);
            Container.RegisterType<object, Information>(typeof(Information).FullName);
            Container.RegisterType<object, InformationHelp>(typeof(InformationHelp).FullName);
            Container.RegisterType<object, InformationAbout>(typeof(InformationAbout).FullName);

            // Now playing
            Container.RegisterType<object, NowPlayingArtistInformation>(typeof(NowPlayingArtistInformation).FullName);
            Container.RegisterType<object, NowPlayingLyrics>(typeof(NowPlayingLyrics).FullName);
            Container.RegisterType<object, NowPlayingPlaylist>(typeof(NowPlayingPlaylist).FullName);
            Container.RegisterType<object, NowPlayingShowcase>(typeof(NowPlayingShowcase).FullName);
        }

        protected void RegisterViewModels()
        {
        }

        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            this.InitializeWCFServices();

            Application.Current.MainWindow = (Window)this.Shell;

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
    }
}
