Public Class IconTextButton
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
    Public Shared ReadOnly DataProperty As DependencyProperty = DependencyProperty.Register("Data", GetType(Geometry), GetType(IconTextButton), New PropertyMetadata(Nothing))
#End Region

#Region "Construction"
    Shared Sub New()
        'This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
        'This style is defined in themes\generic.xaml
        DefaultStyleKeyProperty.OverrideMetadata(GetType(IconTextButton), New FrameworkPropertyMetadata(GetType(IconTextButton)))
    End Sub
#End Region
End Class
