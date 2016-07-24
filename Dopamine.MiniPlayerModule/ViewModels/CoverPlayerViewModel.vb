Imports Microsoft.Practices.Prism.Commands
Imports Dopamine.Core.Prism
Imports Dopamine.Core.Settings
Imports Microsoft.Practices.Prism.Regions
Imports Dopamine.Common.Services.Search
Imports Microsoft.Practices.Prism.PubSubEvents

Namespace ViewModels
    Public Class CoverPlayerViewModel
        Inherits CommonMiniPlayerViewModel

#Region "Variables"
        Private mEventAggregator As IEventAggregator
        Private mAlwaysShowPlaybackInfo As Boolean
        Private mAlignPlaylistVertically As Boolean
#End Region

#Region "Commands"
        Public Property CoverPlayerPlaylistButtonCommand As DelegateCommand(Of Boolean?)
        Public Property ToggleAlwaysShowPlaybackInfoCommand As DelegateCommand
        Public Property ToggleAlignPlaylistVerticallyCommand As DelegateCommand
#End Region

#Region "Properties"
        Public Property AlwaysShowPlaybackInfo() As Boolean
            Get
                Return mAlwaysShowPlaybackInfo
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mAlwaysShowPlaybackInfo, value)
            End Set
        End Property


        Public Property AlignPlaylistVertically() As Boolean
            Get
                Return mAlignPlaylistVertically
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mAlignPlaylistVertically, value)
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New(iEventAggregator As IEventAggregator)

            MyBase.New()

            mEventAggregator = iEventAggregator

            ' Commands
            Me.CoverPlayerPlaylistButtonCommand = New DelegateCommand(Of Boolean?)(Sub(iIsPlaylistButtonChecked)
                                                                                       mEventAggregator.GetEvent(Of CoverPlayerPlaylistButtonClicked).Publish(iIsPlaylistButtonChecked.Value)
                                                                                       Me.IsPlaylistVisible = iIsPlaylistButtonChecked.Value
                                                                                   End Sub)

            Me.ToggleAlwaysShowPlaybackInfoCommand = New DelegateCommand(Sub()
                                                                             AlwaysShowPlaybackInfo = Not AlwaysShowPlaybackInfo
                                                                             XmlSettingsClient.Instance.Set(Of Boolean)("Behaviour", "CoverPlayerAlwaysShowPlaybackInfo", AlwaysShowPlaybackInfo)
                                                                         End Sub)


            Me.ToggleAlignPlaylistVerticallyCommand = New DelegateCommand(Sub()
                                                                              AlignPlaylistVertically = Not AlignPlaylistVertically
                                                                              XmlSettingsClient.Instance.Set(Of Boolean)("Behaviour", "CoverPlayerAlignPlaylistVertically", AlignPlaylistVertically)
                                                                              mEventAggregator.GetEvent(Of ToggledCoverPlayerAlignPlaylistVertically).Publish(AlignPlaylistVertically)
                                                                          End Sub)

            ApplicationCommands.ToggleAlwaysShowPlaybackInfoCommand.RegisterCommand(Me.ToggleAlwaysShowPlaybackInfoCommand)
            ApplicationCommands.ToggleAlignPlaylistVerticallyCommand.RegisterCommand(Me.ToggleAlignPlaylistVerticallyCommand)
            ApplicationCommands.CoverPlayerPlaylistButtonCommand.RegisterCommand(Me.CoverPlayerPlaylistButtonCommand)

            Me.AlwaysShowPlaybackInfo = XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "CoverPlayerAlwaysShowPlaybackInfo")
            Me.AlignPlaylistVertically = XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "CoverPlayerAlignPlaylistVertically")
        End Sub
#End Region

    End Class
End Namespace
