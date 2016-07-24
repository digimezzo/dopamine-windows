Imports Microsoft.Practices.Prism.Mvvm

Namespace Views
    Public Class SettingsBehaviour
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