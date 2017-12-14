using Digimezzo.Utilities.Settings;
using Dopamine.Core.Enums;
using Dopamine.Common.Prism;
using Microsoft.Practices.Unity;
using Prism.Commands;

namespace Dopamine.Common.Presentation.ViewModels.Base
{
    public class MiniPlayerViewModelBase : ContextMenuViewModelBase
    {
        protected bool isPlaylistVisible;
        private bool isCoverPlayerChecked;
        private bool isMicroPlayerChecked;
        private bool isNanoPlayerChecked;
        private bool isMiniPlayerAlwaysOnTop;
        private bool isMiniPlayerPositionLocked;

        public DelegateCommand<string> ChangePlayerTypeCommand { get; set; }
        public DelegateCommand ToggleMiniPlayerPositionLockedCommand { get; set; }
        public DelegateCommand ToggleMiniPlayerAlwaysOnTopCommand { get; set; }

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

        public MiniPlayerViewModelBase(IUnityContainer container) : base(container)
        {
            // Commands
            this.ChangePlayerTypeCommand = new DelegateCommand<string>(miniPlayerType => this.SetPlayerContextMenuCheckBoxes((MiniPlayerType)(int.Parse(miniPlayerType))));
            ApplicationCommands.ChangePlayerTypeCommand.RegisterCommand(this.ChangePlayerTypeCommand);

            this.ToggleMiniPlayerPositionLockedCommand = new DelegateCommand(() =>
            {
                IsMiniPlayerPositionLocked = !IsMiniPlayerPositionLocked;
                SettingsClient.Set<bool>("Behaviour", "MiniPlayerPositionLocked", IsMiniPlayerPositionLocked);
            });
            ApplicationCommands.ToggleMiniPlayerPositionLockedCommand.RegisterCommand(this.ToggleMiniPlayerPositionLockedCommand);

            this.ToggleMiniPlayerAlwaysOnTopCommand = new DelegateCommand(() =>
            {
                this.IsMiniPlayerAlwaysOnTop = !this.IsMiniPlayerAlwaysOnTop;
                SettingsClient.Set<bool>("Behaviour", "MiniPlayerOnTop", this.IsMiniPlayerAlwaysOnTop);
            });
            ApplicationCommands.ToggleMiniPlayerAlwaysOnTopCommand.RegisterCommand(this.ToggleMiniPlayerAlwaysOnTopCommand);

            //Initialize
            this.Initialize();
        }

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

        protected override void SearchOnline(string id)
        {
            // No implementation required here
        }
    }
}
