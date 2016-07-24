Imports Microsoft.Practices.Prism.Mvvm
Imports Dopamine.Controls
Imports System.Windows.Media.Animation

Namespace Views
    Public Class MainScreen
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
    End Class
End Namespace
