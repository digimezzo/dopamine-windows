Imports Microsoft.Practices.Prism.Mvvm

Namespace Views
    Public Class FullPlayer
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

        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
        End Sub
    End Class
End Namespace
