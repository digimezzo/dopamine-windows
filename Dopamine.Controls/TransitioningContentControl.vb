Imports System.Windows.Media.Animation
Imports System.Timers

Public Class TransitioningContentControl
    Inherits ContentControl

#Region "Variables"
    Private mTimer As Timer
#End Region

#Region "Dependency Properties"
    Public Shared ReadOnly FadeInProperty As DependencyProperty = DependencyProperty.Register("FadeIn", GetType(Boolean), GetType(TransitioningContentControl), New PropertyMetadata(Nothing))
    Public Shared ReadOnly FadeInTimeoutProperty As DependencyProperty = DependencyProperty.Register("FadeInTimeout", GetType(Double), GetType(TransitioningContentControl), New PropertyMetadata(Nothing))
    Public Shared ReadOnly SlideInProperty As DependencyProperty = DependencyProperty.Register("SlideIn", GetType(Boolean), GetType(TransitioningContentControl), New PropertyMetadata(Nothing))
    Public Shared ReadOnly SlideInTimeoutProperty As DependencyProperty = DependencyProperty.Register("SlideInTimeout", GetType(Double), GetType(TransitioningContentControl), New PropertyMetadata(Nothing))
    Public Shared ReadOnly SlideInFromProperty As DependencyProperty = DependencyProperty.Register("SlideInFrom", GetType(Integer), GetType(TransitioningContentControl), New PropertyMetadata(Nothing))
    Public Shared ReadOnly SlideInToProperty As DependencyProperty = DependencyProperty.Register("SlideInTo", GetType(Integer), GetType(TransitioningContentControl), New PropertyMetadata(Nothing))
    Public Shared ReadOnly RightToLeftProperty As DependencyProperty = DependencyProperty.Register("RightToLeft", GetType(Boolean), GetType(TransitioningContentControl), New PropertyMetadata(Nothing))
#End Region

#Region "Routed Events"
    Public Shared ReadOnly ContentChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent("ContentChanged", RoutingStrategy.Bubble, GetType(RoutedEventHandler), GetType(TransitioningContentControl))

    Public Custom Event ContentChanged As RoutedEventHandler
        AddHandler(ByVal value As RoutedEventHandler)
            Me.AddHandler(ContentChangedEvent, value)
        End AddHandler

        RemoveHandler(ByVal value As RoutedEventHandler)
            Me.RemoveHandler(ContentChangedEvent, value)
        End RemoveHandler

        RaiseEvent(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Me.RaiseEvent(e)
        End RaiseEvent
    End Event

    Private Sub RaiseContentChangedEvent()
        Dim newEventArgs As New RoutedEventArgs(TransitioningContentControl.ContentChangedEvent)
        MyBase.RaiseEvent(newEventArgs)
    End Sub
#End Region

#Region "Properties"
    Public Property FadeIn As Boolean
        Get
            Return CBool(GetValue(FadeInProperty))
        End Get

        Set(ByVal value As Boolean)
            SetValue(FadeInProperty, value)
        End Set
    End Property

    Public Property FadeInTimeout As Double
        Get
            Return CDbl(GetValue(FadeInTimeoutProperty))
        End Get

        Set(ByVal value As Double)
            SetValue(FadeInTimeoutProperty, value)
        End Set
    End Property

    Public Property SlideIn As Boolean
        Get
            Return CBool(GetValue(SlideInProperty))
        End Get

        Set(ByVal value As Boolean)
            SetValue(SlideInProperty, value)
        End Set
    End Property

    Public Property SlideInTimeout As Double
        Get
            Return CDbl(GetValue(SlideInTimeoutProperty))
        End Get

        Set(ByVal value As Double)
            SetValue(SlideInTimeoutProperty, value)
        End Set
    End Property

    Public Property SlideInFrom As Integer
        Get
            Return CInt(GetValue(SlideInFromProperty))
        End Get

        Set(ByVal value As Integer)
            SetValue(SlideInFromProperty, value)
        End Set
    End Property

    Public Property SlideInTo As Integer
        Get
            Return CInt(GetValue(SlideInToProperty))
        End Get

        Set(ByVal value As Integer)
            SetValue(SlideInToProperty, value)
        End Set
    End Property

    Public Property RightToLeft As Boolean
        Get
            Return CBool(GetValue(RightToLeftProperty))
        End Get

        Set(ByVal value As Boolean)
            SetValue(RightToLeftProperty, value)
        End Set
    End Property
#End Region

#Region "Functions"
    Protected Overrides Sub OnContentChanged(oldContent As Object, newContent As Object)
        Me.DoAnimation()
    End Sub

    Private Sub DoAnimation()
        If Me.FadeInTimeout <> Nothing AndAlso Me.FadeIn Then
            Dim da As New DoubleAnimation
            da.From = 0
            da.To = 1
            da.Duration = New Duration(TimeSpan.FromSeconds(Me.FadeInTimeout))
            Me.BeginAnimation(OpacityProperty, da)
        End If

        If Me.SlideInTimeout <> Nothing AndAlso Me.SlideInTimeout > 0 AndAlso Me.SlideIn Then

            If Not Me.RightToLeft Then
                Dim ta As New ThicknessAnimation
                ta.From = New Thickness(Me.SlideInFrom, Me.Margin.Top, 2 * Me.SlideInTo - Me.SlideInFrom, Me.Margin.Bottom)
                ta.To = New Thickness(Me.SlideInTo, Me.Margin.Top, Me.SlideInTo, Me.Margin.Bottom)
                ta.Duration = New Duration(TimeSpan.FromSeconds(Me.SlideInTimeout))
                Me.BeginAnimation(MarginProperty, ta)
            Else
                Dim ta As New ThicknessAnimation
                ta.From = New Thickness(2 * Me.SlideInTo - Me.SlideInFrom, Me.Margin.Top, Me.SlideInFrom, Me.Margin.Bottom)
                ta.To = New Thickness(Me.SlideInTo, Me.Margin.Top, Me.SlideInTo, Me.Margin.Bottom)
                ta.Duration = New Duration(TimeSpan.FromSeconds(Me.SlideInTimeout))
                Me.BeginAnimation(MarginProperty, ta)
            End If
        End If

        If Me.mTimer IsNot Nothing Then
            Me.mTimer.Stop()
            RemoveHandler Me.mTimer.Elapsed, New ElapsedEventHandler(AddressOf Me.TimerElapsedHandler)
        End If

        Me.mTimer = New Timer

        Dim biggestTimeout As Double = Me.SlideInTimeout

        If Me.FadeInTimeout > Me.SlideInTimeout Then
            biggestTimeout = Me.FadeInTimeout
        End If

        Me.mTimer.Interval = TimeSpan.FromSeconds(biggestTimeout).TotalMilliseconds

        AddHandler Me.mTimer.Elapsed, New ElapsedEventHandler(AddressOf Me.TimerElapsedHandler)

        Me.mTimer.Start()
    End Sub

    Private Sub TimerElapsedHandler(sender As Object, e As ElapsedEventArgs)
        Me.mTimer.Stop()

        Try
            Application.Current.Dispatcher.BeginInvoke(Sub() Me.RaiseContentChangedEvent())
        Catch ex As Exception

        End Try
        
    End Sub
#End Region
End Class
