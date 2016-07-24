Imports RaphaelGodart.Controls
Imports Dopamine.Core.Base

Public Class DopamineWindow
    Inherits GlowingBorderlessWindow

#Region "Properties"
    Public Property Accent As Brush
        Get
            Return CType(GetValue(AccentProperty), Brush)
        End Get

        Set(ByVal value As Brush)
            SetValue(AccentProperty, value)
        End Set
    End Property

    Public Property ButtonWidth As Double
        Get
            Return CType(GetValue(ButtonWidthProperty), Double)
        End Get

        Set(ByVal value As Double)
            SetValue(ButtonWidthProperty, value)
        End Set
    End Property
#End Region

#Region "Dependency Properties"
    Public Shared ReadOnly AccentProperty As DependencyProperty =
                        DependencyProperty.Register("Accent",
                        GetType(Brush), GetType(DopamineWindow),
                        New PropertyMetadata(Nothing))

    Public Shared ReadOnly ButtonWidthProperty As DependencyProperty =
                       DependencyProperty.Register("ButtonWidth",
                       GetType(Double), GetType(DopamineWindow),
                       New PropertyMetadata(Nothing))
#End Region

#Region "Construction"
    Shared Sub New()

        'This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
        'This style is defined in themes\generic.xaml
        DefaultStyleKeyProperty.OverrideMetadata(GetType(DopamineWindow), New FrameworkPropertyMetadata(GetType(DopamineWindow)))
    End Sub
#End Region

#Region "Public"
    Public Overrides Sub OnApplyTemplate()

        MyBase.OnApplyTemplate()

        Me.ButtonWidth = Constants.DefaultWindowButtonWidth
        Me.TitleBarHeight = Convert.ToInt32(Constants.DefaultWindowButtonHeight)
    End Sub

#End Region
End Class