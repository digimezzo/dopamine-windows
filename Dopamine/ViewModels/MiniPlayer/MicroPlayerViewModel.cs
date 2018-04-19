using Dopamine.ViewModels.Common.Base;
using Dopamine.Core.Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;

namespace Dopamine.ViewModels.MiniPlayer
{
    public class MicroPlayerViewModel : MiniPlayerViewModelBase
    {
        private IEventAggregator eventAggregator;

        public DelegateCommand<bool?> MicroPlayerPlaylistButtonCommand { get; set; }

        public MicroPlayerViewModel(IContainerProvider container) : base(container)
        {

            this.eventAggregator = container.Resolve<IEventAggregator>();

            this.MicroPlayerPlaylistButtonCommand = new DelegateCommand<bool?>(isPlaylistButtonChecked =>
            {
                this.IsPlaylistVisible = isPlaylistButtonChecked.Value;
            });
            ApplicationCommands.MicroPlayerPlaylistButtonCommand.RegisterCommand(this.MicroPlayerPlaylistButtonCommand);

        }
    }
}
