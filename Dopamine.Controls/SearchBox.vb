Public Class SearchBox
    Inherits TextBox

#Region "Variables"
    Private mSearchIconCross As Viewbox
    Private mSearchIconGlass As Viewbox
    Private mSearchButton As Label
#End Region

#Region "Properties"
    Public Property HasText As Boolean
        Get
            Return CBool(GetValue(HasTextProperty))
        End Get

        Set(ByVal value As Boolean)
            SetValue(HasTextProperty, value)
        End Set
    End Property

    Public Property HasFocus As Boolean
        Get
            Return CBool(GetValue(HasFocusProperty))
        End Get

        Set(ByVal value As Boolean)
            SetValue(HasFocusProperty, value)
        End Set
    End Property

    Public Property Accent As Brush
        Get
            Return CType(GetValue(AccentProperty), Brush)
        End Get

        Set(ByVal value As Brush)
            SetValue(AccentProperty, value)
        End Set
    End Property

    Public Property SearchGlassForeground As Brush
        Get
            Return CType(GetValue(SearchGlassForegroundProperty), Brush)
        End Get

        Set(ByVal value As Brush)
            SetValue(SearchGlassForegroundProperty, value)
        End Set
    End Property
#End Region

#Region "Dependency Properties"
    Public Shared ReadOnly HasTextProperty As DependencyProperty = DependencyProperty.Register("HasText", GetType(Boolean), GetType(SearchBox), New PropertyMetadata(False))
    Public Shared ReadOnly HasFocusProperty As DependencyProperty = DependencyProperty.Register("HasFocus", GetType(Boolean), GetType(SearchBox), New PropertyMetadata(False))
    Public Shared ReadOnly AccentProperty As DependencyProperty = DependencyProperty.Register("Accent", GetType(Brush), GetType(SearchBox), New PropertyMetadata(Nothing))
    Public Shared ReadOnly SearchGlassForegroundProperty As DependencyProperty = DependencyProperty.Register("SearchGlassForeground", GetType(Brush), GetType(SearchBox), New PropertyMetadata(Nothing))
#End Region
#Region "Construction"
    Shared Sub New()
        'This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
        'This style is defined in themes\generic.xaml
        DefaultStyleKeyProperty.OverrideMetadata(GetType(SearchBox), New FrameworkPropertyMetadata(GetType(SearchBox)))
    End Sub
#End Region

#Region "Functions"
    Public Overrides Sub OnApplyTemplate()
        MyBase.OnApplyTemplate()

        Me.mSearchIconCross = DirectCast(GetTemplateChild("PART_SearchIconCross"), Viewbox)
        Me.mSearchIconGlass = DirectCast(GetTemplateChild("PART_SearchIconGlass"), Viewbox)
        Me.mSearchButton = DirectCast(GetTemplateChild("PART_SearchButton"), Label)

        If Me.mSearchButton IsNot Nothing Then
            AddHandler Me.mSearchButton.MouseLeftButtonUp, AddressOf SearchButton_MouseLeftButtonUphandler
        End If

        Me.SetButtonState()
    End Sub

    Private Sub SetButtonState()

        If Me.mSearchIconCross IsNot Nothing AndAlso Me.mSearchIconGlass IsNot Nothing Then

            If Me.HasText Then
                Me.mSearchIconCross.Visibility = Windows.Visibility.Visible
                Me.mSearchIconGlass.Visibility = Windows.Visibility.Collapsed
            Else
                Me.mSearchIconCross.Visibility = Windows.Visibility.Collapsed
                Me.mSearchIconGlass.Visibility = Windows.Visibility.Visible
            End If
        End If
    End Sub
#End Region

#Region "Event handlers"
    Protected Overrides Sub OnTextChanged(e As TextChangedEventArgs)
        MyBase.OnTextChanged(e)

        Me.HasText = Text.Length > 0
        Me.SetButtonState()
    End Sub

    Private Sub SearchButton_MouseLeftButtonUphandler(sender As Object, e As MouseButtonEventArgs)

        If Me.HasText Then
            Me.Text = String.Empty
        End If
        Me.SetButtonState()
    End Sub
#End Region
End Class