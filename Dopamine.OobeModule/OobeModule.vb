Imports Microsoft.Practices.Prism.Modularity
Imports Microsoft.Practices.Prism.Regions
Imports Microsoft.Practices.Unity
Imports Dopamine.OobeModule.ViewModels
Imports Dopamine.OobeModule.Views

Public Class OobeModule
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
        Me.mContainer.RegisterType(Of Object, OobeAppName)(GetType(OobeAppName).FullName)
        Me.mContainer.RegisterType(Of Object, OobeAppNameViewModel)(GetType(OobeAppNameViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, OobeControls)(GetType(OobeControls).FullName)
        Me.mContainer.RegisterType(Of Object, OobeControlsViewModel)(GetType(OobeControlsViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, OobeWelcome)(GetType(OobeWelcome).FullName)
        Me.mContainer.RegisterType(Of Object, OobeWelcomeViewModel)(GetType(OobeWelcomeViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, OobeLanguage)(GetType(OobeLanguage).FullName)
        Me.mContainer.RegisterType(Of Object, OobeLanguageViewModel)(GetType(OobeLanguageViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, OobeAppearance)(GetType(OobeAppearance).FullName)
        Me.mContainer.RegisterType(Of Object, OobeAppearanceViewModel)(GetType(OobeAppearanceViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, OobeCollection)(GetType(OobeCollection).FullName)
        Me.mContainer.RegisterType(Of Object, OobeCollectionViewModel)(GetType(OobeCollectionViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, OobeDonate)(GetType(OobeDonate).FullName)
        Me.mContainer.RegisterType(Of Object, OobeDonateViewModel)(GetType(OobeDonateViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, OobeFinish)(GetType(OobeFinish).FullName)
        Me.mContainer.RegisterType(Of Object, OobeFinishViewModel)(GetType(OobeFinishViewModel).FullName)
    End Sub
#End Region
End Class
