Imports Microsoft.Practices.Prism.Mvvm
Imports Dopamine.Core.Settings
Imports Microsoft.Practices.Prism.PubSubEvents
Imports Microsoft.Practices.Prism.Commands
Imports Dopamine.Core.Prism
Imports Dopamine.Core.Database
Imports Dopamine.Core.Logging
Imports Dopamine.Core.IO
Imports Microsoft.Practices.ServiceLocation
Imports Dopamine.CollectionModule.ViewModels
Imports Dopamine.Core.Utils
Imports Dopamine.Common
Imports Microsoft.Practices.Prism.Regions
Imports Dopamine.Core
Imports Dopamine.Common.Presentation.Views
Imports Dopamine.Common.Presentation.Extensions
Imports Dopamine.Core.Base

Namespace Views
    Public Class CollectionAlbums
        Inherits CommonTracksView
        Implements INavigationAware

#Region "Variables"
        Private mScrollToPlayingTrackToken As SubscriptionToken
        Private mShellSizeChangedToken As SubscriptionToken
#End Region

#Region "Construction"
        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

            ' Commands
            Me.ViewInExplorerCommand = New DelegateCommand(Sub() Me.ViewInExplorer(Me.ListBoxTracks))
            Me.JumpToPlayingTrackCommand = New DelegateCommand(Async Sub() Await Me.ScrollToPlayingTrackAsync(Me.ListBoxTracks))

            ' Events and Commands
            Me.Subscribe()
        End Sub
#End Region

#Region "Private"
        Private Async Sub ListBoxAlbums_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)

            Await Me.ListActionHandler(sender)
        End Sub

        Private Async Sub ListBoxAlbums_PreviewKeyDown(sender As Object, e As KeyEventArgs)

            If e.Key = Key.Enter Then
                Await Me.ListActionHandler(sender)
            End If
        End Sub

        Private Async Sub ListBoxTracks_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)

            Await Me.ListActionHandler(sender)
        End Sub

        Private Async Sub ListBoxTracks_PreviewKeyDown(sender As Object, e As KeyEventArgs)

            If e.Key = Key.Enter Then
                Await Me.ListActionHandler(sender)
            End If
        End Sub

        Private Async Sub ListBoxTracks_KeyUp(sender As Object, e As KeyEventArgs)
            Await Me.TracksKeyUpHandlerAsync(sender, e)
        End Sub

        Private Sub AlbumsButton_Click(sender As Object, e As RoutedEventArgs)
            Me.ListBoxAlbums.SelectedItem = Nothing
        End Sub

        Private Sub Unsubscribe()

            ' Events
            Me.eventAggregator.GetEvent(Of ScrollToPlayingTrack)().Unsubscribe(mScrollToPlayingTrackToken)
        End Sub

        Private Sub Subscribe()

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
            XmlSettingsClient.Instance.Set(Of Integer)("FullPlayer", "SelectedPage", SelectedPage.Albums)
        End Sub
#End Region
    End Class
End Namespace