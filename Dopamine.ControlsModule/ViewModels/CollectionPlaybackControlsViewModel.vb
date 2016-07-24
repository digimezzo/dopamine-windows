Imports Microsoft.Practices.Prism.Mvvm
Imports Dopamine.Core.Utils
Imports Microsoft.Practices.Prism.PubSubEvents
Imports Dopamine.Core.Prism
Imports Dopamine.Common.Services.Collection
Imports Dopamine.Common.Services.Playback

Namespace ViewModels
    Public Class CollectionPlaybackControlsViewModel
        Inherits BindableBase

#Region "Variables"
        Private mPlaybackService As IPlaybackService
#End Region

#Region "Properties"

        Public ReadOnly Property IsPlaying() As Boolean
            Get
                Return Not mPlaybackService.IsStopped And mPlaybackService.IsPlaying
            End Get
        End Property
#End Region

#Region "Construction"
        Public Sub New(iPlaybackService As IPlaybackService)

            mPlaybackService = iPlaybackService

            AddHandler mPlaybackService.PlaybackFailed, Sub() OnPropertyChanged(Function() Me.IsPlaying)
            AddHandler mPlaybackService.PlaybackPaused, Sub() OnPropertyChanged(Function() Me.IsPlaying)
            AddHandler mPlaybackService.PlaybackResumed, Sub() OnPropertyChanged(Function() Me.IsPlaying)
            AddHandler mPlaybackService.PlaybackStopped, Sub() OnPropertyChanged(Function() Me.IsPlaying)
            AddHandler mPlaybackService.PlaybackSuccess, Sub() OnPropertyChanged(Function() Me.IsPlaying)
        End Sub
#End Region
    End Class
End Namespace
