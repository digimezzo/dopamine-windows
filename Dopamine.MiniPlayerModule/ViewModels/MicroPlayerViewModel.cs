using Dopamine.Common.Prism;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;

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
        public MicroPlayerViewModel(IUnityContainer container) : base(container)
        {

            this.eventAggregator = container.Resolve<IEventAggregator>();

            // Commands
            this.MicroPlayerPlaylistButtonCommand = new DelegateCommand<bool?>(isPlaylistButtonChecked =>
            {
                this.eventAggregator.GetEvent<MicroPlayerPlaylistButtonClicked>().Publish(isPlaylistButtonChecked.Value);
                this.IsPlaylistVisible = isPlaylistButtonChecked.Value;
            });

            ApplicationCommands.MicroPlayerPlaylistButtonCommand.RegisterCommand(this.MicroPlayerPlaylistButtonCommand);

        }
        #endregion
    }
}
