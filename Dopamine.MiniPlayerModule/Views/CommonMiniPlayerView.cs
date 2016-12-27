using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Presentation.Views;
using Prism.Commands;
using System.Windows;
using System.Windows.Input;

namespace Dopamine.MiniPlayerModule.Views
{
    public class CommonMiniPlayerView : CommonTracksView
    {
        #region Variables
        private bool isMiniPlayerPositionLocked;
        #endregion

        #region Commands
        public DelegateCommand ToggleMiniPlayerPositionLockedCommand { get; set; }
        #endregion

        #region Construction
        public CommonMiniPlayerView() : base()
        {
            this.ToggleMiniPlayerPositionLockedCommand = new DelegateCommand(() => this.isMiniPlayerPositionLocked = !this.isMiniPlayerPositionLocked);
            Core.Prism.ApplicationCommands.ToggleMiniPlayerPositionLockedCommand.RegisterCommand(this.ToggleMiniPlayerPositionLockedCommand);

            this.isMiniPlayerPositionLocked = SettingsClient.Get<bool>("Behaviour", "MiniPlayerPositionLocked");
        }
        #endregion

        #region Protected
        protected void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!this.isMiniPlayerPositionLocked)
            {
                // We need to use a custom function because the built-in DragMove function causes 
                // flickering when blurring the cover art and releasing the mouse button after a drag.
                if (e.ClickCount == 1) WindowUtils.MoveWindow(Application.Current.MainWindow);
            }
        }
        #endregion
    }
}
