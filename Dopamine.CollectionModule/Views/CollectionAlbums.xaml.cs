using Digimezzo.Utilities.Settings;
using Dopamine.Common.Enums;
using Dopamine.Common.Presentation.Views.Base;
using Dopamine.Common.Prism;
using Prism.Commands;
using Prism.Regions;
using System.Windows;
using System.Windows.Input;

namespace Dopamine.CollectionModule.Views
{
    public partial class CollectionAlbums : TracksViewBase, INavigationAware
    {
        #region Construction
        public CollectionAlbums()
        {
            InitializeComponent();

            // Commands
            this.ViewInExplorerCommand = new DelegateCommand(() => this.ViewInExplorer(this.ListBoxTracks));
            this.JumpToPlayingTrackCommand = new DelegateCommand(async () => await this.ScrollToPlayingTrackAsync(this.ListBoxTracks));

            // PubSub Events
            this.eventAggregator.GetEvent<ScrollToPlayingTrack>().Subscribe(async (_) => await this.ScrollToPlayingTrackAsync(this.ListBoxTracks));
        }
        #endregion

        #region Private
        private async void ListBoxAlbums_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await this.ActionHandler(sender, e.OriginalSource as DependencyObject,true);
        }

        private async void ListBoxAlbums_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await this.ActionHandler(sender, e.OriginalSource as DependencyObject, true);
            }
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

        private async void ListBoxTracks_KeyUp(object sender, KeyEventArgs e)
        {
            await this.KeyUpHandlerAsync(sender, e);
        }

        private async void AlbumsButton_Click(object sender, RoutedEventArgs e)
        {
            this.ListBoxAlbums.SelectedItem = null;
        }
        #endregion

        #region INavigationAware
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            SettingsClient.Set<int>("FullPlayer", "SelectedPage", (int)SelectedPage.Albums);
        }
        #endregion
    }
}
