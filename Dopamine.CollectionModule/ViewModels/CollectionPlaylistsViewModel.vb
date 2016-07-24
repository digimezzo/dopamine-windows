Imports System.Collections.ObjectModel
Imports Dopamine.Common.Presentation.ViewModels
Imports Dopamine.Common.Services.Metadata
Imports Dopamine.Common.Services.Playback
Imports Dopamine.Core.Base
Imports Dopamine.Core.Database.Entities
Imports Dopamine.Core.Database
Imports Dopamine.Core.Database.Repositories.Interfaces
Imports Dopamine.Core.Logging
Imports Dopamine.Core.Prism
Imports Dopamine.Core.Utils
Imports Microsoft.Practices.Prism.Commands
Imports WPFFolderBrowser
Imports Dopamine.Core.Settings
Imports Dopamine.Core.IO

Namespace ViewModels
    Public Class CollectionPlaylistsViewModel
        Inherits CommonTracksViewModel

#Region "Variables"

        ' Lists
        Private mPlaylists As ObservableCollection(Of PlaylistViewModel)
        Private mSelectedPlaylists As IList(Of Playlist)

        ' Flags
        Private mIsLoadingPlaylists As Boolean

        ' Repositories
        Private mPlaylistRepository As IPlaylistRepository

        ' Other
        Private mPlaylistsCount As Long
        Private mLeftPaneWidthPercent As Double
#End Region

#Region "Commands"
        Public Property NewPlaylistCommand As DelegateCommand
        Public Property OpenPlaylistCommand As DelegateCommand
        Public Property DeletePlaylistByNameCommand As DelegateCommand(Of String)
        Public Property RenameSelectedPlaylistCommand As DelegateCommand
        Public Property DeleteSelectedPlaylistsCommand As DelegateCommand
        Public Property SaveSelectedPlaylistsCommand As DelegateCommand
        Public Property SelectedPlaylistsCommand As DelegateCommand(Of Object)
        Public Property AddPlaylistsToNowPlayingCommand As DelegateCommand
#End Region

#Region "Properties"
        Public Property LeftPaneWidthPercent() As Double
            Get
                Return mLeftPaneWidthPercent
            End Get
            Set(ByVal value As Double)
                SetProperty(Of Double)(mLeftPaneWidthPercent, value)
                XmlSettingsClient.Instance.Set(Of Integer)("ColumnWidths", "PlaylistsLeftPaneWidthPercent", CInt(value))
            End Set
        End Property

        Public ReadOnly Property AllowRename() As Boolean
            Get
                If Me.SelectedPlaylists IsNot Nothing Then
                    Return Me.SelectedPlaylists.Count = 1
                Else
                    Return False
                End If
            End Get
        End Property

        Public Property IsLoadingPlaylists() As Boolean
            Get
                Return mIsLoadingPlaylists
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mIsLoadingPlaylists, value)
            End Set
        End Property

        Public Property Playlists() As ObservableCollection(Of PlaylistViewModel)
            Get
                Return mPlaylists
            End Get
            Set(ByVal value As ObservableCollection(Of PlaylistViewModel))
                SetProperty(Of ObservableCollection(Of PlaylistViewModel))(Me.mPlaylists, value)
            End Set
        End Property

        Public Property SelectedPlaylists() As IList(Of Playlist)
            Get
                Return mSelectedPlaylists
            End Get
            Set(ByVal value As IList(Of Playlist))
                SetProperty(Of IList(Of Playlist))(mSelectedPlaylists, value)
            End Set
        End Property

        Public Property PlaylistsCount() As Long
            Get
                Return mPlaylistsCount
            End Get
            Set(ByVal value As Long)
                SetProperty(Of Long)(mPlaylistsCount, value)
            End Set
        End Property

        Public Overrides ReadOnly Property CanOrderByAlbum As Boolean
            Get
                Return False ' Doesn't need to return a useful value in this class
            End Get
        End Property
#End Region

#Region "Construction"
        Public Sub New(iPlaylistRepository As IPlaylistRepository)

            MyBase.New()

            ' Repositories
            mPlaylistRepository = iPlaylistRepository

            ' Commands
            Me.NewPlaylistCommand = New DelegateCommand(Async Sub() Await Me.ConfirmAddPlaylistAsync())
            Me.OpenPlaylistCommand = New DelegateCommand(Async Sub() Await Me.OpenPlaylistAsync)
            Me.DeletePlaylistByNameCommand = New DelegateCommand(Of String)(Async Sub(iPlaylistName) Await Me.DeletePlaylistByNameAsync(iPlaylistName))
            Me.DeleteSelectedPlaylistsCommand = New DelegateCommand(Async Sub() Await Me.DeleteSelectedPlaylistsAsync)
            Me.RenameSelectedPlaylistCommand = New DelegateCommand(Async Sub() Await Me.RenameSelectedPlaylistAsync)
            Me.RemoveSelectedTracksCommand = New DelegateCommand(Async Sub() Await Me.DeleteTracksFromPlaylistsAsync)
            Me.SaveSelectedPlaylistsCommand = New DelegateCommand(Async Sub() Await Me.SaveSelectedPlaylistsAsync)
            Me.SelectedPlaylistsCommand = New DelegateCommand(Of Object)(Async Sub(iParameter) Await SelectedPlaylistsHandlerAsync(iParameter))
            Me.AddPlaylistsToNowPlayingCommand = New DelegateCommand(Async Sub() Await Me.AddPLaylistsToNowPlayingAsync(Me.SelectedPlaylists))

            ' Events
            Me.eventAggregator.GetEvent(Of SettingEnableRatingChanged)().Subscribe(Sub(iEnableRating) Me.EnableRating = iEnableRating)
            Me.eventAggregator.GetEvent(Of SettingUseStarRatingChanged)().Subscribe(Sub(iUseStarRating) Me.UseStarRating = iUseStarRating)

            ' MetadataService
            AddHandler Me.metadataService.MetadataChanged, AddressOf MetadataChangedHandlerAsync

            ' CollectionService
            AddHandler Me.collectionService.AddedTracksToPlaylist, Async Sub() Await ReloadPlaylistsAsync()
            AddHandler Me.collectionService.DeletedTracksFromPlaylists, Async Sub() Await ReloadPlaylistsAsync()
            AddHandler Me.collectionService.PlaylistsChanged, Async Sub() Await FillListsAsync() ' Refreshes the lists when the playlists have changed

            ' Events
            Me.eventAggregator.GetEvent(Of RenameSelectedPlaylistWithKeyF2)().Subscribe(Async Sub() Await Me.RenameSelectedPlaylistAsync())
            Me.eventAggregator.GetEvent(Of DeleteSelectedPlaylistsWithKeyDelete)().Subscribe(Async Sub() Await Me.DeleteSelectedPlaylistsAsync())

            Me.TrackOrder = TrackOrder.ByAlbum

            ' Subscribe to Events and Commands on creation
            Me.Subscribe()

            ' Set width of the panels
            Me.LeftPaneWidthPercent = XmlSettingsClient.Instance.Get(Of Integer)("ColumnWidths", "PlaylistsLeftPaneWidthPercent")
        End Sub
#End Region

#Region "Private"
        Private Async Sub MetadataChangedHandlerAsync(e As MetadataChangedEventArgs)

            If e.IsAlbumArtworkMetadataChanged Then
                Await Me.collectionService.RefreshArtworkAsync(Nothing, Me.Tracks)
            End If

            If e.IsAlbumTitleMetadataChanged Or e.IsAlbumArtistMetadataChanged Or e.IsTrackMetadataChanged Then
                Await Me.GetTracksAsync(Me.SelectedPlaylists, Me.TrackOrder)
            End If
        End Sub

        Protected Async Function AddPLaylistsToNowPlayingAsync(iPlaylists As IList(Of Playlist)) As Task

            Dim result As AddToQueueResult = Await Me.playbackService.AddToQueue(iPlaylists)

            If Not result.IsSuccess Then

                Me.dialogService.ShowNotification(&HE711,
                                                16,
                                                ResourceUtils.GetStringResource("Language_Error"),
                                                ResourceUtils.GetStringResource("Language_Error_Adding_Playlists_To_Now_Playing"),
                                                ResourceUtils.GetStringResource("Language_Ok"),
                                                True,
                                                ResourceUtils.GetStringResource("Language_Log_File"))
            End If
        End Function

        Private Async Function GetPlaylistsAsync() As Task

            Try
                ' Notify the user
                Me.IsLoadingPlaylists = True

                ' Get the Albums from the database
                Dim playlists As IList(Of Playlist) = Await mPlaylistRepository.GetPlaylistsAsync()

                ' Set the count
                Me.PlaylistsCount = playlists.Count

                ' Populate an ObservableCollection
                Dim playlistViewModels As New ObservableCollection(Of PlaylistViewModel)

                Await Task.Run(Sub()
                                   For Each pl As Playlist In playlists
                                       playlistViewModels.Add(New PlaylistViewModel With {.Playlist = pl})
                                   Next
                               End Sub)

                ' Unbind and rebind to improve UI performance
                Me.Playlists = Nothing
                Me.Playlists = playlistViewModels
            Catch ex As Exception
                LogClient.Instance.Logger.Error("An error occured while getting Playlists. Exception: {0}", ex.Message)

                ' If loading from the database failed, create and empty Collection.
                Me.Playlists = New ObservableCollection(Of PlaylistViewModel)
            Finally
                ' Stop notifying
                Me.IsLoadingPlaylists = False
            End Try
        End Function

        Public Overloads Async Function GetTracksAsync(iSelectedPlaylists As IList(Of Playlist), iTrackOrder As TrackOrder) As Task

            Await Me.GetTracksCommonAsync(Await Me.trackRepository.GetTracksAsync(iSelectedPlaylists), iTrackOrder)
        End Function

        Private Async Function ConfirmAddPlaylistAsync() As Task

            Dim responseText As String = ResourceUtils.GetStringResource("Language_New_Playlist")

            If Me.dialogService.ShowInputDialog(&HEA37,
                                              16,
                                              ResourceUtils.GetStringResource("Language_New_Playlist"),
                                              ResourceUtils.GetStringResource("Language_Enter_Name_For_New_Playlist"),
                                              ResourceUtils.GetStringResource("Language_Ok"),
                                              ResourceUtils.GetStringResource("Language_Cancel"),
                                              responseText) Then

                Await Me.AddPlaylistAsync(responseText)
            End If
        End Function

        Private Async Function OpenPlaylistAsync() As Task

            ' Set up the file dialog box
            Dim dlg As New Microsoft.Win32.OpenFileDialog
            dlg.Title = Application.Current.FindResource("Language_Open_Playlist").ToString
            dlg.DefaultExt = FileFormats.M3U ' Default file extension
            dlg.Filter = ResourceUtils.GetStringResource("Language_Playlists") & " (*" & FileFormats.M3U & ";*" & FileFormats.ZPL & ")|*" & FileFormats.M3U & ";*" & FileFormats.ZPL ' Filter files by extension

            ' Show the file dialog box
            Dim dialogResult As Boolean? = dlg.ShowDialog()

            ' Process the file dialog box result
            If dialogResult Then

                Me.IsLoadingPlaylists = True

                Dim openResult As OpenPlaylistResult = Await Me.collectionService.OpenPlaylistAsync(dlg.FileName)

                If openResult = OpenPlaylistResult.Error Then
                    Me.IsLoadingPlaylists = False

                    Me.dialogService.ShowNotification(&HE711,
                                                    16,
                                                    ResourceUtils.GetStringResource("Language_Error"),
                                                    ResourceUtils.GetStringResource("Language_Error_Opening_Playlist"),
                                                    ResourceUtils.GetStringResource("Language_Ok"),
                                                    True,
                                                    ResourceUtils.GetStringResource("Language_Log_File"))
                End If
            End If
        End Function

        Private Async Function AddPlaylistAsync(iPlaylistName As String) As Task

            Me.IsLoadingPlaylists = True

            Dim result As AddPlaylistResult = Await Me.collectionService.AddPlaylistAsync(iPlaylistName)

            Select Case result
                Case AddPlaylistResult.Success
                    Await Me.FillListsAsync()
                Case AddPlaylistResult.Duplicate
                    Me.IsLoadingPlaylists = False
                    Me.dialogService.ShowNotification(&HE711,
                                                    16,
                                                    ResourceUtils.GetStringResource("Language_Already_Exists"),
                                                    Replace(ResourceUtils.GetStringResource("Language_Already_Playlist_With_That_Name"), "%playlistname%", """" & iPlaylistName & """"),
                                                    ResourceUtils.GetStringResource("Language_Ok"),
                                                    False,
                                                    String.Empty)
                Case AddPlaylistResult.Error
                    Me.IsLoadingPlaylists = False
                    Me.dialogService.ShowNotification(&HE711,
                                                    16,
                                                    ResourceUtils.GetStringResource("Language_Error"),
                                                    ResourceUtils.GetStringResource("Language_Error_Adding_Playlist"),
                                                    ResourceUtils.GetStringResource("Language_Ok"),
                                                    True,
                                                    ResourceUtils.GetStringResource("Language_Log_File"))
                Case AddPlaylistResult.Blank
                    Me.IsLoadingPlaylists = False
                    Me.dialogService.ShowNotification(&HE711,
                                                    16,
                                                    ResourceUtils.GetStringResource("Language_Error"),
                                                    ResourceUtils.GetStringResource("Language_Provide_Playlist_Name"),
                                                    ResourceUtils.GetStringResource("Language_Ok"),
                                                    False,
                                                    String.Empty)

                Case Else
                    ' Never happens
                    Me.IsLoadingPlaylists = False
            End Select
        End Function

        Private Async Function DeletePlaylistByNameAsync(iPlaylistName As String) As Task

            If Me.dialogService.ShowConfirmation(&HE11B,
                                               16,
                                               ResourceUtils.GetStringResource("Language_Delete"),
                                               Replace(ResourceUtils.GetStringResource("Language_Are_You_Sure_To_Delete_Playlist"), "%playlistname%", """" & iPlaylistName & """"),
                                               ResourceUtils.GetStringResource("Language_Yes"),
                                               ResourceUtils.GetStringResource("Language_No")) Then

                Dim playlists As New List(Of Playlist)
                playlists.Add(New Playlist With {.PlaylistName = iPlaylistName})

                Await Me.DeletePlaylistsAsync(playlists)
            End If
        End Function

        Private Async Function DeleteSelectedPlaylistsAsync() As Task
            If Me.SelectedPlaylists IsNot Nothing AndAlso Me.SelectedPlaylists.Count > 0 Then

                Dim title As String = ResourceUtils.GetStringResource("Language_Delete")
                Dim message As String = ResourceUtils.GetStringResource("Language_Are_You_Sure_To_Delete_Playlists")

                If Me.SelectedPlaylists.Count = 1 Then
                    message = Replace(ResourceUtils.GetStringResource("Language_Are_You_Sure_To_Delete_Playlist"), "%playlistname%", """" & Me.SelectedPlaylists(0).PlaylistName & """")
                End If

                If Me.dialogService.ShowConfirmation(&HE11B,
                                                   16,
                                                   title,
                                                   message,
                                                   ResourceUtils.GetStringResource("Language_Yes"),
                                                   ResourceUtils.GetStringResource("Language_No")) Then

                    Await Me.DeletePlaylistsAsync(Me.SelectedPlaylists)
                End If
            End If
        End Function

        Private Async Function DeletePlaylistsAsync(iPlaylists As IList(Of Playlist)) As Task

            Me.IsLoadingPlaylists = True
            Dim result As DeletePlaylistResult = Await Me.collectionService.DeletePlaylistsAsync(iPlaylists)

            Await Me.FillListsAsync()

            If result = DeletePlaylistResult.Error Then

                Dim message As String = ResourceUtils.GetStringResource("Language_Error_Deleting_Playlists")

                If iPlaylists.Count = 1 Then
                    message = Replace(ResourceUtils.GetStringResource("Language_Error_Deleting_Playlist"), "%playlistname%", """" & iPlaylists(0).PlaylistName & """")
                End If

                Me.dialogService.ShowNotification(&HE711,
                                                16,
                                                ResourceUtils.GetStringResource("Language_Error"),
                                                message,
                                                ResourceUtils.GetStringResource("Language_Ok"),
                                                True,
                                                ResourceUtils.GetStringResource("Language_Log_File"))
            End If
        End Function

        Private Async Function RenameSelectedPlaylistAsync() As Task
            If Me.SelectedPlaylists IsNot Nothing AndAlso Me.SelectedPlaylists.Count > 0 Then

                Dim oldPlaylistName As String = Me.SelectedPlaylists(0).PlaylistName
                Dim responseText As String = oldPlaylistName

                If Me.dialogService.ShowInputDialog(&HEA37,
                                                  16,
                                                  ResourceUtils.GetStringResource("Language_Rename_Playlist"),
                                                  Replace(ResourceUtils.GetStringResource("Language_Enter_New_Name_For_Playlist"), "%playlistname%", oldPlaylistName),
                                                  ResourceUtils.GetStringResource("Language_Ok"),
                                                  ResourceUtils.GetStringResource("Language_Cancel"),
                                                  responseText) Then

                    Await Me.RenamePlaylistAsync(oldPlaylistName, responseText)
                End If
            End If
        End Function

        Private Async Function RenamePlaylistAsync(iOldPlaylistName As String, iNewPlaylistName As String) As Task

            Me.IsLoadingPlaylists = True
            Dim result As RenamePlaylistResult = Await Me.collectionService.RenamePlaylistAsync(iOldPlaylistName, iNewPlaylistName)

            Select Case result
                Case RenamePlaylistResult.Success
                    Await Me.FillListsAsync
                Case RenamePlaylistResult.Duplicate
                    Me.IsLoadingPlaylists = False
                    Me.dialogService.ShowNotification(&HE711,
                                                    16,
                                                    ResourceUtils.GetStringResource("Language_Already_Exists"),
                                                    Replace(ResourceUtils.GetStringResource("Language_Already_Playlist_With_That_Name"), "%playlistname%", """" & iNewPlaylistName & """"),
                                                    ResourceUtils.GetStringResource("Language_Ok"),
                                                    False,
                                                    String.Empty)
                Case RenamePlaylistResult.Error
                    Me.IsLoadingPlaylists = False
                    Me.dialogService.ShowNotification(&HE711,
                                                    16,
                                                    ResourceUtils.GetStringResource("Language_Error"),
                                                    ResourceUtils.GetStringResource("Language_Error_Renaming_Playlist"),
                                                    ResourceUtils.GetStringResource("Language_Ok"),
                                                    True,
                                                    ResourceUtils.GetStringResource("Language_Log_File"))
                Case RenamePlaylistResult.Blank
                    Me.IsLoadingPlaylists = False
                    Me.dialogService.ShowNotification(&HE711,
                                                    16,
                                                    ResourceUtils.GetStringResource("Language_Error"),
                                                    ResourceUtils.GetStringResource("Language_Provide_Playlist_Name"),
                                                    ResourceUtils.GetStringResource("Language_Ok"),
                                                    False,
                                                    String.Empty)

                Case Else
                    ' Never happens
                    Me.IsLoadingPlaylists = False
            End Select
        End Function

        Private Async Function DeleteTracksFromPlaylistsAsync() As Task

            Dim result As DeleteTracksFromPlaylistsResult = Await Me.collectionService.DeleteTracksFromPlaylistAsync(Me.SelectedTracks, Me.SelectedPlaylists.FirstOrDefault)

            Select Case result
                Case DeleteTracksFromPlaylistsResult.Success

                    Await Me.GetTracksAsync(Me.SelectedPlaylists, Me.TrackOrder)
                Case DeleteTracksFromPlaylistsResult.Error
                    Me.dialogService.ShowNotification(&HE711,
                                                    16,
                                                    ResourceUtils.GetStringResource("Language_Error"),
                                                    ResourceUtils.GetStringResource("Language_Error_Removing_From_Playlist"),
                                                    ResourceUtils.GetStringResource("Language_Ok"),
                                                    True,
                                                    ResourceUtils.GetStringResource("Language_Log_File"))
                Case Else
                    ' Never happens
            End Select
        End Function

        Private Async Function ReloadPlaylistsAsync() As Task

            If Me.SelectedPlaylists IsNot Nothing AndAlso Me.SelectedPlaylists.Count > 0 Then
                Await Me.GetTracksAsync(Me.SelectedPlaylists, Me.TrackOrder)
            End If
        End Function

        Private Async Function SelectedPlaylistsHandlerAsync(iParameter As Object) As Task

            If iParameter IsNot Nothing Then

                Me.SelectedPlaylists = New List(Of Playlist)

                For Each item As PlaylistViewModel In CType(iParameter, IList)
                    Me.SelectedPlaylists.Add(item.Playlist)
                Next
                OnPropertyChanged(Function() Me.AllowRename)
            End If

            ' Don't reload the lists when updating Metadata. MetadataChangedHandlerAsync handles that.
            If Me.metadataService.IsUpdatingDatabaseMetadata Then Return

            Await Me.GetTracksAsync(Me.SelectedPlaylists, Me.TrackOrder)
        End Function

        Private Async Function SaveSelectedPlaylistsAsync() As Task

            If Me.SelectedPlaylists IsNot Nothing AndAlso Me.SelectedPlaylists.Count > 0 Then

                If Me.SelectedPlaylists.Count > 1 Then

                    ' Save all the selected playlists
                    ' -------------------------------
                    Dim dlg As New WPFFolderBrowserDialog

                    If dlg.ShowDialog Then

                        Try
                            Dim result As Integer = Await Me.collectionService.ExportPlaylistsAsync(Me.SelectedPlaylists, dlg.FileName)

                            Select Case result
                                Case ExportPlaylistsResult.Success
                                    ' Do nothing
                                Case ExportPlaylistsResult.Error
                                    Me.dialogService.ShowNotification(&HE711,
                                                                       16,
                                                                       ResourceUtils.GetStringResource("Language_Error"),
                                                                       ResourceUtils.GetStringResource("Language_Error_Saving_Playlists"),
                                                                       ResourceUtils.GetStringResource("Language_Ok"),
                                                                       True,
                                                                       ResourceUtils.GetStringResource("Language_Log_File"))
                            End Select
                        Catch ex As Exception
                            LogClient.Instance.Logger.Error("Exception: {0}", ex.Message)

                            Me.dialogService.ShowNotification(&HE711,
                                                               16,
                                                               ResourceUtils.GetStringResource("Language_Error"),
                                                               ResourceUtils.GetStringResource("Language_Error_Saving_Playlists"),
                                                               ResourceUtils.GetStringResource("Language_Ok"),
                                                               True,
                                                               ResourceUtils.GetStringResource("Language_Log_File"))
                        End Try
                    End If
                ElseIf Me.SelectedPlaylists.Count = 1 Then

                    ' Save 1 playlist
                    ' ---------------

                    Dim dlg As New Microsoft.Win32.SaveFileDialog()
                    dlg.FileName = FileOperations.SanitizeFilename(Me.SelectedPlaylists(0).PlaylistName)
                    dlg.DefaultExt = FileFormats.M3U
                    dlg.Filter = String.Concat(ResourceUtils.GetStringResource("Language_Playlists"), " (", FileFormats.M3U, ")|*", FileFormats.M3U)

                    If dlg.ShowDialog Then
                        Try
                            Dim result As Integer = Await Me.collectionService.ExportPlaylistAsync(Me.SelectedPlaylists(0), dlg.FileName, False)

                            Select Case result
                                Case ExportPlaylistsResult.Success
                                    ' Do nothing
                                Case ExportPlaylistsResult.Error
                                    Me.dialogService.ShowNotification(&HE711,
                                                                       16,
                                                                       ResourceUtils.GetStringResource("Language_Error"),
                                                                       ResourceUtils.GetStringResource("Language_Error_Saving_Playlist"),
                                                                       ResourceUtils.GetStringResource("Language_Ok"),
                                                                       True,
                                                                       ResourceUtils.GetStringResource("Language_Log_File"))
                            End Select
                        Catch ex As Exception
                            LogClient.Instance.Logger.Error("Exception: {0}", ex.Message)

                            Me.dialogService.ShowNotification(&HE711,
                                                               16,
                                                               ResourceUtils.GetStringResource("Language_Error"),
                                                               ResourceUtils.GetStringResource("Language_Error_Saving_Playlist"),
                                                               ResourceUtils.GetStringResource("Language_Ok"),
                                                               True,
                                                               ResourceUtils.GetStringResource("Language_Log_File"))
                        End Try
                    End If
                Else
                    ' Should not happen
                End If
            End If
        End Function
#End Region

#Region "Overrides"
        Protected Overrides Async Function FillListsAsync() As Task

            Await Me.GetPlaylistsAsync()
        End Function

        Protected Overrides Sub UnSubscribe()

            ' Commands
            ApplicationCommands.RemoveSelectedTracksCommand.UnregisterCommand(Me.RemoveSelectedTracksCommand)
        End Sub

        Protected Overrides Sub Subscribe()

            ' Prevents subscribing twice
            Me.UnSubscribe()

            ' Commands
            ApplicationCommands.RemoveSelectedTracksCommand.RegisterCommand(Me.RemoveSelectedTracksCommand)
        End Sub

        Protected Overrides Sub RefreshLanguage()
            ' Do Nothing
        End Sub
#End Region

    End Class
End Namespace
