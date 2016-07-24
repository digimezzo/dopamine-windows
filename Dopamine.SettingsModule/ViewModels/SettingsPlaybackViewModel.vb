Imports System.Collections.ObjectModel
Imports Dopamine.Common.Services.Notification
Imports Dopamine.Common.Services.Playback
Imports Dopamine.Common.Services.Taskbar
Imports Dopamine.Core.Settings
Imports Dopamine.Core.Helpers
Imports Dopamine.Core.Utils
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Prism.Mvvm
Imports Dopamine.Core.Base

Namespace ViewModels
    Public Class SettingsPlaybackViewModel
        Inherits BindableBase

#Region "Variables"
        Private mLatencies As ObservableCollection(Of NameValue)
        Private mSelectedLatency As NameValue
        Private mPlaybackService As IPlaybackService
        Private mTaskbarService As ITaskbarService
        Private mNotificationService As INotificationService
        'Private mCheckBoxWasapiEventModeChecked As Boolean
        Private mCheckBoxWasapiExclusiveModeChecked As Boolean
        Private mCheckBoxShowNotificationChecked As Boolean
        Private mCheckBoxShowNotificationControlsChecked As Boolean
        Private mCheckBoxShowProgressInTaskbarChecked As Boolean
        Private mCheckBoxShowNotificationOnlyWhenPlayerNotVisibleChecked As Boolean
        Private mNotificationPositions As ObservableCollection(Of NameValue)
        Private mSelectedNotificationPosition As NameValue
        Private mNotificationSeconds As ObservableCollection(Of Integer)
        Private mSelectedNotificationSecond As Integer
#End Region

#Region "Commands"
        Public Property ShowTestNotificationCommand As delegatecommand
#End Region

#Region "Properties"
        Public Property Latencies() As ObservableCollection(Of NameValue)
            Get
                Return mLatencies
            End Get
            Set(ByVal value As ObservableCollection(Of NameValue))
                SetProperty(Of ObservableCollection(Of NameValue))(mLatencies, value)
            End Set
        End Property

        Public Property SelectedLatency() As NameValue
            Get
                Return mSelectedLatency
            End Get
            Set(ByVal value As NameValue)
                XmlSettingsClient.Instance.Set(Of Integer)("Playback", "AudioLatency", value.Value)
                SetProperty(Of NameValue)(mSelectedLatency, value)

                If mPlaybackService IsNot Nothing Then
                    mPlaybackService.Latency = value.Value
                End If
            End Set
        End Property

        'Public Property CheckBoxWasapiEventModeChecked() As Boolean
        '    Get
        '        Return mCheckBoxWasapiEventModeChecked
        '    End Get
        '    Set(ByVal value As Boolean)
        '        XmlSettingsClient.Instance.Set(Of Boolean)("Playback", "WasapiEventMode", value)
        '        SetProperty(Of Boolean)(mCheckBoxWasapiEventModeChecked, value)

        '        If mPlaybackService IsNot Nothing Then
        '            mPlaybackService.EventMode = value
        '        End If
        '    End Set
        'End Property

        Public Property CheckBoxWasapiExclusiveModeChecked() As Boolean
            Get
                Return Me.mCheckBoxWasapiExclusiveModeChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Playback", "WasapiExclusiveMode", value)
                SetProperty(Of Boolean)(mCheckBoxWasapiExclusiveModeChecked, value)

                If mPlaybackService IsNot Nothing Then
                    mPlaybackService.ExclusiveMode = value
                End If
            End Set
        End Property

        Public Property CheckBoxShowNotificationChecked() As Boolean
            Get
                Return mCheckBoxShowNotificationChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Behaviour", "ShowNotification", value)
                SetProperty(Of Boolean)(mCheckBoxShowNotificationChecked, value)
            End Set
        End Property

        Public Property CheckBoxShowNotificationControlsChecked() As Boolean
            Get
                Return mCheckBoxShowNotificationControlsChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Behaviour", "ShowNotificationControls", value)
                SetProperty(Of Boolean)(mCheckBoxShowNotificationControlsChecked, value)
            End Set
        End Property

        Public Property CheckBoxShowProgressInTaskbarChecked() As Boolean
            Get
                Return mCheckBoxShowProgressInTaskbarChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Playback", "ShowProgressInTaskbar", value)
                SetProperty(Of Boolean)(Me.mCheckBoxShowProgressInTaskbarChecked, value)

                If mTaskbarService IsNot Nothing AndAlso mPlaybackService IsNot Nothing Then
                    mTaskbarService.SetTaskbarProgressState(value, mPlaybackService.IsPlaying)
                End If
            End Set
        End Property

        Public Property CheckBoxShowNotificationOnlyWhenPlayerNotVisibleChecked() As Boolean
            Get
                Return mCheckBoxShowNotificationOnlyWhenPlayerNotVisibleChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Behaviour", "ShowNotificationOnlyWhenPlayerNotVisible", value)
                SetProperty(Of Boolean)(mCheckBoxShowNotificationOnlyWhenPlayerNotVisibleChecked, value)
            End Set
        End Property

        Public Property NotificationPositions() As ObservableCollection(Of NameValue)
            Get
                Return mNotificationPositions
            End Get
            Set(ByVal value As ObservableCollection(Of NameValue))
                SetProperty(Of ObservableCollection(Of NameValue))(mNotificationPositions, value)
            End Set
        End Property

        Public Property SelectedNotificationPosition() As NameValue
            Get
                Return mSelectedNotificationPosition
            End Get
            Set(ByVal value As NameValue)
                XmlSettingsClient.Instance.Set(Of Integer)("Behaviour", "NotificationPosition", value.Value)
                SetProperty(Of NameValue)(mSelectedNotificationPosition, value)
            End Set
        End Property

        Public Property NotificationSeconds() As ObservableCollection(Of Integer)
            Get
                Return mNotificationSeconds
            End Get
            Set(ByVal value As ObservableCollection(Of Integer))
                SetProperty(Of ObservableCollection(Of Integer))(mNotificationSeconds, value)
            End Set
        End Property

        Public Property SelectedNotificationSecond() As Integer
            Get
                Return mSelectedNotificationSecond
            End Get
            Set(ByVal value As Integer)
                XmlSettingsClient.Instance.Set(Of Integer)("Behaviour", "NotificationAutoCloseSeconds", value)
                SetProperty(Of Integer)(mSelectedNotificationSecond, value)
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New(iPlaybackService As IPlaybackService, iTaskbarService As ITaskbarService, iNotificationService As INotificationService)

            mPlaybackService = iPlaybackService
            mTaskbarService = iTaskbarService
            mNotificationService = iNotificationService

            ShowTestNotificationCommand = New DelegateCommand(Sub() mNotificationService.ShowNotificationAsync())

            Me.GetCheckBoxesAsync()
            Me.GetNotificationPositionsAsync()
            Me.GetNotificationSecondsAsync()
            Me.GetLatenciesAsync()
        End Sub
#End Region

#Region "Private"
        Private Async Sub GetLatenciesAsync()

            Dim localLatencies As New ObservableCollection(Of NameValue)

            Await Task.Run(Sub()
                               For index = 50 To 500 Step 50 ' Increment by 50
                                   If index = 200 Then
                                       localLatencies.Add(New NameValue With {.Name = index & " ms (" & Application.Current.FindResource("Language_Default").ToString.ToLower & ")", .Value = index})
                                   Else
                                       localLatencies.Add(New NameValue With {.Name = index & " ms", .Value = index})
                                   End If
                               Next
                           End Sub)

            Me.Latencies = localLatencies

            Dim localSelectedLatency As NameValue = Nothing

            Await Task.Run(Sub() localSelectedLatency = Latencies.Where(Function(pa) pa.Value = XmlSettingsClient.Instance.Get(Of Integer)("Playback", "AudioLatency")).Select(Function(pa) pa).First())

            Me.SelectedLatency = localSelectedLatency
        End Sub

        Private Async Sub GetNotificationPositionsAsync()

            Dim localNotificationPositions As New ObservableCollection(Of NameValue)

            Await Task.Run(Sub()
                               localNotificationPositions.Add(New NameValue With {.Name = ResourceUtils.GetStringResource("Language_Bottom_Left"), .Value = NotificationPosition.BottomLeft})
                               localNotificationPositions.Add(New NameValue With {.Name = ResourceUtils.GetStringResource("Language_Top_Left"), .Value = NotificationPosition.TopLeft})
                               localNotificationPositions.Add(New NameValue With {.Name = ResourceUtils.GetStringResource("Language_Top_Right"), .Value = NotificationPosition.TopRight})
                               localNotificationPositions.Add(New NameValue With {.Name = ResourceUtils.GetStringResource("Language_Bottom_Right"), .Value = NotificationPosition.BottomRight})
                           End Sub)

            Me.NotificationPositions = localNotificationPositions

            Dim localSelectedNotificationPosition As NameValue = Nothing

            Await Task.Run(Sub() localSelectedNotificationPosition = NotificationPositions.Where(Function(np) np.Value = XmlSettingsClient.Instance.Get(Of Integer)("Behaviour", "NotificationPosition")).Select(Function(np) np).First())

            Me.SelectedNotificationPosition = localSelectedNotificationPosition
        End Sub

        Private Async Sub GetNotificationSecondsAsync()

            Dim localNotificationSeconds As New ObservableCollection(Of Integer)

            Await Task.Run(Sub()
                               For index = 1 To 5
                                   localNotificationSeconds.Add(index)
                               Next
                           End Sub)

            Me.NotificationSeconds = localNotificationSeconds

            Dim localSelectedNotificationSecond As Integer = 0

            Await Task.Run(Sub() localSelectedNotificationSecond = NotificationSeconds.Where(Function(ns) ns = XmlSettingsClient.Instance.Get(Of Integer)("Behaviour", "NotificationAutoCloseSeconds")).Select(Function(ns) ns).First())

            Me.SelectedNotificationSecond = localSelectedNotificationSecond
        End Sub

        Private Async Sub GetCheckBoxesAsync()

            Await Task.Run(Sub()
                               'Me.CheckBoxWasapiEventModeChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Playback", "WasapiEventMode")
                               Me.CheckBoxWasapiExclusiveModeChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Playback", "WasapiExclusiveMode")
                               Me.CheckBoxShowNotificationChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "ShowNotification")
                               Me.CheckBoxShowNotificationControlsChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "ShowNotificationControls")
                               Me.CheckBoxShowProgressInTaskbarChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Playback", "ShowProgressInTaskbar")
                               Me.CheckBoxShowNotificationOnlyWhenPlayerNotVisibleChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "ShowNotificationOnlyWhenPlayerNotVisible")
                           End Sub)

        End Sub
#End Region
    End Class
End Namespace
