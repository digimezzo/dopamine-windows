using Digimezzo.Utilities.Settings;
using Dopamine.Common.Enums;
using Dopamine.Common.Presentation.Views.Base;
using Dopamine.Common.Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System.Windows;
using System.Windows.Input;

namespace Dopamine.CollectionModule.Views
{
    public partial class CollectionPlaylists : PlaylistViewBase, INavigationAware
    {
        #region Variables
        private SubscriptionToken scrollToPlayingTrackToken;
        #endregion

        #region Construction
        public CollectionPlaylists() : base()
        {
            InitializeComponent();

            // Commands
            this.ViewPlaylistInExplorerCommand = new DelegateCommand(() => this.ViewPlaylistInExplorer(this.ListBoxPlaylists));
            this.ViewInExplorerCommand = new DelegateCommand(() => this.ViewInExplorer(this.ListBoxTracks));
            this.JumpToPlayingTrackCommand = new DelegateCommand(() => this.ScrollToPlayingTrackAsync(this.ListBoxTracks));

            // Events and Commands
            this.Subscribe();
        }
        #endregion

        #region Private
        private async void ListBoxPlaylists_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await this.ActionHandler(sender, e.OriginalSource as DependencyObject, true);
        }

        private async void ListBoxPlaylists_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await this.ActionHandler(sender, e.OriginalSource as DependencyObject, true);
            }
        }

        private async void ListBoxTracks_KeyUp(object sender, KeyEventArgs e)
        {
            await this.KeyUpHandlerAsync(sender, e);
        }

        private async void ListBoxTracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await this.ActionHandler(sender, e.OriginalSource as DependencyObject, true);
        }

        private async void ListBoxTracks_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await this.ActionHandler(sender, e.OriginalSource as DependencyObject, true);
            }
        }

        private void PlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            this.ListBoxPlaylists.SelectedItem = null;
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

        private void ListBoxPlaylists_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.E && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (this.ListBoxPlaylists.SelectedItem != null)
                {
                    this.ViewPlaylistInExplorer(this.ListBoxPlaylists);
                }
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
            SettingsClient.Set<int>("FullPlayer", "SelectedPage", (int)SelectedPage.Playlists);
        }
        #endregion
    }
}
