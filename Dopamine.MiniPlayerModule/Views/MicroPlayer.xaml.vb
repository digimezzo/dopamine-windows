Imports Dopamine.Common
Imports Dopamine.Core.Utils
Imports Microsoft.Practices.Prism.PubSubEvents
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Prism.Regions
Imports Dopamine.Core.Prism

Namespace Views
    Public Class MicroPlayer
        Inherits CommonMiniPlayerView

#Region "Private"
        Private Sub CoverGrid_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)

            Me.MouseLeftButtonDownHandler(sender, e)
        End Sub
#End Region
    End Class
End Namespace