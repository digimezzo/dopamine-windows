using Dopamine.Core.Prism;
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
        public NanoPlayerViewModel(IEventAggregator eventAggregator) : base()
        {
            this.eventAggregator = eventAggregator;

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
