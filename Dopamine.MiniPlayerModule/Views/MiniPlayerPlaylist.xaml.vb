Imports Dopamine.Common.Presentation.ViewModels
Imports Dopamine.Common.Presentation.Views
Imports Dopamine.Core
Imports Dopamine.Core.Logging
Imports Dopamine.Core.Prism
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Prism.PubSubEvents
Imports Microsoft.Practices.Prism.Regions

Namespace Views
    Public Class MiniPlayerPlaylist
        Inherits CommonTracksView
        Implements INavigationAware

#Region "Variables"
        Private scrollToPlayingTrackToken As SubscriptionToken
#End Region

#Region "Construction"
        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            Me.ViewInExplorerCommand = New DelegateCommand(Sub() Me.ViewInExplorer(Me.ListBoxTracks))
            Me.JumpToPlayingTrackCommand = New DelegateCommand(Sub() Me.ScrollToPlayingTrackAsync(Me.ListBoxTracks))

            ' Events and Commands
            Me.Subscribe()
        End Sub
#End Region

#Region "Private"
        Private Async Sub ListBoxTracks_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)

            Try
                Dim lb As ListBox = CType(sender, ListBox)

                If lb.SelectedItem Is Nothing Then Return

                Await Me.playBackService.PlaySelectedAsync(CType(lb.SelectedItem, TrackInfoViewModel).TrackInfo)
            Catch ex As Exception
                LogClient.Instance.Logger.Error("Error while handling ListBox action. Exception: {0}", ex.Message)
            End Try
        End Sub

        Private Sub ListBoxTracks_KeyUp(sender As Object, e As KeyEventArgs)
            Me.TracksKeyUpHandlerAsync(sender, e)
        End Sub

        Private Sub Subscribe()
            Me.Unsubscribe()

            scrollToPlayingTrackToken = Me.eventAggregator.GetEvent(Of ScrollToPlayingTrack)().Subscribe(Async Sub() Await Me.ScrollToPlayingTrackAsync(Me.ListBoxTracks))
        End Sub

        Private Sub Unsubscribe()
            Me.eventAggregator.GetEvent(Of ScrollToPlayingTrack)().Unsubscribe(scrollToPlayingTrackToken)
        End Sub

        Private Sub ListBoxTracks_PreviewKeyDown(sender As Object, e As KeyEventArgs)

            If e.Key = Key.Enter Then
                Me.ListActionHandler(sender)
            End If
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
        End Sub
#End Region
    End Class
End Namespace