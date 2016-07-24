Imports Microsoft.Practices.Prism.Mvvm
Imports Microsoft.Practices.Prism.PubSubEvents
Imports Dopamine.Core.Prism
Imports Dopamine.Common.Services.Appearance
Imports System.Windows.Media.Animation
Imports Dopamine.Common.Services.Indexing
Imports RaphaelGodart.Controls
Imports Microsoft.Practices.Prism.Regions
Imports Dopamine.OobeModule.Views
Imports Dopamine.Core.Settings
Imports Dopamine.Common.Controls

Namespace Views
    Public Class Oobe
        Inherits DopamineWindow
        Implements IView

#Region "Variables"
        Private mEventAggregator As IEventAggregator
        Private mAppearanceService As IAppearanceService
        Private mBackgroundAnimation As Storyboard
        Private mIndexingService As IIndexingService
        Private mRegionManager As IRegionManager
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
        Public Sub New(iEventAggregator As IEventAggregator, iAppearanceService As IAppearanceService, iIndexingService As IIndexingService, iRegionManager As IRegionManager)

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            Me.mEventAggregator = iEventAggregator
            Me.mAppearanceService = iAppearanceService
            Me.mIndexingService = iIndexingService
            Me.mRegionManager = iRegionManager

            Me.mEventAggregator.GetEvent(Of CloseOobeEvent).Subscribe(Sub()
                                                                          Me.Close()
                                                                      End Sub)

            AddHandler Me.mAppearanceService.ThemeChanged, AddressOf Me.ThemeChangedHandler
        End Sub
#End Region

#Region "Event handlers"
        Public Overrides Sub OnApplyTemplate()
            MyBase.OnApplyTemplate()

            ' Retrieve BackgroundAnimation storyboard
            Me.mBackgroundAnimation = TryCast(Me.WindowBorder.Resources("BackgroundAnimation"), Storyboard)

            If Me.mBackgroundAnimation IsNot Nothing Then
                Me.mBackgroundAnimation.Begin()
            End If
        End Sub

        Private Sub MetroWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs)

            ' Prevent the Oobe window from appearing the next time the application is started
            XmlSettingsClient.Instance.Set(Of Boolean)("General", "ShowOobe", False)

            ' Closing the Oobe windows, must show the main window
            Application.Current.MainWindow.Show()

            ' We're closeing the OOBE screen, tell the IndexingService to start.
            Me.mIndexingService.IndexCollectionAsync(XmlSettingsClient.Instance.Get(Of Boolean)("Indexing", "IgnoreRemovedFiles"), False)
        End Sub

        Private Sub ThemeChangedHandler(sender As Object, e As EventArgs)
            If Me.mBackgroundAnimation IsNot Nothing Then
                Me.mBackgroundAnimation.Begin()
            End If
        End Sub

        Private Sub BorderlessWindow_Loaded(sender As Object, e As RoutedEventArgs)
            ShowWelcome()
        End Sub
#End Region

#Region "Private"
        Private Async Sub ShowWelcome()

            ' Make sure that the regions are initialized
            While Me.mRegionManager.Regions(RegionNames.OobeAppNameRegion) Is Nothing
                Await Task.Delay(100)
            End While

            Await Task.Delay(500)
            Me.mRegionManager.RequestNavigate(RegionNames.OobeAppNameRegion, GetType(OobeAppName).FullName)
            Await Task.Delay(500)
            Me.mRegionManager.RequestNavigate(RegionNames.OobeContentRegion, GetType(OobeWelcome).FullName)
            Await Task.Delay(500)
            Me.mRegionManager.RequestNavigate(RegionNames.OobeControlsRegion, GetType(OobeControls).FullName)
        End Sub
#End Region

    End Class
End Namespace
