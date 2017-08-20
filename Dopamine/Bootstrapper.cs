using Digimezzo.Utilities.Settings;
using Digimezzo.WPFControls;
using Dopamine.Common.Base;
using Dopamine.Common.Database.Repositories;
using Dopamine.Common.Database.Repositories.Interfaces;
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
using Dopamine.Common.Services.Taskbar;
using Dopamine.Common.Services.Update;
using Dopamine.Common.Services.Win32Input;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Extensions;
using Dopamine.Core.Logging;
using Dopamine.Core.Services.Appearance;
using Dopamine.Views;
using Microsoft.Practices.Unity;
using Prism.Modularity;
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
        protected override void ConfigureModuleCatalog()
        {
            base.ConfigureModuleCatalog();
            ModuleCatalog moduleCatalog = (ModuleCatalog)this.ModuleCatalog;

            moduleCatalog.AddModule(typeof(OobeModule.OobeModule));
            moduleCatalog.AddModule(typeof(ControlsModule.ControlsModule));
            moduleCatalog.AddModule(typeof(CollectionModule.CollectionModule));
            moduleCatalog.AddModule(typeof(InformationModule.InformationModule));
            moduleCatalog.AddModule(typeof(SettingsModule.SettingsModule));
            moduleCatalog.AddModule(typeof(FullPlayerModule.FullPlayerModule));
            moduleCatalog.AddModule(typeof(MiniPlayerModule.MiniPlayerModule));
        }

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
            //Container.RegisterSingletonType<ISettingsClient, Settings.SettingsClient>();
            Container.RegisterSingletonType<ILogClient, Logging.LogClient>();
            Container.RegisterSingletonType<ISQLiteConnectionFactory, Common.Database.SQLiteConnectionFactory>();
            Container.RegisterSingletonType<IDbMigrator, Common.Database.DbMigrator>();
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
            Container.RegisterSingletonType<INotificationService, NotificationService>();
            Container.RegisterSingletonType<IEqualizerService, EqualizerService>();
            Container.RegisterSingletonType<IProviderService, ProviderService>();
            Container.RegisterSingletonType<IScrobblingService, LastFmScrobblingService>();
            Container.RegisterSingletonType<IPlaylistService, PlaylistService>();
            Container.RegisterSingletonType<IExternalControlService, ExternalControlService>();
        }

        private void InitializeServices()
        {
            // Making sure resources are set before we need them
            Container.Resolve<II18nService>().ApplyLanguageAsync(SettingsClient.Get<string>("Appearance", "Language"));
            Container.Resolve<IAppearanceService>().ApplyTheme(SettingsClient.Get<bool>("Appearance", "EnableLightTheme"));
            Container.Resolve<IAppearanceService>().ApplyColorSchemeAsync(
                SettingsClient.Get<string>("Appearance", "ColorScheme"),
                SettingsClient.Get<bool>("Appearance", "FollowWindowsColor"),
                SettingsClient.Get<bool>("Appearance", "FollowAlbumCoverColor"),
                true
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
            Container.RegisterType<object, Views.Oobe>(typeof(Views.Oobe).FullName);
            Container.RegisterType<object, Views.Playlist>(typeof(Views.Playlist).FullName);
            Container.RegisterType<object, Views.TrayControls>(typeof(Views.TrayControls).FullName);
            Container.RegisterType<object, Views.Shell>(typeof(Views.Shell).FullName);
            Container.RegisterType<object, Empty>(typeof(Empty).FullName);
        }

        protected void RegisterViewModels()
        {
            Container.RegisterType<object, ViewModels.OobeViewModel>(typeof(ViewModels.OobeViewModel).FullName);
            Container.RegisterType<object, ViewModels.ShellViewModel>(typeof(ViewModels.ShellViewModel).FullName);
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
                Window oobeWin = Container.Resolve<Oobe>();

                // These 2 lines are required to set the RegionManager of the child window.
                // If we don't do this, regions on child windows are never known by the Shell 
                // RegionManager and navigation doesn't work
                RegionManager.SetRegionManager(oobeWin, Container.Resolve<IRegionManager>());
                RegionManager.UpdateRegions();

                // Show the OOBE window. Don't tell the Indexer to start. 
                // It will get a signal to start when the OOBE window closes.
                LogClient.Current.Info("Showing Oobe screen");
                oobeWin.Show();
            }
            else
            {
                LogClient.Current.Info("Showing Main screen");
                Application.Current.MainWindow.Show();

                // We're not showing the OOBE screen, tell the IndexingService to start.
                if (SettingsClient.Get<bool>("Indexing", "RefreshCollectionOnStartup"))
                {
                    Container.Resolve<IIndexingService>().CheckCollectionAsync(SettingsClient.Get<bool>("Indexing", "IgnoreRemovedFiles"), false);
                }
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
                LogClient.Current.Info("CommandService was started successfully");
            }
            catch (Exception ex)
            {
                LogClient.Current.Error("Could not start CommandService. Exception: {0}", ex.Message);
            }

            // FileService
            // -----------
            UnityServiceHost fileServicehost = new UnityServiceHost(Container, Container.Resolve<IFileService>().GetType(), new Uri[] { new Uri(string.Format("net.pipe://localhost/{0}/FileService", ProductInformation.ApplicationName)) });
            fileServicehost.AddServiceEndpoint(typeof(IFileService), new StrongNetNamedPipeBinding(), "FileServiceEndpoint");

            try
            {
                fileServicehost.Open();
                LogClient.Current.Info("FileService was started successfully");
            }
            catch (Exception ex)
            {
                LogClient.Current.Error("Could not start FileService. Exception: {0}", ex.Message);
            }
        }
    }
}
