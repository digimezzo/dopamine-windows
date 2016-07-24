Imports Digimezzo.WPFControls
Imports Dopamine.Common.Presentation.Utils
Imports Dopamine.Common.Presentation.Views
Imports Dopamine.Common.Services.Appearance
Imports Dopamine.Common.Services.Collection
Imports Dopamine.Common.Services.Command
Imports Dopamine.Common.Services.Dialog
Imports Dopamine.Common.Services.File
Imports Dopamine.Common.Services.I18n
Imports Dopamine.Common.Services.Indexing
Imports Dopamine.Common.Services.JumpList
Imports Dopamine.Common.Services.Metadata
Imports Dopamine.Common.Services.Notification
Imports Dopamine.Common.Services.Playback
Imports Dopamine.Common.Services.Search
Imports Dopamine.Common.Services.Taskbar
Imports Dopamine.Common.Services.Update
Imports Dopamine.Common.Services.Win32Input
Imports Dopamine.Core.Base
Imports Dopamine.Core.Database.Repositories
Imports Dopamine.Core.Database.Repositories.Interfaces
Imports Dopamine.Core.Extensions
Imports Dopamine.Core.IO
Imports Dopamine.Core.Logging
Imports Dopamine.Core.Settings
Imports Dopamine.Views
Imports Microsoft.Practices.Prism.Modularity
Imports Microsoft.Practices.Prism.Mvvm
Imports Microsoft.Practices.Prism.Regions
Imports Microsoft.Practices.Prism.UnityExtensions
Imports Microsoft.Practices.Unity
Imports RaphaelGodart.Controls
Imports Unity.Wcf

Public Class Bootstrapper
    Inherits UnityBootstrapper

    Protected Overrides Sub ConfigureModuleCatalog()
        MyBase.ConfigureModuleCatalog()
        Dim moduleCatalog As ModuleCatalog = DirectCast(Me.ModuleCatalog, ModuleCatalog)

        moduleCatalog.AddModule(GetType(OobeModule.OobeModule))
        moduleCatalog.AddModule(GetType(ControlsModule.ControlsModule))
        moduleCatalog.AddModule(GetType(CollectionModule.CollectionModule))
        moduleCatalog.AddModule(GetType(InformationModule.InformationModule))
        moduleCatalog.AddModule(GetType(SettingsModule.SettingsModule))
        moduleCatalog.AddModule(GetType(FullPlayerModule.FullPlayerModule))
        moduleCatalog.AddModule(GetType(MiniPlayerModule.MiniPlayerModule))
    End Sub

    Protected Overrides Sub ConfigureContainer()
        MyBase.ConfigureContainer()

        Me.RegisterRepositories()
        Me.RegisterServices()
        Me.InitializeServices()
        Me.RegisterViews()
        Me.RegisterViewModels()

        ViewModelLocationProvider.SetDefaultViewModelFactory(Function(type)
                                                                 Return Container.Resolve(type)
                                                             End Function)
    End Sub

    Protected Overrides Function ConfigureRegionAdapterMappings() As RegionAdapterMappings

        Dim mappings As RegionAdapterMappings = MyBase.ConfigureRegionAdapterMappings()
        mappings.RegisterMapping(GetType(SlidingContentControl), Container.Resolve(Of SlidingContentControlRegionAdapter)())

        Return mappings
    End Function

    Protected Sub RegisterServices()
        Container.RegisterSingletonType(Of IUpdateService, UpdateService)()
        Container.RegisterSingletonType(Of IAppearanceService, AppearanceService)()
        Container.RegisterSingletonType(Of II18nService, I18nService)()
        Container.RegisterSingletonType(Of IDialogService, DialogService)()
        Container.RegisterSingletonType(Of IIndexingService, IndexingService)()
        Container.RegisterSingletonType(Of IPlaybackService, PlaybackService)()
        Container.RegisterSingletonType(Of IWin32InputService, Win32InputService)()
        Container.RegisterSingletonType(Of ISearchService, SearchService)()
        Container.RegisterSingletonType(Of ITaskbarService, TaskbarService)()
        Container.RegisterSingletonType(Of ICollectionService, CollectionService)()
        Container.RegisterSingletonType(Of IJumpListService, JumpListService)()
        Container.RegisterSingletonType(Of IFileService, FileService)()
        Container.RegisterSingletonType(Of ICommandService, CommandService)()
        Container.RegisterSingletonType(Of IMetadataService, MetadataService)()
        Container.RegisterSingletonType(Of INotificationService, NotificationService)()
    End Sub

    Private Sub InitializeServices()

        ' Making sure resources are set before we need them
        Container.Resolve(Of II18nService)().ApplyLanguageAsync(XmlSettingsClient.Instance.Get(Of String)("Appearance", "Language"))
        Container.Resolve(Of IAppearanceService)().ApplyTheme(XmlSettingsClient.Instance.Get(Of Boolean)("Appearance", "EnableLightTheme"))
        Container.Resolve(Of IAppearanceService)().ApplyColorScheme(XmlSettingsClient.Instance.Get(Of Boolean)("Appearance", "FollowWindowsColor"), XmlSettingsClient.Instance.Get(Of String)("Appearance", "ColorScheme"))
    End Sub

    Protected Sub RegisterRepositories()

        Container.RegisterSingletonType(Of IFolderRepository, FolderRepository)()
        Container.RegisterSingletonType(Of IPlaylistRepository, PlaylistRepository)()
        Container.RegisterSingletonType(Of IPlaylistEntryRepository, PlaylistEntryRepository)()
        Container.RegisterSingletonType(Of IAlbumRepository, AlbumRepository)()
        Container.RegisterSingletonType(Of IArtistRepository, ArtistRepository)()
        Container.RegisterSingletonType(Of IGenreRepository, GenreRepository)()
        Container.RegisterSingletonType(Of ITrackRepository, TrackRepository)()
        Container.RegisterSingletonType(Of IQueuedTrackRepository, QueuedTrackRepository)()
    End Sub


    Protected Sub RegisterViews()
        Container.RegisterType(Of Object, Views.Oobe)(GetType(Views.Oobe).FullName)
        Container.RegisterType(Of Object, Views.Playlist)(GetType(Views.Playlist).FullName)
        Container.RegisterType(Of Object, Views.TrayControls)(GetType(Views.TrayControls).FullName)
        Container.RegisterType(Of Object, Views.Shell)(GetType(Views.Shell).FullName)
        Container.RegisterType(Of Object, Empty)(GetType(Empty).FullName)
    End Sub

    Protected Sub RegisterViewModels()
        Container.RegisterType(Of Object, ViewModels.OobeViewModel)(GetType(ViewModels.OobeViewModel).FullName)
        Container.RegisterType(Of Object, ViewModels.ShellViewModel)(GetType(ViewModels.ShellViewModel).FullName)
    End Sub

    Protected Overrides Function CreateShell() As DependencyObject

        Return Container.Resolve(Of Shell)()
    End Function

    Protected Overrides Sub InitializeShell()
        MyBase.InitializeShell()

        Me.InitializeWCFServices()

        Application.Current.MainWindow = DirectCast(Me.Shell, Window)

        If XmlSettingsClient.Instance.Get(Of Boolean)("General", "ShowOobe") Then
            Dim oobeWin As Window = Container.Resolve(Of Oobe)()

            ' These 2 lines are required to set the RegionManager of the child window.
            ' If we don't do this, regions on child windows are never known by the Shell 
            ' RegionManager and navigation doesn't work
            RegionManager.SetRegionManager(oobeWin, Container.Resolve(Of IRegionManager)())
            RegionManager.UpdateRegions()

            ' Show the OOBE window. Don't tell the Indexer to start. 
            ' It will get a signal to start when the OOBE window closes.
            LogClient.Instance.Logger.Info("Showing Oobe screen")
            oobeWin.Show()
        Else
            LogClient.Instance.Logger.Info("Showing Main screen")
            Application.Current.MainWindow.Show()

            ' We're not showing the OOBE screen, tell the IndexingService to start.
            Container.Resolve(Of IIndexingService)().CheckCollectionAsync(XmlSettingsClient.Instance.Get(Of Boolean)("Indexing", "IgnoreRemovedFiles"), False)
        End If
    End Sub

    Protected Sub InitializeWCFServices()

        ' CommandService
        ' --------------
        Dim commandServicehost As New UnityServiceHost(Container, Container.Resolve(Of ICommandService).GetType, New Uri() {New Uri(String.Format("net.pipe://localhost/{0}/CommandService", ProductInformation.ApplicationDisplayName))})
        commandServicehost.AddServiceEndpoint(GetType(ICommandService), New StrongNetNamedPipeBinding(), "CommandServiceEndpoint")

        Try
            commandServicehost.Open()
            LogClient.Instance.Logger.Info("CommandService was started successfully")
        Catch ex As Exception
            LogClient.Instance.Logger.Error("Could not start CommandService. Exception: {0}", ex.Message)
        End Try

        ' FileService
        ' -----------
        Dim fileServicehost As New UnityServiceHost(Container, Container.Resolve(Of IFileService).GetType, New Uri() {New Uri(String.Format("net.pipe://localhost/{0}/FileService", ProductInformation.ApplicationDisplayName))})
        fileServicehost.AddServiceEndpoint(GetType(IFileService), New StrongNetNamedPipeBinding(), "FileServiceEndpoint")

        Try
            fileServicehost.Open()
            LogClient.Instance.Logger.Info("FileService was started successfully")
        Catch ex As Exception
            LogClient.Instance.Logger.Error("Could not start FileService. Exception: {0}", ex.Message)
        End Try
    End Sub
End Class
