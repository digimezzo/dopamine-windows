using Dopamine.Common.Enums;
using Dopamine.Common.Presentation.Views;
using Dopamine.Core.Prism;
using Dopamine.Core.Settings;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System.Windows;
using System.Windows.Input;

namespace Dopamine.CollectionModule.Views
{
    public partial class CollectionPlaylists : CommonTracksView, INavigationAware
    {
        #region Variables
        private SubscriptionToken scrollToPlayingTrackToken;
        #endregion

        #region Construction
        public CollectionPlaylists()
        {
            InitializeComponent();

            this.screenName = typeof(CollectionPlaylists).FullName;

            // Commands
            this.ViewInExplorerCommand = new DelegateCommand(() => this.ViewInExplorer(this.ListBoxTracks));
            this.JumpToPlayingTrackCommand = new DelegateCommand(() => this.ScrollToPlayingTrackAsync(this.ListBoxTracks));

            // Events and Commands
            this.Subscribe();
        }
        #endregion

        #region Private
        private void PlaylistsKeyUpHandlerAsync(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                if (this.ListBoxPlaylists.SelectedItem != null)
                {
                    this.eventAggregator.GetEvent<RenameSelectedPlaylistWithKeyF2>().Publish(null);
                }
            }
            else if (e.Key == Key.Delete)
            {
                if (this.ListBoxPlaylists.SelectedItem != null)
                {
                    this.eventAggregator.GetEvent<DeleteSelectedPlaylistsWithKeyDelete>().Publish(null);
                }
            }
        }

        private async void ListBoxPlaylists_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await this.ListActionHandler(sender, e.OriginalSource as DependencyObject, true);
        }

        private async void ListBoxPlaylists_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await this.ListActionHandler(sender, e.OriginalSource as DependencyObject, true);
            }
        }

        private void ListBoxPlaylists_KeyUp(object sender, KeyEventArgs e)
        {
            this.PlaylistsKeyUpHandlerAsync(sender, e);
        }

        private async void ListBoxTracks_KeyUp(object sender, KeyEventArgs e)
        {
            await this.TracksKeyUpHandlerAsync(sender, e);
        }

        private async void ListBoxTracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await this.ListActionHandler(sender, e.OriginalSource as DependencyObject, true);
        }

        private async void ListBoxTracks_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await this.ListActionHandler(sender, e.OriginalSource as DependencyObject, true);
            }
        }

        private void TracksButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: what to do here?
        }

        private void PlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: what to do here?
        }

        private void Unsubscribe()
        {
            // Events
            this.eventAggregator.GetEvent<ScrollToPlayingTrack>().Unsubscribe(this.scrollToPlayingTrackToken);
        }

        private void Subscribe()
        {
            // Prevents subscribing twice
            this.Unsubscribe();

            // Events
            this.scrollToPlayingTrackToken = this.eventAggregator.GetEvent<ScrollToPlayingTrack>().Subscribe(async (s) => await this.ScrollToPlayingTrackAsync(this.ListBoxTracks));
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
            XmlSettingsClient.Instance.Set<int>("FullPlayer", "SelectedPage", (int)SelectedPage.Playlists);
        }
        #endregion
    }
}
