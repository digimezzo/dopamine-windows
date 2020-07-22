using Dopamine.Core.Alex;  //Digimezzo.Foundation.Core.Settings
using Dopamine.ViewModels.Common.Base;
using Dopamine.Core.Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;

namespace Dopamine.ViewModels.MiniPlayer
{
    public class CoverPlayerViewModel : MiniPlayerViewModelBase
    {
        private IEventAggregator eventAggregator;
        private bool alwaysShowPlaybackInfo;
        private bool alignPlaylistVertically;

        public DelegateCommand<bool?> CoverPlayerPlaylistButtonCommand { get; set; }
        public DelegateCommand ToggleAlwaysShowPlaybackInfoCommand { get; set; }
        public DelegateCommand ToggleAlignPlaylistVerticallyCommand { get; set; }

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

        public CoverPlayerViewModel(IContainerProvider container) : base(container)
        {
            this.eventAggregator = container.Resolve<IEventAggregator>();

            this.CoverPlayerPlaylistButtonCommand = new DelegateCommand<bool?>(isPlaylistButtonChecked =>
            {
                this.IsPlaylistVisible = isPlaylistButtonChecked.Value;
            });
            ApplicationCommands.CoverPlayerPlaylistButtonCommand.RegisterCommand(this.CoverPlayerPlaylistButtonCommand);

            this.ToggleAlwaysShowPlaybackInfoCommand = new DelegateCommand(() =>
            {
                this.AlwaysShowPlaybackInfo = !this.AlwaysShowPlaybackInfo;
                SettingsClient.Set<bool>("Behaviour", "CoverPlayerAlwaysShowPlaybackInfo", this.AlwaysShowPlaybackInfo);
            });
            ApplicationCommands.ToggleAlwaysShowPlaybackInfoCommand.RegisterCommand(this.ToggleAlwaysShowPlaybackInfoCommand);

            this.ToggleAlignPlaylistVerticallyCommand = new DelegateCommand(() =>
            {
                this.AlignPlaylistVertically = !this.AlignPlaylistVertically;
                SettingsClient.Set<bool>("Behaviour", "CoverPlayerAlignPlaylistVertically", this.AlignPlaylistVertically);
                this.eventAggregator.GetEvent<ToggledCoverPlayerAlignPlaylistVertically>().Publish(this.AlignPlaylistVertically);
            });
            ApplicationCommands.ToggleAlignPlaylistVerticallyCommand.RegisterCommand(this.ToggleAlignPlaylistVerticallyCommand);

            this.AlwaysShowPlaybackInfo = SettingsClient.Get<bool>("Behaviour", "CoverPlayerAlwaysShowPlaybackInfo");
            this.AlignPlaylistVertically = SettingsClient.Get<bool>("Behaviour", "CoverPlayerAlignPlaylistVertically");
        }
    }
}
