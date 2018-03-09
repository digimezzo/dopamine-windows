using Dopamine.ViewModels.Common.Base;
using Dopamine.Core.Prism;
using Unity;
using Prism.Commands;
using Prism.Events;

namespace Dopamine.ViewModels.MiniPlayer
{
    public class NanoPlayerViewModel : MiniPlayerViewModelBase
    {
        private IEventAggregator eventAggregator;

        public DelegateCommand<bool?> NanoPlayerPlaylistButtonCommand { get; set; }

        public NanoPlayerViewModel(IUnityContainer container) : base(container)
        {
            this.eventAggregator = container.Resolve<IEventAggregator>();

            this.NanoPlayerPlaylistButtonCommand = new DelegateCommand<bool?>(iIsPlaylistButtonChecked =>
            {
                this.IsPlaylistVisible = iIsPlaylistButtonChecked.Value;
            });
            ApplicationCommands.NanoPlayerPlaylistButtonCommand.RegisterCommand(this.NanoPlayerPlaylistButtonCommand);
        }
    }
}
