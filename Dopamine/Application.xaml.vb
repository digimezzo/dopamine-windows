Imports System.ServiceModel
Imports System.Threading
Imports System.Windows.Shell
Imports System.Windows.Threading
Imports Dopamine.Common.Services.Command
Imports Dopamine.Common.Services.File
Imports Dopamine.Core.Base
Imports Dopamine.Core.Database
Imports Dopamine.Core.IO
Imports Dopamine.Core.Logging
Imports Dopamine.Core.Settings
Imports Dopamine.Views

Class Application

#Region "Variables"
    Private instanceMutex As Mutex = Nothing
#End Region

#Region "Functions"
    Protected Overrides Sub OnStartup(e As StartupEventArgs)
        MyBase.OnStartup(e)

        ' Create a jumplist and assign it to the current application
        JumpList.SetJumpList(Application.Current, New JumpList)

        ' Check that there is only one instance of the application running
        Dim iIsNewInstance As Boolean = False
        instanceMutex = New Mutex(True, String.Format("{0}-{1}", ProductInformation.ApplicationGuid, ProductInformation.AssemblyVersion.ToString), iIsNewInstance)

        ' Process the commandline arguments
        Me.ProcessCommandLineArguments(iIsNewInstance)

        If iIsNewInstance Then
            instanceMutex.ReleaseMutex()
            Me.ExecuteStartup()
        Else
            LogClient.Instance.Logger.Warn("Dopamine is already running. Shutting down.")

            Me.Shutdown()
        End If
    End Sub

    Private Sub ExecuteStartup()

        LogClient.Instance.Logger.Info("### STARTING {0}, version {1}, IsPortable = {2} ###", ProductInformation.ApplicationDisplayName, ProductInformation.FormattedAssemblyVersion, XmlSettingsClient.Instance.Get(Of Boolean)("Application", "IsPortable"))

        ' Handler for unhandled AppDomain exceptions
        AddHandler AppDomain.CurrentDomain.UnhandledException, New UnhandledExceptionEventHandler(AddressOf CurrentDomain_UnhandledException)

        ' Initialize the Entity Framework Metadata asynchronously
        Utils.InitializeEntityFrameworkMetaDataAsync()

        ' Show the Splash Window
        Dim splashWin As Window = New Splash
        splashWin.Show()
    End Sub

    Private Sub ProcessCommandLineArguments(iIsNewInstance As Boolean)

        ' Get the commandline arguments
        Dim args As String() = Environment.GetCommandLineArgs()

        If args.Length > 1 Then

            LogClient.Instance.Logger.Info("Found commandline arguments.")

            Select Case args(1)
                Case "/donate"
                    LogClient.Instance.Logger.Info("Detected DonateCommand from JumpList.")

                    Try
                        Actions.TryOpenLink(args(2))
                    Catch ex As Exception
                        LogClient.Instance.Logger.Error("Could not open the link {0} in Internet Explorer. Exception: {1}", args(2), ex.Message)
                    End Try
                    Me.Shutdown()
                Case Else

                    LogClient.Instance.Logger.Info("Processing Non-JumpList commandline arguments.")

                    If Not iIsNewInstance Then

                        ' Send the commandline arguments to the running instance
                        Me.TrySendCommandlineArguments(args)
                    Else
                        ' Do nothing. The commandline arguments of a single instance will be processed,
                        ' in the ShellViewModel because over there we have access to the FileService.
                    End If
            End Select
        Else
            ' When started without command line arguments, and when not the first instance: try to show the running instance.
            If Not iIsNewInstance Then Me.TryShowRunningInstance()
        End If
    End Sub

    Private Sub TryShowRunningInstance()

        Dim commandServiceProxy As ICommandService
        Dim commandServiceFactory As New ChannelFactory(Of ICommandService)(New StrongNetNamedPipeBinding(), New EndpointAddress(String.Format("net.pipe://localhost/{0}/CommandService/CommandServiceEndpoint", ProductInformation.ApplicationDisplayName)))

        Try
            commandServiceProxy = commandServiceFactory.CreateChannel
            commandServiceProxy.ShowMainWindowCommand()
            LogClient.Instance.Logger.Info("Trying to show the running instance")
        Catch ex As Exception
            LogClient.Instance.Logger.Error("A problem occured while trying to show the running instance. Exception: {0}", ex.Message)
        End Try
    End Sub

    Private Sub TrySendCommandlineArguments(iArgs() As String)

        LogClient.Instance.Logger.Info("Trying to send {0} commandline arguments to the running instance", iArgs.Count)

        Dim needsSending As Boolean = True
        Dim startTime As DateTime = Now

        Dim fileServiceProxy As IFileService
        Dim fileServiceFactory As New ChannelFactory(Of IFileService)(New StrongNetNamedPipeBinding(), New EndpointAddress(String.Format("net.pipe://localhost/{0}/FileService/FileServiceEndpoint", ProductInformation.ApplicationDisplayName)))

        While needsSending

            Try
                ' Try to send the commandline arguments to the running instance
                fileServiceProxy = fileServiceFactory.CreateChannel()
                fileServiceProxy.ProcessArguments(iArgs)
                LogClient.Instance.Logger.Info("Sent {0} commandline arguments to the running instance", iArgs.Count)

                needsSending = False
            Catch ex As Exception
                If TypeOf ex Is EndpointNotFoundException Then

                    ' When selecting multiple files, the first file is opened by the first instance.
                    ' This instance takes some time to start. To avoid an EndpointNotFoundException
                    ' when sending the second file to the first instance, we wait 10 ms repetitively,
                    ' until there is an endpoint to talk to.
                    System.Threading.Thread.Sleep(10)
                Else
                    ' Log any other Exception and stop trying to send the file to the running instance
                    needsSending = False
                    LogClient.Instance.Logger.Info("A problem occured while trying to send {0} commandline arguments to the running instance. Exception: {1}", iArgs.Count, ex.Message)
                End If
            End Try

            ' This makes sure we don't try to send for longer than 30 seconds, 
            ' so this instance won't stay open forever.
            If Convert.ToInt64(Now.Subtract(startTime).TotalSeconds) > 30 Then
                needsSending = False
            End If
        End While
    End Sub

    Private Sub CurrentDomain_UnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
        Dim ex As Exception = TryCast(e.ExceptionObject, Exception)

        ' Log the exception and stop the application
        Me.ExecuteEmergencyStop(ex)
    End Sub

    Private Sub App_DispatcherUnhandledException(ByVal sender As Object, ByVal e As DispatcherUnhandledExceptionEventArgs)

        ' Prevent default unhandled exception processing
        e.Handled = True

        ' Log the exception and stop the application
        Me.ExecuteEmergencyStop(e.Exception)
    End Sub

    Private Sub ExecuteEmergencyStop(iException As Exception)

        ' This is a workaround for a bug in the .Net framework, which randomly causes a System.ArgumentNullException when
        ' scrolling through a Virtualizing StackPanel. Scroll to playing song sometimes triggers this bug. We catch the
        ' Exception here, and do nothing with it. The application can just proceed. This prevents a complete crash.
        ' This might be fixed in .Net 4.5.2. See here: https://connect.microsoft.com/VisualStudio/feedback/details/789438/scrolling-in-virtualized-wpf-treeview-is-very-unstable
        If iException.GetType().ToString.Equals("System.ArgumentNullException") And iException.Source.ToString.Equals("PresentationCore") Then
            LogClient.Instance.Logger.Warn("Avoided Unhandled Exception: {0}", iException.ToString)
            Return
        End If

        LogClient.Instance.Logger.Error("Unhandled Exception. {0}", LogClient.GetAllExceptions(iException))

        ' Close the application to prevent further problems
        LogClient.Instance.Logger.Info("### FORCED STOP of {0}, version {1} ###", ProductInformation.ApplicationDisplayName, ProductInformation.FormattedAssemblyVersion)

        ' Emergency save of the settings
        XmlSettingsClient.Instance.Write()

        Application.Current.Shutdown()
    End Sub
#End Region
End Class
