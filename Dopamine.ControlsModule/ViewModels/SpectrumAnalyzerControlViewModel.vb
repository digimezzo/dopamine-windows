Imports Microsoft.Practices.Prism.Mvvm
Imports Dopamine.Services
Imports Dopamine.Core.Settings
Imports Dopamine.Common.Services.Playback

Namespace ViewModels
    Public Class SpectrumAnalyzerControlViewModel
        Inherits BindableBase

#Region "Variables"
        Private mPlaybackService As IPlaybackService
        Private mShowSpectrumAnalyzer As Boolean
        Private mIsPlaying As Boolean
#End Region

#Region "Properties"
        Public Property ShowSpectrumAnalyzer() As Boolean
            Get
                Return mShowSpectrumAnalyzer
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mShowSpectrumAnalyzer, value)
            End Set
        End Property

        Public Property IsPlaying() As Boolean
            Get
                Return mIsPlaying
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsPlaying, value)
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New(iPlaybackService As IPlaybackService)
            mPlaybackService = iPlaybackService

            AddHandler mPlaybackService.SpectrumVisibilityChanged, Sub(isSpectrumVisible) Me.ShowSpectrumAnalyzer = isSpectrumVisible

            AddHandler mPlaybackService.PlaybackFailed, Sub() Me.IsPlaying = False
            AddHandler mPlaybackService.PlaybackStopped, Sub() Me.IsPlaying = False
            AddHandler mPlaybackService.PlaybackPaused, Sub() Me.IsPlaying = False
            AddHandler mPlaybackService.PlaybackResumed, Sub() Me.IsPlaying = True
            AddHandler mPlaybackService.PlaybackSuccess, Sub() Me.IsPlaying = True

            Me.ShowSpectrumAnalyzer = XmlSettingsClient.Instance.Get(Of Boolean)("Playback", "ShowSpectrumAnalyzer")

            ' Initial value
            If Not mPlaybackService.IsStopped And mPlaybackService.IsPlaying Then
                Me.IsPlaying = True
            Else
                Me.IsPlaying = False
            End If
        End Sub
#End Region
    End Class
End Namespace