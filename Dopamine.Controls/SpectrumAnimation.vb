Imports System.Windows.Media.Animation
Imports System.ComponentModel

Public Class SpectrumAnimation
    Inherits Control

#Region "Private"
    Private mSpectrumPanel As StackPanel
    Private mSpectrumStoryBoard As Storyboard
#End Region

#Region "Properties"
    Public Property Accent As Brush
        Get
            Return CType(GetValue(AccentProperty), Brush)
        End Get

        Set(ByVal value As Brush)
            SetValue(AccentProperty, value)
        End Set
    End Property

    Public Property IsActive As Boolean
        Get
            Return CType(GetValue(IsActiveProperty), Boolean)
        End Get

        Set(ByVal value As Boolean)
            SetValue(IsActiveProperty, value)
        End Set
    End Property

    Public Property IsDisplayed As Boolean
        Get
            Return CType(GetValue(IsDisplayedProperty), Boolean)
        End Get

        Set(ByVal value As Boolean)
            SetValue(IsDisplayedProperty, value)
        End Set
    End Property
#End Region

#Region "Dependency Properties"
    Public Shared ReadOnly AccentProperty As DependencyProperty =
                      DependencyProperty.Register("Accent",
                      GetType(Brush), GetType(SpectrumAnimation),
                      New PropertyMetadata(TryCast(New BrushConverter().ConvertFromString("#1D7DD4"), SolidColorBrush)))

    Public Shared ReadOnly IsActiveProperty As DependencyProperty =
                      DependencyProperty.Register("IsActive",
                      GetType(Boolean), GetType(SpectrumAnimation),
                      New PropertyMetadata(Nothing))

    Public Shared ReadOnly IsDisplayedProperty As DependencyProperty =
                      DependencyProperty.Register("IsDisplayed",
                      GetType(Boolean), GetType(SpectrumAnimation),
                      New PropertyMetadata(Nothing))
#End Region

#Region "Construction"
    Shared Sub New()
        'This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
        'This style is defined in themes\generic.xaml
        DefaultStyleKeyProperty.OverrideMetadata(GetType(SpectrumAnimation), New FrameworkPropertyMetadata(GetType(SpectrumAnimation)))
    End Sub
#End Region

#Region "Public"
    Public Overrides Sub OnApplyTemplate()
        MyBase.OnApplyTemplate()

        mSpectrumPanel = DirectCast(GetTemplateChild("SpectrumPanel"), StackPanel)
        mSpectrumStoryBoard = CType(mSpectrumPanel.TryFindResource("SpectrumStoryBoard"), Storyboard)

        Dim d1 As DependencyPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(SpectrumAnimation.IsActiveProperty, GetType(SpectrumAnimation))
        d1.AddValueChanged(Me, New EventHandler(Sub() Me.ToggleAnimation()))

        Dim d2 As DependencyPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(SpectrumAnimation.IsDisplayedProperty, GetType(SpectrumAnimation))
        d2.AddValueChanged(Me, New EventHandler(Sub() Me.ToggleAnimation()))

        Me.ToggleAnimation()
    End Sub
#End Region

#Region "Private"
    Private Sub ToggleAnimation()

        If Me.IsDisplayed Then
            If Me.IsActive Then
                mSpectrumStoryBoard.Begin()
            Else
                mSpectrumStoryBoard.Pause()
            End If
        Else
            mSpectrumStoryBoard.Stop()
        End If
    End Sub
#End Region
End Class
