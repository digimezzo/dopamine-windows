Imports Microsoft.Practices.Prism.Mvvm
Imports Dopamine.Common.Services.Search

Namespace ViewModels
    Public Class SearchControlViewModel
        Inherits BindableBase

#Region "Variables"
        Private mSearchText As String
        Private mSearchService As ISearchService
#End Region

#Region "Properties"
        Public Property SearchText() As String
            Get
                Return mSearchText
            End Get
            Set(ByVal value As String)
                SetProperty(Of String)(mSearchText, value)
                mSearchService.SearchText = value
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New(iSearchService As ISearchService)
            mSearchService = iSearchService

            AddHandler mSearchService.DoSearch, Sub() Me.UpdateSearchText()

            Me.UpdateSearchText()
        End Sub
#End Region

#Region "Private"
        Private Sub UpdateSearchText()
            mSearchText = mSearchService.SearchText
            OnPropertyChanged(Function() Me.SearchText)
        End Sub
#End Region
    End Class
End Namespace