Imports Microsoft.Practices.Prism.Mvvm
Imports Microsoft.Practices.Prism.Regions
Imports Microsoft.Practices.Prism.Commands
Imports Dopamine.Core.Prism
Imports Microsoft.Practices.Prism
Imports Dopamine.Common.Services.Indexing
Imports Dopamine.Core.Settings
Imports Dopamine.Common.Services.Collection

Namespace ViewModels
    Public Class SettingsViewModel
        Inherits BindableBase
        Implements IActiveAware
        Implements INavigationAware


#Region "Variables"
        Private ReadOnly mRegionManager As IRegionManager
        Private mIndexingService As IIndexingService
        Private mcollectionservice As ICollectionService
        Private mPreviousIndex As Integer = 0
        Private mSlideInFrom As Integer
#End Region

#Region "Properties"
        Public NavigateBetweenSettingsCommand As DelegateCommand(Of String)

        Public Property SlideInFrom() As Integer
            Get
                Return mSlideInFrom
            End Get
            Set(ByVal value As Integer)
                SetProperty(Of Integer)(mSlideInFrom, value)
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New(iRegionManager As IRegionManager, iIndexingService As IIndexingService, iCollectionService As ICollectionService)
            mRegionManager = iRegionManager
            mIndexingService = iIndexingService
            mcollectionservice = iCollectionService
            Me.NavigateBetweenSettingsCommand = New DelegateCommand(Of String)(AddressOf NavigateBetweenSettings)
            ApplicationCommands.NavigateBetweenSettingsCommand.RegisterCommand(Me.NavigateBetweenSettingsCommand)
            Me.SlideInFrom = 30
        End Sub
#End Region

#Region "Events"
        Public Event IsActiveChanged(sender As Object, e As EventArgs) Implements IActiveAware.IsActiveChanged
#End Region

#Region "Private"
        Private Sub NavigateBetweenSettings(iIndex As String)

            If String.IsNullOrWhiteSpace(iIndex) Then Return

            Dim index As Integer = 0

            Integer.TryParse(iIndex, index)

            If index = 0 Then Return

            Me.SlideInFrom = If(index <= mPreviousIndex, -30, 30)

            mPreviousIndex = index

            mRegionManager.RequestNavigate(RegionNames.SettingsRegion, Me.GetPageForIndex(index))
        End Sub

        Private Function GetPageForIndex(iIndex As Integer) As String

            Dim page As String = String.Empty

            Select Case iIndex
                Case 1
                    page = GetType(Views.SettingsCollection).FullName
                Case 2
                    page = GetType(Views.SettingsAppearance).FullName
                Case 3
                    page = GetType(Views.SettingsBehaviour).FullName
                Case 4
                    page = GetType(Views.SettingsPlayback).FullName
                Case 5
                    page = GetType(Views.SettingsStartup).FullName
                Case Else
                    page = GetType(Views.SettingsCollection).FullName
            End Select

            Return page
        End Function
#End Region

#Region "Public"
        Public Function IsNavigationTarget(navigationContext As NavigationContext) As Boolean Implements INavigationAware.IsNavigationTarget
            Return True
        End Function

        Public Sub OnNavigatedFrom(navigationContext As NavigationContext) Implements INavigationAware.OnNavigatedFrom
            Me.mIndexingService.IndexCollectionAsync(XmlSettingsClient.Instance.Get(Of Boolean)("Indexing", "IgnoreRemovedFiles"), False)
            mcollectionservice.SaveMarkedFoldersAsync()
        End Sub

        Public Sub OnNavigatedTo(navigationContext As NavigationContext) Implements INavigationAware.OnNavigatedTo

        End Sub

        Public Property IsActive As Boolean Implements IActiveAware.IsActive
#End Region

    End Class
End Namespace
