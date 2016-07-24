Imports Microsoft.Practices.Prism.Mvvm
Imports System.Collections.ObjectModel
Imports Dopamine.Common.Services.Appearance
Imports Dopamine.Core.Settings
Imports Dopamine.Core.IO
Imports Dopamine.Core.Base
Imports Dopamine.Common.Services.I18n
Imports Dopamine.Common.Services.Playback
Imports Microsoft.Practices.Prism.PubSubEvents
Imports Dopamine.Core.Prism
Imports Dopamine.Core.Utils
Imports Dopamine.Core

Namespace ViewModels
    Public Class SettingsAppearanceViewModel
        Inherits BindableBase

#Region "Variables"
        Private mPlaybackService As IPlaybackService
        Private mCheckBoxShowSpectrumAnalyzerChecked As Boolean
        Private mCheckBoxCheckBoxShowWindowBorderChecked As Boolean
        Private mCheckBoxEnableTransparencyChecked As Boolean
        Private mEventAggregator As IEventAggregator
#End Region

#Region "Construction"
        Public Sub New(iPlaybackService As IPlaybackService, iEventAggregator As IEventAggregator)

            mPlaybackService = iPlaybackService
            mEventAggregator = iEventAggregator

            Me.GetCheckBoxesAsync()
        End Sub
#End Region

#Region "Properties"
        Public Property ColorSchemesDirectory As String = System.IO.Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, ApplicationPaths.ColorSchemesSubDirectory)

        Public Property CheckBoxCheckBoxShowWindowBorderChecked() As Boolean
            Get
                Return Me.mCheckBoxCheckBoxShowWindowBorderChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Appearance", "ShowWindowBorder", value)
                SetProperty(Of Boolean)(Me.mCheckBoxCheckBoxShowWindowBorderChecked, value)
                Application.Current.Dispatcher.Invoke(Sub() mEventAggregator.GetEvent(Of SettingShowWindowBorderChanged)().Publish(value))
            End Set
        End Property

        Public Property CheckBoxShowSpectrumAnalyzerChecked() As Boolean
            Get
                Return Me.mCheckBoxShowSpectrumAnalyzerChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Playback", "ShowSpectrumAnalyzer", value)
                SetProperty(Of Boolean)(Me.mCheckBoxShowSpectrumAnalyzerChecked, value)
                mPlaybackService.IsSpectrumVisible = value
            End Set
        End Property

        Public Property CheckBoxEnableTransparencyChecked() As Boolean
            Get
                Return Me.mCheckBoxEnableTransparencyChecked
            End Get
            Set(ByVal value As Boolean)
                XmlSettingsClient.Instance.Set(Of Boolean)("Appearance", "EnableTransparency", value)
                SetProperty(Of Boolean)(Me.mCheckBoxEnableTransparencyChecked, value)
            End Set
        End Property

        Public ReadOnly Property IsWindows10() As Boolean
            Get
                Return EnvironmentUtils.IsWindows10
            End Get
        End Property
#End Region

#Region "Private"
        Public Async Sub GetCheckBoxesAsync()

            Await Task.Run(Sub()
                               Me.CheckBoxShowSpectrumAnalyzerChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Playback", "ShowSpectrumAnalyzer")
                               Me.CheckBoxCheckBoxShowWindowBorderChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Appearance", "ShowWindowBorder")
                               Me.CheckBoxEnableTransparencyChecked = XmlSettingsClient.Instance.Get(Of Boolean)("Appearance", "EnableTransparency")
                           End Sub)
        End Sub
#End Region
    End Class
End Namespace