Imports Microsoft.Practices.Prism.Modularity
Imports Microsoft.Practices.Prism.Regions
Imports Microsoft.Practices.Unity
Imports Dopamine.SettingsModule.ViewModels
Imports Dopamine.SettingsModule.Views
Imports Dopamine.Core.Prism

Public Class SettingsModule
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
        Me.mContainer.RegisterType(Of Object, SettingsViewModel)(GetType(SettingsViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, Settings)(GetType(Settings).FullName)
        Me.mContainer.RegisterType(Of Object, SettingsSubMenu)(GetType(SettingsSubMenu).FullName)

        Me.mContainer.RegisterType(Of Object, SettingsCollectionFoldersViewModel)(GetType(SettingsCollectionFoldersViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, SettingsCollectionFolders)(GetType(SettingsCollectionFolders).FullName)
        Me.mContainer.RegisterType(Of Object, SettingsCollectionViewModel)(GetType(SettingsCollectionViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, SettingsCollection)(GetType(SettingsCollection).FullName)
        Me.mContainer.RegisterType(Of Object, SettingsAppearanceThemeViewModel)(GetType(SettingsAppearanceThemeViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, SettingsAppearanceTheme)(GetType(SettingsAppearanceTheme).FullName)
        Me.mContainer.RegisterType(Of Object, SettingsAppearanceLanguageViewModel)(GetType(SettingsAppearanceLanguageViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, SettingsAppearanceLanguage)(GetType(SettingsAppearanceLanguage).FullName)
        Me.mContainer.RegisterType(Of Object, SettingsAppearanceViewModel)(GetType(SettingsAppearanceViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, SettingsAppearance)(GetType(SettingsAppearance).FullName)
        Me.mContainer.RegisterType(Of Object, SettingsBehaviourViewModel)(GetType(SettingsBehaviourViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, SettingsBehaviour)(GetType(SettingsBehaviour).FullName)
        Me.mContainer.RegisterType(Of Object, FolderViewModel)(GetType(FolderViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, SettingsPlaybackViewModel)(GetType(SettingsPlaybackViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, SettingsPlayback)(GetType(SettingsPlayback).FullName)
        Me.mContainer.RegisterType(Of Object, SettingsStartupViewModel)(GetType(SettingsStartupViewModel).FullName)
        Me.mContainer.RegisterType(Of Object, SettingsStartup)(GetType(SettingsStartup).FullName)

        Me.mRegionManager.RegisterViewWithRegion(RegionNames.SettingsRegion, GetType(Views.SettingsCollection))
    End Sub
#End Region
End Class
