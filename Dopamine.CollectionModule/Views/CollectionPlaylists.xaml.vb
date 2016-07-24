Imports Dopamine.Common.Presentation.Views
Imports Dopamine.Core.Prism
Imports Dopamine.Core.Settings
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Prism.PubSubEvents
Imports Microsoft.Practices.Prism.Regions

Namespace Views
    Public Class CollectionPlaylists
        Inherits CommonTracksView
        Implements INavigationAware

#Region "Variables"
        Private mScrollToPlayingTrackToken As SubscriptionToken
#End Region

#Region "Construction"
        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

            ' Commands
            Me.ViewInExplorerCommand = New DelegateCommand(Sub() Me.ViewInExplorer(Me.ListBoxTracks))
            Me.JumpToPlayingTrackCommand = New DelegateCommand(Sub() Me.ScrollToPlayingTrackAsync(Me.ListBoxTracks))

            ' Events and Commands
            Me.Subscribe()
        End Sub
#End Region

#Region "Private"
        Private Sub PlaylistsKeyUpHandlerAsync(sender As Object, e As KeyEventArgs)

            If e.Key = Key.F2 Then
                If Me.ListBoxPlaylists.SelectedItem IsNot Nothing Then
                    Me.eventAggregator.GetEvent(Of RenameSelectedPlaylistWithKeyF2).Publish(String.Empty)
                End If
            ElseIf e.Key = Key.Delete Then
                If Me.ListBoxPlaylists.SelectedItem IsNot Nothing Then
                    Me.eventAggregator.GetEvent(Of DeleteSelectedPlaylistsWithKeyDelete).Publish(String.Empty)
                End If
            End If
        End Sub

        Private Async Sub ListBoxPlaylists_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)

            Await Me.ListActionHandler(sender)
        End Sub

        Private Async Sub ListBoxPlaylists_PreviewKeyDown(sender As Object, e As KeyEventArgs)

            If e.Key = Key.Enter Then
                Await Me.ListActionHandler(sender)
            End If
        End Sub

        Private Sub ListBoxPlaylists_KeyUp(sender As Object, e As KeyEventArgs)
            Me.PlaylistsKeyUpHandlerAsync(sender, e)
        End Sub

        Private Async Sub ListBoxTracks_KeyUp(sender As Object, e As KeyEventArgs)
            Await Me.TracksKeyUpHandlerAsync(sender, e)
        End Sub

        Private Async Sub ListBoxTracks_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
            Await Me.ListActionHandler(sender)
        End Sub

        Private Async Sub ListBoxTracks_PreviewKeyDown(sender As Object, e As KeyEventArgs)

            If e.Key = Key.Enter Then
                Await Me.ListActionHandler(sender)
            End If
        End Sub

        Private Sub TracksButton_Click(sender As Object, e As RoutedEventArgs)
            ' TODO: what to do here?
        End Sub

        Private Sub PlaylistsButton_Click(sender As Object, e As RoutedEventArgs)
            ' TODO: what to do here?
        End Sub


        Private Sub Unsubscribe()

            ' Events
            Me.eventAggregator.GetEvent(Of ScrollToPlayingTrack)().Unsubscribe(mScrollToPlayingTrackToken)
        End Sub

        Private Sub Subscribe()

            ' Prevents subscribing twice
            Me.Unsubscribe()

            ' Events
            mScrollToPlayingTrackToken = Me.eventAggregator.GetEvent(Of ScrollToPlayingTrack)().Subscribe(Async Sub() Await Me.ScrollToPlayingTrackAsync(Me.ListBoxTracks))
        End Sub
#End Region

#Region "INavigationAware"
        Public Function IsNavigationTarget(navigationContext As NavigationContext) As Boolean Implements INavigationAware.IsNavigationTarget
            Return True
        End Function

        Public Sub OnNavigatedFrom(navigationContext As NavigationContext) Implements INavigationAware.OnNavigatedFrom
            Me.Unsubscribe()
        End Sub

        Public Sub OnNavigatedTo(navigationContext As NavigationContext) Implements INavigationAware.OnNavigatedTo
            Me.Subscribe()
            XmlSettingsClient.Instance.Set(Of Integer)("FullPlayer", "SelectedPage", SelectedPage.Playlists)
        End Sub
#End Region
    End Class
End Namespace