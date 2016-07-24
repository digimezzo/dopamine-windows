Imports System.Windows.Media.Animation
Imports Microsoft.Practices.Prism.Mvvm

Namespace Views
    Public Class NowPlayingScreenShowcase
        Inherits UserControl
        Implements IView

#Region "Properties"
        Public Shadows Property DataContext() As Object Implements IView.DataContext
            Get
                Return MyBase.DataContext
            End Get
            Set(ByVal value As Object)
                MyBase.DataContext = value
            End Set
        End Property
#End Region

#Region "Private"
        Private Sub ResizePlaybackInfo()

            Dim resizeCoverArtStoryboard As Storyboard = Nothing

            For index = 250 To 600 Step 50
                If Me.ActualHeight >= 1.5 * index Then
                    resizeCoverArtStoryboard = TryCast(Me.PlaybackInfoPanel.Resources(String.Format("ResizeCoverArt{0}", index)), Storyboard)
                End If
            Next

            If resizeCoverArtStoryboard IsNot Nothing Then resizeCoverArtStoryboard.Begin()
        End Sub

        Private Sub UserControl_SizeChanged(sender As Object, e As SizeChangedEventArgs)

            Me.ResizePlaybackInfo()
        End Sub
#End Region
    End Class
End Namespace