Imports System.Reflection
Imports System.Timers
Imports System.Windows.Media.Animation
Imports Dopamine.Common.Controls
Imports Dopamine.Common.Enums
Imports Dopamine.Common.Presentation.Views
Imports Dopamine.Common.Services.Appearance
Imports Dopamine.Common.Services.Metadata
Imports Dopamine.Common.Services.Notification
Imports Dopamine.Common.Services.Playback
Imports Dopamine.Common.Services.Win32Input
Imports Dopamine.Core.Base
Imports Dopamine.Core.Extensions
Imports Dopamine.Core.IO
Imports Dopamine.Core.Logging
Imports Dopamine.Core.Prism
Imports Dopamine.Core.Settings
Imports Dopamine.Core.Utils
Imports Dopamine.FullPlayerModule.Views
Imports Dopamine.MiniPlayerModule.Views
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Prism.Mvvm
Imports Microsoft.Practices.Prism.PubSubEvents
Imports Microsoft.Practices.Prism.Regions
Imports Microsoft.Practices.Unity

Namespace Views
    Class Shell
        Inherits DopamineWindow
        Implements IView

#Region "Variables"
        Private mAllowSaveWindowGeometry As Boolean = False
        Private mBackgroundAnimation As Storyboard
        Private mContainer As IUnityContainer
        Private ReadOnly mRegionManager As IRegionManager
        Private mAppearanceService As IAppearanceService
        Private mPlaybackService As IPlaybackService
        Private mWin32InputService As IWin32InputService
        Private mNotificationService As INotificationService
        Private mMetadataService As MetadataService
        Private mTrayIcon As System.Windows.Forms.NotifyIcon
        Private mTrayIconContextMenu As ContextMenu
        Private mIsMiniPlayer As Boolean
        Private mSelectedMiniPlayerType As MiniPlayerType
        Private mIsCoverPlayerListExpanded As Boolean
        Private mIsMicroPlayerListExpanded As Boolean
        Private mIsNanoPlayerListExpanded As Boolean
        Private mIsMiniPlayerPositionLocked As Boolean
        Private mIsMiniPlayerAlwaysOnTop As Boolean
        Private mEventAggregator As IEventAggregator
        Private mIsNowPlayingActive As Boolean
        Private mTrayControls As TrayControls
        Private mEnableWindowTransparencyStoryboard As Storyboard
        Private mDisableWindowTransparencyStoryboard As Storyboard
        Private mMiniPlayerPlaylist As Playlist
        Private mIsShuttingDown As Boolean
        Private mMustPerformClosingTasks As Boolean
#End Region

#Region "Commands"
        Public Property RestoreWindowCommand As DelegateCommand
        Public Property MinimizeWindowCommand As DelegateCommand
        Public Property MaximizeRestoreWindowCommand As DelegateCommand
        Public Property CloseWindowCommand As DelegateCommand
        Public Property ChangePlayerTypeCommand As DelegateCommand(Of String)
        Public Property ToggleMiniPlayerPositionLockedCommand As DelegateCommand
        Public Property ToggleMiniPlayerAlwaysOnTopCommand As DelegateCommand
        Public Property TaskbarItemInfoPlayCommand As DelegateCommand
        Public Property NavigateToMainScreenCommand As DelegateCommand
        Public Property NavigateToNowPlayingScreenCommand As DelegateCommand
        Public Property TogglePlayerCommand As DelegateCommand
        Public Property ShowMainWindowCommand As DelegateCommand
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
        Public Sub New(iContainer As IUnityContainer, iRegionManager As IRegionManager, iAppearanceService As IAppearanceService, iPlaybackService As IPlaybackService, iWin32InputService As IWin32InputService, iEventAggregator As IEventAggregator, iNotificationService As INotificationService, iMetadataService As MetadataService)

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

            ' Dependency injection
            mContainer = iContainer
            mRegionManager = iRegionManager
            mAppearanceService = iAppearanceService
            mPlaybackService = iPlaybackService
            mWin32InputService = iWin32InputService
            mEventAggregator = iEventAggregator
            mNotificationService = iNotificationService
            mMetadataService = iMetadataService

            ' Flags
            mMustPerformClosingTasks = True

            ' Window
            Me.InitializeWindow()

            ' Tray icon
            Me.InitializeTrayIcon()

            ' Services
            Me.InitializeServices()

            ' PubSub Events
            Me.InitializePubSubEvents()

            ' Commands
            Me.InitializeCommands()
        End Sub
#End Region

#Region "Private"
        Private Sub InitializeServices()

            ' IWin32InputService
            ' ------------------
            mWin32InputService.SetKeyboardHook() ' listen to media keys

            AddHandler mWin32InputService.MediaKeyNextPressed, Async Sub() Await mPlaybackService.PlayNextAsync()
            AddHandler mWin32InputService.MediaKeyPreviousPressed, Async Sub() Await mPlaybackService.PlayPreviousAsync()
            AddHandler mWin32InputService.MediaKeyPlayPressed, Async Sub() Await mPlaybackService.PlayOrPauseAsync()

            ' IAppearanceService
            ' ------------------
            AddHandler mAppearanceService.ThemeChanged, AddressOf Me.ThemeChangedHandler
        End Sub

        Private Sub InitializePubSubEvents()

            ' Window border
            ' -------------

            mEventAggregator.GetEvent(Of SettingShowWindowBorderChanged).Subscribe(Sub(iShowWindowBorder) Me.SetWindowBorder(iShowWindowBorder))

            ' Cover Player
            ' ------------
            mEventAggregator.GetEvent(Of CoverPlayerPlaylistButtonClicked).Subscribe(Sub(iIsPlaylistButtonChecked) Me.ToggleMiniPlayerPlaylist(MiniPlayerType.CoverPlayer, iIsPlaylistButtonChecked))

            ' Micro Player
            ' ------------
            mEventAggregator.GetEvent(Of MicroPlayerPlaylistButtonClicked).Subscribe(Sub(iIsPlaylistButtonChecked) Me.ToggleMiniPlayerPlaylist(MiniPlayerType.MicroPlayer, iIsPlaylistButtonChecked))

            ' Nano Player
            ' -----------
            mEventAggregator.GetEvent(Of NanoPlayerPlaylistButtonClicked).Subscribe(Sub(iIsPlaylistButtonChecked) Me.ToggleMiniPlayerPlaylist(MiniPlayerType.NanoPlayer, iIsPlaylistButtonChecked))

            ' Tray icon
            ' ---------
            mEventAggregator.GetEvent(Of SettingShowTrayIconChanged).Subscribe(Sub(iShowTrayIcon) mTrayIcon.Visible = iShowTrayIcon)
        End Sub

        Private Sub InitializeCommands()

            ' TaskbarItemInfo
            ' ---------------
            TaskbarItemInfoPlayCommand = New DelegateCommand(Async Sub() Await mPlaybackService.PlayOrPauseAsync())
            ApplicationCommands.TaskbarItemInfoPlayCommand.RegisterCommand(Me.TaskbarItemInfoPlayCommand)

            ' Window State
            ' ------------
            Me.MinimizeWindowCommand = New DelegateCommand(Sub() Me.WindowState = Windows.WindowState.Minimized)
            ApplicationCommands.MinimizeWindowCommand.RegisterCommand(Me.MinimizeWindowCommand)

            Me.RestoreWindowCommand = New DelegateCommand(Sub() Me.SetPlayer(False, MiniPlayerType.CoverPlayer))
            ApplicationCommands.RestoreWindowCommand.RegisterCommand(Me.RestoreWindowCommand)

            Me.MaximizeRestoreWindowCommand = New DelegateCommand(Sub()
                                                                      If Me.WindowState = Windows.WindowState.Maximized Then
                                                                          Me.WindowState = Windows.WindowState.Normal
                                                                      Else
                                                                          Me.WindowState = Windows.WindowState.Maximized
                                                                      End If
                                                                  End Sub)
            ApplicationCommands.MaximizeRestoreWindowCommand.RegisterCommand(Me.MaximizeRestoreWindowCommand)

            Me.CloseWindowCommand = New DelegateCommand(Sub() Me.Close())
            ApplicationCommands.CloseWindowCommand.RegisterCommand(Me.CloseWindowCommand)

            ' Player type
            ' -----------
            Me.ChangePlayerTypeCommand = New DelegateCommand(Of String)(Sub(iMiniPlayerType) Me.SetPlayer(True, CType(iMiniPlayerType, MiniPlayerType)))
            ApplicationCommands.ChangePlayerTypeCommand.RegisterCommand(Me.ChangePlayerTypeCommand)

            Me.TogglePlayerCommand = New DelegateCommand(Sub() Me.TogglePlayer())
            ApplicationCommands.TogglePlayerCommand.RegisterCommand(Me.TogglePlayerCommand)

            ' Mini Player
            ' -----------

            mIsMiniPlayerPositionLocked = XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "MiniPlayerPositionLocked")
            mIsMiniPlayerAlwaysOnTop = XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "MiniPlayerOnTop")

            Me.ToggleMiniPlayerPositionLockedCommand = New DelegateCommand(Sub()
                                                                               mIsMiniPlayerPositionLocked = Not mIsMiniPlayerPositionLocked
                                                                               Me.SetWindowPositionLocked(mIsMiniPlayer)
                                                                           End Sub)
            ApplicationCommands.ToggleMiniPlayerPositionLockedCommand.RegisterCommand(Me.ToggleMiniPlayerPositionLockedCommand)

            Me.ToggleMiniPlayerAlwaysOnTopCommand = New DelegateCommand(Sub()
                                                                            mIsMiniPlayerAlwaysOnTop = Not mIsMiniPlayerAlwaysOnTop
                                                                            Me.SetWindowAlwaysOnTop(mIsMiniPlayer)
                                                                        End Sub)
            ApplicationCommands.ToggleMiniPlayerAlwaysOnTopCommand.RegisterCommand(Me.ToggleMiniPlayerAlwaysOnTopCommand)

            ' Screens
            ' -------
            Me.NavigateToMainScreenCommand = New DelegateCommand(Sub()
                                                                     mIsNowPlayingActive = False
                                                                     Me.ShowWindowControls = True
                                                                     Me.mRegionManager.RequestNavigate(RegionNames.ScreenTypeRegion, GetType(MainScreen).FullName)
                                                                 End Sub)
            ApplicationCommands.NavigateToMainScreenCommand.RegisterCommand(Me.NavigateToMainScreenCommand)

            Me.NavigateToNowPlayingScreenCommand = New DelegateCommand(Sub()
                                                                           mIsNowPlayingActive = True
                                                                           Me.ShowWindowControls = False
                                                                           Me.mRegionManager.RequestNavigate(RegionNames.ScreenTypeRegion, GetType(NowPlayingScreen).FullName)
                                                                       End Sub)
            ApplicationCommands.NavigateToNowPlayingScreenCommand.RegisterCommand(Me.NavigateToNowPlayingScreenCommand)

            ' Application
            ' -----------

            Me.ShowMainWindowCommand = New DelegateCommand(Sub() Me.ActivateNow())
            ApplicationCommands.ShowMainWindowCommand.RegisterCommand(Me.ShowMainWindowCommand)
        End Sub

        Private Sub InitializeTrayIcon()
            mTrayIcon = New System.Windows.Forms.NotifyIcon
            mTrayIcon.Text = ProductInformation.ApplicationAssemblyName

            ' Reflection is needed to get the full path of the executable. Because when starting the application from the start menu
            ' without specifying the full path, the application fails to find the Tray icon and crashes here
            Dim iconPath As String = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ApplicationPaths.IconsSubDirectory, "Tray.ico")
            mTrayIcon.Icon = New System.Drawing.Icon(iconPath, System.Windows.Forms.SystemInformation.SmallIconSize)

            AddHandler mTrayIcon.MouseClick, New System.Windows.Forms.MouseEventHandler(AddressOf TrayIcon_MouseClick)
            AddHandler mTrayIcon.MouseDoubleClick, New System.Windows.Forms.MouseEventHandler(AddressOf TrayIcon_MouseDoubleClick)

            mTrayIconContextMenu = CType(Me.FindResource("TrayIconContextMenu"), ContextMenu)

            If XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "ShowTrayIcon") Then
                mTrayIcon.Visible = True
            End If
        End Sub

        Private Sub InitializeWindow()

            mTrayControls = mContainer.Resolve(Of Views.TrayControls)()

            mMiniPlayerPlaylist = mContainer.Resolve(Of Views.Playlist)(New DependencyOverride(GetType(DopamineWindow), Me))
            RegionManager.SetRegionManager(mMiniPlayerPlaylist, mRegionManager)
            RegionManager.UpdateRegions()

            mNotificationService.SetApplicationWindows(Me, mMiniPlayerPlaylist, mTrayControls)

            ' Handler
            AddHandler Me.Restored, AddressOf Shell_Restored

            ' Workaround to make sure the PART_MiniPlayerButton ToolTip also gets updated on a language change
            AddHandler Me.CloseToolTipChanged, AddressOf Shell_CloseToolTipChanged

            ' This makes sure the position and size of the window is correct and avoids jumping of 
            ' the window to the correct position when SetPlayer() is called later while starting up
            Me.SetPlayerType(
                      XmlSettingsClient.Instance.Get(Of Boolean)("General", "IsMiniPlayer"),
                      CType(XmlSettingsClient.Instance.Get(Of Integer)("General", "MiniPlayerType"), MiniPlayerType)
                      )
        End Sub

        Private Sub TogglePlayer()

            If mIsMiniPlayer Then
                ' Show the Full Player
                Me.SetPlayer(False, MiniPlayerType.CoverPlayer)
            Else
                ' Show the Mini Player, with the player type which is saved in the settings
                Me.SetPlayer(True, CType(XmlSettingsClient.Instance.Get(Of Integer)("General", "MiniPlayerType"), MiniPlayerType))
            End If
        End Sub

        Private Sub SetWindowPositionLocked(iIsMiniPlayer As Boolean)

            ' Only lock position when the mini player is active
            If iIsMiniPlayer Then
                Me.IsMovable = Not mIsMiniPlayerPositionLocked
            Else
                Me.IsMovable = True
            End If

        End Sub

        Private Sub SetWindowAlwaysOnTop(iIsMiniPlayer As Boolean)

            If iIsMiniPlayer Then
                Me.Topmost = mIsMiniPlayerAlwaysOnTop
            Else
                Me.Topmost = False
            End If
        End Sub

        Private Sub SetPlayer(iIsMiniPlayer As Boolean, iMiniPlayerType As MiniPlayerType)

            ' Clear the player's content for smoother Window resizing
            Me.ClearPlayerContent()

            ' Determine if the player position is locked
            Me.SetWindowPositionLocked(iIsMiniPlayer)

            ' Set the player type
            Me.SetPlayerType(iIsMiniPlayer, iMiniPlayerType)

            ' Set the content of the player window
            Me.SetPlayerContent(150)
        End Sub

        Private Sub SetPlayerType(iIsMiniPlayer As Boolean, iMiniPlayerType As MiniPlayerType)

            ' Save the player type in the settings
            XmlSettingsClient.Instance.Set(Of Boolean)("General", "IsMiniPlayer", iIsMiniPlayer)

            ' Only save the Mini Player Type in the settings if the current player is set to the Mini Player
            If iIsMiniPlayer Then XmlSettingsClient.Instance.Set(Of Integer)("General", "MiniPlayerType", iMiniPlayerType)

            ' Set the current player type
            mIsMiniPlayer = iIsMiniPlayer
            mSelectedMiniPlayerType = iMiniPlayerType

            ' Prevents saving window state and size to the Settings XML while switching players
            mAllowSaveWindowGeometry = False

            ' Sets the geometry of the player
            If iIsMiniPlayer Then

                PART_MiniPlayerButton.ToolTip = ResourceUtils.GetStringResource("Language_Restore")

                Select Case iMiniPlayerType
                    Case MiniPlayerType.CoverPlayer
                        Me.ClosingText.FontSize = Constants.MediumBackgroundFontSize
                        Me.SetMiniPlayer(MiniPlayerType.CoverPlayer, Constants.CoverPlayerWidth, Constants.CoverPlayerHeight, mIsCoverPlayerListExpanded)
                    Case MiniPlayerType.MicroPlayer
                        Me.ClosingText.FontSize = Constants.MediumBackgroundFontSize
                        Me.SetMiniPlayer(MiniPlayerType.MicroPlayer, Constants.MicroPlayerWidth, Constants.MicroPlayerHeight, mIsMicroPlayerListExpanded)
                    Case MiniPlayerType.NanoPlayer
                        Me.ClosingText.FontSize = Constants.SmallBackgroundFontSize
                        Me.SetMiniPlayer(MiniPlayerType.NanoPlayer, Constants.NanoPlayerWidth, Constants.NanoPlayerHeight, mIsNanoPlayerListExpanded)
                    Case Else
                        ' Doesn't happen
                End Select
            Else
                Me.ClosingText.FontSize = Constants.LargeBackgroundFontSize
                PART_MiniPlayerButton.ToolTip = ResourceUtils.GetStringResource("Language_Mini_Player")
                Me.SetFullPlayer()
            End If

            mAllowSaveWindowGeometry = True
        End Sub

        Private Sub ClearPlayerContent()
            mRegionManager.RequestNavigate(RegionNames.PlayerTypeRegion, GetType(Empty).FullName)
        End Sub

        Private Async Sub SetPlayerContent(Optional iDelayMilliseconds As Integer = 0)

            ' This delay makes sure the content of the window is shown only after the specified delay
            Await Task.Delay(iDelayMilliseconds)

            If mIsMiniPlayer Then
                Select Case mSelectedMiniPlayerType
                    Case MiniPlayerType.CoverPlayer
                        mRegionManager.RequestNavigate(RegionNames.PlayerTypeRegion, GetType(CoverPlayer).FullName)
                    Case MiniPlayerType.MicroPlayer
                        mRegionManager.RequestNavigate(RegionNames.PlayerTypeRegion, GetType(MicroPlayer).FullName)
                    Case MiniPlayerType.NanoPlayer
                        mRegionManager.RequestNavigate(RegionNames.PlayerTypeRegion, GetType(NanoPlayer).FullName)
                    Case Else
                        ' Doesn't happen
                End Select
            Else
                mRegionManager.RequestNavigate(RegionNames.PlayerTypeRegion, GetType(FullPlayer).FullName)
            End If
        End Sub

        Private Sub SaveWindowState()

            If mAllowSaveWindowGeometry Then
                If Me.WindowState = Windows.WindowState.Maximized Then
                    XmlSettingsClient.Instance.Set(Of Boolean)("FullPlayer", "IsMaximized", True)
                Else
                    XmlSettingsClient.Instance.Set(Of Boolean)("FullPlayer", "IsMaximized", False)
                End If
            End If
        End Sub

        Private Sub SaveWindowSize()

            If mAllowSaveWindowGeometry Then
                If Not mIsMiniPlayer And Not Me.WindowState = Windows.WindowState.Maximized Then
                    XmlSettingsClient.Instance.Set(Of Integer)("FullPlayer", "Width", CInt(Me.ActualWidth))
                    XmlSettingsClient.Instance.Set(Of Integer)("FullPlayer", "Height", CInt(Me.ActualHeight))
                End If
            End If
        End Sub

        Private Sub SaveWindowLocation()

            If mAllowSaveWindowGeometry Then

                If mIsMiniPlayer Then
                    XmlSettingsClient.Instance.Set(Of Integer)("MiniPlayer", "Top", CInt(Me.Top))
                    XmlSettingsClient.Instance.Set(Of Integer)("MiniPlayer", "Left", CInt(Me.Left))
                ElseIf Not mIsMiniPlayer And Not Me.WindowState = Windows.WindowState.Maximized Then
                    XmlSettingsClient.Instance.Set(Of Integer)("FullPlayer", "Top", CInt(Me.Top))
                    XmlSettingsClient.Instance.Set(Of Integer)("FullPlayer", "Left", CInt(Me.Left))
                End If
            End If
        End Sub

        Private Sub SetFullPlayer()

            mMiniPlayerPlaylist.Hide()

            Me.ResizeMode = Windows.ResizeMode.CanResize

            If Not mIsNowPlayingActive Then Me.ShowWindowControls = True

            If XmlSettingsClient.Instance.Get(Of Boolean)("FullPlayer", "IsMaximized") Then

                Me.WindowState = Windows.WindowState.Maximized
            Else
                Me.WindowState = Windows.WindowState.Normal

                Me.SetGeometry(XmlSettingsClient.Instance.Get(Of Integer)("FullPlayer", "Top"),
                               XmlSettingsClient.Instance.Get(Of Integer)("FullPlayer", "Left"),
                               XmlSettingsClient.Instance.Get(Of Integer)("FullPlayer", "Width"),
                               XmlSettingsClient.Instance.Get(Of Integer)("FullPlayer", "Height"))
            End If

            ' Set MinWidth and MinHeight AFTER SetGeometry(). This prevents flicker.
            Me.MinWidth = Constants.MinShellWidth
            Me.MinHeight = Constants.MinShellHeight

            ' The FullPlayer window is never TopMost
            Me.Topmost = False
        End Sub

        Private Sub SetMiniPlayerDimensions()

            Me.SetGeometry(XmlSettingsClient.Instance.Get(Of Integer)("MiniPlayer", "Top"),
                           XmlSettingsClient.Instance.Get(Of Integer)("MiniPlayer", "Left"),
                           Convert.ToInt32(Me.MinWidth),
                           Convert.ToInt32(Me.MinHeight))

            Me.SetWindowAlwaysOnTop(True)
        End Sub

        Private Sub SetMiniPlayer(iMiniPlayerType As MiniPlayerType, iPlayerWidth As Double, iPlayerHeight As Double, iIsMiniPlayerListExpanded As Boolean)

            ' Hide the playlist BEFORE changing window dimensions to avoid strange behaviour
            mMiniPlayerPlaylist.Hide()

            Me.WindowState = Windows.WindowState.Normal
            Me.ResizeMode = Windows.ResizeMode.CanMinimize
            Me.ShowWindowControls = False

            ' Set MinWidth and MinHeight BEFORE SetMiniPlayerDimensions(). This prevents flicker.
            If Me.HasBorder Then
                ' Correction to take into account the window border, otherwise the content 
                'misses 2px horizontally and vertically when displaying the window border
                Me.MinWidth = iPlayerWidth + 2
                Me.MinHeight = iPlayerHeight + 2
            Else
                Me.MinWidth = iPlayerWidth
                Me.MinHeight = iPlayerHeight
            End If

            Me.SetMiniPlayerDimensions()

            ' Show the playlist AFTER changing window dimensions to avoid strange behaviour
            If iIsMiniPlayerListExpanded Then mMiniPlayerPlaylist.Show(iMiniPlayerType)
        End Sub

        Private Sub ToggleMiniPlayerPlaylist(iMiniPlayerType As MiniPlayerType, iIsMiniPlayerListExpanded As Boolean)

            Select Case iMiniPlayerType
                Case MiniPlayerType.CoverPlayer
                    mIsCoverPlayerListExpanded = iIsMiniPlayerListExpanded
                Case MiniPlayerType.MicroPlayer
                    mIsMicroPlayerListExpanded = iIsMiniPlayerListExpanded
                Case MiniPlayerType.NanoPlayer
                    mIsNanoPlayerListExpanded = iIsMiniPlayerListExpanded
                Case Else
                    ' Shouldn't happen
            End Select

            If iIsMiniPlayerListExpanded Then
                mMiniPlayerPlaylist.Show(iMiniPlayerType)
            Else
                mMiniPlayerPlaylist.Hide()
            End If
        End Sub

        Private Sub Shell_Deactivated(sender As Object, e As EventArgs)
            mTrayIconContextMenu.IsOpen = False
        End Sub
#End Region

#Region "Event handlers"
        Private Sub Shell_MouseUp(sender As Object, e As MouseButtonEventArgs)
            mEventAggregator.GetEvent(Of ShellMouseUp).Publish(String.Empty)
        End Sub

        Private Sub Shell_ContentRendered(sender As Object, e As EventArgs)

            ' Corrects size of the window (taking into account the HasBorders Property which is now set) and sets the content of the player
            Me.SetPlayer(
                      XmlSettingsClient.Instance.Get(Of Boolean)("General", "IsMiniPlayer"),
                      CType(XmlSettingsClient.Instance.Get(Of Integer)("General", "MiniPlayerType"), MiniPlayerType)
                      )
        End Sub

        Private Sub ThemeChangedHandler(sender As Object, e As EventArgs)

            Application.Current.Dispatcher.Invoke(Sub() If mBackgroundAnimation IsNot Nothing Then mBackgroundAnimation.Begin())
        End Sub

        Private Sub TrayIcon_MouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)

            If e.Button = Forms.MouseButtons.Left Then
                mTrayControls.Topmost = True ' Make sure this appears above the Windows Tray popup
                mTrayControls.Show()
            End If

            If e.Button = Forms.MouseButtons.Right Then

                ' Open the Notify icon context menu
                mTrayIconContextMenu.IsOpen = True

                ' Required to close the Tray icon when Deactivated is called
                ' See: http://copycodetheory.blogspot.be/2012/07/notify-icon-in-wpf-applications.html
                Me.Activate()
            End If
        End Sub

        Private Sub TrayIcon_MouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)

            Me.ActivateNow()
        End Sub

        Private Sub TrayIconContextMenuAppName_Click(sender As Object, e As RoutedEventArgs)

            ' By default, the window appears in the background when showing
            ' from the tray menu. We force it on the foreground here.
            Me.ActivateNow()
        End Sub

        Private Sub Shutdown()
            LogClient.Instance.Logger.Info("### STOPPING {0}, version {1} ###", ProductInformation.ApplicationDisplayName, ProductInformation.FormattedAssemblyVersion)
            mIsShuttingDown = True
            Application.Current.Shutdown()
        End Sub

        Private Sub TrayIconContextMenuExit_Click(sender As Object, e As RoutedEventArgs)
            Me.Shutdown()
        End Sub

        Private Sub Shell_SourceInitialized(sender As Object, e As EventArgs) Handles MyBase.SourceInitialized
            Me.mAppearanceService.WatchWindowsColor(Me)
        End Sub

        Private Sub Shell_StateChanged(sender As Object, e As EventArgs)

            If Me.WindowState = WindowState.Minimized Then

                If XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "ShowTrayIcon") And
                    XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "MinimizeToTray") Then
                    Me.ShowInTaskbar = False
                End If
            Else
                Me.ShowInTaskbar = True
            End If

            Me.SaveWindowState()
        End Sub

        Private Sub Shell_Closing(sender As Object, e As ComponentModel.CancelEventArgs)

            If XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "ShowTrayIcon") And
                                  XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "CloseToTray") And
                                  Not mIsShuttingDown Then

                e.Cancel = True

                ' Minimize first, then hide from Taskbar. Otherwise a small window
                ' remains visible in the lower left corner of the screen.
                Me.WindowState = WindowState.Minimized
                Me.ShowInTaskbar = False
            Else
                If mMustPerformClosingTasks Then
                    e.Cancel = True
                    Me.PerformClosingTasksAsync()
                End If
            End If
        End Sub

        Private Async Function PerformClosingTasksAsync() As Task

            LogClient.Instance.Logger.Info("Performing closing tasks")

            Me.ShowClosingAnimation()
            Await mPlaybackService.FadeOutAsync(Constants.ClosingFadeOutDelay)

            ' Write the settings
            ' ------------------
            XmlSettingsClient.Instance.Write()

            ' Stop playing
            ' ------------
            mPlaybackService.Stop()

            ' Update file metadata if still queued
            ' ------------------------------------
            If mMetadataService.IsUpdatingFileMetadata Then
                While mMetadataService.IsUpdatingFileMetadata
                    Await Task.Delay(50)
                End While
            Else
                Await mMetadataService.UpdateFilemetadataAsync()
            End If

            ' Save queued tracks
            ' ------------------
            If mPlaybackService.IsSavingQueuedTracks Then
                While mPlaybackService.IsSavingQueuedTracks
                    Await Task.Delay(50)
                End While
            ElseIf mPlaybackService.NeedsSavingQueuedTracks Then
                Await mPlaybackService.SaveQueuedTracksAsync()
            End If

            ' Save track statistics
            ' ---------------------
            If mPlaybackService.IsSavingTrackStatistics Then
                While mPlaybackService.IsSavingTrackStatistics
                    Await Task.Delay(50)
                End While
            ElseIf mPlaybackService.NeedsSavingTrackStatistics Then
                Await mPlaybackService.SaveTrackStatisticsAsync
            End If


            ' Stop listening to keyboard outside the application
            ' --------------------------------------------------
            mWin32InputService.UnhookKeyboard()

            LogClient.Instance.Logger.Info("### STOPPING {0}, version {1} ###", ProductInformation.ApplicationDisplayName, ProductInformation.FormattedAssemblyVersion)

            mMustPerformClosingTasks = False
            Me.Close()
        End Function


        Private Sub Shell_SizeChanged(sender As Object, e As SizeChangedEventArgs)
            Me.SaveWindowSize()
        End Sub

        Private Sub Shell_LocationChanged(sender As Object, e As EventArgs)

            ' We need to put SaveWindowLocation() in the queue of the Dispatcher.
            ' SaveWindowLocation() needs to be executed after LocationChanged was 
            ' handled, when the WindowState has been updated otherwise we get 
            ' incorrect values for Left and Top (both -7 last I checked).
            Me.Dispatcher.BeginInvoke(Sub() Me.SaveWindowLocation())
        End Sub

        Private Sub Shell_Closed(sender As Object, e As EventArgs)

            ' Make sure the Tray icon is removed from the tray
            mTrayIcon.Visible = False

            ' This makes sure the application doesn't keep running when the main window is closed.
            ' Extra windows created by the main window can keep a WPF application running even when
            ' the main window is closed, because the default ShutDownMode of a WPF application is
            ' OnLastWindowClose. This was happening here because of the Mini Player Playlist.
            Application.Current.Shutdown()
        End Sub

        Public Overrides Sub OnApplyTemplate()
            MyBase.OnApplyTemplate()

            ' Retrieve BackgroundAnimation storyboard
            ' ---------------------------------------
            mBackgroundAnimation = TryCast(Me.WindowBorder.Resources("BackgroundAnimation"), Storyboard)
            If mBackgroundAnimation IsNot Nothing Then Me.mBackgroundAnimation.Begin()
        End Sub

        Private Sub ShowClosingAnimation()

            Me.ShowWindowControls = False
            Dim closingAnimation As Storyboard = TryCast(Me.ClosingBorder.Resources("ClosingAnimation"), Storyboard)

            Me.ClosingBorder.Visibility = Visibility.Visible
            closingAnimation.Begin()
        End Sub

        Private Sub Shell_KeyDown(sender As Object, e As KeyEventArgs)

            If e.Key = Key.OemPlus Or e.Key = Key.Add Then

                ' [Ctrl] allows fine-tuning of the volume
                If Keyboard.Modifiers = ModifierKeys.Control Then
                    mPlaybackService.Volume = Convert.ToSingle(mPlaybackService.Volume + 0.01)
                Else
                    mPlaybackService.Volume = Convert.ToSingle(mPlaybackService.Volume + 0.05)
                End If

            ElseIf e.Key = Key.OemMinus Or e.Key = Key.Subtract Then

                ' [Ctrl] allows fine-tuning of the volume
                If Keyboard.Modifiers = ModifierKeys.Control Then
                    mPlaybackService.Volume = Convert.ToSingle(mPlaybackService.Volume - 0.01)
                Else
                    mPlaybackService.Volume = Convert.ToSingle(mPlaybackService.Volume - 0.05)
                End If
            ElseIf Keyboard.Modifiers = ModifierKeys.Control And e.Key = Key.L Then

                ' View the log file
                Try
                    Actions.TryViewInExplorer(LogClient.Instance.LogFile)
                Catch ex As Exception
                    LogClient.Instance.Logger.Error("Could not view the log file {0} in explorer. Exception: {1}", LogClient.Instance.LogFile, ex.Message)
                End Try
            End If
        End Sub

        Private Sub Shell_Restored(sender As Object, e As EventArgs)

            ' This workaround is needed because when executing the following 
            ' sequence, the window is restored to the Restore Position of 
            ' the Mini Player: Maximize, Mini Player, Full Player, Restore.
            ' That's because the property RestoreBounds of this window is updated
            ' with the coordinates of the Mini Player when switching to the Mini
            ' Player. Returning to the full player doesn't update RestoreBounds,
            ' because the full player is still maximized at that point.
            Me.SetGeometry(XmlSettingsClient.Instance.Get(Of Integer)("FullPlayer", "Top"),
                           XmlSettingsClient.Instance.Get(Of Integer)("FullPlayer", "Left"),
                           XmlSettingsClient.Instance.Get(Of Integer)("FullPlayer", "Width"),
                           XmlSettingsClient.Instance.Get(Of Integer)("FullPlayer", "Height"))
        End Sub

        Private Sub Shell_CloseToolTipChanged(sender As Object, e As EventArgs)
            Me.PART_MiniPlayerButton.ToolTip = ResourceUtils.GetStringResource("Language_Mini_Player")
        End Sub
#End Region
    End Class
End Namespace
