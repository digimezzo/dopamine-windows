using Dopamine.Common.Prism;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;

namespace Dopamine.MiniPlayerModule.ViewModels
{
    public class NanoPlayerViewModel : CommonMiniPlayerViewModel
    {
        #region Variables
        private IEventAggregator eventAggregator;
        #endregion

        #region Commands
        public DelegateCommand<bool?> NanoPlayerPlaylistButtonCommand { get; set; }
        #endregion

        #region Construction
        public NanoPlayerViewModel(IUnityContainer container) : base(container)
        {
            this.eventAggregator = container.Resolve<IEventAggregator>();

            // Commands
            this.NanoPlayerPlaylistButtonCommand = new DelegateCommand<bool?>(iIsPlaylistButtonChecked =>
            {
                this.eventAggregator.GetEvent<NanoPlayerPlaylistButtonClicked>().Publish(iIsPlaylistButtonChecked.Value);
                this.IsPlaylistVisible = iIsPlaylistButtonChecked.Value;
            });

            ApplicationCommands.NanoPlayerPlaylistButtonCommand.RegisterCommand(this.NanoPlayerPlaylistButtonCommand);
        }
        #endregion
    }
}
