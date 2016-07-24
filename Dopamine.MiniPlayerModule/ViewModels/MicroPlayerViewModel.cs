using Dopamine.Core.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Dopamine.MiniPlayerModule.ViewModels
{
    public class MicroPlayerViewModel : CommonMiniPlayerViewModel
    {
        #region Variables
        private IEventAggregator eventAggregator;
        #endregion

        #region Commands
        public DelegateCommand<bool?> MicroPlayerPlaylistButtonCommand { get; set; }
        #endregion

        #region Construction
        public MicroPlayerViewModel(IEventAggregator eventAggregator) : base()
        {

            this.eventAggregator = eventAggregator;

            // Commands
            this.MicroPlayerPlaylistButtonCommand = new DelegateCommand<bool?>(iIsPlaylistButtonChecked =>
            {
                this.eventAggregator.GetEvent<MicroPlayerPlaylistButtonClicked>().Publish(iIsPlaylistButtonChecked.Value);
                this.IsPlaylistVisible = iIsPlaylistButtonChecked.Value;
            });

            ApplicationCommands.MicroPlayerPlaylistButtonCommand.RegisterCommand(this.MicroPlayerPlaylistButtonCommand);

        }
        #endregion
    }
}
