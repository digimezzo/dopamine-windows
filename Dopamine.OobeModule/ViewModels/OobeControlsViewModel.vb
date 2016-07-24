Imports Microsoft.Practices.Prism.Mvvm
Imports Microsoft.Practices.Prism.Regions
Imports Microsoft.Practices.Prism.Commands
Imports Dopamine.Core.Prism
Imports Dopamine.OobeModule.Views
Imports Microsoft.Practices.Prism.PubSubEvents
Imports Microsoft.Practices.Unity
Imports Digimezzo.WPFControls.Enums

Namespace ViewModels
    Public Class OobeControlsViewModel
        Inherits BindableBase

#Region "Variables"
        Private ReadOnly mRegionManager As IRegionManager
        Private mContainer As IUnityContainer
        Private mEventAggregator As IEventAggregator
        Private mShowPreviousButton As Boolean
        Private mActiveViewModelName As String
        Private mIsDone As Boolean
#End Region

#Region "Properties"
        Public Property PreviousCommand As DelegateCommand
        Public Property NextCommand As DelegateCommand

        Public Property ShowPreviousButton() As Boolean
            Get
                Return mShowPreviousButton
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mShowPreviousButton, value)
            End Set
        End Property

        Public Property IsDone() As Boolean
            Get
                Return mIsDone
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mIsDone, value)
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New(iContainer As IUnityContainer, iRegionManager As IRegionManager, iEventAggregator As IEventAggregator)
            Me.mRegionManager = iRegionManager
            Me.mContainer = iContainer
            Me.mEventAggregator = iEventAggregator

            Me.IsDone = False
            Me.ShowPreviousButton = False

            Me.PreviousCommand = New DelegateCommand(Sub()
                                                         Me.mEventAggregator.GetEvent(Of ChangeOobeSlideDirectionEvent)().Publish(SlideDirection.LeftToRight)

                                                         Select Case Me.mActiveViewModelName
                                                             Case GetType(OobeFinishViewModel).FullName
                                                                 Me.mRegionManager.RequestNavigate(RegionNames.OobeContentRegion, GetType(OobeDonate).FullName)
                                                             Case GetType(OobeDonateViewModel).FullName
                                                                 Me.mRegionManager.RequestNavigate(RegionNames.OobeContentRegion, GetType(OobeCollection).FullName)
                                                             Case GetType(OobeCollectionViewModel).FullName
                                                                 Me.mRegionManager.RequestNavigate(RegionNames.OobeContentRegion, GetType(OobeAppearance).FullName)
                                                             Case GetType(OobeAppearanceViewModel).FullName
                                                                 Me.mRegionManager.RequestNavigate(RegionNames.OobeContentRegion, GetType(OobeLanguage).FullName)
                                                             Case GetType(OobeLanguageViewModel).FullName
                                                                 Me.mRegionManager.RequestNavigate(RegionNames.OobeContentRegion, GetType(OobeWelcome).FullName)
                                                         End Select
                                                     End Sub)

            Me.NextCommand = New DelegateCommand(Sub()
                                                     Me.mEventAggregator.GetEvent(Of ChangeOobeSlideDirectionEvent)().Publish(SlideDirection.RightToLeft)

                                                     Select Case Me.mActiveViewModelName
                                                         Case GetType(OobeLanguageViewModel).FullName
                                                             Me.mRegionManager.RequestNavigate(RegionNames.OobeContentRegion, GetType(OobeAppearance).FullName)
                                                         Case GetType(OobeAppearanceViewModel).FullName
                                                             Me.mRegionManager.RequestNavigate(RegionNames.OobeContentRegion, GetType(OobeCollection).FullName)
                                                         Case GetType(OobeCollectionViewModel).FullName
                                                             Me.mRegionManager.RequestNavigate(RegionNames.OobeContentRegion, GetType(OobeDonate).FullName)
                                                         Case GetType(OobeDonateViewModel).FullName
                                                             Me.mRegionManager.RequestNavigate(RegionNames.OobeContentRegion, GetType(OobeFinish).FullName)
                                                         Case GetType(OobeFinishViewModel).FullName
                                                             ' Close the OOBE window
                                                             Me.mEventAggregator.GetEvent(Of CloseOobeEvent).Publish("")
                                                         Case Else
                                                             ' OobeWelcomeViewModel is not navigated to when the OOBE window is shown. So this is handled here.
                                                             Me.mRegionManager.RequestNavigate(RegionNames.OobeContentRegion, GetType(OobeLanguage).FullName)
                                                     End Select
                                                 End Sub)

            Me.mEventAggregator.GetEvent(Of OobeNavigatedToEvent)().Subscribe(Sub(iActiveViewModelName)
                                                                                  Me.mActiveViewModelName = iActiveViewModelName

                                                                                  Select Case iActiveViewModelName
                                                                                      Case GetType(OobeWelcomeViewModel).FullName
                                                                                          Me.ShowPreviousButton = False
                                                                                      Case GetType(OobeLanguageViewModel).FullName
                                                                                          Me.ShowPreviousButton = False
                                                                                      Case Else
                                                                                          Me.ShowPreviousButton = True
                                                                                  End Select

                                                                                  Select Case iActiveViewModelName
                                                                                      Case GetType(OobeFinishViewModel).FullName
                                                                                          Me.IsDone = True
                                                                                      Case Else
                                                                                          Me.IsDone = False
                                                                                  End Select
                                                                              End Sub)
        End Sub
#End Region
    End Class
End Namespace

