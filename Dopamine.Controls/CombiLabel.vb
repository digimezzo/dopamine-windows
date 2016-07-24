Public Class CombiLabel
    Inherits Label

#Region "Dependency Properties"
    Public Shared ReadOnly FontSize2Property As DependencyProperty = DependencyProperty.Register("FontSize2", GetType(Integer), GetType(CombiLabel), New PropertyMetadata(Nothing))
    Public Shared ReadOnly FontWeight2Property As DependencyProperty = DependencyProperty.Register("FontWeight2", GetType(FontWeight), GetType(CombiLabel), New PropertyMetadata(Nothing))
    Public Shared ReadOnly FontStyle2Property As DependencyProperty = DependencyProperty.Register("FontStyle2", GetType(FontStyle), GetType(CombiLabel), New PropertyMetadata(Nothing))
    Public Shared ReadOnly Content2Property As DependencyProperty = DependencyProperty.Register("Content2", GetType(String), GetType(CombiLabel), New PropertyMetadata(Nothing))
    Public Shared ReadOnly Foreground2Property As DependencyProperty = DependencyProperty.Register("Foreground2", GetType(Brush), GetType(CombiLabel), New PropertyMetadata(Nothing))
#End Region

#Region "Properties"
    Public Property FontSize2 As Integer
        Get
            Return CInt(GetValue(FontSize2Property))
        End Get

        Set(ByVal value As Integer)
            SetValue(FontSize2Property, value)
        End Set
    End Property

    Public Property FontWeight2 As FontWeight
        Get
            Return CType(GetValue(FontWeight2Property), FontWeight)
        End Get

        Set(ByVal value As FontWeight)
            SetValue(FontWeight2Property, value)
        End Set
    End Property

    Public Property FontStyle2 As FontStyle
        Get
            Return CType(GetValue(FontStyle2Property), FontStyle)
        End Get

        Set(ByVal value As FontStyle)
            SetValue(FontStyle2Property, value)
        End Set
    End Property

    Public Property Content2 As String
        Get
            Return CStr(GetValue(Content2Property))
        End Get

        Set(ByVal value As String)
            SetValue(Content2Property, value)
        End Set
    End Property

    Public Property Foreground2 As Brush
        Get
            Return CType(GetValue(Foreground2Property), Brush)
        End Get

        Set(ByVal value As Brush)
            SetValue(Foreground2Property, value)
        End Set
    End Property
#End Region

#Region "Construction"
    Shared Sub New()
        'This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
        'This style is defined in themes\generic.xaml
        DefaultStyleKeyProperty.OverrideMetadata(GetType(CombiLabel), New FrameworkPropertyMetadata(GetType(CombiLabel)))
    End Sub
#End Region
End Class
