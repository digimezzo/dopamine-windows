Imports Microsoft.Practices.Prism.Mvvm
Imports Microsoft.Practices.Prism.Regions
Imports Microsoft.Practices.Prism.Commands
Imports Dopamine.Core.Prism

Namespace ViewModels
    Public Class InformationViewModel
        Inherits BindableBase

#Region "Variables"
        Private ReadOnly mRegionManager As IRegionManager
        Private mPreviousIndex As Integer = 0
        Private mSlideInFrom As Integer
#End Region

#Region "Properties"
        Public NavigateBetweenInformationCommand As DelegateCommand(Of String)

        Public Property SlideInFrom() As Integer
            Get
                Return mSlideInFrom
            End Get
            Set(ByVal value As Integer)
                SetProperty(Of Integer)(mSlideInFrom, value)
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New(iRegionManager As IRegionManager)
            Me.mRegionManager = iRegionManager
            Me.NavigateBetweenInformationCommand = New DelegateCommand(Of String)(AddressOf NavigateBetweenInformation)
            ApplicationCommands.NavigateBetweenInformationCommand.RegisterCommand(Me.NavigateBetweenInformationCommand)
            Me.SlideInFrom = 30
        End Sub
#End Region

#Region "Functions"
        Private Sub NavigateBetweenInformation(iIndex As String)

            If String.IsNullOrWhiteSpace(iIndex) Then Return

            Dim index As Integer = 0

            Integer.TryParse(iIndex, index)

            If index = 0 Then Return

            Me.SlideInFrom = If(index <= mPreviousIndex, -30, 30)

            mPreviousIndex = index

            mRegionManager.RequestNavigate(RegionNames.InformationRegion, Me.GetPageForIndex(index))
        End Sub

        Private Function GetPageForIndex(iIndex As Integer) As String

            Dim page As String = String.Empty

            Select Case iIndex
                Case 1
                    page = GetType(Views.InformationHelp).FullName
                Case 2
                    page = GetType(Views.InformationAbout).FullName
                Case Else
                    page = GetType(Views.InformationHelp).FullName
            End Select

            Return page
        End Function
#End Region
    End Class
End Namespace
