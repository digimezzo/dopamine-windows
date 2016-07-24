Imports Microsoft.Practices.Prism.Mvvm
Imports Dopamine.Core.Settings
Imports System.Collections.ObjectModel
Imports Dopamine.Common.Services.I18n
Imports Dopamine.Core.Base

Namespace ViewModels
    Public Class SettingsAppearanceLanguageViewModel
        Inherits BindableBase

#Region "Variables"
        Private mI18nService As II18nService
        Private mLanguages As ObservableCollection(Of Language)
        Private mSelectedLanguage As Language
#End Region

#Region "Properties"
        Public Property Languages() As ObservableCollection(Of Language)
            Get
                Return Me.mLanguages
            End Get
            Set(ByVal value As ObservableCollection(Of Language))
                SetProperty(Of ObservableCollection(Of Language))(Me.mLanguages, value)
            End Set
        End Property

        Public Property SelectedLanguage() As Language
            Get
                Return Me.mSelectedLanguage
            End Get
            Set(ByVal value As Language)
                SetProperty(Of Language)(Me.mSelectedLanguage, value)

                If value IsNot Nothing Then
                    XmlSettingsClient.Instance.Set(Of String)("Appearance", "Language", value.Code)
                    Application.Current.Dispatcher.Invoke(Sub() mI18nService.ApplyLanguageAsync(value.Code))
                End If

            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New(iI18nService As II18nService)

            mI18nService = iI18nService

            Me.GetLanguagesAsync()

            AddHandler mI18nService.LanguagesChanged, Sub() Me.GetLanguagesAsync()
        End Sub
#End Region

#Region "private"
        Private Async Sub GetLanguagesAsync()

            Dim languagesList As List(Of Language) = mI18nService.GetLanguages()

            Dim localLanguages As New ObservableCollection(Of Language)

            Await Task.Run(Sub()
                               For Each lang As Language In languagesList
                                   localLanguages.Add(lang)
                               Next
                           End Sub)

            Me.Languages = localLanguages

            Dim tempLanguage As Language = Nothing

            Await Task.Run(Sub()
                               Dim savedLanguageCode As String = XmlSettingsClient.Instance.Get(Of String)("Appearance", "Language")

                               If Not String.IsNullOrEmpty(savedLanguageCode) Then
                                   tempLanguage = mI18nService.GetLanguage(savedLanguageCode)
                               End If

                               ' If, for some reason, SelectedLanguage is Nothing (e.g. when the user 
                               ' deleted a previously existing language file), select the default language.
                               If tempLanguage Is Nothing Then
                                   tempLanguage = mI18nService.GetDefaultLanguage()
                               End If
                           End Sub)

            ' Only set SelectedLanguage when we are sure that it is not Nothing. Otherwise this could trigger strange 
            ' behaviour in the setter of the SelectedLanguage Property (because the "value" would be Nothing)
            Me.SelectedLanguage = tempLanguage
        End Sub
#End Region
    End Class

End Namespace