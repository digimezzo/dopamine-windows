Imports Microsoft.Practices.Prism.Mvvm

Namespace Views
    Public Class SettingsCollectionFolders
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

        Public Property ShowControls As Boolean
            Get
                Return CType(GetValue(ShowControlsProperty), Boolean)
            End Get

            Set(ByVal value As Boolean)
                SetValue(ShowControlsProperty, value)
            End Set
        End Property
#End Region

#Region "Dependency Properties"
        Public Shared ReadOnly ShowControlsProperty As DependencyProperty = DependencyProperty.Register("ShowControls", GetType(Boolean), GetType(SettingsCollectionFolders), New PropertyMetadata(Nothing))
#End Region
    End Class
End Namespace