Imports Microsoft.Practices.Prism.Mvvm

Namespace Views
    Public Class Collection
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
        Private Sub UserControl_SizeChanged(sender As Object, e As SizeChangedEventArgs)

            ' This makes sure the spectrum analyzer is centered on the screen, based on the left pixel.
            ' When we align center, alignment is sometimes (depending on the width of the screen) done
            ' on a half pixel. This causes a blurry spectrum analyzer.
            Try
                CollectionSpectrumAnalyzerRegion.Margin = New Thickness(Convert.ToInt32(Me.ActualWidth / 2) - Convert.ToInt32(CollectionSpectrumAnalyzerRegion.ActualWidth / 2), 0, 0, 0)
            Catch ex As Exception
                ' Swallow this exception
            End Try
        End Sub
#End Region
    End Class
End Namespace
