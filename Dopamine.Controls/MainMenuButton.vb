Public Class MainMenuButton
    Inherits RadioButton

#Region "Construction"
    Shared Sub New()
        'This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
        'This style is defined in themes\generic.xaml
        DefaultStyleKeyProperty.OverrideMetadata(GetType(MainMenuButton), New FrameworkPropertyMetadata(GetType(MainMenuButton)))
    End Sub
#End Region
End Class
