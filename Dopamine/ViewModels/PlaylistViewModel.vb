Imports Dopamine.Common.Services.Playback
Imports Dopamine.ControlsModule.Views
Imports Dopamine.Core.Prism
Imports Dopamine.MiniPlayerModule.Views
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Prism.Mvvm
Imports Microsoft.Practices.Prism.Regions

Namespace ViewModels
    Public Class PlaylistViewModel
        Inherits BindableBase

#Region "Variables"
        Private mPlaybackService As IPlaybackService
        Private mRegionManager As IRegionManager
#End Region

#Region "Commands"
        Public Property LoadedCommand As DelegateCommand
#End Region

#Region "Construction"
        Public Sub New(iPlaybackService As IPlaybackService, iRegionManager As IRegionManager)

            mPlaybackService = iPlaybackService
            mRegionManager = iRegionManager

            Me.LoadedCommand = New DelegateCommand(Sub() Me.SetNowPlaying())

            AddHandler mPlaybackService.PlaybackSuccess, Sub() Me.SetNowPlaying()
        End Sub
#End Region

#Region "Private"
        Private Sub SetNowPlaying()

            If mPlaybackService.Queue.Count > 0 Then
                mRegionManager.RequestNavigate(RegionNames.MiniPlayerPlaylistRegion, GetType(MiniPlayerPlaylist).FullName)
            Else
                mRegionManager.RequestNavigate(RegionNames.MiniPlayerPlaylistRegion, GetType(NothingPlayingControl).FullName)
            End If
        End Sub
#End Region
    End Class
End Namespace

