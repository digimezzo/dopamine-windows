using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Presentation.Views;
using Dopamine.Core.Logging;
using Dopamine.Core.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dopamine.MiniPlayerModule.Views
{
    public partial class MiniPlayerPlaylist : CommonTracksView, INavigationAware
    {
        #region Variable
        private SubscriptionToken scrollToPlayingTrackToken;
        #endregion

        #region Construction
        public MiniPlayerPlaylist() : base()
        {
            InitializeComponent();

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

                if (lb.SelectedItem == null) return;

                await this.playBackService.PlaySelectedAsync(((TrackInfoViewModel)lb.SelectedItem).TrackInfo);
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Error while handling ListBox action. Exception: {0}", ex.Message);
            }
        }

        private void ListBoxTracks_KeyUp(object sender, KeyEventArgs e)
        {
            this.TracksKeyUpHandlerAsync(sender, e);
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
            if( e.Key == Key.Enter)
            {
                this.ListActionHandler(sender);
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
