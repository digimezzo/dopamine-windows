''' <summary>
''' DataGrid which propagates its DataContext to its Columns
''' </summary>
''' <remarks></remarks>
Public Class DataGridEx
    Inherits DataGrid

    Protected Overrides Sub OnInitialized(e As EventArgs)
        MyBase.OnInitialized(e)

        If Me.DataContext IsNot Nothing AndAlso Me.Columns.Count > 0 Then
            For Each col As DataGridColumn In Me.Columns
                col.SetValue(FrameworkElement.DataContextProperty, Me.DataContext)
            Next
        End If
    End Sub
End Class
