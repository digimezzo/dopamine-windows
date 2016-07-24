Imports Microsoft.Practices.Prism.Mvvm
Imports Dopamine.Core.Settings
Imports Microsoft.Practices.Prism
Imports Microsoft.Practices.Prism.Regions
Imports Dopamine.Common.Services.Indexing
Imports Microsoft.Practices.Prism.Commands
Imports Dopamine.Common.Services.Collection

Namespace ViewModels
    Public Class SettingsCollectionViewModel
        Inherits BindableBase
        Implements IActiveAware
        Implements INavigationAware

#Region "Variables"
        Private mIsActive As Boolean
        Private mCheckIgnoreRemovedFilesChecked As Boolean
        Private mIndexingService As IIndexingService
        Private mcollectionservice As ICollectionService
#End Region

#Region "Commands"
        Public Property RefreshNowCommand As DelegateCommand
#End Region

#Region "Properties"
        Public Property IsActive() As Boolean Implements IActiveAware.IsActive
            Get
                Return mIsActive
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mIsActive, value)
            End Set
        End Property

        Public Property CheckIgnoreRemovedFilesChecked() As Boolean
            Get
                Return mCheckIgnoreRemovedFilesChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Indexing", "IgnoreRemovedFiles", value)
                SetProperty(Of Boolean)(Me.mCheckIgnoreRemovedFilesChecked, value)
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New(iIndexingService As IIndexingService, iCollectionService As ICollectionService)
            mIndexingService = iIndexingService
            mcollectionservice = iCollectionService

            Me.RefreshNowCommand = New DelegateCommand(AddressOf Me.RefreshNow)

            Me.GetCheckBoxesAsync()
        End Sub
#End Region

#Region "Events"
        Public Event IsActiveChanged(sender As Object, e As EventArgs) Implements IActiveAware.IsActiveChanged
#End Region

#Region "Private"
        Private Async Sub GetCheckBoxesAsync()

            Await Task.Run(Sub()
                               Me.CheckIgnoreRemovedFilesChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Indexing", "IgnoreRemovedFiles")
                           End Sub)
        End Sub

        Private Sub RefreshNow()
            mIndexingService.NeedsIndexing = True
            mIndexingService.IndexCollectionAsync(XmlSettingsClient.Instance.Get(Of Boolean)("Indexing", "IgnoreRemovedFiles"), False)
        End Sub
#End Region

#Region "INavigationAware"
        Public Function IsNavigationTarget(navigationContext As NavigationContext) As Boolean Implements INavigationAware.IsNavigationTarget
            Return True
        End Function

        Public Sub OnNavigatedFrom(navigationContext As NavigationContext) Implements INavigationAware.OnNavigatedFrom
            mIndexingService.IndexCollectionAsync(XmlSettingsClient.Instance.Get(Of Boolean)("Indexing", "IgnoreRemovedFiles"), False)
            mcollectionservice.SaveMarkedFoldersAsync()
        End Sub

        Public Sub OnNavigatedTo(navigationContext As NavigationContext) Implements INavigationAware.OnNavigatedTo

        End Sub
#End Region
    End Class
End Namespace
