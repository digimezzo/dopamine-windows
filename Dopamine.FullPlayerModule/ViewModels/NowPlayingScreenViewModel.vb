Imports Digimezzo.WPFControls.Enums
Imports Dopamine.Common.Services.Playback
Imports Dopamine.ControlsModule.Views
Imports Dopamine.Core.Prism
Imports Dopamine.FullPlayerModule.Views
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Prism.Mvvm
Imports Microsoft.Practices.Prism.Regions

Namespace ViewModels
    Public Class NowPlayingScreenViewModel
        Inherits BindableBase
        Implements INavigationAware

#Region "Variables"
        Private mIsShowcaseButtonChecked As Boolean
        Private mRegionManager As IRegionManager
        Private mSlideDirection As SlideDirection
        Private mPlaybackService As IPlaybackService
#End Region

#Region "Commands"
        Public Property LoadedCommand As DelegateCommand
        Public Property FullPlayerShowcaseButtonCommand As DelegateCommand(Of Boolean?)
#End Region

#Region "Properties"
        Public Property IsShowcaseButtonChecked() As Boolean
            Get
                Return mIsShowcaseButtonChecked
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsShowcaseButtonChecked, value)
            End Set
        End Property

        Public Property SlideDirection() As SlideDirection
            Get
                Return mSlideDirection
            End Get
            Set(ByVal value As SlideDirection)
                SetProperty(Of SlideDirection)(mSlideDirection, value)
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New(iRegionManager As IRegionManager, iPlaybackService As IPlaybackService)

            mRegionManager = iRegionManager
            mPlaybackService = iPlaybackService

            AddHandler mPlaybackService.PlaybackSuccess, Sub() Me.SetNowPlaying()


            Me.FullPlayerShowcaseButtonCommand = New DelegateCommand(Of Boolean?)(Sub(iIsShowcaseButtonChecked)

                                                                                      Me.IsShowcaseButtonChecked = iIsShowcaseButtonChecked.Value

                                                                                      Me.SetNowPlaying()
                                                                                  End Sub)
            Me.SlideDirection = SlideDirection.LeftToRight
            ApplicationCommands.FullPlayerShowcaseButtonCommand.RegisterCommand(Me.FullPlayerShowcaseButtonCommand)
        End Sub
#End Region

#Region "Private"
        Private Sub SetNowPlaying()
            If mPlaybackService.Queue.Count > 0 Then

                If Me.IsShowcaseButtonChecked Then
                    Me.SlideDirection = SlideDirection.LeftToRight
                    mRegionManager.RequestNavigate(RegionNames.NowPlayingContentRegion, GetType(NowPlayingScreenShowcase).FullName)
                Else
                    Me.SlideDirection = SlideDirection.RightToLeft
                    mRegionManager.RequestNavigate(RegionNames.NowPlayingContentRegion, GetType(NowPlayingScreenList).FullName)
                End If
            Else
                Me.SlideDirection = SlideDirection.RightToLeft
                mRegionManager.RequestNavigate(RegionNames.NowPlayingContentRegion, GetType(NothingPlayingControl).FullName)
            End If
        End Sub
#End Region

#Region "INavigationAware"
        Public Function IsNavigationTarget(navigationContext As NavigationContext) As Boolean Implements INavigationAware.IsNavigationTarget

            Return True
        End Function

        Public Sub OnNavigatedFrom(navigationContext As NavigationContext) Implements INavigationAware.OnNavigatedFrom

        End Sub

        Public Sub OnNavigatedTo(navigationContext As NavigationContext) Implements INavigationAware.OnNavigatedTo

            Me.SetNowPlaying()
        End Sub
#End Region
    End Class
End Namespace
