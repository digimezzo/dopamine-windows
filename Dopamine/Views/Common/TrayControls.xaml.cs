using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Digimezzo.Utilities.Win32;
using Dopamine.Common.Base;
using Dopamine.Common.Services.Notification;
using System;
using System.Windows;

namespace Dopamine.Views.Common
{
    public partial class TrayControls : Window
    {
        private INotificationService notificationService;

        public TrayControls(INotificationService notificationService)
        {
            InitializeComponent();

            this.notificationService = notificationService;
        }

        public void Show()
        {
            LogClient.Info("Showing tray controls");
            this.notificationService.HideNotification(); // If a notification is shown, hide it.

            base.Show();

            this.SetTransparency();
            this.SetGeometry();

            this.Topmost = true; // this window should always be on top of all others


            // This is important so Deactivated is called even when the window was never clicked
            // (When a manual activate is not triggered, Deactivated doesn't get called when
            // clicking outside the window)
            this.Activate();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            // Closes this window when the mouse is clicked outside it
            this.Hide();
        }

        private void SetTransparency()
        {
            if (EnvironmentUtils.IsWindows10() && SettingsClient.Get<bool>("Appearance", "EnableTransparency"))
            {
                this.WindowBackground.Opacity = Constants.OpacityWhenBlurred;
                WindowUtils.EnableBlur(this);
            }
            else
            {
                this.WindowBackground.Opacity = 1.0;
            }
        }

        private void SetGeometry()
        {
            var taskbar = new Taskbar();
           
            Rect desktopWorkingArea = System.Windows.SystemParameters.WorkArea;

            this.Left = desktopWorkingArea.Right - Constants.TrayControlsWidth - 5;
            this.Top = desktopWorkingArea.Bottom - Constants.TrayControlsHeight - 5;

            if (taskbar.Position == TaskbarPosition.Top)
            {
                this.Left = desktopWorkingArea.Right - Constants.TrayControlsWidth - 5;
                this.Top = desktopWorkingArea.Top + 5;
            }
            else if (taskbar.Position == TaskbarPosition.Left)
            {
                this.Left = desktopWorkingArea.Left + 5;
                this.Top = desktopWorkingArea.Bottom - Constants.TrayControlsHeight - 5;
            }
            else if (taskbar.Position == TaskbarPosition.Right)
            {
                this.Left = desktopWorkingArea.Right - Constants.TrayControlsWidth - 5;
                this.Top = desktopWorkingArea.Bottom - Constants.TrayControlsHeight - 5;
            }
            else
            {
                this.Left = desktopWorkingArea.Right - Constants.TrayControlsWidth - 5;
                this.Top = desktopWorkingArea.Bottom - Constants.TrayControlsHeight - 5;
            }

            LogClient.Info("Tray controls position: Taskbar position = {0}, Left = {1}px, Top = {2}px", taskbar.Position.ToString(), this.Left.ToString(), this.Top.ToString());
        }
    }
}
