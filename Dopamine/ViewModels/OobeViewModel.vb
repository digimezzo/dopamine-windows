Imports Digimezzo.WPFControls.Enums
Imports Dopamine.Common.Services.Dialog
Imports Dopamine.Core.Prism
Imports Microsoft.Practices.Prism.Mvvm
Imports Microsoft.Practices.Prism.PubSubEvents

Namespace ViewModels
    Public Class OobeViewModel
        Inherits BindableBase

#Region "Variables"
        Private mDialogService As IDialogService
        Private mEventAggregator As IEventAggregator
        Private mIsOverlayVisible As Boolean
        Private mSlideDirection As SlideDirection
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

        Public Property SlideDirection() As SlideDirection
            Get
                Return mSlideDirection
            End Get
            Set(ByVal value As SlideDirection)
                SetProperty(Of SlideDirection)(Me.mSlideDirection, value)
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New(iDialogService As IDialogService, iEventAggregator As IEventAggregator)

            Me.mDialogService = iDialogService
            Me.mEventAggregator = iEventAggregator

            AddHandler Me.mDialogService.DialogVisibleChanged, Sub(iIsDialogVisible)
                                                                   Me.IsOverlayVisible = iIsDialogVisible
                                                               End Sub

            Me.mEventAggregator.GetEvent(Of ChangeOobeSlideDirectionEvent)().Subscribe(Sub(iSlideDirection)
                                                                                           Me.SlideDirection = iSlideDirection
                                                                                       End Sub)

            ' Initial slide direction
            Me.SlideDirection = SlideDirection.RightToLeft
        End Sub
#End Region
    End Class
End Namespace
