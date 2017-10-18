using Dopamine.Common.Presentation.ViewModels.Base;
using Dopamine.Common.Prism;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;

namespace Dopamine.ViewModels.MiniPlayer
{
    public class MicroPlayerViewModel : MiniPlayerViewModelBase
    {
        private IEventAggregator eventAggregator;

        public DelegateCommand<bool?> MicroPlayerPlaylistButtonCommand { get; set; }

        public MicroPlayerViewModel(IUnityContainer container) : base(container)
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
