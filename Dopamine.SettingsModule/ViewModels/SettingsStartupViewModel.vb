Imports Dopamine.Common.Services.Update
Imports Dopamine.Core
Imports Dopamine.Core.Settings
Imports Microsoft.Practices.Prism.Mvvm

Namespace ViewModels
    Public Class SettingsStartupViewModel
        Inherits BindableBase

#Region "Variables"
        Private mCheckBoxCheckForUpdatesChecked As Boolean
        Private mCheckBoxAlsoCheckForPreReleasesChecked As Boolean
        Private mCheckBoxInstallUpdatesAutomaticallyChecked As Boolean
        Private mCheckBoxStartupPageChecked As Boolean
        Private mUpdateService As IUpdateService
        Private mIsportable As Boolean
#End Region

#Region "Properties"
        Public Property CheckBoxCheckForUpdatesChecked() As Boolean
            Get
                Return mCheckBoxCheckForUpdatesChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Updates", "CheckForUpdates", value)
                SetProperty(Of Boolean)(mCheckBoxCheckForUpdatesChecked, value)

                If value Then
                    mUpdateService.EnableUpdateCheck()
                Else
                    mUpdateService.DisableUpdateCheck()
                End If
            End Set
        End Property

        Public Property CheckBoxAlsoCheckForPreReleasesChecked() As Boolean
            Get
                Return mCheckBoxAlsoCheckForPreReleasesChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Updates", "AlsoCheckForPreReleases", value)
                SetProperty(Of Boolean)(mCheckBoxAlsoCheckForPreReleasesChecked, value)

                If Me.CheckBoxCheckForUpdatesChecked Then
                    mUpdateService.EnableUpdateCheck()
                Else
                    mUpdateService.DisableUpdateCheck()
                End If
            End Set
        End Property

        Public Property CheckBoxInstallUpdatesAutomaticallyChecked() As Boolean
            Get
                Return mCheckBoxInstallUpdatesAutomaticallyChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Updates", "AutomaticDownload", value)
                SetProperty(Of Boolean)(mCheckBoxInstallUpdatesAutomaticallyChecked, value)

                If Me.CheckBoxCheckForUpdatesChecked Then
                    mUpdateService.EnableUpdateCheck()
                Else
                    mUpdateService.DisableUpdateCheck()
                End If
            End Set
        End Property

        Public Property CheckBoxStartupPageChecked() As Boolean
            Get
                Return mCheckBoxStartupPageChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Startup", "ShowLastSelectedPage", value)
                SetProperty(Of Boolean)(mCheckBoxStartupPageChecked, value)
            End Set
        End Property

        Public Property IsPortable() As Boolean
            Get
                Return mIsportable
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsportable, value)
            End Set
        End Property

#End Region

#Region "Construction"
        Public Sub New(iUpdateService As IUpdateService)

            Me.mUpdateService = iUpdateService

            Me.IsPortable = XmlSettingsClient.Instance.BaseGet(Of Boolean)("Application", "IsPortable")

            ' CheckBoxes
            Me.GetCheckBoxesAsync()

            ' No automatic updates in the portable version
            If Not Me.IsPortable Then
                Me.CheckBoxInstallUpdatesAutomaticallyChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Updates", "AutomaticDownload")
            Else
                Me.CheckBoxInstallUpdatesAutomaticallyChecked = False
            End If
        End Sub
#End Region

#Region "Private"
        Private Async Sub GetCheckBoxesAsync()

            Await Task.Run(Sub()
                               Me.CheckBoxCheckForUpdatesChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Updates", "CheckForUpdates")
                               Me.CheckBoxAlsoCheckForPreReleasesChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Updates", "AlsoCheckForPreReleases")
                               Me.CheckBoxStartupPageChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Startup", "ShowLastSelectedPage")
                           End Sub)
        End Sub
#End Region
    End Class
End Namespace