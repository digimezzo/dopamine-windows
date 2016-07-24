Public Class IconButton
    Inherits Button

#Region "Properties"
    Public Property Data As Geometry
        Get
            Return CType(GetValue(DataProperty), Geometry)
        End Get

        Set(ByVal value As Geometry)
            SetValue(DataProperty, value)
        End Set
    End Property
#End Region

#Region "Dependency Properties"
    Public Shared ReadOnly DataProperty As DependencyProperty = DependencyProperty.Register("Data", GetType(Geometry), GetType(IconButton), New PropertyMetadata(Nothing))
#End Region

#Region "Construction"
    Shared Sub New()
        'This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
        'This style is defined in themes\generic.xaml
        DefaultStyleKeyProperty.OverrideMetadata(GetType(IconButton), New FrameworkPropertyMetadata(GetType(IconButton)))
    End Sub
#End Region
End Class
