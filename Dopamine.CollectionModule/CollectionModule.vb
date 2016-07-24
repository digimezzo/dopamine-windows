Imports Microsoft.Practices.Prism.Modularity
Imports Microsoft.Practices.Prism.Regions
Imports Microsoft.Practices.Unity
Imports Dopamine.CollectionModule.ViewModels
Imports Dopamine.CollectionModule.Views
Imports Dopamine.Core.Prism
Imports Dopamine.ControlsModule.Views
Imports Dopamine.Core.Settings

Public Class CollectionModule
    Implements IModule

#Region "Variables"
    Private ReadOnly mRegionManager As IRegionManager
    Private mContainer As IUnityContainer
#End Region

#Region "Construction"
    Public Sub New(iRegionManager As IRegionManager, iContainer As IUnityContainer)
        Me.mRegionManager = iRegionManager
        Me.mContainer = iContainer
    End Sub
#End Region

#Region "Functions"
    Public Sub Initialize() Implements IModule.Initialize
        Me.mContainer.RegisterType(Of Object, CollectionViewModel)(GetType(CollectionViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, Collection)(GetType(Collection).FullName)
        Me.mContainer.RegisterType(Of Object, CollectionSubMenu)(GetType(CollectionSubMenu).FullName)
        Me.mContainer.RegisterType(Of Object, CollectionAlbumsViewModel)(GetType(CollectionAlbumsViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, CollectionAlbums)(GetType(CollectionAlbums).FullName)
        Me.mContainer.RegisterType(Of Object, CollectionArtistsViewModel)(GetType(CollectionArtistsViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, CollectionArtists)(GetType(CollectionArtists).FullName)
        Me.mContainer.RegisterType(Of Object, CollectionGenresViewModel)(GetType(CollectionGenresViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, CollectionGenres)(GetType(CollectionGenres).FullName)
        Me.mContainer.RegisterType(Of Object, CollectionPlaylistsViewModel)(GetType(CollectionPlaylistsViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, CollectionPlaylists)(GetType(CollectionPlaylists).FullName)
        Me.mContainer.RegisterType(Of Object, CollectionTracksViewModel)(GetType(CollectionTracksViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, CollectionTracks)(GetType(CollectionTracks).FullName)
        Me.mContainer.RegisterType(Of Object, CollectionCloudViewModel)(GetType(CollectionCloudViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, CollectionCloud)(GetType(CollectionCloud).FullName)
        Me.mContainer.RegisterType(Of Object, CollectionTracksColumnsViewModel)(GetType(CollectionTracksColumnsViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, CollectionTracksColumns)(GetType(CollectionTracksColumns).FullName)

        Me.mRegionManager.RegisterViewWithRegion(RegionNames.SubMenuRegion, GetType(CollectionSubMenu))
        Me.mRegionManager.RegisterViewWithRegion(RegionNames.ContentRegion, GetType(Collection))

        Me.mRegionManager.RegisterViewWithRegion(RegionNames.CollectionPlaybackControlsRegion, GetType(CollectionPlaybackControls))
        Me.mRegionManager.RegisterViewWithRegion(RegionNames.CollectionSpectrumAnalyzerRegion, GetType(SpectrumAnalyzerControl))

        If XmlSettingsClient.Instance.Get(Of Boolean)("Startup", "ShowLastSelectedPage") Then
            Dim screen As SelectedPage = CType(XmlSettingsClient.Instance.Get(Of Integer)("FullPlayer", "SelectedPage"), SelectedPage)

            Select Case screen
                Case SelectedPage.Artists
                    Me.mRegionManager.RegisterViewWithRegion(RegionNames.CollectionContentRegion, GetType(CollectionArtists))
                Case SelectedPage.Genres
                    Me.mRegionManager.RegisterViewWithRegion(RegionNames.CollectionContentRegion, GetType(CollectionGenres))
                Case SelectedPage.Albums
                    Me.mRegionManager.RegisterViewWithRegion(RegionNames.CollectionContentRegion, GetType(CollectionAlbums))
                Case SelectedPage.Tracks
                    Me.mRegionManager.RegisterViewWithRegion(RegionNames.CollectionContentRegion, GetType(CollectionTracks))
                Case SelectedPage.Playlists
                    Me.mRegionManager.RegisterViewWithRegion(RegionNames.CollectionContentRegion, GetType(CollectionPlaylists))
                Case SelectedPage.Recent
                    Me.mRegionManager.RegisterViewWithRegion(RegionNames.CollectionContentRegion, GetType(CollectionCloud))
                Case Else
                    Me.mRegionManager.RegisterViewWithRegion(RegionNames.CollectionContentRegion, GetType(CollectionArtists))
            End Select
        Else
            Me.mRegionManager.RegisterViewWithRegion(RegionNames.CollectionContentRegion, GetType(CollectionArtists))
        End If

    End Sub
#End Region
End Class
