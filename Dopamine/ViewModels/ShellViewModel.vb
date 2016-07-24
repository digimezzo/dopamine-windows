Imports Dopamine.Common.Services.Dialog
Imports Dopamine.Common.Services.File
Imports Dopamine.Common.Services.I18n
Imports Dopamine.Common.Services.JumpList
Imports Dopamine.Common.Services.Playback
Imports Dopamine.Common.Services.Taskbar
Imports Dopamine.Core.Base
Imports Dopamine.Core.IO
Imports Dopamine.Core.Logging
Imports Dopamine.Core.Prism
Imports Dopamine.Core.Settings
Imports Dopamine.Core.Utils
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Prism.Mvvm
Imports Microsoft.Practices.Prism.Regions

Namespace ViewModels
    Public Class ShellViewModel
        Inherits BindableBase

#Region "Variables"
        Private ReadOnly mRegionManager As IRegionManager
        Private mDialogService As IDialogService
        Private mPlaybackService As IPlaybackService
        Private mI18nService As II18nService
        Private mTaskbarService As ITaskbarService
        Private mJumpListService As IJumpListService
        Private mFileService As IFileService
        Private mIsOverlayVisible As Boolean
        Private mPlayPauseText As String
        Private mPlayPauseIcon As ImageSource
#End Region

#Region "Commands"
        Public Property OpenLinkCommand As DelegateCommand(Of String)
        Public Property OpenMailCommand As DelegateCommand(Of String)
        Public Property OpenPathCommand As DelegateCommand(Of String)
        Public Property PreviousCommand As DelegateCommand
        Public Property NextCommand As DelegateCommand
        Public Property LoadedCommand As DelegateCommand
#End Region

#Region "Properties"
        Public Property IsOverlayVisible() As Boolean
            Get
                Return mIsOverlayVisible
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mIsOverlayVisible, value)
            End Set
        End Property

        Public Property PlayPauseText() As String
            Get
                Return mPlayPauseText
            End Get
            Set(ByVal value As String)
                SetProperty(Of String)(Me.mPlayPauseText, value)
            End Set
        End Property

        Public Property PlayPauseIcon() As ImageSource
            Get
                Return mPlayPauseIcon
            End Get
            Set(ByVal value As ImageSource)
                SetProperty(Of ImageSource)(Me.mPlayPauseIcon, value)
            End Set
        End Property

        Public Property TaskbarService() As ITaskbarService
            Get
                Return mTaskbarService
            End Get
            Set(ByVal value As ITaskbarService)
                mTaskbarService = value
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New(iRegionManager As IRegionManager, iDialogService As IDialogService, iPlaybackService As IPlaybackService, iI18nService As II18nService, iTaskbarService As ITaskbarService, iJumpListService As IJumpListService, iFileService As IFileService)

            mRegionManager = iRegionManager
            mDialogService = iDialogService
            mPlaybackService = iPlaybackService
            mI18nService = iI18nService
            mTaskbarService = iTaskbarService
            mJumpListService = iJumpListService
            mFileService = iFileService

            ' When starting, we're not playing yet
            Me.ShowTaskBarItemInfoPause(False)

            Me.TaskbarService.Description = ProductInformation.ApplicationDisplayName

            ' Event handlers
            AddHandler Me.mDialogService.DialogVisibleChanged, Sub(iIsDialogVisible)
                                                                   Me.IsOverlayVisible = iIsDialogVisible
                                                               End Sub

            Me.OpenPathCommand = New DelegateCommand(Of String)(Sub(ipath As String)
                                                                    Try
                                                                        Actions.TryOpenPath(ipath)
                                                                    Catch ex As Exception
                                                                        LogClient.Instance.Logger.Error("Could not open the path {0} in Explorer. Exception: {1}", ipath, ex.Message)
                                                                    End Try
                                                                End Sub)
            ApplicationCommands.OpenPathCommand.RegisterCommand(Me.OpenPathCommand)

            Me.OpenLinkCommand = New DelegateCommand(Of String)(Sub(iLink As String)
                                                                    Try
                                                                        Actions.TryOpenLink(iLink)
                                                                    Catch ex As Exception
                                                                        LogClient.Instance.Logger.Error("Could not open the link {0} in Internet Explorer. Exception: {1}", iLink, ex.Message)
                                                                    End Try
                                                                End Sub)
            ApplicationCommands.OpenLinkCommand.RegisterCommand(Me.OpenLinkCommand)

            Me.OpenMailCommand = New DelegateCommand(Of String)(Sub(iEmailAddress As String)
                                                                    Try
                                                                        Actions.TryOpenMail(iEmailAddress)
                                                                    Catch ex As Exception
                                                                        LogClient.Instance.Logger.Error("Could not execute mailto command for the e-mail {0}. Exception: {1}", iEmailAddress, ex.Message)
                                                                    End Try
                                                                End Sub)
            ApplicationCommands.OpenMailCommand.RegisterCommand(Me.OpenMailCommand)

            Me.PreviousCommand = New DelegateCommand(Async Sub() Await Me.mPlaybackService.PlayPreviousAsync())

            Me.NextCommand = New DelegateCommand(Async Sub() Await Me.mPlaybackService.PlayNextAsync())

            Me.LoadedCommand = New DelegateCommand(Sub() mFileService.ProcessArguments(Environment.GetCommandLineArgs()))

            AddHandler Me.mPlaybackService.PlaybackFailed, Sub(iSender, iPlaybackFailedEventArgs)
                                                               Me.TaskbarService.Description = ProductInformation.ApplicationDisplayName
                                                               Me.TaskbarService.SetTaskbarProgressState(XmlSettingsClient.Instance.Get(Of Boolean)("Playback", "ShowProgressInTaskbar"), mPlaybackService.IsPlaying)
                                                               Me.ShowTaskBarItemInfoPause(False)

                                                               Select Case iPlaybackFailedEventArgs.FailureReason
                                                                   Case PlaybackFailureReason.FileNotFound
                                                                       mDialogService.ShowNotification(&HE711,
                                                                                                  16,
                                                                                                  ResourceUtils.GetStringResource("Language_Error"),
                                                                                                  ResourceUtils.GetStringResource("Language_Error_Cannot_Play_This_Song_File_Not_Found"),
                                                                                                  ResourceUtils.GetStringResource("Language_Ok"),
                                                                                                  False,
                                                                                                  String.Empty)
                                                                   Case Else
                                                                       mDialogService.ShowNotification(&HE711,
                                                                                                  16,
                                                                                                  ResourceUtils.GetStringResource("Language_Error"),
                                                                                                  ResourceUtils.GetStringResource("Language_Error_Cannot_Play_This_Song"),
                                                                                                  ResourceUtils.GetStringResource("Language_Ok"),
                                                                                                  True,
                                                                                                  ResourceUtils.GetStringResource("Language_Log_File"))
                                                               End Select
                                                           End Sub

            AddHandler Me.mPlaybackService.PlaybackPaused, Sub()
                                                               Me.TaskbarService.SetTaskbarProgressState(XmlSettingsClient.Instance.Get(Of Boolean)("Playback", "ShowProgressInTaskbar"), mPlaybackService.IsPlaying)
                                                               Me.ShowTaskBarItemInfoPause(False)
                                                           End Sub

            AddHandler Me.mPlaybackService.PlaybackResumed, Sub()
                                                                Me.TaskbarService.SetTaskbarProgressState(XmlSettingsClient.Instance.Get(Of Boolean)("Playback", "ShowProgressInTaskbar"), mPlaybackService.IsPlaying)
                                                                Me.ShowTaskBarItemInfoPause(True)
                                                            End Sub

            AddHandler Me.mPlaybackService.PlaybackStopped, Sub()
                                                                Me.TaskbarService.Description = ProductInformation.ApplicationDisplayName
                                                                Me.TaskbarService.SetTaskbarProgressState(False, False)
                                                                Me.ShowTaskBarItemInfoPause(False)
                                                            End Sub

            AddHandler Me.mPlaybackService.PlaybackSuccess, Sub()
                                                                Me.TaskbarService.Description = Me.mPlaybackService.PlayingTrack.Artist.ArtistName & " - " & Me.mPlaybackService.PlayingTrack.Track.TrackTitle
                                                                Me.TaskbarService.SetTaskbarProgressState(XmlSettingsClient.Instance.Get(Of Boolean)("Playback", "ShowProgressInTaskbar"), mPlaybackService.IsPlaying)
                                                                Me.ShowTaskBarItemInfoPause(True)
                                                            End Sub

            AddHandler Me.mPlaybackService.PlaybackProgressChanged, Sub()
                                                                        Me.TaskbarService.ProgressValue = Me.mPlaybackService.Progress
                                                                    End Sub

            ' Populate the JumpList
            mJumpListService.PopulateJumpListAsync()
        End Sub
#End Region

#Region "Private"
        Private Sub ShowTaskBarItemInfoPause(iShowPause As Boolean)

            Dim value As String = "Play"

            Try
                If iShowPause Then
                    value = "Pause"
                End If

                Me.PlayPauseText = Application.Current.TryFindResource("Language_" & value).ToString

                Application.Current.Dispatcher.Invoke(Sub()
                                                          Me.PlayPauseIcon = CType(New ImageSourceConverter().ConvertFromString("pack://application:,,,/Icons/TaskbarItemInfo_" & value & ".ico"), ImageSource)
                                                      End Sub)
            Catch ex As Exception
                LogClient.Instance.Logger.Error("Could not change the TaskBarItemInfo Play/Pause icon to '{0}'. Exception: {1}", ex.Message, value)
            End Try

        End Sub
#End Region
    End Class
End Namespace
