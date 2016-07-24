Imports Microsoft.Practices.Prism.Mvvm
Imports Microsoft.Practices.Prism
Imports Microsoft.Practices.Prism.PubSubEvents
Imports Dopamine.Core.Prism
Imports Microsoft.Practices.Prism.Regions

Namespace ViewModels
    Public Class OobeFinishViewModel
        Inherits BindableBase
        Implements IActiveAware
        Implements INavigationAware


#Region "Variables"
        Private mIsActive As Boolean
        Private mEventAggregator As IEventAggregator
#End Region

#Region "Properties"
        Public Property IsActive() As Boolean Implements IActiveAware.IsActive
            Get
                Return mIsActive
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mIsActive, value)
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New(iEventAggregator As IEventAggregator)
            Me.mEventAggregator = iEventAggregator
        End Sub
#End Region

#Region "Events"
        Public Event IsActiveChanged(sender As Object, e As EventArgs) Implements IActiveAware.IsActiveChanged
#End Region

#Region "Functions"
        Public Function IsNavigationTarget(navigationContext As NavigationContext) As Boolean Implements INavigationAware.IsNavigationTarget
            Return True
        End Function

        Public Sub OnNavigatedFrom(navigationContext As NavigationContext) Implements INavigationAware.OnNavigatedFrom

        End Sub

        Public Sub OnNavigatedTo(navigationContext As NavigationContext) Implements INavigationAware.OnNavigatedTo
            Me.mEventAggregator.GetEvent(Of OobeNavigatedToEvent)().Publish(GetType(OobeFinishViewModel).FullName)
        End Sub
#End Region
    End Class
End Namespace

