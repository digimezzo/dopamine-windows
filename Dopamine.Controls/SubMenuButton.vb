Public Class SubMenuButton
    Inherits RadioButton

#Region "Construction"
    Shared Sub New()
        'This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
        'This style is defined in themes\generic.xaml
        DefaultStyleKeyProperty.OverrideMetadata(GetType(SubMenuButton), New FrameworkPropertyMetadata(GetType(SubMenuButton)))
    End Sub
#End Region
End Class