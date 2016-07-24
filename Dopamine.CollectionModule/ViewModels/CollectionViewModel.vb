Imports Dopamine.Core.Prism
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Prism.Mvvm
Imports Microsoft.Practices.Prism.Regions

Namespace ViewModels
    Public Class CollectionViewModel
        Inherits BindableBase

#Region "Variables"
        Private ReadOnly mRegionManager As IRegionManager
        Private mSlideInFrom As Integer
        Private mPreviousIndex As Integer = 0
#End Region

#Region "Properties"
        Public NavigateBetweenCollectionCommand As DelegateCommand(Of String)

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
        Public Sub New(iRegionManager As IRegionManager)
            Me.mRegionManager = iRegionManager
            Me.NavigateBetweenCollectionCommand = New DelegateCommand(Of String)(AddressOf NavigateBetweenCollection)
            ApplicationCommands.NavigateBetweenCollectionCommand.RegisterCommand(Me.NavigateBetweenCollectionCommand)
            Me.SlideInFrom = 30
        End Sub
#End Region

#Region "Functions"
        Private Sub NavigateBetweenCollection(iIndex As String)

            If String.IsNullOrWhiteSpace(iIndex) Then Return

            Dim index As Integer = 0

            Integer.TryParse(iIndex, index)

            If index = 0 Then Return

            Me.SlideInFrom = If(index <= mPreviousIndex, -30, 30)

            mPreviousIndex = index

            mRegionManager.RequestNavigate(RegionNames.CollectionContentRegion, Me.GetPageForIndex(index))
        End Sub

        Private Function GetPageForIndex(iIndex As Integer) As String

            Dim page As String = String.Empty

            Select Case iIndex
                Case 1
                    page = GetType(Views.CollectionArtists).FullName
                Case 2
                    page = GetType(Views.CollectionGenres).FullName
                Case 3
                    page = GetType(Views.CollectionAlbums).FullName
                Case 4
                    page = GetType(Views.CollectionTracks).FullName
                Case 5
                    page = GetType(Views.CollectionPlaylists).FullName
                Case 6
                    page = GetType(Views.CollectionCloud).FullName
                Case Else
                    page = GetType(Views.CollectionArtists).FullName
            End Select

            Return page
        End Function
#End Region
    End Class
End Namespace
