Imports Dopamine.Common
Imports Microsoft.Practices.Prism.Commands
Imports Dopamine.Core.Prism
Imports Dopamine.Common.Services.Search
Imports Microsoft.Practices.Prism.PubSubEvents

Namespace ViewModels
    Public Class MicroPlayerViewModel
        Inherits CommonMiniPlayerViewModel

#Region "Variables"
        Private mEventAggregator As IEventAggregator
#End Region

#Region "Commands"
        Public Property MicroPlayerPlaylistButtonCommand As DelegateCommand(Of Boolean?)
#End Region

#Region "Construction"
        Public Sub New(iEventAggregator As IEventAggregator)

            MyBase.New()

            mEventAggregator = iEventAggregator

            ' Commands
            Me.MicroPlayerPlaylistButtonCommand = New DelegateCommand(Of Boolean?)(Sub(iIsPlaylistButtonChecked)
                                                                                       mEventAggregator.GetEvent(Of MicroPlayerPlaylistButtonClicked).Publish(iIsPlaylistButtonChecked.Value)
                                                                                       Me.IsPlaylistVisible = iIsPlaylistButtonChecked.Value
                                                                                   End Sub)

            ApplicationCommands.MicroPlayerPlaylistButtonCommand.RegisterCommand(Me.MicroPlayerPlaylistButtonCommand)

        End Sub
#End Region
    End Class
End Namespace