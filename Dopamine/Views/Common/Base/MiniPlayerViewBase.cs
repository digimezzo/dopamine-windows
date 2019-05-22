using Digimezzo.Foundation.Core.Settings;
using Digimezzo.Foundation.Core.Utils;
using Prism.Commands;
using System.Windows;
using System.Windows.Input;

namespace Dopamine.Views.Common.Base
{
    public class MiniPlayerViewBase : TracksViewBase
    {
        private bool isMiniPlayerPositionLocked;

        public DelegateCommand ToggleMiniPlayerPositionLockedCommand { get; set; }

        public MiniPlayerViewBase() : base()
        {
            this.ToggleMiniPlayerPositionLockedCommand = new DelegateCommand(() => this.isMiniPlayerPositionLocked = !this.isMiniPlayerPositionLocked);
            Core.Prism.ApplicationCommands.ToggleMiniPlayerPositionLockedCommand.RegisterCommand(this.ToggleMiniPlayerPositionLockedCommand);
            this.isMiniPlayerPositionLocked = SettingsClient.Get<bool>("Behaviour", "MiniPlayerPositionLocked");
        }

        protected void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!this.isMiniPlayerPositionLocked)
            {
                // We need to use a custom function because the built-in DragMove function causes 
                // flickering when blurring the cover art and releasing the mouse button after a drag.
                if (e.ClickCount == 1) WindowUtils.MoveWindow(Application.Current.MainWindow);
            }
        }
    }
}
