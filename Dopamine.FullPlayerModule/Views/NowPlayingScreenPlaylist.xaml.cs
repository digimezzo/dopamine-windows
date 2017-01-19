using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Presentation.Views;
using Dopamine.Common.IO;
using Digimezzo.Utilities.Log;
using Dopamine.Common.Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dopamine.FullPlayerModule.Views
{
    public partial class NowPlayingScreenPlaylist : CommonTracksView, INavigationAware
    {
        #region Variables
        private SubscriptionToken scrollToPlayingTrackToken;
        #endregion

        #region Construction
        public NowPlayingScreenPlaylist()
        {
            InitializeComponent();

            this.screenName = typeof(NowPlayingScreenPlaylist).FullName;

            // Add any initialization after the InitializeComponent() call.
            this.ViewInExplorerCommand = new DelegateCommand(() => this.ViewInExplorer(this.ListBoxTracks));
            this.JumpToPlayingTrackCommand = new DelegateCommand(() => this.ScrollToPlayingTrackAsync(this.ListBoxTracks));

            // Events and Commands
            this.Subscribe();
        }
        #endregion

        #region Private
        private async void ListBoxTracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ListBox lb = (ListBox)sender;

                if (lb.SelectedItem == null)
                    return;

                await this.playBackService.PlaySelectedAsync(((TrackViewModel)lb.SelectedItem).Track);
            }
            catch (Exception ex)
            {
                LogClient.Error("Error while handling ListBox action. Exception: {0}", ex.Message);
            }
        }

        private void ListBoxTracks_KeyUp(object sender, KeyEventArgs e)
        {
            this.TracksKeyUpHandlerAsync(sender, e);
        }

        private void Subscribe()
        {
            this.Unsubscribe();

            scrollToPlayingTrackToken = this.eventAggregator.GetEvent<ScrollToPlayingTrack>().Subscribe(async (str) => await this.ScrollToPlayingTrackAsync(this.ListBoxTracks));
        }

        private void Unsubscribe()
        {
            this.eventAggregator.GetEvent<ScrollToPlayingTrack>().Unsubscribe(scrollToPlayingTrackToken);
        }

        private void ListBoxTracks_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.ListActionHandler(sender, e.OriginalSource as DependencyObject,false);
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
