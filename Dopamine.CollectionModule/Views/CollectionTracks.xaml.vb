Imports Dopamine.Common.Presentation.Utils
Imports Dopamine.Common.Presentation.ViewModels
Imports Dopamine.Common.Presentation.Views
Imports Dopamine.Core.Base
Imports Dopamine.Core.IO
Imports Dopamine.Core.Logging
Imports Dopamine.Core.Prism
Imports Dopamine.Core.Settings
Imports Dopamine.Core.Utils
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Prism.PubSubEvents
Imports Microsoft.Practices.Prism.Regions

Namespace Views
    Public Class CollectionTracks
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

            ' Commands
            Me.ViewInExplorerCommand = New DelegateCommand(Sub() Me.ViewInExplorer(Me.DataGridTracks))
            Me.JumpToPlayingTrackCommand = New DelegateCommand(Async Sub() Await Me.ScrollToPlayingTrackAsync(Me.DataGridTracks))

            ' Events and Commands
            Me.Subscribe()
        End Sub
#End Region

#Region "Private"
        Private Async Sub DataGridTracks_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)

            Await Me.DataGridActionHandler(sender)
        End Sub

        Private Overloads Async Function ScrollToPlayingTrackAsync(iDataGrid As DataGrid) As Task

            ' This should provide a smoother experience because after this wait,
            ' other animations on the UI should have finished executing.
            Await Task.Delay(Convert.ToInt32(Constants.ScrollToPlayingTrackTimeoutSeconds * 1000))

            Try
                Await Application.Current.Dispatcher.Invoke(Async Function()
                                                                Await ScrollUtils.ScrollToPlayingTrackAsync(iDataGrid)
                                                            End Function)
            Catch ex As Exception
                LogClient.Instance.Logger.Error("Could not scroll to the playing track. Exception: {1}", ex.Message)
            End Try
        End Function

        Private Overloads Sub ViewInExplorer(iDataGrid As DataGrid)

            If iDataGrid.SelectedItem IsNot Nothing Then
                Try
                    Actions.TryViewInExplorer(CType(iDataGrid.SelectedItem, TrackInfoViewModel).TrackInfo.Track.Path)
                Catch ex As Exception
                    LogClient.Instance.Logger.Error("Could not view Track in Windows Explorer. Exception: {0}", ex.Message)
                End Try
            End If
        End Sub

        Private Async Sub DataGridTracks_KeyUp(sender As Object, e As KeyEventArgs)
            Await Me.TracksKeyUpHandlerAsync(sender, e)
        End Sub

        Private Async Sub DataGridTracks_PreviewKeyDown(sender As Object, e As KeyEventArgs)

            If e.Key = Key.Enter Then

                e.Handled = True ' Prevent DataGrid.KeyDown to make the selection to go to the next row when pressing Enter

                ' Makes sure that this action is triggered by a DataGridCell. This prevents  
                ' enqueuing when clicking other ListBox elements (e.g. the ScrollBar)
                Dim dataGridCell As DataGridCell = VisualTreeUtils.FindAncestor(Of DataGridCell)(DirectCast(e.OriginalSource, DependencyObject))

                If dataGridCell Is Nothing Then Return

                Dim dg As DataGrid = CType(sender, DataGrid)
                Await Me.playBackService.Enqueue(dg.Items.OfType(Of TrackInfoViewModel)().ToList().Select(Function(tivm) tivm.TrackInfo).ToList, CType(dg.SelectedItem, TrackInfoViewModel).TrackInfo)
            End If
        End Sub

        Private Sub Unsubscribe()

            ' Events
            Me.eventAggregator.GetEvent(Of ScrollToPlayingTrack)().Unsubscribe(scrollToPlayingTrackToken)
        End Sub

        Private Sub Subscribe()

            ' Prevents subscribing twice
            Me.Unsubscribe()

            ' Events
            scrollToPlayingTrackToken = Me.eventAggregator.GetEvent(Of ScrollToPlayingTrack)().Subscribe(Async Sub() Await Me.ScrollToPlayingTrackAsync(Me.DataGridTracks))
        End Sub
#End Region

#Region "Protected"
        Protected Overloads Async Function TracksKeyUpHandlerAsync(sender As Object, e As KeyEventArgs) As Task

            Dim dg As DataGrid = CType(sender, DataGrid)

            If e.Key = Key.J AndAlso Keyboard.Modifiers = ModifierKeys.Control Then
                Await Me.ScrollToPlayingTrackAsync(dg)
            ElseIf e.Key = Key.E AndAlso Keyboard.Modifiers = ModifierKeys.Control Then

                If dg.SelectedItem IsNot Nothing Then
                    Actions.TryViewInExplorer(CType(dg.SelectedItem, TrackInfoViewModel).TrackInfo.Track.Path)
                End If
            ElseIf e.Key = Key.Delete Then
                ApplicationCommands.RemoveSelectedTracksCommand.Execute(Nothing)
            End If
        End Function
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
            XmlSettingsClient.Instance.Set(Of Integer)("FullPlayer", "SelectedPage", SelectedPage.Tracks)
        End Sub
#End Region
    End Class
End Namespace