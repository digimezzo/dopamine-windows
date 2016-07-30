using Dopamine.Core.Prism;
using Dopamine.Core.Settings;
using Prism.Commands;
using Prism.Events;

namespace Dopamine.MiniPlayerModule.ViewModels
{
    public class CoverPlayerViewModel : CommonMiniPlayerViewModel
    {
        #region Variables
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
        public CoverPlayerViewModel(IEventAggregator eventAggregator) : base()
        {
            this.eventAggregator = eventAggregator;

            // Commands
            this.CoverPlayerPlaylistButtonCommand = new DelegateCommand<bool?>(isPlaylistButtonChecked =>
            {
                this.eventAggregator.GetEvent<CoverPlayerPlaylistButtonClicked>().Publish(isPlaylistButtonChecked.Value);
                this.IsPlaylistVisible = isPlaylistButtonChecked.Value;
            });

            this.ToggleAlwaysShowPlaybackInfoCommand = new DelegateCommand(() =>
            {
                this.AlwaysShowPlaybackInfo = !this.AlwaysShowPlaybackInfo;
                XmlSettingsClient.Instance.Set<bool>("Behaviour", "CoverPlayerAlwaysShowPlaybackInfo", this.AlwaysShowPlaybackInfo);
            });

            this.ToggleAlignPlaylistVerticallyCommand = new DelegateCommand(() =>
            {
                this.AlignPlaylistVertically = !this.AlignPlaylistVertically;
                XmlSettingsClient.Instance.Set<bool>("Behaviour", "CoverPlayerAlignPlaylistVertically", this.AlignPlaylistVertically);
                this.eventAggregator.GetEvent<ToggledCoverPlayerAlignPlaylistVertically>().Publish(this.AlignPlaylistVertically);
            });

            ApplicationCommands.ToggleAlwaysShowPlaybackInfoCommand.RegisterCommand(this.ToggleAlwaysShowPlaybackInfoCommand);
            ApplicationCommands.ToggleAlignPlaylistVerticallyCommand.RegisterCommand(this.ToggleAlignPlaylistVerticallyCommand);
            ApplicationCommands.CoverPlayerPlaylistButtonCommand.RegisterCommand(this.CoverPlayerPlaylistButtonCommand);

            this.AlwaysShowPlaybackInfo = XmlSettingsClient.Instance.Get<bool>("Behaviour", "CoverPlayerAlwaysShowPlaybackInfo");
            this.AlignPlaylistVertically = XmlSettingsClient.Instance.Get<bool>("Behaviour", "CoverPlayerAlignPlaylistVertically");
        }
        #endregion
    }
}
