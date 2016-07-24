Imports Dopamine.Common.Presentation.Views
Imports Dopamine.ControlsModule.Views
Imports Dopamine.Core.Prism
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Prism.Mvvm
Imports Microsoft.Practices.Prism.Regions

Namespace ViewModels
    Public Class MainScreenViewModel
        Inherits BindableBase

#Region "Variables"
        Private ReadOnly mRegionManager As IRegionManager
        Private mPreviousIndex As Integer = 0
        Private mSubMenuSlideInFrom As Integer
        Private mSearchSlideInFrom As Integer
        Private mContentSlideInFrom As Integer
#End Region

#Region "Properties"
        Public Property NavigateBetweenMainCommand As DelegateCommand(Of String)

        Public Property SubMenuSlideInFrom() As Integer
            Get
                Return mSubMenuSlideInFrom
            End Get
            Set(ByVal value As Integer)
                SetProperty(Of Integer)(mSubMenuSlideInFrom, value)
            End Set
        End Property

        Public Property SearchSlideInFrom() As Integer
            Get
                Return mSearchSlideInFrom
            End Get
            Set(ByVal value As Integer)
                SetProperty(Of Integer)(mSearchSlideInFrom, value)
            End Set
        End Property

        Public Property ContentSlideInFrom() As Integer
            Get
                Return mContentSlideInFrom
            End Get
            Set(ByVal value As Integer)
                SetProperty(Of Integer)(mContentSlideInFrom, value)
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New(iRegionManager As IRegionManager)
            Me.mRegionManager = iRegionManager

            Me.NavigateBetweenMainCommand = New DelegateCommand(Of String)(Sub(iIndex) Me.NavigateBetweenMain(iIndex))
            ApplicationCommands.NavigateBetweenMainCommand.RegisterCommand(Me.NavigateBetweenMainCommand)
            Me.SubMenuSlideInFrom = 30
            Me.SearchSlideInFrom = 30
            Me.ContentSlideInFrom = 30
        End Sub
#End Region

#Region "Private"
        Private Sub NavigateBetweenMain(iIndex As String)

            If String.IsNullOrWhiteSpace(iIndex) Then Return

            Dim index As Integer = 0

            Integer.TryParse(iIndex, index)

            If index = 0 Then Return

            Me.SubMenuSlideInFrom = If(index <= mPreviousIndex, 0, 30)
            Me.SearchSlideInFrom = If(index <= mPreviousIndex, -30, 30)
            Me.ContentSlideInFrom = If(index <= mPreviousIndex, -30, 30)

            mPreviousIndex = index

            If index = 2 Then
                ' Settings
                Me.mRegionManager.RequestNavigate(RegionNames.ContentRegion, GetType(Dopamine.SettingsModule.Views.Settings).FullName)
                Me.mRegionManager.RequestNavigate(RegionNames.SubMenuRegion, GetType(Dopamine.SettingsModule.Views.SettingsSubMenu).FullName)
                Me.mRegionManager.RequestNavigate(RegionNames.FullPlayerSearchRegion, GetType(Empty).FullName)
            ElseIf index = 3 Then
                ' Information
                Me.mRegionManager.RequestNavigate(RegionNames.ContentRegion, GetType(Dopamine.InformationModule.Views.Information).FullName)
                Me.mRegionManager.RequestNavigate(RegionNames.SubMenuRegion, GetType(Dopamine.InformationModule.Views.InformationSubMenu).FullName)
                Me.mRegionManager.RequestNavigate(RegionNames.FullPlayerSearchRegion, GetType(Empty).FullName)
            Else
                ' Collection
                Me.mRegionManager.RequestNavigate(RegionNames.ContentRegion, GetType(Dopamine.CollectionModule.Views.Collection).FullName)
                Me.mRegionManager.RequestNavigate(RegionNames.SubMenuRegion, GetType(Dopamine.CollectionModule.Views.CollectionSubMenu).FullName)
                Me.mRegionManager.RequestNavigate(RegionNames.FullPlayerSearchRegion, GetType(SearchControl).FullName)
            End If
        End Sub
#End Region
    End Class
End Namespace
