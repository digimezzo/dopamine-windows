Imports System.ComponentModel

Public Class Tile
    Inherits Label

#Region "Variables"
    Private mTile As Border
#End Region

#Region "Properties"
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public Property IconSize As Double
        Get
            Return CDbl(GetValue(IconSizeProperty))
        End Get

        Set(ByVal value As Double)
            SetValue(IconSizeProperty, value)
        End Set
    End Property

    Public Property IconSizePercent As Double
        Get
            Return CDbl(GetValue(IconSizePercentProperty))
        End Get

        Set(ByVal value As Double)
            SetValue(IconSizePercentProperty, value)
        End Set
    End Property
#End Region

#Region "Dependency properties"
    Public Shared ReadOnly IconSizeProperty As DependencyProperty = DependencyProperty.Register("IconSize", GetType(Double), GetType(Tile), New PropertyMetadata(Nothing))
    Public Shared ReadOnly IconSizePercentProperty As DependencyProperty = DependencyProperty.Register("IconSizePercent", GetType(Double), GetType(Tile), New PropertyMetadata(Nothing))
#End Region

#Region "Construction"
    Shared Sub New()
        'This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
        'This style is defined in themes\generic.xaml
        DefaultStyleKeyProperty.OverrideMetadata(GetType(Tile), New FrameworkPropertyMetadata(GetType(Tile)))
    End Sub
#End Region

#Region "Functions"
    Public Overrides Sub OnApplyTemplate()
        MyBase.OnApplyTemplate()

        Me.mTile = DirectCast(GetTemplateChild("PART_Tile"), Border)

        If Me.mTile IsNot Nothing Then
            Me.SetIconSize(Me.mTile)

            AddHandler Me.mTile.SizeChanged, AddressOf SizeChangedHandler
        End If
    End Sub

    Private Sub SizeChangedHandler(sender As Object, e As SizeChangedEventArgs)
        Me.SetIconSize(CType(sender, Border))
    End Sub

    Private Sub SetIconSize(iTile As Border)
        Try
            ' For some reason, "ActualHeight" is always 0 when arriving here, so we use "Height"
            Me.IconSize = iTile.Height * (Me.IconSizePercent / 100)
        Catch ex As Exception
            Me.IconSize = 0
        End Try
    End Sub
#End Region
End Class
