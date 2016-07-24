Imports System.Data.SQLite
Imports Dopamine.Core.Database
Imports Dopamine.Core.Logging
Imports Dopamine.Core.Settings

Namespace Views
    Public Class Splash
        Inherits Window

#Region "Variables"
        Private mUIWaitMilliSeconds As Integer = 300
        Private mErrorMessage As String
#End Region

#Region "Properties"
        Public Property ShowErrorPanel As Boolean
            Get
                Return CBool(GetValue(ShowErrorPanelProperty))
            End Get

            Set(ByVal value As Boolean)
                SetValue(ShowErrorPanelProperty, value)
            End Set
        End Property

        Public Property ShowProgressRing As Boolean
            Get
                Return CBool(GetValue(ShowProgressRingProperty))
            End Get

            Set(ByVal value As Boolean)
                SetValue(ShowProgressRingProperty, value)
            End Set
        End Property
#End Region

#Region "Dependency Properties"
        Public Shared ReadOnly ShowErrorPanelProperty As DependencyProperty = DependencyProperty.Register("ShowErrorPanel", GetType(Boolean), GetType(Splash), New PropertyMetadata(Nothing))
        Public Shared ReadOnly ShowProgressRingProperty As DependencyProperty = DependencyProperty.Register("ShowProgressRing", GetType(Boolean), GetType(Splash), New PropertyMetadata(Nothing))
#End Region

#Region "Construction"
        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
        End Sub
#End Region

#Region "private"
        Private Sub ShowError(iMessage As String)
            Me.ErrorMessage.Text = iMessage
            Me.ShowErrorPanel = True
        End Sub

        Private Sub ShowErrorDetails()
            Dim currentTime As DateTime = Now
            Dim currentTimeString As String = currentTime.Year & currentTime.Month & currentTime.Day & currentTime.Hour & currentTime.Minute & currentTime.Second & currentTime.Millisecond

            Dim path As String = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Dopamine_" & currentTimeString & ".txt")
            System.IO.File.WriteAllText(path, mErrorMessage)
            System.Diagnostics.Process.Start(path)
        End Sub

        Private Async Sub Initialize()

            Dim continueInitializing As Boolean = True

            ' Give the UI some time to show the progress ring
            Await Task.Delay(Me.mUIWaitMilliSeconds)

            If continueInitializing Then
                ' Initialize the settings
                continueInitializing = Await Me.InitializeSettingsAsync
            End If

            If continueInitializing Then
                ' Initialize the database
                continueInitializing = Await Me.InitializeDatabaseAsync
            End If

            If continueInitializing Then
                ' If initializing was successful, start the application.

                If Me.ShowProgressRing Then

                    Me.ShowProgressRing = False

                    ' Give the UI some time to hide the progress ring
                    Await Task.Delay(Me.mUIWaitMilliSeconds)
                End If

                Dim bootstrapper As New Bootstrapper()
                bootstrapper.Run()
                Me.Close()
            Else
                Me.ShowError("I was not able to start. Please click 'Show details' for more information.")
            End If
        End Sub

        Private Async Function InitializeSettingsAsync() As Task(Of Boolean)

            Dim isInitializeSettingsSuccess As Boolean

            Try
                ' Checks if an upgrade of the settings is needed
                If XmlSettingsClient.Instance.IsSettingsUpgradeNeeded Then
                    Me.ShowProgressRing = True
                    LogClient.Instance.Logger.Info("Upgrading settings")
                    Await Task.Run(Sub() XmlSettingsClient.Instance.UpgradeSettings())
                End If

                isInitializeSettingsSuccess = True
            Catch ex As Exception
                LogClient.Instance.Logger.Error("There was a problem initializing the settings. Exception: {0}", ex.Message)
                mErrorMessage = ex.Message
                isInitializeSettingsSuccess = False
            End Try

            Return isInitializeSettingsSuccess
        End Function

        Private Async Function InitializeDatabaseAsync() As Task(Of Boolean)

            Dim isInitializeDatabaseSuccess As Boolean

            Try
                Dim needsMetadataInit As Boolean = False

                Dim con As New SQLiteConnection(DbConnection.ConnectionString)
                Dim dbm As New DbCreator(con)

                If Not DbCreator.DatabaseExists Then
                    ' Create the database if it doesn't exist
                    Me.ShowProgressRing = True
                    LogClient.Instance.Logger.Info("Creating database")
                    Await Task.Run(Sub() dbm.InitializeNewDatabase())

                    needsMetadataInit = True
                Else
                    ' Upgrade the database if it is not the latest version
                    If dbm.DatabaseNeedsUpgrade Then

                        Me.ShowProgressRing = True
                        LogClient.Instance.Logger.Info("Upgrading database")
                        Await Task.Run(Sub() dbm.UpgradeDatabase())

                        needsMetadataInit = True
                    End If
                End If

                If needsMetadataInit Then
                    Me.ShowProgressRing = True
                    LogClient.Instance.Logger.Info("Initializing EntityFramework MetaData")
                    Await Utils.InitializeEntityFrameworkMetaDataAsync()
                End If

                isInitializeDatabaseSuccess = True
            Catch ex As Exception
                LogClient.Instance.Logger.Error("There was a problem initializing the database. Exception: {0}", ex.Message)
                mErrorMessage = ex.Message
                isInitializeDatabaseSuccess = False
            End Try

            Return isInitializeDatabaseSuccess
        End Function

        Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
            Me.Initialize()
        End Sub

        Private Sub BtnQuit_Click(sender As Object, e As RoutedEventArgs)
            Application.Current.Shutdown()
        End Sub

        Private Sub BtnShowDetails_Click(sender As Object, e As RoutedEventArgs)

            Me.ShowErrorDetails()
        End Sub
#End Region

    End Class
End Namespace



