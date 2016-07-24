Imports Dopamine.Common.Controls
Imports Dopamine.Common.Enums
Imports Dopamine.Common.Presentation.Views
Imports Dopamine.Common.Services.Playback
Imports Dopamine.Core
Imports Dopamine.Core.Base
Imports Dopamine.Core.Prism
Imports Dopamine.Core.Settings
Imports Dopamine.Core.Utils
Imports Dopamine.MiniPlayerModule.Views
Imports Microsoft.Practices.Prism.Mvvm
Imports Microsoft.Practices.Prism.PubSubEvents
Imports Microsoft.Practices.Prism.Regions

Namespace Views
    Public Class Playlist
        Inherits DopamineWindow
        Implements IView

#Region "Variables"
        Private mParent As DopamineWindow
        Private mPlaybackService As IPlaybackService
        Private mRegionManager As IRegionManager
        Private mEventAggregator As IEventAggregator
        Private mMiniPlayerType As MiniPlayerType
        Private mSeparationSize As Double = 5
        Private mAlignCoverPlayerAlignPlaylistVertically As Boolean
#End Region

#Region "Properties"
        Public Shadows Property DataContext() As Object Implements IView.DataContext
            Get
                Return MyBase.DataContext
            End Get
            Set(ByVal value As Object)
                MyBase.DataContext = value
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New(iParent As DopamineWindow, iPlaybackService As IPlaybackService, iRegionManager As IRegionManager, iEventAggregator As IEventAggregator)

            ' This call is required by the designer.
            InitializeComponent()

            mParent = iParent
            mPlaybackService = iPlaybackService
            mRegionManager = iRegionManager
            mEventAggregator = iEventAggregator

            mEventAggregator.GetEvent(Of ToggledCoverPlayerAlignPlaylistVertically).Subscribe(Sub(iAlignPlaylistVertically)
                                                                                                  mAlignCoverPlayerAlignPlaylistVertically = iAlignPlaylistVertically
                                                                                                  If Me.IsVisible Then Me.SetGeometry()
                                                                                              End Sub)
        End Sub
#End Region

#Region "Parent Handlers"
        Private Sub Parent_LocationChanged(sender As Object, e As EventArgs)
            Me.SetGeometry()
        End Sub
#End Region

#Region "Handlers"
        Protected Overrides Sub OnActivated(e As EventArgs)
            Me.SetGeometry()
        End Sub
#End Region

#Region "Public"
        Public Overloads Sub Show(iMiniPlayerType As MiniPlayerType)

            mMiniPlayerType = iMiniPlayerType

            ' The order in the owner chain is very important.
            ' It makes sure the overlapping of the windows is correct:
            ' From Top to Bottom: Me -> mParent -> mShadow
            Me.Owner = mParent

            AddHandler mParent.LocationChanged, AddressOf Parent_LocationChanged

            mAlignCoverPlayerAlignPlaylistVertically = XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "CoverPlayerAlignPlaylistVertically")
            Me.SetWindowBorder(XmlSettingsClient.Instance.Get(Of Boolean)("Appearance", "ShowWindowBorder"))

            ' Makes sure the playlist doesn't appear briefly at the topleft 
            ' of the screen just before it is positioned by Me.SetGeometry()
            Me.Top = mParent.Top
            Me.Left = mParent.Left

            MyBase.Show()

            Me.SetTransparency()
            Me.SetGeometry()
        End Sub

        Public Overloads Sub Hide()

            RemoveHandler mParent.LocationChanged, AddressOf Parent_LocationChanged

            MyBase.Hide()
        End Sub
#End Region

#Region "Private"
        Private Sub SetTransparency()

            If EnvironmentUtils.IsWindows10 AndAlso XmlSettingsClient.Instance.Get(Of Boolean)("Appearance", "EnableTransparency") Then
                Me.PlaylistBackground.Opacity = Constants.OpacityWhenBlurred
                WindowUtils.EnableBlur(Me)
            Else
                Me.PlaylistBackground.Opacity = 1.0
            End If
        End Sub
        Private Sub SetGeometry()

            Select Case mMiniPlayerType
                Case MiniPlayerType.CoverPlayer

                    If mAlignCoverPlayerAlignPlaylistVertically Then
                        Me.Width = mParent.ActualWidth
                        Me.Height = Constants.CoverPlayerVerticalPlaylistHeight
                    Else
                        Me.Width = Constants.CoverPlayerHorizontalPlaylistWidth
                        Me.Height = mParent.ActualHeight
                    End If
                Case MiniPlayerType.MicroPlayer
                    Me.Width = mParent.ActualWidth
                    Me.Height = Constants.MicroPlayerPlaylistHeight
                Case MiniPlayerType.NanoPlayer
                    Me.Width = mParent.ActualWidth
                    Me.Height = Constants.NanoPlayerPlaylistHeight
                Case Else
                    ' Doesn't happen
            End Select

            Me.Top = Me.GetTop
            Me.Left = Me.GetLeft
        End Sub

        Private Function GetTop() As Double

            If mMiniPlayerType = MiniPlayerType.CoverPlayer And Not mAlignCoverPlayerAlignPlaylistVertically Then

                Return mParent.Top
            Else
                ' We're using the Windows Forms Screen class to get correct screen information.
                ' WPF doesn't provide such detailed information about the screens.
                Dim screen = System.Windows.Forms.Screen.FromRectangle(New System.Drawing.Rectangle(CInt(Me.Left), CInt(Me.Top), CInt(Me.Width), CInt(Me.Height)))

                If mParent.Top + mParent.ActualHeight + Me.Height <= screen.WorkingArea.Bottom Then

                    ' Position at the bottom of the main window
                    Return mParent.Top + mParent.ActualHeight + mSeparationSize
                Else
                    ' Position at the top of the main window
                    Return mParent.Top - Me.Height - mSeparationSize
                End If
            End If
        End Function

        Private Function GetLeft() As Double

            If mMiniPlayerType = MiniPlayerType.CoverPlayer And Not mAlignCoverPlayerAlignPlaylistVertically Then

                ' We're using the Windows Forms Screen class to get correct screen information.
                ' WPF doesn't provide such detailed information about the screens.
                Dim screen = System.Windows.Forms.Screen.FromRectangle(New System.Drawing.Rectangle(CInt(Me.Left), CInt(Me.Top), CInt(Me.Width), CInt(Me.Height)))

                If mParent.Left + mParent.ActualWidth + Me.Width <= screen.WorkingArea.Right Then

                    ' Position at the right of the main window
                    Return mParent.Left + mParent.ActualWidth + mSeparationSize
                Else
                    ' Position at the left of the main window
                    Return mParent.Left - Me.Width - mSeparationSize
                End If
            Else
                Return mParent.Left
            End If
        End Function

        Private Async Sub PlaylistWindow_Loaded(sender As Object, e As RoutedEventArgs)

            ' Duration is set after 1/2 second
            Await Task.Delay(500)
            Me.MiniPlayerPlaylistRegion.SlideDuration = Constants.SlideTimeoutSeconds
            Me.MiniPlayerPlaylistRegion.FadeOutDuration = Constants.FadeOutTimeoutSeconds
            Me.MiniPlayerPlaylistRegion.FadeInDuration = Constants.FadeInTimeoutSeconds
        End Sub
#End Region
    End Class
End Namespace