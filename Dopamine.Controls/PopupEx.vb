Imports System.Windows.Controls.Primitives

Public Class PopupEx
    Inherits Popup

#Region "Variables"
    Private Shared mFi As Reflection.FieldInfo = GetType(SystemParameters).GetField("_menuDropAlignment", CType(Reflection.BindingFlags.NonPublic + Reflection.BindingFlags.Static, Reflection.BindingFlags))
#End Region

#Region "Public"
    Public Sub Open()
        If SystemParameters.MenuDropAlignment Then
            mFi.SetValue(Nothing, False)
            IsOpen = True
            mFi.SetValue(Nothing, True)
        Else
            IsOpen = True
        End If
    End Sub

    Public Sub Close()
        IsOpen = False
    End Sub
#End Region
End Class
