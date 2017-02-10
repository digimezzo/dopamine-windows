using Dopamine.Common.Services.Playback;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Events;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dopamine.Common.Presentation.Views.Base
{
    public abstract class CommonViewBase : UserControl
    {
        #region Variables
        protected IEventAggregator eventAggregator;
        protected IPlaybackService playBackService;
        #endregion

        #region Commands
        public DelegateCommand ViewInExplorerCommand { get; set; }
        public DelegateCommand JumpToPlayingTrackCommand { get; set; }
        #endregion

        #region Construction
        public CommonViewBase()
        {
            // We need a parameterless constructor to be able to use this UserControl in other UserControls without dependency injection.
            // So for now there is no better solution than to find the EventAggregator by using the ServiceLocator.
            this.eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            this.playBackService = ServiceLocator.Current.GetInstance<IPlaybackService>();
        }
        #endregion

        #region Abstract
        protected abstract Task KeyUpHandlerAsync(object sender, KeyEventArgs e);
        protected abstract Task ActionHandler(Object sender, DependencyObject source, bool enqueue);
        protected abstract Task ScrollToPlayingTrackAsync(Object sender);
        protected abstract void ViewInExplorer(Object sender);
        #endregion
    }
}
