using Digimezzo.Utilities.Settings;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Core.Helpers;
using Dopamine.Data;
using Dopamine.Data.Repositories;
using Dopamine.Data.Repositories.Interfaces;
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
using Microsoft.Practices.Unity;
using Prism.Modularity;

namespace Dopamine.Services
{
    public class ServicesModule : IModule
    {
        private readonly IUnityContainer container;

        public ServicesModule(IUnityContainer container)
        {
            this.container = container;
        }

        public void Initialize()
        {
            this.RegisterCoreComponents();
            this.RegisterRepositories();
            this.RegisterServices();
            this.InitializeServices();
        }

        private void RegisterCoreComponents()
        {
            this.container.RegisterSingletonType<ISQLiteConnectionFactory, SQLiteConnectionFactory>();
            this.container.RegisterInstance<ILocalizationInfo>(new LocalizationInfo());
        }

        protected void RegisterRepositories()
        {
            this.container.RegisterSingletonType<IFolderRepository, FolderRepository>();
            this.container.RegisterSingletonType<IAlbumRepository, AlbumRepository>();
            this.container.RegisterSingletonType<IArtistRepository, ArtistRepository>();
            this.container.RegisterSingletonType<IGenreRepository, GenreRepository>();
            this.container.RegisterSingletonType<ITrackRepository, TrackRepository>();
            this.container.RegisterSingletonType<ITrackStatisticRepository, TrackStatisticRepository>();
            this.container.RegisterSingletonType<IQueuedTrackRepository, QueuedTrackRepository>();
        }

        private void RegisterServices()
        {
            this.container.RegisterSingletonType<ICacheService, CacheService>();
            this.container.RegisterSingletonType<IUpdateService, UpdateService>();
            this.container.RegisterSingletonType<IAppearanceService, AppearanceService>();
            this.container.RegisterSingletonType<II18nService, I18nService>();
            this.container.RegisterSingletonType<IDialogService, DialogService>();
            this.container.RegisterSingletonType<IIndexingService, IndexingService>();
            this.container.RegisterSingletonType<IPlaybackService, PlaybackService>();
            this.container.RegisterSingletonType<IWin32InputService, Win32InputService>();
            this.container.RegisterSingletonType<ISearchService, SearchService>();
            this.container.RegisterSingletonType<ITaskbarService, TaskbarService>();
            this.container.RegisterSingletonType<ICollectionService, CollectionService>();
            this.container.RegisterSingletonType<IJumpListService, JumpListService>();
            this.container.RegisterSingletonType<IFileService, FileService>();
            this.container.RegisterSingletonType<ICommandService, CommandService>();
            this.container.RegisterSingletonType<IMetadataService, MetadataService>();
            this.container.RegisterSingletonType<IEqualizerService, EqualizerService>();
            this.container.RegisterSingletonType<IProviderService, ProviderService>();
            this.container.RegisterSingletonType<IScrobblingService, LastFmScrobblingService>();
            this.container.RegisterSingletonType<IPlaylistService, PlaylistService>();
            this.container.RegisterSingletonType<IExternalControlService, ExternalControlService>();
            this.container.RegisterSingletonType<IWindowsIntegrationService, WindowsIntegrationService>();
            this.container.RegisterSingletonType<IShellService, ShellService>();

            if (Constants.IsWindows10)
            {
                // NotificationService contains code that is not supported on older versions of Windows
                this.container.RegisterSingletonType<INotificationService, NotificationService>();
            }
            else
            {
                this.container.RegisterSingletonType<INotificationService, LegacyNotificationService>();
            }
        }

        private void InitializeServices()
        {
            // Making sure resources are set before we need them
            this.container.Resolve<II18nService>().ApplyLanguageAsync(SettingsClient.Get<string>("Appearance", "Language"));
            this.container.Resolve<IAppearanceService>().ApplyTheme(SettingsClient.Get<bool>("Appearance", "EnableLightTheme"));
            this.container.Resolve<IAppearanceService>().ApplyColorSchemeAsync(
                SettingsClient.Get<string>("Appearance", "ColorScheme"),
                SettingsClient.Get<bool>("Appearance", "FollowWindowsColor"),
                SettingsClient.Get<bool>("Appearance", "FollowAlbumCoverColor")
            );
            this.container.Resolve<IExternalControlService>();
        }
    }
}
