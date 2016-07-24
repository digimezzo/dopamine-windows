Imports Dopamine.Common.Presentation.ViewModels
Imports Dopamine.Core.Base

Namespace ViewModels
    Public Class NowPlayingScreenListViewModel
        Inherits NowPlayingViewModel

#Region "Construction"
        Public Sub New()
            MyBase.New()
        End Sub
#End Region

#Region "Overrides"
        Protected Overrides Async Function LoadedCommandAsync() As Task

            If Me.isFirstLoad Then
                Me.isFirstLoad = False

                Await Task.Delay(Constants.NowPlayingListLoadDelay)  ' Wait for the UI to slide in
                Await Me.FillListsAsync() ' Fill all the lists
            End If
        End Function
#End Region
    End Class
End Namespace