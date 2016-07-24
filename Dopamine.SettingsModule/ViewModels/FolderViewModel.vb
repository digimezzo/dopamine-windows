Imports Microsoft.Practices.Prism.Mvvm
Imports Dopamine.Core.Database
Imports Dopamine.Core.Database.Entities

Public Class FolderViewModel
    Inherits BindableBase

#Region "Variables"
    Private mFolder As Folder
#End Region

#Region "Properties"
    Public Property Folder() As Folder
        Get
            Return mFolder
        End Get
        Set(ByVal value As Folder)
            SetProperty(Of Folder)(Me.mFolder, value)
            OnPropertyChanged(Function() Me.Path)
            OnPropertyChanged(Function() Me.Directory)
        End Set
    End Property

    Public ReadOnly Property Path() As String
        Get
            Return Me.Folder.Path
        End Get
    End Property

    Public ReadOnly Property Directory() As String
        Get
            Return System.IO.Path.GetFileName(Me.Folder.Path)
        End Get
    End Property

    Public Property ShowInCollection() As Boolean
        Get
            Return If(Me.Folder.ShowInCollection = 1, True, False)
        End Get
        Set(ByVal value As Boolean)

            If value Then
                Me.Folder.ShowInCollection = 1
            Else
                Me.Folder.ShowInCollection = 0
            End If

            OnPropertyChanged(Function() Me.ShowInCollection)
        End Set
    End Property
#End Region
End Class
