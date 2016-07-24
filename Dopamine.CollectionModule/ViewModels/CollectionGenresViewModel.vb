Imports System.Collections.ObjectModel
Imports Dopamine.Common.Presentation.Interfaces
Imports Dopamine.Common.Presentation.Utils
Imports Dopamine.Common.Presentation.ViewModels
Imports Dopamine.Common.Services.Metadata
Imports Dopamine.Common.Services.Playback
Imports Dopamine.Core.Base
Imports Dopamine.Core.Database.Entities
Imports Dopamine.Core.Database.Repositories.Interfaces
Imports Dopamine.Core.Logging
Imports Dopamine.Core.Prism
Imports Dopamine.Core.Settings
Imports Dopamine.Core.Utils
Imports Dopamine.Core.Helpers
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Prism.PubSubEvents
Imports Dopamine.Core.Database

Namespace ViewModels
    Public Class CollectionGenresViewModel
        Inherits CommonAlbumsViewModel
        Implements ISemanticZoomViewModel

#Region "Variables"
        ' Repositories
        Private mGenreRepository As IGenreRepository

        ' Lists
        Private mGenres As ObservableCollection(Of ISemanticZoomable)
        Private mGenresCvs As CollectionViewSource
        Private mSelectedGenres As IList(Of Genre)
        Private mGenresZoomSelectors As ObservableCollection(Of ISemanticZoomSelector)

        ' Flags
        Private mIsGenresZoomVisible As Boolean

        ' Other
        Private mGenresCount As Long
        Private mShellMouseUpToken As SubscriptionToken
        Private mLeftPaneWidthPercent As Double
        Private mRightPaneWidthPercent As Double
#End Region

#Region "Commands"
        Public Property AddGenresToPlaylistCommand As DelegateCommand(Of String)
        Public Property SelectedGenresCommand As DelegateCommand(Of Object)
        Public Property ShowGenresZoomCommand As DelegateCommand
        Public Property SemanticJumpCommand As DelegateCommand Implements ISemanticZoomViewModel.SemanticJumpCommand
        Public Property AddGenresToNowPlayingCommand As DelegateCommand
#End Region

#Region "Properties"
        Public Property LeftPaneWidthPercent() As Double
            Get
                Return mLeftPaneWidthPercent
            End Get
            Set(ByVal value As Double)
                SetProperty(Of Double)(mLeftPaneWidthPercent, value)
                XmlSettingsClient.Instance.Set(Of Integer)("ColumnWidths", "GenresLeftPaneWidthPercent", CInt(value))
            End Set
        End Property

        Public Property RightPaneWidthPercent() As Double
            Get
                Return mRightPaneWidthPercent
            End Get
            Set(ByVal value As Double)
                SetProperty(Of Double)(mRightPaneWidthPercent, value)
                XmlSettingsClient.Instance.Set(Of Integer)("ColumnWidths", "GenresRightPaneWidthPercent", CInt(value))
            End Set
        End Property

        Public Property Genres() As ObservableCollection(Of ISemanticZoomable) Implements ISemanticZoomViewModel.SemanticZoomables
            Get
                Return mGenres
            End Get
            Set(ByVal value As ObservableCollection(Of ISemanticZoomable))
                SetProperty(Of ObservableCollection(Of ISemanticZoomable))(mGenres, value)
            End Set
        End Property

        Public Property GenresCvs() As CollectionViewSource
            Get
                Return mGenresCvs
            End Get
            Set(ByVal value As CollectionViewSource)
                SetProperty(Of CollectionViewSource)(mGenresCvs, value)
            End Set
        End Property

        Public Property SelectedGenres() As IList(Of Genre)
            Get
                Return mSelectedGenres
            End Get
            Set(ByVal value As IList(Of Genre))
                SetProperty(Of IList(Of Genre))(mSelectedGenres, value)
            End Set
        End Property

        Public Property GenresCount() As Long
            Get
                Return mGenresCount
            End Get
            Set(ByVal value As Long)
                SetProperty(Of Long)(mGenresCount, value)
            End Set
        End Property

        Public Property IsGenresZoomVisible() As Boolean
            Get
                Return mIsGenresZoomVisible
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsGenresZoomVisible, value)
            End Set
        End Property

        Public Property GenresZoomSelectors() As ObservableCollection(Of ISemanticZoomSelector) Implements ISemanticZoomViewModel.SemanticZoomSelectors
            Get
                Return mGenresZoomSelectors
            End Get
            Set(ByVal value As ObservableCollection(Of ISemanticZoomSelector))
                SetProperty(Of ObservableCollection(Of ISemanticZoomSelector))(mGenresZoomSelectors, value)
            End Set
        End Property

        Public Overrides ReadOnly Property CanOrderByAlbum As Boolean
            Get
                Return (Me.SelectedGenres IsNot Nothing AndAlso
                       Me.SelectedGenres.Count > 0) Or
                       (Me.SelectedAlbums IsNot Nothing AndAlso
                       Me.SelectedAlbums.Count > 0)
            End Get
        End Property
#End Region

#Region "Construction"
        Public Sub New(iGenreRepository As IGenreRepository)

            MyBase.New()

            ' Repositories
            mGenreRepository = iGenreRepository

            ' Commands
            Me.ToggleTrackOrderCommand = New DelegateCommand(Async Sub() Await Me.ToggleTrackOrderAsync)
            Me.ToggleAlbumOrderCommand = New DelegateCommand(Async Sub() Await Me.ToggleAlbumOrderAsync)
            Me.RemoveSelectedTracksCommand = New DelegateCommand(Async Sub() Await Me.RemoveTracksFromCollectionAsync(Me.SelectedTracks), Function() Not Me.IsIndexing)
            Me.AddGenresToPlaylistCommand = New DelegateCommand(Of String)(Async Sub(iPlaylistName) Await Me.AddGenresToPlaylistAsync(Me.SelectedGenres, iPlaylistName))
            Me.SelectedGenresCommand = New DelegateCommand(Of Object)(Async Sub(iParameter) Await Me.SelectedGenresHandlerAsync(iParameter))
            Me.ShowGenresZoomCommand = New DelegateCommand(Async Sub() Await Me.ShowSemanticZoomAsync())
            Me.SemanticJumpCommand = New DelegateCommand(Sub() Me.IsGenresZoomVisible = False)
            Me.AddGenresToNowPlayingCommand = New DelegateCommand(Async Sub() Await Me.AddGenresToNowPlayingAsync(Me.SelectedGenres))

            ' Events
            Me.eventAggregator.GetEvent(Of SettingEnableRatingChanged)().Subscribe(Async Sub(iEnableRating)
                                                                                       Me.EnableRating = iEnableRating
                                                                                       Me.SetTrackOrder("GenresTrackOrder")
                                                                                       Await Me.GetTracksAsync(Nothing, Me.SelectedGenres, Me.SelectedAlbums, Me.TrackOrder)
                                                                                   End Sub)

            Me.eventAggregator.GetEvent(Of SettingUseStarRatingChanged)().Subscribe(Sub(iUseStarRating) Me.UseStarRating = iUseStarRating)

            ' MetadataService
            AddHandler Me.metadataService.MetadataChanged, AddressOf MetadataChangedHandlerAsync

            ' IndexingService
            AddHandler Me.indexingService.RefreshArtwork, Async Sub() Await Me.collectionService.RefreshArtworkAsync(Me.Albums, Me.Tracks)

            ' Set the initial AlbumOrder
            Me.AlbumOrder = CType(XmlSettingsClient.Instance.Get(Of Integer)("Ordering", "GenresAlbumOrder"), AlbumOrder)

            ' Set the initial TrackOrder
            Me.SetTrackOrder("GenresTrackOrder")

            ' Subscribe to Events and Commands on creation
            Me.Subscribe()

            ' Set width of the panels
            Me.LeftPaneWidthPercent = XmlSettingsClient.Instance.Get(Of Integer)("ColumnWidths", "GenresLeftPaneWidthPercent")
            Me.RightPaneWidthPercent = XmlSettingsClient.Instance.Get(Of Integer)("ColumnWidths", "GenresRightPaneWidthPercent")

            ' Cover size
            Me.SetCoversizeasync(CType(XmlSettingsClient.Instance.Get(Of Integer)("CoverSizes", "GenresCoverSize"), CoverSizeType))
        End Sub
#End Region

#Region "ISemanticZoomViewModel"
        Public Async Function ShowSemanticZoomAsync() As Task Implements ISemanticZoomViewModel.ShowSemanticZoomAsync

            Me.GenresZoomSelectors = Await SemanticZoomUtils.UpdateSemanticZoomSelectors(Me.GenresCvs.View)

            Me.IsGenresZoomVisible = True
        End Function

        Public Sub HideSemanticZoom() Implements ISemanticZoomViewModel.HideSemanticZoom
            Me.IsGenresZoomVisible = False
        End Sub

        Public Sub UpdateSemanticZoomHeaders() Implements ISemanticZoomViewModel.UpdateSemanticZoomHeaders

            Dim previousHeader As String = String.Empty

            For Each gvm As GenreViewModel In Me.GenresCvs.View

                If String.IsNullOrEmpty(previousHeader) OrElse Not gvm.Header.Equals(previousHeader) Then

                    previousHeader = gvm.Header
                    gvm.IsHeader = True
                End If
            Next
        End Sub
#End Region

#Region "Private"
        Private Async Sub MetadataChangedHandlerAsync(e As MetadataChangedEventArgs)

            If e.IsAlbumArtworkMetadataChanged Then
                Await Me.collectionService.RefreshArtworkAsync(Me.Albums, Me.Tracks)
            End If

            If e.IsGenreMetadataChanged Then
                Await Me.GetGenresAsync()
            End If

            If e.IsGenreMetadataChanged Or e.IsAlbumTitleMetadataChanged Or e.IsAlbumArtistMetadataChanged Or e.IsAlbumYearMetadataChanged Then
                Await Me.GetAlbumsAsync(Nothing, Me.SelectedGenres, Me.AlbumOrder)
            End If

            If e.IsGenreMetadataChanged Or e.IsAlbumTitleMetadataChanged Or e.IsAlbumArtistMetadataChanged Or e.IsTrackMetadataChanged Then
                Await Me.GetTracksAsync(Nothing, Me.SelectedGenres, Me.SelectedAlbums, Me.TrackOrder)
            End If
        End Sub

        Private Async Function GetGenresAsync() As Task

            Try
                ' Get Genres from database
                Dim genres As List(Of Genre) = Await mGenreRepository.GetGenresAsync()

                ' Create new ObservableCollection
                Dim genreViewModels As New ObservableCollection(Of GenreViewModel)

                Await Task.Run(Sub()
                                   Dim tempGenreViewModels As New List(Of GenreViewModel)

                                   ' Workaround to make sure the "#" GroupHeader is shown at the top of the list
                                   tempGenreViewModels.AddRange(genres.Select(Function(gen) New GenreViewModel With {.Genre = gen, .IsHeader = False}).Where(Function(gvm) gvm.Header.Equals("#")))
                                   tempGenreViewModels.AddRange(genres.Select(Function(gen) New GenreViewModel With {.Genre = gen, .IsHeader = False}).Where(Function(gvm) Not gvm.Header.Equals("#")))

                                   For Each gvm As GenreViewModel In tempGenreViewModels
                                       genreViewModels.Add(gvm)
                                   Next
                               End Sub)

                ' Unbind to improve UI performance
                If Me.GenresCvs IsNot Nothing Then RemoveHandler Me.GenresCvs.Filter, New FilterEventHandler(AddressOf GenresCvs_Filter)
                Me.Genres = Nothing
                Me.GenresCvs = Nothing

                ' Populate ObservableCollection
                Me.Genres = New ObservableCollection(Of ISemanticZoomable)(genreViewModels)
            Catch ex As Exception
                LogClient.Instance.Logger.Error("An error occurred while getting Genres. Exception: {0}", ex.Message)

                ' Failed getting Genres. Create empty ObservableCollection.
                Me.Genres = New ObservableCollection(Of ISemanticZoomable)
            End Try

            ' Populate CollectionViewSource
            Me.GenresCvs = New CollectionViewSource With {.Source = Me.Genres}
            AddHandler Me.GenresCvs.Filter, New FilterEventHandler(AddressOf GenresCvs_Filter)

            ' Update count
            Me.GenresCount = Me.GenresCvs.View.Cast(Of ISemanticZoomable)().Count

            ' Update Semantic Zoom Headers
            Me.UpdateSemanticZoomHeaders()
        End Function

        Private Async Function SelectedGenresHandlerAsync(iParameter As Object) As Task

            If iParameter IsNot Nothing Then

                Me.SelectedGenres = New List(Of Genre)

                For Each item As GenreViewModel In CType(iParameter, IList)
                    Me.SelectedGenres.Add(item.Genre)
                Next
            End If

            ' Don't reload the lists when updating Metadata. MetadataChangedHandlerAsync handles that.
            If Me.metadataService.IsUpdatingDatabaseMetadata Then Return

            Await Me.GetAlbumsAsync(Nothing, Me.SelectedGenres, CType(XmlSettingsClient.Instance.Get(Of Integer)("Ordering", "GenresAlbumOrder"), AlbumOrder))
            Me.SetTrackOrder("GenresTrackOrder")
            Await Me.GetTracksAsync(Nothing, Me.SelectedGenres, Me.SelectedAlbums, Me.TrackOrder)
        End Function

        Private Async Function AddGenresToPlaylistAsync(iGenres As IList(Of Genre), iPlaylistName As String) As Task

            Dim result As AddToPlaylistResult = Await Me.collectionService.AddGenresToPlaylistAsync(iGenres, iPlaylistName)

            If Not result.IsSuccess Then

                Me.dialogService.ShowNotification(&HE711,
                                                16,
                                                ResourceUtils.GetStringResource("Language_Error"),
                                                Replace(ResourceUtils.GetStringResource("Language_Error_Adding_Genres_To_Playlist"), "%playlistname%", """" & iPlaylistName & """"),
                                                ResourceUtils.GetStringResource("Language_Ok"),
                                                True,
                                                ResourceUtils.GetStringResource("Language_Log_File"))
            End If
        End Function

        Private Async Function AddGenresToNowPlayingAsync(iGenres As IList(Of Genre)) As Task

            Dim result As AddToQueueResult = Await Me.playbackService.AddToQueue(iGenres)

            If Not result.IsSuccess Then

                Me.dialogService.ShowNotification(&HE711,
                                                16,
                                                ResourceUtils.GetStringResource("Language_Error"),
                                                ResourceUtils.GetStringResource("Language_Error_Adding_Genres_To_Now_Playing"),
                                                ResourceUtils.GetStringResource("Language_Ok"),
                                                True,
                                                ResourceUtils.GetStringResource("Language_Log_File"))
            End If
        End Function

        Private Sub GenresCvs_Filter(sender As Object, e As FilterEventArgs)

            Dim gvm As GenreViewModel = TryCast(e.Item, GenreViewModel)

            e.Accepted = Dopamine.Core.Database.Utils.FilterGenres(gvm.Genre, Me.searchService.SearchText)
        End Sub
#End Region

#Region "Protected"
        Protected Async Function ToggleTrackOrderAsync() As Task

            MyBase.ToggleTrackOrder()

            XmlSettingsClient.Instance.Set(Of Integer)("Ordering", "GenresTrackOrder", Me.TrackOrder)
            Await Me.GetTracksCommonAsync(Me.Tracks.Select(Function(t) t.TrackInfo).ToList, Me.TrackOrder)
        End Function

        Protected Async Function ToggleAlbumOrderAsync() As Task

            MyBase.ToggleAlbumOrder()

            XmlSettingsClient.Instance.Set(Of Integer)("Ordering", "GenresAlbumOrder", Me.AlbumOrder)
            Await Me.GetAlbumsCommonAsync(Me.Albums.Select(Function(a) a.Album).ToList, Me.AlbumOrder)
        End Function
#End Region

#Region "Overrides"
        Protected Overrides Async Function SetCoversizeasync(iCoverSize As CoverSizeType) As Task

            Await MyBase.SetCoversizeAsync(iCoverSize)
            XmlSettingsClient.Instance.Set(Of Integer)("CoverSizes", "GenresCoverSize", iCoverSize)
        End Function

        Protected Overrides Async Function FillListsAsync() As Task

            Await Me.GetGenresAsync()
            Await Me.GetAlbumsAsync(Nothing, Nothing, Me.AlbumOrder)
            Await Me.GetTracksAsync(Nothing, Nothing, Nothing, Me.TrackOrder)
        End Function

        Protected Overrides Sub FilterLists()

            Application.Current.Dispatcher.Invoke(Sub()
                                                      ' Genres
                                                      If Me.GenresCvs IsNot Nothing Then
                                                          Me.GenresCvs.View.Refresh()
                                                          Me.GenresCount = Me.GenresCvs.View.Cast(Of ISemanticZoomable)().Count
                                                          Me.UpdateSemanticZoomHeaders()
                                                      End If
                                                  End Sub)

            MyBase.FilterLists()
        End Sub

        Protected Overrides Async Function SelectedAlbumsHandlerAsync(iParameter As Object) As Task

            Await MyBase.SelectedAlbumsHandlerAsync(iParameter)

            Me.SetTrackOrder("GenresTrackOrder")
            Await Me.GetTracksAsync(Nothing, Me.SelectedGenres, Me.SelectedAlbums, Me.TrackOrder)
        End Function

        Protected Overrides Sub Unsubscribe()

            ' Commands
            ApplicationCommands.RemoveSelectedTracksCommand.UnregisterCommand(Me.RemoveSelectedTracksCommand)
            ApplicationCommands.SemanticJumpCommand.UnregisterCommand(Me.SemanticJumpCommand)

            ' Events
            Me.eventAggregator.GetEvent(Of ShellMouseUp).Unsubscribe(mShellMouseUpToken)

            ' Other
            Me.IsGenresZoomVisible = False
        End Sub

        Protected Overrides Sub Subscribe()

            ' Prevents subscribing twice
            Me.Unsubscribe()

            ' Commands
            ApplicationCommands.RemoveSelectedTracksCommand.RegisterCommand(Me.RemoveSelectedTracksCommand)
            ApplicationCommands.SemanticJumpCommand.RegisterCommand(Me.SemanticJumpCommand)

            ' Events
            mShellMouseUpToken = Me.eventAggregator.GetEvent(Of ShellMouseUp).Subscribe(Sub() Me.IsGenresZoomVisible = False)
        End Sub

        Protected Overrides Sub RefreshLanguage()

            Me.UpdateAlbumOrderText(Me.AlbumOrder)
            Me.UpdateTrackOrderText(Me.TrackOrder)
        End Sub
#End Region
    End Class
End Namespace

