Imports Microsoft.Practices.Prism.Mvvm
Imports Dopamine.Core.Settings
Imports System.Collections.ObjectModel
Imports Dopamine.Core.Utils
Imports Dopamine.Core.Base
Imports Microsoft.Practices.Prism.PubSubEvents
Imports Dopamine.Core.Prism

Namespace ViewModels
    Public Class SettingsBehaviourViewModel
        Inherits BindableBase

#Region "Variables"
        Private mEventAggregator As IEventAggregator
        Private mCheckBoxShowTrayIconChecked As Boolean
        Private mCheckBoxMinimizeToTrayChecked As Boolean
        Private mCheckBoxFollowTrackChecked As Boolean
        Private mCheckBoxCloseToTrayChecked As Boolean
        Private mCheckBoxEnableRatingChecked As Boolean
        Private mCheckBoxUseStarRatingChecked As Boolean
        Private mCheckBoxSaveRatingInAudioFilesChecked As Boolean
#End Region

#Region "Properties"
        Public Property CheckBoxShowTrayIconChecked() As Boolean
            Get
                Return mCheckBoxShowTrayIconChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Behaviour", "ShowTrayIcon", value)
                SetProperty(Of Boolean)(mCheckBoxShowTrayIconChecked, value)
                Application.Current.Dispatcher.Invoke(Sub() mEventAggregator.GetEvent(Of SettingShowTrayIconChanged).Publish(value))
            End Set
        End Property

        Public Property CheckBoxMinimizeToTrayChecked() As Boolean
            Get
                Return mCheckBoxMinimizeToTrayChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Behaviour", "MinimizeToTray", value)
                SetProperty(Of Boolean)(mCheckBoxMinimizeToTrayChecked, value)
            End Set
        End Property

        Public Property CheckBoxCloseToTrayChecked() As Boolean
            Get
                Return mCheckBoxCloseToTrayChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Behaviour", "CloseToTray", value)
                SetProperty(Of Boolean)(mCheckBoxCloseToTrayChecked, value)
            End Set
        End Property

        Public Property CheckBoxFollowTrackChecked() As Boolean
            Get
                Return mCheckBoxFollowTrackChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Behaviour", "FollowTrack", value)
                SetProperty(Of Boolean)(mCheckBoxFollowTrackChecked, value)
            End Set
        End Property

        Public Property CheckBoxEnableRatingChecked() As Boolean
            Get
                Return mCheckBoxEnableRatingChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Behaviour", "EnableRating", value)
                SetProperty(Of Boolean)(mCheckBoxEnableRatingChecked, value)
                Application.Current.Dispatcher.Invoke(Sub() mEventAggregator.GetEvent(Of SettingEnableRatingChanged)().Publish(value))
            End Set
        End Property

        Public Property CheckBoxUseStarRatingChecked() As Boolean
            Get
                Return mCheckBoxUseStarRatingChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Behaviour", "UseStarRating", value)
                SetProperty(Of Boolean)(mCheckBoxUseStarRatingChecked, value)
                Application.Current.Dispatcher.Invoke(Sub() mEventAggregator.GetEvent(Of SettingUseStarRatingChanged)().Publish(value))
            End Set
        End Property

        Public Property CheckBoxSaveRatingInAudioFilesChecked() As Boolean
            Get
                Return mCheckBoxSaveRatingInAudioFilesChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Behaviour", "SaveRatingToAudioFiles", value)
                SetProperty(Of Boolean)(mCheckBoxSaveRatingInAudioFilesChecked, value)
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New(iEventAggregator As IEventAggregator)

            mEventAggregator = IEventAggregator

            Me.GetCheckBoxesAsync()
        End Sub
#End Region

#Region "Private"
        Private Async Sub GetCheckBoxesAsync()

            Await Task.Run(Sub()
                               Me.CheckBoxShowTrayIconChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "ShowTrayIcon")
                               Me.CheckBoxMinimizeToTrayChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "MinimizeToTray")
                               Me.CheckBoxCloseToTrayChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "CloseToTray")
                               Me.CheckBoxFollowTrackChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "FollowTrack")
                               Me.CheckBoxEnableRatingChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "EnableRating")
                               Me.CheckBoxUseStarRatingChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "UseStarRating")
                               Me.CheckBoxSaveRatingInAudioFilesChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "SaveRatingToAudioFiles")
                           End Sub)
        End Sub
#End Region
    End Class
End Namespace