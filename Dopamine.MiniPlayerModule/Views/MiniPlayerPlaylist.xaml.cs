using Dopamine.Common.Presentation.Views.Base;
using Dopamine.Common.Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System.Windows;
using System.Windows.Input;

namespace Dopamine.MiniPlayerModule.Views
{
    public partial class MiniPlayerPlaylist : TracksViewBase, INavigationAware
    {
        #region Variable
        private SubscriptionToken scrollToPlayingTrackToken;
        #endregion

        #region Construction
        public MiniPlayerPlaylist() : base()
        {
            InitializeComponent();

            this.screenName = typeof(MiniPlayerPlaylist).FullName;

            this.ViewInExplorerCommand = new DelegateCommand(() => this.ViewInExplorer(this.ListBoxTracks));
            this.JumpToPlayingTrackCommand = new DelegateCommand(() => this.ScrollToPlayingTrackAsync(this.ListBoxTracks));

            // Events and Commands
            this.Subscribe();
        }
        #endregion

        #region Private
        private async void ListBoxTracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await this.ActionHandler(sender, e.OriginalSource as DependencyObject, false);
        }

        private void ListBoxTracks_KeyUp(object sender, KeyEventArgs e)
        {
            this.KeyUpHandlerAsync(sender, e);
        }

        private void Subscribe()
        {
            this.Unsubscribe();

            this.scrollToPlayingTrackToken = this.eventAggregator.GetEvent<ScrollToPlayingTrack>().Subscribe(async (_) => await this.ScrollToPlayingTrackAsync(this.ListBoxTracks));
        }

        private void Unsubscribe()
        {
            this.eventAggregator.GetEvent<ScrollToPlayingTrack>().Unsubscribe(scrollToPlayingTrackToken);
        }

        private void ListBoxTracks_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.ActionHandler(sender, e.OriginalSource as DependencyObject, false);
            }
        }
        #endregion

        #region INavigationAware
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            this.Unsubscribe();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.Subscribe();
        }
        #endregion
    }
}
