Imports Microsoft.Practices.Prism.Mvvm
Imports Dopamine.Core.Settings
Imports System.Collections.ObjectModel
Imports Dopamine.Common.Services.Appearance
Imports Dopamine.Core.IO
Imports Dopamine.Core.Base

Namespace ViewModels
    Public Class SettingsAppearanceThemeViewModel
        Inherits BindableBase

#Region "Variables"
        Private mAppearanceService As IAppearanceService
        Private mThemes As New ObservableCollection(Of String)
        Private mSelectedTheme As String
        Private mColorSchemes As New ObservableCollection(Of ColorScheme)
        Private mSelectedColorScheme As ColorScheme
        Private mCheckBoxWindowsColorChecked As Boolean
        Private mCheckBoxThemeChecked As Boolean
#End Region

#Region "Properties"
        Public Property Themes() As ObservableCollection(Of String)
            Get
                Return Me.mThemes
            End Get
            Set(ByVal value As ObservableCollection(Of String))
                SetProperty(Of ObservableCollection(Of String))(Me.mThemes, value)
            End Set
        End Property

        Public Property CheckBoxThemeChecked() As Boolean
            Get
                Return Me.mCheckBoxThemeChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Appearance", "EnableLightTheme", value)
                Application.Current.Dispatcher.Invoke(Sub() mAppearanceService.ApplyTheme(value))
                SetProperty(Of Boolean)(Me.mCheckBoxThemeChecked, value)
            End Set
        End Property

        Public Property ColorSchemes() As ObservableCollection(Of ColorScheme)
            Get
                Return Me.mColorSchemes
            End Get
            Set(ByVal value As ObservableCollection(Of ColorScheme))
                SetProperty(Of ObservableCollection(Of ColorScheme))(Me.mColorSchemes, value)
            End Set
        End Property

        Public Property SelectedColorScheme() As ColorScheme
            Get
                Return Me.mSelectedColorScheme
            End Get
            Set(ByVal value As ColorScheme)

                ' value can be Nothing when a ColorScheme is removed from the ColorSchemes directory
                If value IsNot Nothing Then
                    XmlSettingsClient.Instance.Set(Of String)("Appearance", "ColorScheme", value.Name)
                    Application.Current.Dispatcher.Invoke(Sub() mAppearanceService.ApplyColorScheme(XmlSettingsClient.Instance.Get(Of Boolean)("Appearance", "FollowWindowsColor"), value.Name))
                End If

                SetProperty(Of ColorScheme)(Me.mSelectedColorScheme, value)
            End Set
        End Property

        Public Property CheckBoxWindowsColorChecked() As Boolean
            Get
                Return Me.mCheckBoxWindowsColorChecked
            End Get
            Set(ByVal value As Boolean)

                XmlSettingsClient.Instance.Set(Of Boolean)("Appearance", "FollowWindowsColor", value)
                Application.Current.Dispatcher.Invoke(Sub() mAppearanceService.ApplyColorScheme(value, XmlSettingsClient.Instance.Get(Of String)("Appearance", "ColorScheme")))

                SetProperty(Of Boolean)(Me.mCheckBoxWindowsColorChecked, value)
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New(iAppearanceService As IAppearanceService)
            mAppearanceService = iAppearanceService

            Me.GetColorSchemesAsync()
            Me.GetCheckBoxesAsync()

            AddHandler mAppearanceService.ColorSchemesChanged, AddressOf ColorSchemesChangedHandler
        End Sub
#End Region

#Region "Private"
        Private Async Sub GetColorSchemesAsync()

            Dim localColorSchemes As New ObservableCollection(Of ColorScheme)

            Await Task.Run(Sub()
                               For Each cs As ColorScheme In mAppearanceService.GetColorSchemes()
                                   localColorSchemes.Add(cs)
                               Next
                           End Sub)

            Me.ColorSchemes = localColorSchemes

            Dim savedColorSchemeName As String = XmlSettingsClient.Instance.Get(Of String)("Appearance", "ColorScheme")

            If Not String.IsNullOrEmpty(savedColorSchemeName) Then
                Me.SelectedColorScheme = mAppearanceService.GetColorScheme(savedColorSchemeName)
            Else
                Me.SelectedColorScheme = mAppearanceService.GetColorSchemes().Item(0)
            End If
        End Sub

        Private Async Sub GetCheckBoxesAsync()

            Await Task.Run(Sub()
                               Me.CheckBoxWindowsColorChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Appearance", "FollowWindowsColor")
                               Me.CheckBoxThemeChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Appearance", "EnableLightTheme")
                           End Sub)
        End Sub
#End Region

#Region "Event handlers"
        Private Sub ColorSchemesChangedHandler(sender As Object, e As EventArgs)
            Application.Current.Dispatcher.Invoke(Sub() Me.GetColorSchemesAsync())
        End Sub
#End Region
    End Class
End Namespace
