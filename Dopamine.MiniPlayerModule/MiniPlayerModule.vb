Imports Microsoft.Practices.Prism.Modularity
Imports Microsoft.Practices.Prism.Regions
Imports Dopamine.Core.Prism
Imports Microsoft.Practices.Unity
Imports Dopamine.MiniPlayerModule.Views
Imports Dopamine.MiniPlayerModule.ViewModels
Imports Dopamine.ControlsModule.Views

Public Class MiniPlayerModule
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
        Me.mContainer.RegisterType(Of Object, CoverPlayer)(GetType(CoverPlayer).FullName)
        Me.mContainer.RegisterType(Of Object, CoverPlayerViewModel)(GetType(CoverPlayerViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, MicroPlayer)(GetType(MicroPlayer).FullName)
        Me.mContainer.RegisterType(Of Object, MicroPlayerViewModel)(GetType(MicroPlayerViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, MiniPlayerPlaylist)(GetType(MiniPlayerPlaylist).FullName)
        Me.mContainer.RegisterType(Of Object, MiniPlayerPlaylistViewModel)(GetType(MiniPlayerPlaylistViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, NanoPlayer)(GetType(NanoPlayer).FullName)
        Me.mContainer.RegisterType(Of Object, NanoPlayerViewModel)(GetType(NanoPlayerViewModel).FullName)

        Me.mRegionManager.RegisterViewWithRegion(RegionNames.CoverPlayerControlsRegion, GetType(CoverPlayerControls))
        Me.mRegionManager.RegisterViewWithRegion(RegionNames.CoverPlayerSpectrumAnalyzerRegion, GetType(SpectrumAnalyzerControl))
        Me.mRegionManager.RegisterViewWithRegion(RegionNames.MicroPlayerControlsRegion, GetType(MicroPlayerControls))
        Me.mRegionManager.RegisterViewWithRegion(RegionNames.MicroPlayerSpectrumAnalyzerRegion, GetType(SpectrumAnalyzerControl))
        Me.mRegionManager.RegisterViewWithRegion(RegionNames.NanoPlayerControlsRegion, GetType(NanoPlayerControls))
    End Sub
#End Region
End Class
