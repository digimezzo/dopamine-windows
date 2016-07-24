Imports Dopamine.Common.Services.Dialog
Imports Dopamine.Core.Utils
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Prism.Mvvm
Imports Microsoft.Practices.Unity

Namespace ViewModels
    Public Class InformationAboutViewModel
        Inherits BindableBase

#Region "Variables"
        Private mContainer As IUnityContainer
        Private mDialogService As IDialogService
#End Region

#Region "Commands"
        Public Property ShowLicenseCommand As DelegateCommand
#End Region

#Region "Construction"
        Public Sub New(iContainer As IUnityContainer, iDialogService As IDialogService)

            Me.mContainer = iContainer
            Me.mDialogService = iDialogService

            Me.ShowLicenseCommand = New DelegateCommand(Sub()

                                                            Dim view = Me.mContainer.Resolve(Of InformationAboutLicense)()

                                                            Me.mDialogService.ShowCustomDialog(&HE73E,
                                                                       16,
                                                                       ResourceUtils.GetStringResource("Language_License"),
                                                                       view,
                                                                       400,
                                                                       0,
                                                                       False,
                                                                       False,
                                                                       ResourceUtils.GetStringResource("Language_Ok"),
                                                                       String.Empty,
                                                                       Nothing)
                                                        End Sub)
        End Sub
#End Region
    End Class
End Namespace
