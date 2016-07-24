Imports Dopamine.Common
Imports Dopamine.Core.Utils
Imports Microsoft.Practices.Prism.Commands
Imports Dopamine.Core.Prism
Imports Dopamine.Core.Settings
Imports Dopamine.Common.Presentation.Views
Imports Dopamine.Common.Presentation.ViewModels
Imports Dopamine.Core

Namespace Views
    Public Class CommonMiniPlayerView
        Inherits CommonTracksView

#Region "Variables"
        Private mIsMiniPlayerPositionLocked As Boolean
#End Region

#Region "Commands"
        Public Property ToggleMiniPlayerPositionLockedCommand As DelegateCommand
#End Region

#Region "Construction"
        Public Sub New()

            MyBase.New()

            Me.ToggleMiniPlayerPositionLockedCommand = New DelegateCommand(Sub() mIsMiniPlayerPositionLocked = Not mIsMiniPlayerPositionLocked)
            ApplicationCommands.ToggleMiniPlayerPositionLockedCommand.RegisterCommand(Me.ToggleMiniPlayerPositionLockedCommand)

            mIsMiniPlayerPositionLocked = XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "MiniPlayerPositionLocked")
        End Sub
#End Region

#Region "Protected"
        Protected Sub MouseLeftButtonDownHandler(sender As Object, e As MouseButtonEventArgs)

            If Not mIsMiniPlayerPositionLocked Then
                ' We need to use a custom function because the built-in DragMove function causes 
                ' flickering when blurring the cover art and releasing the mouse button after a drag.
                If e.ClickCount = 1 Then
                    WindowUtils.MoveWindow(Application.Current.MainWindow)
                End If
            End If
        End Sub
#End Region
    End Class
End Namespace