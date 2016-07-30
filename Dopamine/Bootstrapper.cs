using Digimezzo.WPFControls;
using Dopamine.Common.Presentation.Utils;
using Dopamine.Common.Presentation.Views;
using Dopamine.Common.Services.Appearance;
using Dopamine.Common.Services.Collection;
using Dopamine.Common.Services.Command;
using Dopamine.Common.Services.Dialog;
using Dopamine.Common.Services.File;
using Dopamine.Common.Services.I18n;
using Dopamine.Common.Services.Indexing;
using Dopamine.Common.Services.JumpList;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Notification;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Services.Search;
using Dopamine.Common.Services.Taskbar;
using Dopamine.Common.Services.Update;
using Dopamine.Common.Services.Win32Input;
using Dopamine.Core.Base;
using Dopamine.Core.Database.Repositories;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Extensions;
using Dopamine.Core.IO;
using Dopamine.Core.Logging;
using Dopamine.Core.Settings;
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

        protected void RegisterServices()
        {
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
            Container.RegisterSingletonType<INotificationService, NotificationService>();
        }

        private void InitializeServices()
        {
            // Making sure resources are set before we need them
            Container.Resolve<II18nService>().ApplyLanguageAsync(XmlSettingsClient.Instance.Get<string>("Appearance", "Language"));
            Container.Resolve<IAppearanceService>().ApplyTheme(XmlSettingsClient.Instance.Get<bool>("Appearance", "EnableLightTheme"));
            Container.Resolve<IAppearanceService>().ApplyColorScheme(XmlSettingsClient.Instance.Get<bool>("Appearance", "FollowWindowsColor"), XmlSettingsClient.Instance.Get<string>("Appearance", "ColorScheme"));
        }


        protected void RegisterRepositories()
        {
            Container.RegisterSingletonType<IFolderRepository, FolderRepository>();
            Container.RegisterSingletonType<IPlaylistRepository, PlaylistRepository>();
            Container.RegisterSingletonType<IPlaylistEntryRepository, PlaylistEntryRepository>();
            Container.RegisterSingletonType<IAlbumRepository, AlbumRepository>();
            Container.RegisterSingletonType<IArtistRepository, ArtistRepository>();
            Container.RegisterSingletonType<IGenreRepository, GenreRepository>();
            Container.RegisterSingletonType<ITrackRepository, TrackRepository>();
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

            if (XmlSettingsClient.Instance.Get<bool>("General", "ShowOobe"))
            {
                Window oobeWin = Container.Resolve<Oobe>();

                // These 2 lines are required to set the RegionManager of the child window.
                // If we don't do this, regions on child windows are never known by the Shell 
                // RegionManager and navigation doesn't work
                RegionManager.SetRegionManager(oobeWin, Container.Resolve<IRegionManager>());
                RegionManager.UpdateRegions();

                // Show the OOBE window. Don't tell the Indexer to start. 
                // It will get a signal to start when the OOBE window closes.
                LogClient.Instance.Logger.Info("Showing Oobe screen");
                oobeWin.Show();
            }
            else
            {
                LogClient.Instance.Logger.Info("Showing Main screen");
                Application.Current.MainWindow.Show();

                // We're not showing the OOBE screen, tell the IndexingService to start.
                Container.Resolve<IIndexingService>().CheckCollectionAsync(XmlSettingsClient.Instance.Get<bool>("Indexing", "IgnoreRemovedFiles"), false);
            }
        }


        protected void InitializeWCFServices()
        {
            // CommandService
            // --------------
            UnityServiceHost commandServicehost = new UnityServiceHost(Container, Container.Resolve<ICommandService>().GetType(), new Uri[] { new Uri(string.Format("net.pipe://localhost/{0}/CommandService", ProductInformation.ApplicationDisplayName)) });
            commandServicehost.AddServiceEndpoint(typeof(ICommandService), new StrongNetNamedPipeBinding(), "CommandServiceEndpoint");

            try
            {
                commandServicehost.Open();
                LogClient.Instance.Logger.Info("CommandService was started successfully");
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not start CommandService. Exception: {0}", ex.Message);
            }

            // FileService
            // -----------
            UnityServiceHost fileServicehost = new UnityServiceHost(Container, Container.Resolve<IFileService>().GetType(), new Uri[] { new Uri(string.Format("net.pipe://localhost/{0}/FileService", ProductInformation.ApplicationDisplayName)) });
            fileServicehost.AddServiceEndpoint(typeof(IFileService), new StrongNetNamedPipeBinding(), "FileServiceEndpoint");

            try
            {
                fileServicehost.Open();
                LogClient.Instance.Logger.Info("FileService was started successfully");
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not start FileService. Exception: {0}", ex.Message);
            }
        }
    }
}
