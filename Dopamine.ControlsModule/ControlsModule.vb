Imports Microsoft.Practices.Prism.Modularity
Imports Microsoft.Practices.Prism.Regions
Imports Microsoft.Practices.Unity
Imports Dopamine.ControlsModule.Views
Imports Dopamine.ControlsModule.ViewModels

Public Class ControlsModule
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
        Me.mContainer.RegisterType(Of Object, CollectionPlaybackControls)(GetType(CollectionPlaybackControls).FullName)
        Me.mContainer.RegisterType(Of Object, CollectionPlaybackControlsViewModel)(GetType(CollectionPlaybackControlsViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, NowPlayingPlaybackControls)(GetType(NowPlayingPlaybackControls).FullName)
        Me.mContainer.RegisterType(Of Object, NowPlayingPlaybackControlsViewModel)(GetType(NowPlayingPlaybackControlsViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, SearchControl)(GetType(SearchControl).FullName)
        Me.mContainer.RegisterType(Of Object, SearchControlViewModel)(GetType(SearchControlViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, SpectrumAnalyzerControl)(GetType(SpectrumAnalyzerControl).FullName)
        Me.mContainer.RegisterType(Of Object, SpectrumAnalyzerControlViewModel)(GetType(SpectrumAnalyzerControlViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, CoverPlayerControls)(GetType(CoverPlayerControls).FullName)
        Me.mContainer.RegisterType(Of Object, MicroPlayerControls)(GetType(MicroPlayerControls).FullName)
        Me.mContainer.RegisterType(Of Object, NanoPlayerControls)(GetType(NanoPlayerControls).FullName)
        Me.mContainer.RegisterType(Of Object, NothingPlayingControl)(GetType(NothingPlayingControl).FullName)
        Me.mContainer.RegisterType(Of Object, NothingPlayingControlViewModel)(GetType(NothingPlayingControlViewModel).FullName)
    End Sub
#End Region
End Class
