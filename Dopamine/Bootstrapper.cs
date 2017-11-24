using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.WPFControls;
using Dopamine.Common.Base;
using Dopamine.Common.Controls;
using Dopamine.Common.Database;
using Dopamine.Common.Database.Repositories;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.Extensions;
using Dopamine.Common.Helpers;
using Dopamine.Common.IO;
using Dopamine.Common.Presentation.Utils;
using Dopamine.Common.Presentation.Views;
using Dopamine.Common.Services.Appearance;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Collection;
using Dopamine.Common.Services.Command;
using Dopamine.Common.Services.Dialog;
using Dopamine.Common.Services.Equalizer;
using Dopamine.Common.Services.ExternalControl;
using Dopamine.Common.Services.File;
using Dopamine.Common.Services.I18n;
using Dopamine.Common.Services.Indexing;
using Dopamine.Common.Services.JumpList;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Notification;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Services.Playlist;
using Dopamine.Common.Services.Provider;
using Dopamine.Common.Services.Scrobbling;
using Dopamine.Common.Services.Search;
using Dopamine.Common.Services.Shell;
using Dopamine.Common.Services.Taskbar;
using Dopamine.Common.Services.Update;
using Dopamine.Common.Services.Win32Input;
using Dopamine.Common.Services.WindowsIntegration;
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
            Container.RegisterSingletonType<ISQLiteConnectionFactory, SQLiteConnectionFactory>();
            Container.RegisterInstance<ILocalizationInfo>(new LocalizationInfo());
        }

        private void RegisterServices()
        {
            Container.RegisterSingletonType<ICacheService, CacheService>();
            Container.RegisterSingletonType<IUpdateService, UpdateService>();
            Container.RegisterSingletonType<IAppearanceService, Common.Services.Appearance.AppearanceService>();
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

            if (Constants.IsWindows10)
            {
                // NotificationService contains code that is not supported on older versions of Windows
                Container.RegisterSingletonType<INotificationService, NotificationService>();
            }
            else
            {
                Container.RegisterSingletonType<INotificationService, LegacyNotificationService>();
            }
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

        protected void RegisterViews()
        {
            // Misc.
            Container.RegisterType<object, Oobe>(typeof(Oobe).FullName);
            Container.RegisterType<object, TrayControls>(typeof(TrayControls).FullName);
            Container.RegisterType<object, Shell>(typeof(Views.Shell).FullName);
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
