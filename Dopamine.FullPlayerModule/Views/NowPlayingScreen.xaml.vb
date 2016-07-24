Imports Microsoft.Practices.Prism.Mvvm
Imports System.Timers
Imports Dopamine.Core.Base

Namespace Views
    Public Class NowPlayingScreen
        Inherits UserControl
        Implements IView

#Region "Variables"
        Private mCleanupNowPlayingTimer As New Timer
        Private mCleanupNowPlayingTimeout As Integer = 2 ' 2 seconds
        Private mIsMouseOverBackButton As Boolean = False
#End Region

#Region "Dependency properties"
        Public Shared ReadOnly ShowControlsProperty As DependencyProperty = DependencyProperty.Register("ShowControls", GetType(Boolean), GetType(NowPlayingScreen), New PropertyMetadata(Nothing))
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

        Public Property ShowControls As Boolean
            Get
                Return CBool(GetValue(ShowControlsProperty))
            End Get

            Set(ByVal value As Boolean)
                SetValue(ShowControlsProperty, value)
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            Me.mCleanupNowPlayingTimer.Interval = TimeSpan.FromSeconds(Me.mCleanupNowPlayingTimeout).TotalMilliseconds
            AddHandler Me.mCleanupNowPlayingTimer.Elapsed, New ElapsedEventHandler(AddressOf Me.CleanupNowPlayingHandler)
            SetNowPlaying(True)
        End Sub

#End Region

#Region "Private"
        Private Sub SetNowPlaying(iCluttered As Boolean)

            If iCluttered Then
                Me.mCleanupNowPlayingTimer.Stop()
                Me.ShowControls = True
                Me.mCleanupNowPlayingTimer.Start()
            Else
                Me.mCleanupNowPlayingTimer.Stop()
                Me.mCleanupNowPlayingTimer.Start()
            End If
        End Sub
#End Region

#Region "Event handlers"
        Sub CleanupNowPlayingHandler(ByVal sender As Object, ByVal e As ElapsedEventArgs)

            Me.Dispatcher.BeginInvoke(Sub()
                                          If Not Me.BackButton.IsMouseOver Then
                                              Me.ShowControls = False
                                          End If
                                      End Sub)
        End Sub

        Private Sub NowPlaying_MouseMove(sender As Object, e As MouseEventArgs)
            SetNowPlaying(True)
        End Sub

        Private Sub NowPlaying_SizeChanged(sender As Object, e As SizeChangedEventArgs)

            ' This makes sure the spectrum analyzer is centered on the screen, based on the left pixel.
            ' When we align center, alignment is sometimes (depending on the width of the screen) done
            ' on a half pixel. This causes a blurry spectrum analyzer.
            Try
                NowPlayingSpectrumAnalyzerRegion.Margin = New Thickness(Convert.ToInt32(Me.ActualWidth / 2) - Convert.ToInt32(NowPlayingSpectrumAnalyzerRegion.ActualWidth / 2), 0, 0, 0)
            Catch ex As Exception
                ' Swallow this exception
            End Try
        End Sub

        Private Async Sub NowPlaying_Loaded(sender As Object, e As RoutedEventArgs)

            ' Duration is set after 1/2 second so the now playing info screen doesn't 
            ' slide in from the left the first time the now playing screen is loaded.
            ' Slide in from left combined with slide in from bottom for the cover picture,
            ' gives as combined effect a slide in from bottomleft for the cover picture.
            ' That doesn't look so good.
            Await Task.Delay(500)
            Me.NowPlayingContentRegion.SlideDuration = Constants.SlideTimeoutSeconds
            Me.NowPlayingContentRegion.FadeInDuration = Constants.FadeInTimeoutSeconds
            Me.NowPlayingContentRegion.FadeOutDuration = Constants.FadeOutTimeoutSeconds
        End Sub
#End Region
    End Class
End Namespace
