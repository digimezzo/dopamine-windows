Imports Dopamine.Common.Services.Indexing
Imports Dopamine.Common.Services.Update
Imports Dopamine.Core
Imports Dopamine.Core.Base
Imports Dopamine.Core.IO
Imports Dopamine.Core.Logging
Imports Dopamine.Core.Settings
Imports Dopamine.Core.Utils
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Prism.Mvvm

Namespace ViewModels
    Public Class StatusViewModel
        Inherits BindableBase

#Region "Variables"
        ' Services
        Private mIndexingService As IIndexingService
        Private mUpdateService As IUpdateService

        ' Indexing
        Private mIsIndexing As Boolean
        Private mIndexingProgress As String

        Private mIsIndexerScanningFiles As Boolean
        Private mIsIndexerRemovingSongs As Boolean
        Private mIsIndexerAddingSongs As Boolean
        Private mIsIndexerUpdatingSongs As Boolean
        Private mIsIndexerUpdatingArtwork As Boolean

        ' Update status
        Private mIsUpdateAvailable As Boolean
        Private mVersionInfo As VersionInfo
        Private mDestinationPath As String
        Private mUpdateToolTip As String
        Private mIsUpdateStatusHiddenByUser As Boolean
        Private mShowInstallUpdateButton As Boolean
#End Region

#Region "Properties"
        ' Status bar
        Public ReadOnly Property IsStatusBarVisible() As Boolean
            Get
                Return mIsIndexing Or mIsUpdateAvailable
            End Get
        End Property

        ' Indexing
        Public Property IsIndexing() As Boolean
            Get
                Return mIsIndexing
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsIndexing, value)
                OnPropertyChanged(Function() Me.IsStatusBarVisible)
            End Set
        End Property

        Public Property IndexingProgress() As String
            Get
                Return mIndexingProgress
            End Get
            Set(ByVal value As String)
                SetProperty(Of String)(mIndexingProgress, value)
            End Set
        End Property

        Public Property IsIndexerRemovingSongs() As Boolean
            Get
                Return mIsIndexerRemovingSongs
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsIndexerRemovingSongs, value)
            End Set
        End Property

        Public Property IsIndexerAddingSongs() As Boolean
            Get
                Return mIsIndexerAddingSongs
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsIndexerAddingSongs, value)
            End Set
        End Property

        Public Property IsIndexerUpdatingSongs() As Boolean
            Get
                Return mIsIndexerUpdatingSongs
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsIndexerUpdatingSongs, value)
            End Set
        End Property

        Public Property IsIndexerUpdatingArtwork() As Boolean
            Get
                Return mIsIndexerUpdatingArtwork
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsIndexerUpdatingArtwork, value)
            End Set
        End Property

        ' Update status
        Public Property IsUpdateAvailable() As Boolean
            Get
                Return mIsUpdateAvailable
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsUpdateAvailable, value)
                OnPropertyChanged(Function() Me.IsStatusBarVisible)
            End Set
        End Property

        Public Property VersionInfo() As VersionInfo
            Get
                Return mVersionInfo
            End Get
            Set(ByVal value As VersionInfo)
                SetProperty(Of VersionInfo)(mVersionInfo, value)
            End Set
        End Property

        Public Property UpdateToolTip() As String
            Get
                Return mUpdateToolTip
            End Get
            Set(ByVal value As String)
                SetProperty(Of String)(mUpdateToolTip, value)
            End Set
        End Property

        Public ReadOnly Property ShowInstallUpdateButton() As Boolean
            Get
                Return Not String.IsNullOrEmpty(mDestinationPath)
            End Get
        End Property
#End Region

#Region "Commands"
        Public Property DownloadOrInstallUpdateCommand As DelegateCommand
        Public Property HideUpdateStatusCommand As DelegateCommand
#End Region

#Region "Construction"
        Public Sub New(iUpdateService As IUpdateService, iIndexingService As IIndexingService)

            mIndexingService = iIndexingService
            mUpdateService = iUpdateService

            Me.DownloadOrInstallUpdateCommand = New DelegateCommand(AddressOf Me.DownloadOrInstallUpdate)
            Me.HideUpdateStatusCommand = New DelegateCommand(Sub()
                                                                 mIsUpdateStatusHiddenByUser = True
                                                                 Me.IsUpdateAvailable = False
                                                             End Sub)

            AddHandler mIndexingService.IndexingStatusChanged, Async Sub(iIndexingStatusEventArgs) Await IndexingService_IndexingStatusChangedAsync(iIndexingStatusEventArgs)
            AddHandler mIndexingService.IndexingStopped, AddressOf IndexingService_IndexingStopped

            AddHandler mUpdateService.NewDownloadedVersionAvailable, AddressOf NewVersionAvailableHandler
            AddHandler mUpdateService.NewOnlineVersionAvailable, AddressOf NewVersionAvailableHandler
            AddHandler mUpdateService.NoNewVersionAvailable, AddressOf NoNewVersionAvailableHandler
            AddHandler mUpdateService.UpdateCheckDisabled, Sub() Me.IsUpdateAvailable = False

            If XmlSettingsClient.Instance.Get(Of Boolean)("Updates", "CheckForUpdates") Then
                mUpdateService.EnableUpdateCheck()
            End If

            ' Initial status
            Me.IsIndexing = False
            Me.IsUpdateAvailable = False
        End Sub
#End Region

#Region "Event Handlers"
        Private Async Function IndexingService_IndexingStatusChangedAsync(iIndexingStatusEventArgs As IndexingStatusEventArgs) As Task

            Await Task.Run(Sub()
                               Me.IsIndexing = mIndexingService.IsIndexing

                               If Me.IsIndexing Then

                                   mUpdateService.DisableUpdateCheck()
                                   Me.IsUpdateAvailable = False

                                   Select Case iIndexingStatusEventArgs.IndexingAction
                                       Case IndexingAction.RemoveTracks
                                           Me.IsIndexerRemovingSongs = True
                                           Me.IsIndexerAddingSongs = False
                                           Me.IsIndexerUpdatingSongs = False
                                           Me.IsIndexerUpdatingArtwork = False
                                           Me.IndexingProgress = String.Empty
                                       Case IndexingAction.AddTracks
                                           Me.IsIndexerRemovingSongs = False
                                           Me.IsIndexerAddingSongs = True
                                           Me.IsIndexerUpdatingSongs = False
                                           Me.IsIndexerUpdatingArtwork = False
                                           Me.IndexingProgress = "(" & Replace(Replace(ResourceUtils.GetStringResource("Language_Current_Of_Total"), "%current%", iIndexingStatusEventArgs.ProgressCurrent.ToString), "%total%", iIndexingStatusEventArgs.ProgressTotal.ToString) & ")"
                                       Case IndexingAction.UpdateTracks
                                           Me.IsIndexerRemovingSongs = False
                                           Me.IsIndexerAddingSongs = False
                                           Me.IsIndexerUpdatingSongs = True
                                           Me.IsIndexerUpdatingArtwork = False
                                           Me.IndexingProgress = "(" & Replace(Replace(ResourceUtils.GetStringResource("Language_Current_Of_Total"), "%current%", iIndexingStatusEventArgs.ProgressCurrent.ToString), "%total%", iIndexingStatusEventArgs.ProgressTotal.ToString) & ")"
                                       Case IndexingAction.UpdateArtwork
                                           Me.IsIndexerRemovingSongs = False
                                           Me.IsIndexerAddingSongs = False
                                           Me.IsIndexerUpdatingSongs = False
                                           Me.IsIndexerUpdatingArtwork = True
                                           Me.IndexingProgress = String.Empty
                                       Case Else
                                           ' Never happens
                                   End Select
                               Else
                                   Me.IndexingProgress = String.Empty

                                   If XmlSettingsClient.Instance.Get(Of Boolean)("Updates", "CheckForUpdates") Then
                                       mUpdateService.EnableUpdateCheck()
                                   End If
                               End If
                           End Sub)
        End Function

        Private Sub IndexingService_IndexingStopped(sender As Object, e As EventArgs)

            If Me.IsIndexing Then
                Me.IsIndexing = False
                Me.IndexingProgress = String.Empty

                If XmlSettingsClient.Instance.Get(Of Boolean)("Updates", "CheckForUpdates") Then
                    mUpdateService.EnableUpdateCheck()
                End If
            End If
        End Sub
#End Region

#Region "Private"
        Private Overloads Sub NewVersionAvailableHandler(iVersionInfo As VersionInfo)
            Me.NewVersionAvailableHandler(iVersionInfo, String.Empty)
        End Sub

        Private Overloads Sub NewVersionAvailableHandler(iVersionInfo As VersionInfo, iDestinationPath As String)

            If Not mIsUpdateStatusHiddenByUser AndAlso Not Me.IsIndexing Then
                Me.VersionInfo = iVersionInfo
                Me.IsUpdateAvailable = True

                mDestinationPath = iDestinationPath
                OnPropertyChanged(Function() Me.ShowInstallUpdateButton)

                If Not String.IsNullOrEmpty(iDestinationPath) Then
                    Me.UpdateToolTip = ResourceUtils.GetStringResource("Language_Click_Here_To_Install")
                Else
                    Me.UpdateToolTip = ResourceUtils.GetStringResource("Language_Click_Here_To_Download")
                End If
            End If
        End Sub

        Private Overloads Sub NoNewVersionAvailableHandler(iVersionInfo As VersionInfo)

            Me.IsUpdateAvailable = False
        End Sub

        Private Sub DownloadOrInstallUpdate()

            If Not String.IsNullOrEmpty(mDestinationPath) Then
                Try
                    ' A file was downloaded. Start the installer.
                    Dim msiFileInfo As System.IO.FileInfo = New System.IO.DirectoryInfo(mDestinationPath).GetFiles("*" & PackagingInformation.GetInstallablePackageFileExtesion).First
                    Process.Start(msiFileInfo.FullName)
                Catch ex As Exception
                    LogClient.Instance.Logger.Error("Could not start the MSI installer. Download link was opened instead. Exception: {0}", ex.Message)
                    Me.OpenDownloadLink()
                End Try
            Else
                ' Nothing was downloaded, forward to the download site.
                Me.OpenDownloadLink()
            End If
        End Sub

        Private Sub OpenDownloadLink()
            Try
                Dim downloadLink As String = String.Empty

                If Me.VersionInfo.Configuration = Configuration.Debug Then
                    downloadLink = UpdateInformation.PreReleaseDownloadLink
                ElseIf Me.VersionInfo.Configuration = Configuration.Release Then
                    downloadLink = UpdateInformation.ReleaseDownloadLink
                End If

                Actions.TryOpenLink(downloadLink)
            Catch ex As Exception
                LogClient.Instance.Logger.Error("Could not open the download link. Exception: {0}", ex.Message)
            End Try
        End Sub
#End Region
    End Class
End Namespace
