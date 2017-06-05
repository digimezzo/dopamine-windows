using Digimezzo.Utilities.Settings;
using Dopamine.Common.Prism;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;

namespace Dopamine.MiniPlayerModule.ViewModels
{
    public class CoverPlayerViewModel : CommonMiniPlayerViewModel
    {
        #region Variables
        private IEventAggregator eventAggregator;
        private bool alwaysShowPlaybackInfo;
        private bool alignPlaylistVertically;
        #endregion

        #region Commands
        public DelegateCommand<bool?> CoverPlayerPlaylistButtonCommand { get; set; }
        public DelegateCommand ToggleAlwaysShowPlaybackInfoCommand { get; set; }
        public DelegateCommand ToggleAlignPlaylistVerticallyCommand { get; set; }
        #endregion

        #region Properties
        public bool AlwaysShowPlaybackInfo
        {
            get { return this.alwaysShowPlaybackInfo; }
            set { SetProperty<bool>(ref this.alwaysShowPlaybackInfo, value); }
        }


        public bool AlignPlaylistVertically
        {
            get { return this.alignPlaylistVertically; }
            set { SetProperty<bool>(ref this.alignPlaylistVertically, value); }
        }
        #endregion

        #region Construction
        public CoverPlayerViewModel(IUnityContainer container) : base(container)
        {
            this.eventAggregator = container.Resolve<IEventAggregator>();

            // Commands
            this.CoverPlayerPlaylistButtonCommand = new DelegateCommand<bool?>(isPlaylistButtonChecked =>
            {
                this.eventAggregator.GetEvent<CoverPlayerPlaylistButtonClicked>().Publish(isPlaylistButtonChecked.Value);
                this.IsPlaylistVisible = isPlaylistButtonChecked.Value;
            });

            this.ToggleAlwaysShowPlaybackInfoCommand = new DelegateCommand(() =>
            {
                this.AlwaysShowPlaybackInfo = !this.AlwaysShowPlaybackInfo;
                SettingsClient.Set<bool>("Behaviour", "CoverPlayerAlwaysShowPlaybackInfo", this.AlwaysShowPlaybackInfo);
            });

            this.ToggleAlignPlaylistVerticallyCommand = new DelegateCommand(() =>
            {
                this.AlignPlaylistVertically = !this.AlignPlaylistVertically;
                SettingsClient.Set<bool>("Behaviour", "CoverPlayerAlignPlaylistVertically", this.AlignPlaylistVertically);
                this.eventAggregator.GetEvent<ToggledCoverPlayerAlignPlaylistVertically>().Publish(this.AlignPlaylistVertically);
            });

            ApplicationCommands.ToggleAlwaysShowPlaybackInfoCommand.RegisterCommand(this.ToggleAlwaysShowPlaybackInfoCommand);
            ApplicationCommands.ToggleAlignPlaylistVerticallyCommand.RegisterCommand(this.ToggleAlignPlaylistVerticallyCommand);
            ApplicationCommands.CoverPlayerPlaylistButtonCommand.RegisterCommand(this.CoverPlayerPlaylistButtonCommand);

            this.AlwaysShowPlaybackInfo = SettingsClient.Get<bool>("Behaviour", "CoverPlayerAlwaysShowPlaybackInfo");
            this.AlignPlaylistVertically = SettingsClient.Get<bool>("Behaviour", "CoverPlayerAlignPlaylistVertically");
        }
        #endregion
    }
}
