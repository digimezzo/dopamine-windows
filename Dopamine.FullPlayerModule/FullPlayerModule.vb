Imports Microsoft.Practices.Prism.Modularity
Imports Microsoft.Practices.Prism.Regions
Imports Dopamine.Core.Prism
Imports Microsoft.Practices.Unity
Imports Dopamine.FullPlayerModule.Views
Imports Dopamine.FullPlayerModule.ViewModels
Imports Dopamine.ControlsModule.Views

Public Class FullPlayerModule
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
        Me.mContainer.RegisterType(Of Object, FullPlayer)(GetType(FullPlayer).FullName)
        Me.mContainer.RegisterType(Of Object, FullPlayerViewModel)(GetType(FullPlayerViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, MainMenu)(GetType(MainMenu).FullName)
        Me.mContainer.RegisterType(Of Object, MainMenuViewModel)(GetType(MainMenuViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, Status)(GetType(Status).FullName)
        Me.mContainer.RegisterType(Of Object, StatusViewModel)(GetType(StatusViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, MainScreen)(GetType(MainScreen).FullName)
        Me.mContainer.RegisterType(Of Object, MainScreenViewModel)(GetType(MainScreenViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, NowPlayingScreen)(GetType(NowPlayingScreen).FullName)
        Me.mContainer.RegisterType(Of Object, NowPlayingScreenViewModel)(GetType(NowPlayingScreenViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, NowPlayingScreenShowcase)(GetType(NowPlayingScreenShowcase).FullName)
        Me.mContainer.RegisterType(Of Object, NowPlayingScreenShowcaseViewModel)(GetType(NowPlayingScreenShowcaseViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, NowPlayingScreenList)(GetType(NowPlayingScreenList).FullName)
        Me.mContainer.RegisterType(Of Object, NowPlayingScreenListViewModel)(GetType(NowPlayingScreenListViewModel).FullName)

        Me.mRegionManager.RegisterViewWithRegion(RegionNames.ScreenTypeRegion, GetType(Views.MainScreen))
        Me.mRegionManager.RegisterViewWithRegion(RegionNames.StatusRegion, GetType(Views.Status))
        Me.mRegionManager.RegisterViewWithRegion(RegionNames.MainMenuRegion, GetType(Views.MainMenu))
        Me.mRegionManager.RegisterViewWithRegion(RegionNames.NowPlayingPlaybackControlsRegion, GetType(NowPlayingPlaybackControls))
        Me.mRegionManager.RegisterViewWithRegion(RegionNames.NowPlayingSpectrumAnalyzerRegion, GetType(SpectrumAnalyzerControl))
        Me.mRegionManager.RegisterViewWithRegion(RegionNames.FullPlayerSearchRegion, GetType(SearchControl))
    End Sub
#End Region
End Class
