Namespace Views
    Public Class CoverPictureWindowControls
        Inherits UserControl

#Region "Properties"
        Public Property ButtonWidth As Double
            Get
                Return CType(GetValue(ButtonWidthProperty), Double)
            End Get

            Set(ByVal value As Double)
                SetValue(ButtonWidthProperty, value)
            End Set
        End Property

        Public Property ButtonHeight As Double
            Get
                Return CType(GetValue(ButtonHeightProperty), Double)
            End Get

            Set(ByVal value As Double)
                SetValue(ButtonHeightProperty, value)
            End Set
        End Property
#End Region

#Region "Dependency Properties"
        Public Shared ReadOnly ButtonWidthProperty As DependencyProperty = DependencyProperty.Register("ButtonWidth", GetType(Double), GetType(CoverPictureWindowControls), New PropertyMetadata(Nothing))
        Public Shared ReadOnly ButtonHeightProperty As DependencyProperty = DependencyProperty.Register("ButtonHeight", GetType(Double), GetType(CoverPictureWindowControls), New PropertyMetadata(Nothing))
#End Region
    End Class
End Namespace