Imports Dopamine.Common.Services.Playback
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Prism.Mvvm

Namespace ViewModels
    Public Class NothingPlayingControlViewModel
        Inherits BindableBase

#Region "Private"
        Private mPlaybackService As IPlaybackService
#End Region

#Region "Commands"
        Public Property PlayAllCommand As DelegateCommand
        Public Property ShuffleAllCommand As DelegateCommand
#End Region

#Region "Construction"
        Public Sub New(iPlaybackService As IPlaybackService)

            mPlaybackService = iPlaybackService

            Me.PlayAllCommand = New DelegateCommand(Sub()
                                                        If mPlaybackService.Shuffle Then mPlaybackService.SetShuffle(False)
                                                        mPlaybackService.Enqueue()
                                                    End Sub)
            Me.ShuffleAllCommand = New DelegateCommand(Sub()
                                                           If Not mPlaybackService.Shuffle Then mPlaybackService.SetShuffle(True)
                                                           mPlaybackService.Enqueue()
                                                       End Sub)
        End Sub
#End Region
    End Class
End Namespace