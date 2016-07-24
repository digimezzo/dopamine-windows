Imports Microsoft.Practices.Prism.Modularity
Imports Microsoft.Practices.Prism.Regions
Imports Microsoft.Practices.Unity
Imports Dopamine.InformationModule.ViewModels
Imports Dopamine.InformationModule.Views
Imports Dopamine.Core.Prism

Public Class InformationModule
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
        Me.mContainer.RegisterType(Of Object, InformationViewModel)(GetType(InformationViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, Information)(GetType(Information).FullName)
        Me.mContainer.RegisterType(Of Object, InformationAboutViewModel)(GetType(InformationAboutViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, InformationAbout)(GetType(InformationAbout).FullName)
        Me.mContainer.RegisterType(Of Object, InformationHelpViewModel)(GetType(InformationHelpViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, InformationHelp)(GetType(InformationHelp).FullName)
        Me.mContainer.RegisterType(Of Object, InformationSubMenu)(GetType(InformationSubMenu).FullName)
        Me.mContainer.RegisterType(Of Object, InformationAboutLicense)(GetType(InformationAboutLicense).FullName)

        Me.mRegionManager.RegisterViewWithRegion(RegionNames.InformationRegion, GetType(Views.InformationHelp))
    End Sub
#End Region
End Class
