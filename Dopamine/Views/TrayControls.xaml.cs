using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Digimezzo.Utilities.Win32;
using Dopamine.Common.Base;
using Dopamine.Common.Services.Notification;
using System;
using System.Drawing;
using System.Windows;

namespace Dopamine.Views
{
    public partial class TrayControls : Window
    {
        #region Variables
        private INotificationService notificationService;
        #endregion

        #region Construction
        public TrayControls(INotificationService notificationService)
        {
            InitializeComponent();

            this.notificationService = notificationService;
        }
        #endregion

        #region Public
        public void Show()
        {
            this.notificationService.HideNotification(); // If a notification is shown, hide it.

            base.Show();

            this.SetTransparency();
            this.SetGeometry();

            this.Topmost = true; // this window should always be on top of all others


            // This is important so Deactivated is called even when the window was never clicked
            // (When a maual activate is not triggered, Deactivated doesn't get called when
            // clicking outside the window)
            this.Activate();
        }
        #endregion

        #region Private
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
            Rectangle taskbarbounds = taskbar.Bounds;

            if (taskbar.Position == TaskbarPosition.Top)
            {
                this.Left = taskbarbounds.Right - Constants.TrayControlsWidth - 5;
                this.Top = taskbarbounds.Bottom + 5;
            }
            else if (taskbar.Position == TaskbarPosition.Left)
            {
                this.Left = taskbarbounds.Right + 5;
                this.Top = taskbarbounds.Bottom - Constants.TrayControlsHeight - 5;
            }
            else if (taskbar.Position == TaskbarPosition.Right)
            {
                this.Left = taskbarbounds.Left - Constants.TrayControlsWidth - 5;
                this.Top = taskbarbounds.Bottom - Constants.TrayControlsHeight - 5;
            }
            else
            {
                this.Left = taskbarbounds.Right - Constants.TrayControlsWidth - 5;
                this.Top = taskbarbounds.Top - Constants.TrayControlsHeight - 5;
            }
        }
        #endregion
    }
}
