Imports Microsoft.Practices.Prism.Mvvm
Imports Microsoft.Practices.ServiceLocation
Imports Dopamine.Common.Services.Playback
Imports System.ServiceModel
Imports Dopamine.Core.Audio
Imports Dopamine.Core.Settings

Namespace Views
    Public Class SpectrumAnalyzerControl
        Implements IView

#Region "Variables"
        Private mPlayBackService As IPlaybackService
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
        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            Me.mPlayBackService = ServiceLocator.Current.GetInstance(Of IPlaybackService)()

            AddHandler mPlayBackService.PlaybackSuccess, Sub() Me.RegisterPlayer()

            ' Just in case we switched Views after the mPlayBackService.PlaybackSuccess was triggered
            If mPlayBackService.IsPlaying Then Me.RegisterPlayer()
        End Sub
#End Region

#Region "Private"
        Private Sub RegisterPlayer()
            Application.Current.Dispatcher.Invoke(Sub()
                                                      Me.SpectrumAnalyzer.RegisterSoundPlayer(CType(mPlayBackService.Player, WPFSoundVisualizationLib.ISpectrumPlayer))
                                                  End Sub)
        End Sub
#End Region
    End Class
End Namespace

