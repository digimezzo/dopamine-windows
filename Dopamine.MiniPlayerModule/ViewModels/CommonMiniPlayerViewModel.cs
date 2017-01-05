using Digimezzo.Utilities.Settings;
using Dopamine.Common.Enums;
using Dopamine.Common.Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Dopamine.MiniPlayerModule.ViewModels
{
    public class CommonMiniPlayerViewModel : BindableBase
    {
        #region Variables
        protected bool isPlaylistVisible;
        private bool isCoverPlayerChecked;
        private bool isMicroPlayerChecked;
        private bool isNanoPlayerChecked;
        private bool isMiniPlayerAlwaysOnTop;
        private bool isMiniPlayerPositionLocked;
        protected IEventAggregator eventAggregator;
        #endregion

        #region Commands
        public DelegateCommand<string> ChangePlayerTypeCommand { get; set; }
        public DelegateCommand ToggleMiniPlayerPositionLockedCommand { get; set; }
        public DelegateCommand ToggleMiniPlayerAlwaysOnTopCommand { get; set; }
        #endregion

        #region Properties
        public bool IsPlaylistVisible
        {
            get { return this.isPlaylistVisible; }
            set { SetProperty<bool>(ref this.isPlaylistVisible, value); }
        }

        public bool IsCoverPlayerChecked
        {
            get { return this.isCoverPlayerChecked; }
            set { SetProperty<bool>(ref this.isCoverPlayerChecked, value); }
        }

        public bool IsMicroPlayerChecked
        {
            get { return this.isMicroPlayerChecked; }
            set { SetProperty<bool>(ref this.isMicroPlayerChecked, value); }
        }

        public bool IsNanoPlayerChecked
        {
            get { return this.isNanoPlayerChecked; }
            set { SetProperty<bool>(ref this.isNanoPlayerChecked, value); }
        }

        public bool IsMiniPlayerAlwaysOnTop
        {
            get { return this.isMiniPlayerAlwaysOnTop; }
            set { SetProperty<bool>(ref this.isMiniPlayerAlwaysOnTop, value); }
        }

        public bool IsMiniPlayerPositionLocked
        {
            get { return this.isMiniPlayerPositionLocked; }
            set { SetProperty<bool>(ref this.isMiniPlayerPositionLocked, value); }
        }
        #endregion

        #region Construction
        public CommonMiniPlayerViewModel()
        {
            // Commands
            this.ChangePlayerTypeCommand = new DelegateCommand<string>(miniPlayerType => this.SetPlayerContextMenuCheckBoxes((MiniPlayerType)(int.Parse(miniPlayerType))));

            this.ToggleMiniPlayerPositionLockedCommand = new DelegateCommand(() =>
            {
                IsMiniPlayerPositionLocked = !IsMiniPlayerPositionLocked;
                SettingsClient.Set<bool>("Behaviour", "MiniPlayerPositionLocked", IsMiniPlayerPositionLocked);
            });

            this.ToggleMiniPlayerAlwaysOnTopCommand = new DelegateCommand(() =>
            {
                IsMiniPlayerAlwaysOnTop = !IsMiniPlayerAlwaysOnTop;
                SettingsClient.Set<bool>("Behaviour", "MiniPlayerOnTop", IsMiniPlayerAlwaysOnTop);
            });

            // Register Commands: all 3 Mini Players need to listen to these Commands, even if 
            // their Views are not active. That is why we don't use Subscribe and Unsubscribe.
            ApplicationCommands.ChangePlayerTypeCommand.RegisterCommand(this.ChangePlayerTypeCommand);
            ApplicationCommands.ToggleMiniPlayerPositionLockedCommand.RegisterCommand(this.ToggleMiniPlayerPositionLockedCommand);
            ApplicationCommands.ToggleMiniPlayerAlwaysOnTopCommand.RegisterCommand(this.ToggleMiniPlayerAlwaysOnTopCommand);

            //Initialize
            this.Initialize();
        }
        #endregion

        #region Private
        private void SetPlayerContextMenuCheckBoxes(MiniPlayerType miniPlayerType)
        {
            this.IsCoverPlayerChecked = false;
            this.IsMicroPlayerChecked = false;
            this.IsNanoPlayerChecked = false;

            switch (miniPlayerType)
            {
                case MiniPlayerType.CoverPlayer:
                    this.IsCoverPlayerChecked = true;
                    break;
                case MiniPlayerType.MicroPlayer:
                    this.IsMicroPlayerChecked = true;
                    break;
                case MiniPlayerType.NanoPlayer:
                    this.IsNanoPlayerChecked = true;
                    break;
                default:
                    break;
                    // Doesn't happen
            }
        }

        private void Initialize()
        {
            // Set the default IsMiniPlayerPositionLocked value
            this.IsMiniPlayerPositionLocked = SettingsClient.Get<bool>("Behaviour", "MiniPlayerPositionLocked");
            this.IsMiniPlayerAlwaysOnTop = SettingsClient.Get<bool>("Behaviour", "MiniPlayerOnTop");

            // This sets the initial state of the ContextMenu CheckBoxes
            this.SetPlayerContextMenuCheckBoxes((MiniPlayerType)SettingsClient.Get<int>("General", "MiniPlayerType"));
        }
        #endregion
    }
}
