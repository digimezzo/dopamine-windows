Imports Dopamine.Common.Enums
Imports Dopamine.Common.Enums.PlayerEnums
Imports Dopamine.Core.Prism
Imports Dopamine.Core.Settings
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Prism.Mvvm
Imports Microsoft.Practices.Prism.PubSubEvents

Namespace ViewModels
    Public Class CommonMiniPlayerViewModel
        Inherits BindableBase

#Region "Variables"
        Protected mIsPlaylistVisible As Boolean
        Private mIsCoverPlayerChecked As Boolean
        Private mIsMicroPlayerChecked As Boolean
        Private mIsNanoPlayerChecked As Boolean
        Private mIsMiniPlayerAlwaysOnTop As Boolean
        Private mIsMiniPlayerPositionLocked As Boolean
        Private mEventAggregator As IEventAggregator
#End Region

#Region "Commands"
        Public Property ChangePlayerTypeCommand As DelegateCommand(Of String)
        Public Property ToggleMiniPlayerPositionLockedCommand As DelegateCommand
        Public Property ToggleMiniPlayerAlwaysOnTopCommand As DelegateCommand
#End Region

#Region "Properties"
        Public Property IsPlaylistVisible() As Boolean
            Get
                Return mIsPlaylistVisible
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsPlaylistVisible, value)
            End Set
        End Property

        Public Property IsCoverPlayerChecked() As Boolean
            Get
                Return mIsCoverPlayerChecked
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsCoverPlayerChecked, value)
            End Set
        End Property

        Public Property IsMicroPlayerChecked() As Boolean
            Get
                Return mIsMicroPlayerChecked
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsMicroPlayerChecked, value)
            End Set
        End Property

        Public Property IsNanoPlayerChecked() As Boolean
            Get
                Return mIsNanoPlayerChecked
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsNanoPlayerChecked, value)
            End Set
        End Property

        Public Property IsMiniPlayerAlwaysOnTop() As Boolean
            Get
                Return mIsMiniPlayerAlwaysOnTop
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsMiniPlayerAlwaysOnTop, value)
            End Set
        End Property

        Public Property IsMiniPlayerPositionLocked() As Boolean
            Get
                Return mIsMiniPlayerPositionLocked
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsMiniPlayerPositionLocked, value)
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New()

            ' Commands
            Me.ChangePlayerTypeCommand = New DelegateCommand(Of String)(Sub(iMiniPlayerType) Me.SetPlayerContextMenuCheckBoxes(CType(iMiniPlayerType, MiniPlayerType)))

            Me.ToggleMiniPlayerPositionLockedCommand = New DelegateCommand(Sub()
                                                                               IsMiniPlayerPositionLocked = Not IsMiniPlayerPositionLocked
                                                                               XmlSettingsClient.Instance.Set(Of Boolean)("Behaviour", "MiniPlayerPositionLocked", IsMiniPlayerPositionLocked)
                                                                           End Sub)

            Me.ToggleMiniPlayerAlwaysOnTopCommand = New DelegateCommand(Sub()
                                                                            IsMiniPlayerAlwaysOnTop = Not IsMiniPlayerAlwaysOnTop
                                                                            XmlSettingsClient.Instance.Set(Of Boolean)("Behaviour", "MiniPlayerOnTop", IsMiniPlayerAlwaysOnTop)
                                                                        End Sub)

            ' Register Commands: all 3 Mini Players need to listen to these Commands, even if 
            ' their Views are not active. That is why we don't use Subscribe and Unsubscribe.
            ApplicationCommands.ChangePlayerTypeCommand.RegisterCommand(Me.ChangePlayerTypeCommand)
            ApplicationCommands.ToggleMiniPlayerPositionLockedCommand.RegisterCommand(Me.ToggleMiniPlayerPositionLockedCommand)
            ApplicationCommands.ToggleMiniPlayerAlwaysOnTopCommand.RegisterCommand(Me.ToggleMiniPlayerAlwaysOnTopCommand)

            'Initialize
            Me.Initialize()
        End Sub
#End Region

#Region "Private"
        Private Sub SetPlayerContextMenuCheckBoxes(iMiniPlayerType As MiniPlayerType)

            Me.IsCoverPlayerChecked = False
            Me.IsMicroPlayerChecked = False
            Me.IsNanoPlayerChecked = False

            Select Case iMiniPlayerType
                Case MiniPlayerType.CoverPlayer
                    Me.IsCoverPlayerChecked = True
                Case MiniPlayerType.MicroPlayer
                    Me.IsMicroPlayerChecked = True
                Case MiniPlayerType.NanoPlayer
                    Me.IsNanoPlayerChecked = True
                Case Else
                    ' Doesn't happen
            End Select
        End Sub

        Private Sub Initialize()

            ' Set the default IsMiniPlayerPositionLocked value
            Me.IsMiniPlayerPositionLocked = XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "MiniPlayerPositionLocked")
            Me.IsMiniPlayerAlwaysOnTop = XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "MiniPlayerOnTop")

            ' This sets the initial state of the ContextMenu CheckBoxes
            Me.SetPlayerContextMenuCheckBoxes(CType(XmlSettingsClient.Instance.Get(Of Integer)("General", "MiniPlayerType"), MiniPlayerType))
        End Sub
#End Region
    End Class
End Namespace
