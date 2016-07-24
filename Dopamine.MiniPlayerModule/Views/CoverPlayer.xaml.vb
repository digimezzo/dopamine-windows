Imports Microsoft.Practices.Prism.Mvvm
Imports Dopamine.Core.Logging
Imports Dopamine.Core.Prism
Imports Microsoft.Practices.ServiceLocation
Imports Microsoft.Practices.Prism.PubSubEvents
Imports Microsoft.Practices.Prism.Commands
Imports Dopamine.Core.IO
Imports Dopamine.MiniPlayerModule.ViewModels
Imports Dopamine.Core.Utils
Imports Dopamine.Core.Settings
Imports Dopamine.Common
Imports System.Runtime.InteropServices
Imports Microsoft.Practices.Prism.Regions

Namespace Views
    Public Class CoverPlayer
        Inherits CommonMiniPlayerView

#Region "Private"
        Protected Sub CoverGrid_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)

            Me.MouseLeftButtonDownHandler(sender, e)
        End Sub
#End Region
    End Class
End Namespace
