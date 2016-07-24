Imports Dopamine.Common.Presentation.ViewModels
Imports Dopamine.Common.Services.Metadata
Imports Dopamine.Core.Base
Imports Dopamine.Core.Prism
Imports Dopamine.Core.Settings
Imports Microsoft.Practices.Prism.Commands
Imports Dopamine.Core.Database

Namespace ViewModels
    Public Class CollectionAlbumsViewModel
        Inherits CommonAlbumsViewModel

#Region "Variables"
        Private mLeftPaneWidthPercent As Double
#End Region

#Region "Properties"
        Public Property LeftPaneWidthPercent() As Double
            Get
                Return mLeftPaneWidthPercent
            End Get
            Set(ByVal value As Double)
                SetProperty(Of Double)(mLeftPaneWidthPercent, value)
                XmlSettingsClient.Instance.Set(Of Integer)("ColumnWidths", "AlbumsLeftPaneWidthPercent", CInt(value))
            End Set
        End Property

        Public Overrides ReadOnly Property CanOrderByAlbum As Boolean
            Get
                Return Me.SelectedAlbums IsNot Nothing AndAlso Me.SelectedAlbums.Count > 0
            End Get
        End Property
#End Region

#Region "Construction"
        Public Sub New()

            MyBase.New()

            ' IndexingService
            AddHandler Me.indexingService.RefreshArtwork, Async Sub() Await Me.collectionService.RefreshArtworkAsync(Me.Albums, Me.Tracks)

            '  Commands
            Me.ToggleTrackOrderCommand = New DelegateCommand(Async Sub() Await Me.ToggleTrackOrderAsync)
            Me.ToggleAlbumOrderCommand = New DelegateCommand(Async Sub() Await Me.ToggleAlbumOrderAsync)
            Me.RemoveSelectedTracksCommand = New DelegateCommand(Async Sub() Await Me.RemoveTracksFromCollectionAsync(Me.SelectedTracks), Function() Not Me.IsIndexing)

            ' Events
            Me.eventAggregator.GetEvent(Of SettingEnableRatingChanged)().Subscribe(Async Sub(iEnableRating)
                                                                                       Me.EnableRating = iEnableRating
                                                                                       Me.SetTrackOrder("AlbumsTrackOrder")
                                                                                       Await Me.GetTracksAsync(Nothing, Nothing, Me.SelectedAlbums, Me.TrackOrder)
                                                                                   End Sub)

            Me.eventAggregator.GetEvent(Of SettingUseStarRatingChanged)().Subscribe(Sub(iUseStarRating) Me.UseStarRating = iUseStarRating)

            ' MetadataService
            AddHandler Me.metadataService.MetadataChanged, AddressOf MetadataChangedHandlerAsync

            ' Set the initial AlbumOrder
            Me.AlbumOrder = CType(XmlSettingsClient.Instance.Get(Of Integer)("Ordering", "AlbumsAlbumOrder"), AlbumOrder)

            ' Set the initial TrackOrder
            Me.SetTrackOrder("AlbumsTrackOrder")

            ' Subscribe to Events and Commands on creation
            Me.Subscribe()

            ' Set width of the panels
            Me.LeftPaneWidthPercent = XmlSettingsClient.Instance.Get(Of Integer)("ColumnWidths", "AlbumsLeftPaneWidthPercent")

            ' Cover size
            Me.SetCoversizeAsync(CType(XmlSettingsClient.Instance.Get(Of Integer)("CoverSizes", "AlbumsCoverSize"), CoverSizeType))
        End Sub
#End Region

#Region "Private"
        Private Async Sub MetadataChangedHandlerAsync(e As MetadataChangedEventArgs)

            If e.IsAlbumArtworkMetadataChanged Then
                Await Me.collectionService.RefreshArtworkAsync(Me.Albums, Me.Tracks)
            End If

            If e.IsAlbumTitleMetadataChanged Or e.IsAlbumArtistMetadataChanged Or e.IsAlbumYearMetadataChanged Then
                Await Me.GetAlbumsAsync(Nothing, Nothing, Me.AlbumOrder)
            End If

            If e.IsAlbumTitleMetadataChanged Or e.IsAlbumArtistMetadataChanged Or e.IsTrackMetadataChanged Then
                Await Me.GetTracksAsync(Nothing, Nothing, Me.SelectedAlbums, Me.TrackOrder)
            End If
        End Sub
#End Region

#Region "Protected"
        Protected Async Function ToggleTrackOrderAsync() As Task

            MyBase.ToggleTrackOrder()

            XmlSettingsClient.Instance.Set(Of Integer)("Ordering", "AlbumsTrackOrder", Me.TrackOrder)
            Await Me.GetTracksCommonAsync(Me.Tracks.Select(Function(t) t.TrackInfo).ToList, Me.TrackOrder)
        End Function

        Protected Async Function ToggleAlbumOrderAsync() As Task

            MyBase.ToggleAlbumOrder()

            XmlSettingsClient.Instance.Set(Of Integer)("Ordering", "AlbumsAlbumOrder", Me.AlbumOrder)
            Await Me.GetAlbumsCommonAsync(Me.Albums.Select(Function(a) a.Album).ToList, Me.AlbumOrder)
        End Function
#End Region

#Region "Overrides"
        Protected Overrides Async Function SetCoversizeAsync(iCoverSize As CoverSizeType) As Task

            Await MyBase.SetCoversizeAsync(iCoverSize)
            XmlSettingsClient.Instance.Set(Of Integer)("CoverSizes", "AlbumsCoverSize", iCoverSize)
        End Function

        Protected Overrides Async Function FillListsAsync() As Task

            Await Me.GetAlbumsAsync(Nothing, Nothing, Me.AlbumOrder)
            Await Me.GetTracksAsync(Nothing, Nothing, Nothing, Me.TrackOrder)
        End Function

        Protected Overrides Async Function SelectedAlbumsHandlerAsync(iParameter As Object) As Task

            Await MyBase.SelectedAlbumsHandlerAsync(iParameter)

            ' Don't reload the lists when updating Metadata. MetadataChangedHandlerAsync handles that.
            If Me.metadataService.IsUpdatingDatabaseMetadata Then Return

            Me.SetTrackOrder("AlbumsTrackOrder")
            Await Me.GetTracksAsync(Nothing, Nothing, Me.SelectedAlbums, Me.TrackOrder)
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

            Me.UpdateAlbumOrderText(Me.AlbumOrder)
            Me.UpdateTrackOrderText(Me.TrackOrder)
        End Sub
#End Region
    End Class
End Namespace

