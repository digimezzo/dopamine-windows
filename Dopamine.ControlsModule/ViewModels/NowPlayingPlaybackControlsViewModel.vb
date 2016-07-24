Imports Microsoft.Practices.Prism.Mvvm
Imports Dopamine.Core.Utils
Imports Dopamine.Common.Services.Playback
Imports Dopamine.Common.Presentation.ViewModels

Namespace ViewModels
    Public Class NowPlayingPlaybackControlsViewModel
        Inherits BindableBase

#Region "Variables"
        Private mPlaybackInfoViewModel As PlaybackInfoViewModel
        Private mPlaybackService As IPlaybackService
#End Region

#Region "ReadOnly Properties"
        Public ReadOnly Property IsShowcaseAvailable() As Boolean
            Get
                Return Not mPlaybackService.IsStopped
            End Get
        End Property
#End Region

#Region "Properties"
        Public Property PlaybackInfoViewModel() As PlaybackInfoViewModel
            Get
                Return mPlaybackInfoViewModel
            End Get
            Set(ByVal value As PlaybackInfoViewModel)
                SetProperty(Of PlaybackInfoViewModel)(Me.mPlaybackInfoViewModel, value)
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New(iPlaybackService As IPlaybackService)
            Me.mPlaybackService = iPlaybackService

            AddHandler Me.mPlaybackService.PlaybackProgressChanged, Sub() Me.UpdateTime()
            AddHandler Me.mPlaybackService.PlaybackStopped, Sub()
                                                                Me.Reset()
                                                                OnPropertyChanged(Function() Me.IsShowcaseAvailable)
                                                            End Sub
            AddHandler mPlaybackService.PlaybackFailed, Sub() OnPropertyChanged(Function() Me.IsShowcaseAvailable)
            AddHandler mPlaybackService.PlaybackPaused, Sub() OnPropertyChanged(Function() Me.IsShowcaseAvailable)
            AddHandler mPlaybackService.PlaybackResumed, Sub() OnPropertyChanged(Function() Me.IsShowcaseAvailable)
            AddHandler mPlaybackService.PlaybackSuccess, Sub() OnPropertyChanged(Function() Me.IsShowcaseAvailable)

            Me.Reset()
        End Sub
#End Region

#Region "Private"
        Private Sub UpdateTime()
            Me.PlaybackInfoViewModel.CurrentTime = FormatUtils.FormatTime(Me.mPlaybackService.GetCurrentTime)
            Me.PlaybackInfoViewModel.TotalTime = " / " & FormatUtils.FormatTime(Me.mPlaybackService.GetTotalTime)
        End Sub

        Private Sub Reset()
            Me.PlaybackInfoViewModel = New PlaybackInfoViewModel With {.Title = "", .Artist = "", .Album = "", .Year = "", .CurrentTime = "", .TotalTime = ""}
        End Sub
#End Region
    End Class
End Namespace
