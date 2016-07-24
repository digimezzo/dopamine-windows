Imports Dopamine.CollectionModule.Views
Imports Dopamine.Common.Presentation.ViewModels
Imports Dopamine.Common.Services.Metadata
Imports Dopamine.Core.Database
Imports Dopamine.Core.Prism
Imports Dopamine.Core.Utils
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Unity
Imports Dopamine.Core.Base

Namespace ViewModels
    Public Class CollectionTracksViewModel
        Inherits CommonTracksViewModel

#Region "Variables"

        ' Flags
        Private mRatingVisible As Boolean
        Private mArtistVisible As Boolean
        Private mAlbumVisible As Boolean
        Private mGenreVisible As Boolean
        Private mLengthVisible As Boolean
        Private mAlbumArtistVisible As Boolean
        Private mTrackNumberVisible As Boolean
        Private mYearVisible As Boolean
#End Region

#Region "Properties"
        Public Property RatingVisible() As Boolean
            Get
                Return mRatingVisible
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mRatingVisible, value)
            End Set
        End Property

        Public Property ArtistVisible() As Boolean
            Get
                Return mArtistVisible
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mArtistVisible, value)
            End Set
        End Property

        Public Property AlbumVisible() As Boolean
            Get
                Return mAlbumVisible
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mAlbumVisible, value)
            End Set
        End Property

        Public Property GenreVisible() As Boolean
            Get
                Return mGenreVisible
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mGenreVisible, value)
            End Set
        End Property

        Public Property LengthVisible() As Boolean
            Get
                Return mLengthVisible
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mLengthVisible, value)
            End Set
        End Property

        Public Property AlbumArtistVisible() As Boolean
            Get
                Return mAlbumArtistVisible
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mAlbumArtistVisible, value)
            End Set
        End Property

        Public Property TrackNumberVisible() As Boolean
            Get
                Return mTrackNumberVisible
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mTrackNumberVisible, value)
            End Set
        End Property

        Public Property YearVisible() As Boolean
            Get
                Return mYearVisible
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mYearVisible, value)
            End Set
        End Property

        Public Overrides ReadOnly Property CanOrderByAlbum As Boolean
            Get
                Return False ' Doesn't need to return a useful value in this class
            End Get
        End Property
#End Region

#Region "Commands"
        Public Property ChooseColumnsCommand As DelegateCommand
#End Region

#Region "Construction"
        Public Sub New()

            MyBase.New()

            ' Commands
            Me.ChooseColumnsCommand = New DelegateCommand(AddressOf Me.ChooseColumns)
            Me.AddTracksToPlaylistCommand = New DelegateCommand(Of String)(Async Sub(iPlaylistName) Await Me.AddTracksToPlaylistAsync(Me.SelectedTracks, iPlaylistName))
            Me.RemoveSelectedTracksCommand = New DelegateCommand(Async Sub() Await Me.RemoveTracksFromCollectionAsync(Me.SelectedTracks), Function() Not Me.IsIndexing)

            ' Events
            Me.eventAggregator.GetEvent(Of SettingEnableRatingChanged)().Subscribe(Sub(iEnableRating)
                                                                                       Me.EnableRating = iEnableRating
                                                                                       Me.GetVisibleColumns()
                                                                                   End Sub)
            Me.eventAggregator.GetEvent(Of SettingUseStarRatingChanged)().Subscribe(Sub(iUseStarRating) Me.UseStarRating = iUseStarRating)

            ' MetadataService
            AddHandler Me.metadataService.MetadataChanged, AddressOf MetadataChangedHandlerAsync

            ' Show only the columns which are visible
            Me.GetVisibleColumns()

            ' Subscribe to Events and Commands on creation
            Me.Subscribe()
        End Sub
#End Region

#Region "Private"
        Private Async Sub MetadataChangedHandlerAsync(e As MetadataChangedEventArgs)

            If e.IsTrackMetadataChanged Then
                Await Me.GetTracksAsync(Nothing, Nothing, Nothing, TrackOrder.ByAlbum)
            End If
        End Sub

        Private Sub ChooseColumns()

            Dim view As CollectionTracksColumns = Me.container.Resolve(Of CollectionTracksColumns)()
            view.DataContext = Me.container.Resolve(Of CollectionTracksColumnsViewModel)()

            Me.dialogService.ShowCustomDialog(&HE73E,
                                               16,
                                               ResourceUtils.GetStringResource("Language_Columns"),
                                               view,
                                               400,
                                               0,
                                               False,
                                               True,
                                               ResourceUtils.GetStringResource("Language_Ok"),
                                               ResourceUtils.GetStringResource("Language_Cancel"),
                                               AddressOf CType(view.DataContext, CollectionTracksColumnsViewModel).SetVisibleColumns)

            ' When the dialog is closed, update the columns
            Me.GetVisibleColumns()
        End Sub

        Private Sub GetVisibleColumns()

            Dim savedRatingVisible As Boolean = False

            Utils.GetVisibleSongsColumns(savedRatingVisible,
                                         Me.ArtistVisible,
                                         Me.AlbumVisible,
                                         Me.GenreVisible,
                                         Me.LengthVisible,
                                         Me.AlbumArtistVisible,
                                         Me.TrackNumberVisible,
                                         Me.YearVisible)

            If Me.EnableRating AndAlso savedRatingVisible Then
                Me.RatingVisible = True
            Else
                Me.RatingVisible = False
            End If
        End Sub
#End Region

#Region "Overrides"
        Protected Overrides Async Function FillListsAsync() As Task

            Await Me.GetTracksAsync(Nothing, Nothing, Nothing, TrackOrder.ByAlbum)
        End Function

        Protected Overrides Sub Unsubscribe()

            ' Commands
            ApplicationCommands.RemoveSelectedTracksCommand.UnregisterCommand(Me.RemoveSelectedTracksCommand)
        End Sub

        Protected Overrides Sub Subscribe()

            ' Prevents subscribing twice
            Me.Unsubscribe()

            ' Commands
            ApplicationCommands.RemoveSelectedTracksCommand.RegisterCommand(Me.RemoveSelectedTracksCommand)
        End Sub

        Protected Overrides Sub RefreshLanguage()
            ' Do Nothing
        End Sub
#End Region
    End Class
End Namespace
