Imports Dopamine.Core.Utils
Imports Dopamine.Core.Base
Imports Dopamine.Common.Services.Notification
Imports Dopamine.Core.Settings
Imports Dopamine.Core

Namespace Views
    Public Class TrayControls
        Inherits Window

#Region "Variables"
        Private mNotificationService As INotificationService
#End Region

#Region "Construction"
        Public Sub New(iNotificationService As INotificationService)

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            mNotificationService = iNotificationService
        End Sub
#End Region

#Region "Public"
        Public Overloads Sub Show()

            mNotificationService.HideNotification() ' If a notification is shown, hide it.

            MyBase.Show()

            Me.SetTransparency()
            Me.SetGeometry()

            Me.Topmost = True ' this window should always be on top of all others

            ' This is important so Deactivated is called even when the window was never clicked
            ' (When a maual activate is not triggered, Deactivated doesn't get called when
            ' clicking outside the window)
            Me.Activate()
        End Sub
#End Region

#Region "Private"
        Private Sub Window_Deactivated(sender As Object, e As EventArgs)
            ' Closes this window when the mouse is clicked outside it
            Me.Hide()
        End Sub

        Private Sub SetTransparency()

            If EnvironmentUtils.IsWindows10 AndAlso XmlSettingsClient.Instance.Get(Of Boolean)("Appearance", "EnableTransparency") Then
                Me.WindowBackground.Opacity = Constants.OpacityWhenBlurred
                WindowUtils.EnableBlur(Me)
            Else
                Me.WindowBackground.Opacity = 1.0
            End If
        End Sub

        Private Sub SetGeometry()

            Dim desktopWorkingArea As Rect = System.Windows.SystemParameters.WorkArea

            Me.Left = desktopWorkingArea.Right - Constants.TrayControlsWidth - 5
            Me.Top = desktopWorkingArea.Bottom - Constants.TrayControlsHeight - 5
        End Sub
#End Region
    End Class
End Namespace