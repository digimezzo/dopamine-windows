Imports System.Collections.ObjectModel
Imports Dopamine.Common.Presentation.Interfaces
Imports Dopamine.Common.Presentation.Utils
Imports Dopamine.Common.Presentation.ViewModels
Imports Dopamine.Common.Services.Metadata
Imports Dopamine.Common.Services.Playback
Imports Dopamine.Core.Base
Imports Dopamine.Core.Database.Entities
Imports Dopamine.Core.Database
Imports Dopamine.Core.Database.Repositories.Interfaces
Imports Dopamine.Core.Logging
Imports Dopamine.Core.Prism
Imports Dopamine.Core.Settings
Imports Dopamine.Core.Utils
Imports Dopamine.Core.Helpers
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Prism.PubSubEvents

Namespace ViewModels
    Public Class CollectionArtistsViewModel
        Inherits CommonAlbumsViewModel
        Implements ISemanticZoomViewModel

#Region "Variables"
        ' Repositories
        Private mArtistRepository As IArtistRepository

        ' Lists
        Private mArtists As ObservableCollection(Of ISemanticZoomable)
        Private mArtistsCvs As CollectionViewSource
        Private mSelectedArtists As IList(Of Artist)
        Private mArtistsZoomSelectors As ObservableCollection(Of ISemanticZoomSelector)

        ' Flags
        Private mIsArtistsZoomVisible As Boolean

        ' Other
        Private mArtistsCount As Long
        Private mShellMouseUpToken As SubscriptionToken
        Private mArtistType As ArtistType
        Private mArtistTypeText As String
        Private mLeftPaneWidthPercent As Double
        Private mRightPaneWidthPercent As Double
#End Region

#Region "Commands"
        Public Property AddArtistsToPlaylistCommand As DelegateCommand(Of String)
        Public Property SelectedArtistsCommand As DelegateCommand(Of Object)
        Public Property ShowArtistsZoomCommand As DelegateCommand
        Public Property SemanticJumpCommand As DelegateCommand Implements ISemanticZoomViewModel.SemanticJumpCommand
        Public Property ToggleArtistTypeCommand As DelegateCommand
        Public Property AddArtistsToNowPlayingCommand As DelegateCommand
#End Region

#Region "Properties"
        Public Property LeftPaneWidthPercent() As Double
            Get
                Return mLeftPaneWidthPercent
            End Get
            Set(ByVal value As Double)
                SetProperty(Of Double)(mLeftPaneWidthPercent, value)
                XmlSettingsClient.Instance.Set(Of Integer)("ColumnWidths", "ArtistsLeftPaneWidthPercent", CInt(value))
            End Set
        End Property

        Public Property RightPaneWidthPercent() As Double
            Get
                Return mRightPaneWidthPercent
            End Get
            Set(ByVal value As Double)
                SetProperty(Of Double)(mRightPaneWidthPercent, value)
                XmlSettingsClient.Instance.Set(Of Integer)("ColumnWidths", "ArtistsRightPaneWidthPercent", CInt(value))
            End Set
        End Property

        Public Property Artists() As ObservableCollection(Of ISemanticZoomable) Implements ISemanticZoomViewModel.SemanticZoomables
            Get
                Return mArtists
            End Get
            Set(ByVal value As ObservableCollection(Of ISemanticZoomable))
                SetProperty(Of ObservableCollection(Of ISemanticZoomable))(mArtists, value)
            End Set
        End Property

        Public Property ArtistsCvs() As CollectionViewSource
            Get
                Return mArtistsCvs
            End Get
            Set(ByVal value As CollectionViewSource)
                SetProperty(Of CollectionViewSource)(mArtistsCvs, value)
            End Set
        End Property

        Public Property SelectedArtists() As IList(Of Artist)
            Get
                Return mSelectedArtists
            End Get
            Set(ByVal value As IList(Of Artist))
                SetProperty(Of IList(Of Artist))(mSelectedArtists, value)
            End Set
        End Property

        Public Property ArtistsCount() As Long
            Get
                Return mArtistsCount
            End Get
            Set(ByVal value As Long)
                SetProperty(Of Long)(mArtistsCount, value)
            End Set
        End Property

        Public Property IsArtistsZoomVisible() As Boolean
            Get
                Return mIsArtistsZoomVisible
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsArtistsZoomVisible, value)
            End Set
        End Property

        Public Property ArtistsZoomSelectors() As ObservableCollection(Of ISemanticZoomSelector) Implements ISemanticZoomViewModel.SemanticZoomSelectors
            Get
                Return mArtistsZoomSelectors
            End Get
            Set(ByVal value As ObservableCollection(Of ISemanticZoomSelector))
                SetProperty(Of ObservableCollection(Of ISemanticZoomSelector))(mArtistsZoomSelectors, value)
            End Set
        End Property

        Public Property ArtistType() As ArtistType
            Get
                Return mArtistType
            End Get
            Set(ByVal value As ArtistType)
                SetProperty(Of ArtistType)(mArtistType, value)
                Me.UpdateArtistType(value)
            End Set
        End Property

        Public ReadOnly Property ArtistTypeText() As String
            Get
                Return mArtistTypeText
            End Get
        End Property

        Public Overrides ReadOnly Property CanOrderByAlbum As Boolean
            Get
                Return (Me.SelectedArtists IsNot Nothing AndAlso
                       Me.SelectedArtists.Count > 0) Or
                       (Me.SelectedAlbums IsNot Nothing AndAlso
                       Me.SelectedAlbums.Count > 0)
            End Get
        End Property
#End Region

#Region "Construction"
        Public Sub New(iArtistRepository As IArtistRepository)

            MyBase.New()

            ' Repositories
            mArtistRepository = iArtistRepository

            ' Commands
            Me.ToggleTrackOrderCommand = New DelegateCommand(Async Sub() Await Me.ToggleTrackOrderAsync)
            Me.ToggleAlbumOrderCommand = New DelegateCommand(Async Sub() Await Me.ToggleAlbumOrderAsync)
            Me.RemoveSelectedTracksCommand = New DelegateCommand(Async Sub() Await Me.RemoveTracksFromCollectionAsync(Me.SelectedTracks), Function() Not Me.IsIndexing)
            Me.AddArtistsToPlaylistCommand = New DelegateCommand(Of String)(Async Sub(iPlaylistName) Await Me.AddArtistsToPlaylistAsync(Me.SelectedArtists, iPlaylistName))
            Me.SelectedArtistsCommand = New DelegateCommand(Of Object)(Async Sub(iParameter) Await Me.SelectedArtistsHandlerAsync(iParameter))
            Me.ShowArtistsZoomCommand = New DelegateCommand(Async Sub() Await Me.ShowSemanticZoomAsync())
            Me.SemanticJumpCommand = New DelegateCommand(Sub() Me.HideSemanticZoom())
            Me.ToggleArtistTypeCommand = New DelegateCommand(Async Sub() Await Me.ToggleArtistTypeAsync)
            Me.AddArtistsToNowPlayingCommand = New DelegateCommand(Async Sub() Await Me.AddArtistsToNowPlayingAsync(Me.SelectedArtists))

            ' Events
            Me.eventAggregator.GetEvent(Of SettingEnableRatingChanged)().Subscribe(Async Sub(iEnableRating)
                                                                                       Me.EnableRating = iEnableRating
                                                                                       Me.SetTrackOrder("ArtistsTrackOrder")
                                                                                       Await Me.GetTracksAsync(Me.SelectedArtists, Nothing, Me.SelectedAlbums, Me.TrackOrder)
                                                                                   End Sub)

            Me.eventAggregator.GetEvent(Of SettingUseStarRatingChanged)().Subscribe(Sub(iUseStarRating) Me.UseStarRating = iUseStarRating)

            ' MetadataService
            AddHandler Me.metadataService.MetadataChanged, AddressOf MetadataChangedHandlerAsync

            ' IndexingService
            AddHandler Me.indexingService.RefreshArtwork, Async Sub() Await Me.collectionService.RefreshArtworkAsync(Me.Albums, Me.Tracks)

            ' Set the initial ArtistType
            Me.ArtistType = CType(XmlSettingsClient.Instance.Get(Of Integer)("Ordering", "ArtistType"), ArtistType)

            ' Set the initial AlbumOrder
            Me.AlbumOrder = CType(XmlSettingsClient.Instance.Get(Of Integer)("Ordering", "ArtistsAlbumOrder"), AlbumOrder)

            ' Set the initial TrackOrder
            Me.SetTrackOrder("ArtistsTrackOrder")

            ' Subscribe to Events and Commands on creation
            Me.Subscribe()

            ' Set width of the panels
            Me.LeftPaneWidthPercent = XmlSettingsClient.Instance.Get(Of Integer)("ColumnWidths", "ArtistsLeftPaneWidthPercent")
            Me.RightPaneWidthPercent = XmlSettingsClient.Instance.Get(Of Integer)("ColumnWidths", "ArtistsRightPaneWidthPercent")

            ' Cover size
            Me.SetCoversizeasync(CType(XmlSettingsClient.Instance.Get(Of Integer)("CoverSizes", "ArtistsCoverSize"), CoverSizeType))
        End Sub
#End Region

#Region "ISemanticZoomViewModel"
        Public Async Function ShowSemanticZoomAsync() As Task Implements ISemanticZoomViewModel.ShowSemanticZoomAsync

            Me.ArtistsZoomSelectors = Await SemanticZoomUtils.UpdateSemanticZoomSelectors(Me.ArtistsCvs.View)

            Me.IsArtistsZoomVisible = True
        End Function

        Public Sub HideSemanticZoom() Implements ISemanticZoomViewModel.HideSemanticZoom
            Me.IsArtistsZoomVisible = False
        End Sub

        Public Sub UpdateSemanticZoomHeaders() Implements ISemanticZoomViewModel.UpdateSemanticZoomHeaders

            Dim previousHeader As String = String.Empty


            For Each avm As ArtistViewModel In Me.ArtistsCvs.View

                If String.IsNullOrEmpty(previousHeader) OrElse Not avm.Header.Equals(previousHeader) Then

                    previousHeader = avm.Header
                    avm.IsHeader = True
                End If
            Next
        End Sub
#End Region

#Region "Private"
        Private Async Sub MetadataChangedHandlerAsync(e As MetadataChangedEventArgs)

            If e.IsAlbumArtworkMetadataChanged Then
                Await Me.collectionService.RefreshArtworkAsync(Me.Albums, Me.Tracks)
            End If

            If e.IsArtistMetadataChanged Or (e.IsAlbumArtistMetadataChanged And (Me.ArtistType = ArtistType.Album Or Me.ArtistType = ArtistType.All)) Then
                Await Me.GetArtistsAsync(Me.ArtistType)
            End If

            If e.IsArtistMetadataChanged Or e.IsAlbumTitleMetadataChanged Or e.IsAlbumArtistMetadataChanged Or e.IsAlbumYearMetadataChanged Then
                Await Me.GetAlbumsAsync(Me.SelectedArtists, Nothing, Me.AlbumOrder)
            End If

            If e.IsArtistMetadataChanged Or e.IsAlbumTitleMetadataChanged Or e.IsAlbumArtistMetadataChanged Or e.IsTrackMetadataChanged Then
                Await Me.GetTracksAsync(Me.SelectedArtists, Nothing, Me.SelectedAlbums, Me.TrackOrder)
            End If
        End Sub

        Private Async Function GetArtistsAsync(iArtistType As ArtistType) As Task

            Try
                ' Get Artists from database
                Dim artists As List(Of Artist) = Await mArtistRepository.GetArtistsAsync(iArtistType)

                ' Create new ObservableCollection
                Dim artistViewModels As New ObservableCollection(Of ArtistViewModel)

                Await Task.Run(Sub()
                                   Dim tempArtistViewModels As New List(Of ArtistViewModel)

                                   ' Workaround to make sure the "#" GroupHeader is shown at the top of the list
                                   tempArtistViewModels.AddRange(artists.Select(Function(art) New ArtistViewModel With {.Artist = art, .IsHeader = False}).Where(Function(avm) avm.Header.Equals("#")))
                                   tempArtistViewModels.AddRange(artists.Select(Function(art) New ArtistViewModel With {.Artist = art, .IsHeader = False}).Where(Function(avm) Not avm.Header.Equals("#")))

                                   For Each avm As ArtistViewModel In tempArtistViewModels
                                       artistViewModels.Add(avm)
                                   Next
                               End Sub)

                ' Unbind to improve UI performance
                If Me.ArtistsCvs IsNot Nothing Then RemoveHandler Me.ArtistsCvs.Filter, New FilterEventHandler(AddressOf ArtistsCvs_Filter)
                Me.Artists = Nothing
                Me.ArtistsCvs = Nothing

                ' Populate ObservableCollection
                Me.Artists = New ObservableCollection(Of ISemanticZoomable)(artistViewModels)
            Catch ex As Exception
                LogClient.Instance.Logger.Error("An error occured while getting Artists. Exception: {0}", ex.Message)

                ' Failed getting Artists. Create empty ObservableCollection.
                Me.Artists = New ObservableCollection(Of ISemanticZoomable)
            End Try

            ' Populate CollectionViewSource
            Me.ArtistsCvs = New CollectionViewSource With {.Source = Me.Artists}
            AddHandler Me.ArtistsCvs.Filter, New FilterEventHandler(AddressOf ArtistsCvs_Filter)

            ' Update count
            Me.ArtistsCount = Me.ArtistsCvs.View.Cast(Of ISemanticZoomable)().Count

            ' Update Semantic Zoom Headers
            Me.UpdateSemanticZoomHeaders()
        End Function

        Private Async Function SelectedArtistsHandlerAsync(iParameter As Object) As Task

            If iParameter IsNot Nothing Then

                Me.SelectedArtists = New List(Of Artist)

                For Each item As ArtistViewModel In CType(iParameter, IList)
                    Me.SelectedArtists.Add(item.Artist)
                Next
            End If

            ' Don't reload the lists when updating Metadata. MetadataChangedHandlerAsync handles that.
            If Me.metadataService.IsUpdatingDatabaseMetadata Then Return

            Await Me.GetAlbumsAsync(Me.SelectedArtists, Nothing, Me.AlbumOrder)
            Me.SetTrackOrder("ArtistsTrackOrder")
            Await Me.GetTracksAsync(Me.SelectedArtists, Nothing, Me.SelectedAlbums, Me.TrackOrder)
        End Function

        Private Async Function AddArtistsToPlaylistAsync(iArtists As IList(Of Artist), iPlaylistName As String) As Task

            Dim result As AddToPlaylistResult = Await Me.collectionService.AddArtistsToPlaylistAsync(iArtists, iPlaylistName)

            If Not result.IsSuccess Then

                Me.dialogService.ShowNotification(&HE711,
                                                16,
                                                ResourceUtils.GetStringResource("Language_Error"),
                                                Replace(ResourceUtils.GetStringResource("Language_Error_Adding_Artists_To_Playlist"), "%playlistname%", """" & iPlaylistName & """"),
                                                ResourceUtils.GetStringResource("Language_Ok"),
                                                True,
                                                ResourceUtils.GetStringResource("Language_Log_File"))
            End If
        End Function

        Protected Async Function AddArtistsToNowPlayingAsync(iArtists As IList(Of Artist)) As Task

            Dim result As AddToQueueResult = Await Me.playbackService.AddToQueue(iArtists)

            If Not result.IsSuccess Then

                Me.dialogService.ShowNotification(&HE711,
                                                16,
                                                ResourceUtils.GetStringResource("Language_Error"),
                                                ResourceUtils.GetStringResource("Language_Error_Adding_Artists_To_Now_Playing"),
                                                ResourceUtils.GetStringResource("Language_Ok"),
                                                True,
                                                ResourceUtils.GetStringResource("Language_Log_File"))
            End If
        End Function

        Private Async Function ToggleArtistTypeAsync() As Task

            Me.HideSemanticZoom()

            Select Case Me.ArtistType
                Case ArtistType.All
                    Me.ArtistType = ArtistType.Track
                Case ArtistType.Track
                    Me.ArtistType = ArtistType.Album
                Case ArtistType.Album
                    Me.ArtistType = ArtistType.All
                Case Else
                    ' Cannot happen, but just in case.
                    Me.ArtistType = ArtistType.All
            End Select

            XmlSettingsClient.Instance.Set(Of Integer)("Ordering", "ArtistType", Me.ArtistType)
            Await Me.GetArtistsAsync(Me.ArtistType)
        End Function

        Private Sub ArtistsCvs_Filter(sender As Object, e As FilterEventArgs)

            Dim avm As ArtistViewModel = TryCast(e.Item, ArtistViewModel)

            e.Accepted = Dopamine.Core.Database.Utils.FilterArtists(avm.Artist, Me.searchService.SearchText)
        End Sub
#End Region

#Region "Protected"
        Protected Sub UpdateArtistType(iArtistType As ArtistType)

            Select Case iArtistType
                Case ArtistType.All
                    mArtistTypeText = ResourceUtils.GetStringResource("Language_All")
                Case ArtistType.Track
                    mArtistTypeText = ResourceUtils.GetStringResource("Language_Song")
                Case ArtistType.Album
                    mArtistTypeText = ResourceUtils.GetStringResource("Language_Album")
                Case Else
                    ' Cannot happen, but just in case.
                    mArtistTypeText = ResourceUtils.GetStringResource("Language_All")
            End Select

            OnPropertyChanged(Function() Me.ArtistTypeText)
        End Sub

        Protected Async Function ToggleTrackOrderAsync() As Task

            MyBase.ToggleTrackOrder()

            XmlSettingsClient.Instance.Set(Of Integer)("Ordering", "ArtistsTrackOrder", Me.TrackOrder)
            Await Me.GetTracksCommonAsync(Me.Tracks.Select(Function(t) t.TrackInfo).ToList, Me.TrackOrder)
        End Function

        Protected Async Function ToggleAlbumOrderAsync() As Task

            MyBase.ToggleAlbumOrder()

            XmlSettingsClient.Instance.Set(Of Integer)("Ordering", "ArtistsAlbumOrder", Me.AlbumOrder)
            Await Me.GetAlbumsCommonAsync(Me.Albums.Select(Function(a) a.Album).ToList, Me.AlbumOrder)
        End Function
#End Region

#Region "Overrides"
        Protected Overrides Async Function SetCoversizeasync(iCoverSize As CoverSizeType) As Task

            Await MyBase.SetCoversizeAsync(iCoverSize)
            XmlSettingsClient.Instance.Set(Of Integer)("CoverSizes", "ArtistsCoverSize", iCoverSize)
        End Function

        Protected Overrides Async Function FillListsAsync() As Task

            Await Me.GetArtistsAsync(Me.ArtistType)
            Await Me.GetAlbumsAsync(Nothing, Nothing, Me.AlbumOrder)
            Await Me.GetTracksAsync(Nothing, Nothing, Nothing, Me.TrackOrder)
        End Function

        Protected Overrides Sub FilterLists()

            Application.Current.Dispatcher.Invoke(Sub()
                                                      ' Artists
                                                      If Me.ArtistsCvs IsNot Nothing Then
                                                          Me.ArtistsCvs.View.Refresh()
                                                          Me.ArtistsCount = Me.ArtistsCvs.View.Cast(Of ISemanticZoomable)().Count
                                                          Me.UpdateSemanticZoomHeaders()
                                                      End If
                                                  End Sub)

            MyBase.FilterLists()
        End Sub

        Protected Overrides Async Function SelectedAlbumsHandlerAsync(iParameter As Object) As Task

            Await MyBase.SelectedAlbumsHandlerAsync(iParameter)

            Me.SetTrackOrder("ArtistsTrackOrder")
            Await Me.GetTracksAsync(Me.SelectedArtists, Nothing, Me.SelectedAlbums, Me.TrackOrder)
        End Function

        Protected Overrides Sub Unsubscribe()

            ' Commands
            ApplicationCommands.RemoveSelectedTracksCommand.UnregisterCommand(Me.RemoveSelectedTracksCommand)
            ApplicationCommands.SemanticJumpCommand.UnregisterCommand(Me.SemanticJumpCommand)

            ' Events
            Me.eventAggregator.GetEvent(Of ShellMouseUp).Unsubscribe(mShellMouseUpToken)

            ' Other
            Me.IsArtistsZoomVisible = False
        End Sub

        Protected Overrides Sub Subscribe()

            ' Prevents subscribing twice
            Me.Unsubscribe()

            ' Commands
            ApplicationCommands.RemoveSelectedTracksCommand.RegisterCommand(Me.RemoveSelectedTracksCommand)
            ApplicationCommands.SemanticJumpCommand.RegisterCommand(Me.SemanticJumpCommand)

            ' Events
            mShellMouseUpToken = Me.eventAggregator.GetEvent(Of ShellMouseUp).Subscribe(Sub() Me.IsArtistsZoomVisible = False)
        End Sub

        Protected Overrides Sub RefreshLanguage()

            Me.UpdateArtistType(Me.ArtistType)
            Me.UpdateAlbumOrderText(Me.AlbumOrder)
            Me.UpdateTrackOrderText(Me.TrackOrder)
        End Sub
#End Region
    End Class
End Namespace

