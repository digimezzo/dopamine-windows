Imports System.Collections.ObjectModel
Imports Dopamine.Common.Services.Collection
Imports Dopamine.Common.Services.Dialog
Imports Dopamine.Common.Services.Indexing
Imports Dopamine.Core.Database.Entities
Imports Dopamine.Core.Database.Repositories.Interfaces
Imports Dopamine.Core.Logging
Imports Dopamine.Core.Settings
Imports Dopamine.Core.Utils
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Prism.Mvvm
Imports WPFFolderBrowser
Imports Dopamine.Core.Database

Namespace ViewModels
    Public Class SettingsCollectionFoldersViewModel
        Inherits BindableBase

#Region "Variables"
        Private mIndexingService As IIndexingService
        Private mDialogService As IDialogService
        Private mCollectionservice As ICollectionService
        Private mFolderRepository As IFolderRepository
        Private mFolders As ObservableCollection(Of FolderViewModel)
        Private mIsLoadingFolders As Boolean
        Private mShowAllFoldersInCollection As Boolean
        Private mIsIndexing As Boolean
#End Region

#Region "Commands"
        Public Property AddFolderCommand As DelegateCommand(Of String)
        Public Property RemoveFolderCommand As DelegateCommand(Of String)
        Public Property ShowInCollectionChangedCommand As DelegateCommand(Of String)
#End Region

#Region "Properties"
        Public ReadOnly Property IsBusy As Boolean
            Get
                Return Me.IsIndexing Or Me.IsLoadingFolders
            End Get
        End Property

        Public Property IsIndexing() As Boolean
            Get
                Return mIsIndexing
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mIsIndexing, value)
                OnPropertyChanged(Function() Me.IsBusy)
            End Set
        End Property

        Public Property Folders() As ObservableCollection(Of FolderViewModel)
            Get
                Return mFolders
            End Get
            Set(ByVal value As ObservableCollection(Of FolderViewModel))
                SetProperty(Of ObservableCollection(Of FolderViewModel))(Me.mFolders, value)
            End Set
        End Property

        Public Property IsLoadingFolders() As Boolean
            Get
                Return mIsLoadingFolders
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsLoadingFolders, value)
                OnPropertyChanged(Function() Me.IsBusy)
            End Set
        End Property

        Public Property ShowAllFoldersInCollection() As Boolean
            Get
                Return mShowAllFoldersInCollection
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mShowAllFoldersInCollection, value)

                If value Then
                    Me.ForceShowAllFoldersInCollection()
                End If

                XmlSettingsClient.Instance.Set(Of Boolean)("Indexing", "ShowAllFoldersInCollection", value)
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New(iIndexingService As IIndexingService, iDialogService As IDialogService, iCollectionservice As ICollectionService, iFolderRepository As IFolderRepository)

            mIndexingService = iIndexingService
            mDialogService = iDialogService
            mCollectionservice = iCollectionservice
            mFolderRepository = iFolderRepository

            Me.AddFolderCommand = New DelegateCommand(Of String)(Sub()
                                                                     Me.AddFolder()
                                                                 End Sub)

            Me.RemoveFolderCommand = New DelegateCommand(Of String)(Sub(iPath)
                                                                        If Me.mDialogService.ShowConfirmation(&HE11B,
                                                                                                              16,
                                                                                                              ResourceUtils.GetStringResource("Language_Remove"),
                                                                                                              ResourceUtils.GetStringResource("Language_Confirm_Remove_Folder"),
                                                                                                              ResourceUtils.GetStringResource("Language_Yes"),
                                                                                                              ResourceUtils.GetStringResource("Language_No")) Then
                                                                            Me.RemoveFolder(iPath)
                                                                        End If
                                                                    End Sub)

            Me.ShowInCollectionChangedCommand = New DelegateCommand(Of String)(Sub(iPath)
                                                                                   Me.ShowAllFoldersInCollection = False

                                                                                   SyncLock Me.Folders
                                                                                       mCollectionservice.MarkFolderAsync(Me.Folders.Select(Function(f) f.Folder).Where(Function(f) f.Path.ToLower.Equals(iPath.ToLower)).FirstOrDefault)
                                                                                   End SyncLock
                                                                               End Sub)

            Me.ShowAllFoldersInCollection = XmlSettingsClient.Instance.Get(Of Boolean)("Indexing", "ShowAllFoldersInCollection")

            ' Makes sure Me.IsIndexng is set if this ViewModel is created after the Indexer has started indexing
            If Me.mIndexingService.IsIndexing Then Me.IsIndexing = True

            ' These events handle changes of Indexer status after the ViewModel is created
            AddHandler Me.mIndexingService.IndexingStarted, Sub() Me.IsIndexing = True
            AddHandler Me.mIndexingService.IndexingStopped, Sub() Me.IsIndexing = False

            Me.GetFoldersAsync()
        End Sub
#End Region

#Region "Private"
        Private Async Sub AddFolder()
            LogClient.Instance.Logger.Info("Adding a folder to the collection.")

            Dim dlg As New WPFFolderBrowserDialog

            If dlg.ShowDialog Then

                Try
                    Me.IsLoadingFolders = True
                    Dim result As Integer = Await mFolderRepository.AddFolderAsync(New Folder With {.Path = dlg.FileName, .ShowInCollection = 1})
                    Me.IsLoadingFolders = False

                    Select Case result
                        Case AddFolderResult.Success
                            mIndexingService.NeedsIndexing = True
                            Me.GetFoldersAsync()
                        Case AddFolderResult.Error
                            Me.mDialogService.ShowNotification(&HE711,
                                                               16,
                                                               ResourceUtils.GetStringResource("Language_Error"),
                                                               ResourceUtils.GetStringResource("Language_Error_Adding_Folder"),
                                                               ResourceUtils.GetStringResource("Language_Ok"),
                                                               True,
                                                               ResourceUtils.GetStringResource("Language_Log_File"))
                        Case AddFolderResult.Duplicate

                            Me.mDialogService.ShowNotification(&HE711,
                                                               16,
                                                               ResourceUtils.GetStringResource("Language_Already_Exists"),
                                                               ResourceUtils.GetStringResource("Language_Folder_Already_In_Collection"),
                                                               ResourceUtils.GetStringResource("Language_Ok"),
                                                               False, "")
                    End Select
                Catch ex As Exception
                    LogClient.Instance.Logger.Error("Exception: {0}", ex.Message)

                    Me.mDialogService.ShowNotification(&HE711,
                                                       16,
                                                       ResourceUtils.GetStringResource("Language_Error"),
                                                       ResourceUtils.GetStringResource("Language_Error_Adding_Folder"),
                                                       ResourceUtils.GetStringResource("Language_Ok"),
                                                       True,
                                                       ResourceUtils.GetStringResource("Language_Log_File"))
                Finally
                    Me.IsLoadingFolders = False
                End Try
            End If
        End Sub

        Private Async Sub RemoveFolder(iPath As String)

            Try
                Me.IsLoadingFolders = True
                Dim result As Integer = Await mFolderRepository.RemoveFolderAsync(iPath)
                Me.IsLoadingFolders = False

                Select Case result
                    Case AddFolderResult.Success
                        mIndexingService.NeedsIndexing = True
                        Me.GetFoldersAsync()
                    Case AddFolderResult.Error
                        Me.mDialogService.ShowNotification(&HE711,
                                                           16,
                                                           ResourceUtils.GetStringResource("Language_Error"),
                                                           ResourceUtils.GetStringResource("Language_Error_Removing_Folder"),
                                                           ResourceUtils.GetStringResource("Language_Ok"),
                                                           True,
                                                           ResourceUtils.GetStringResource("Language_Log_File"))
                End Select
            Catch ex As Exception
                LogClient.Instance.Logger.Error("Exception: {0}", ex.Message)

                Me.mDialogService.ShowNotification(&HE711,
                                                   16,
                                                   ResourceUtils.GetStringResource("Language_Error"),
                                                   ResourceUtils.GetStringResource("Language_Error_Removing_Folder"),
                                                   ResourceUtils.GetStringResource("Language_Ok"),
                                                   True,
                                                   ResourceUtils.GetStringResource("Language_Log_File"))
            Finally
                Me.IsLoadingFolders = False
            End Try
        End Sub

        Private Async Sub GetFoldersAsync()

            Me.IsLoadingFolders = True

            Dim foldersList As List(Of Folder) = Await mFolderRepository.GetFoldersAsync()

            Dim localFolders As New ObservableCollection(Of FolderViewModel)

            Await Task.Run(Sub()
                               For Each fol As Folder In foldersList
                                   localFolders.Add(New FolderViewModel With {.Folder = fol})
                               Next
                           End Sub)

            Me.IsLoadingFolders = False

            Me.Folders = localFolders
        End Sub

        Private Async Sub ForceShowAllFoldersInCollection()

            If Me.Folders Is Nothing Then Return

            Await Task.Run(Sub()
                               SyncLock Me.Folders
                                   For Each fol As FolderViewModel In Me.Folders
                                       fol.ShowInCollection = True
                                       mCollectionservice.MarkFolderAsync(fol.Folder)
                                   Next
                               End SyncLock
                           End Sub)
        End Sub
#End Region
    End Class
End Namespace
