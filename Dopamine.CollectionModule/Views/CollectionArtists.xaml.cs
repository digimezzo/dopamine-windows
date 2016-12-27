using Digimezzo.Utilities.Settings;
using Dopamine.Common.Enums;
using Dopamine.Common.Presentation.Utils;
using Dopamine.Common.Presentation.Views;
using Dopamine.Core.Logging;
using Dopamine.Core.Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Windows;
using System.Windows.Input;

namespace Dopamine.CollectionModule.Views
{
    public partial class CollectionArtists : CommonTracksView, INavigationAware
    {
        #region Variables
        private SubscriptionToken scrollToPlayingTrackToken;
        #endregion

        #region Commands
        public DelegateCommand<string> SemanticJumpCommand { get; set; }
        #endregion

        #region Construction
        public CollectionArtists()
        {
            InitializeComponent();

            this.screenName = typeof(CollectionArtists).FullName;

            // Commands
            this.ViewInExplorerCommand = new DelegateCommand(() => this.ViewInExplorer(this.ListBoxTracks));
            this.JumpToPlayingTrackCommand = new DelegateCommand(async () => await this.ScrollToPlayingTrackAsync(this.ListBoxTracks));
            this.SemanticJumpCommand = new DelegateCommand<string>(async (header) =>
            {
                try
                {
                    await SemanticZoomUtils.SemanticScrollAsync(this.ListBoxArtists, header);
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not perform semantic zoom on Artists. Exception: {0}", ex.Message);
                }
            });

            // Events and Commands
            this.Subscribe();
        }
        #endregion

        #region Private
        private async void ListBoxArtists_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await this.ListActionHandler(sender, e.OriginalSource as DependencyObject, true);
        }

        private async void ListBoxArtists_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await this.ListActionHandler(sender, e.OriginalSource as DependencyObject, true);
            }
        }

        private async void ListBoxAlbums_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await this.ListActionHandler(sender, e.OriginalSource as DependencyObject, true);
        }

        private async void ListBoxAlbums_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await this.ListActionHandler(sender, e.OriginalSource as DependencyObject, true);
            }
        }

        private async void ListBoxTracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await this.ListActionHandler(sender, e.OriginalSource as DependencyObject, true);
        }

        private async void ListBoxTracks_KeyUp(object sender, KeyEventArgs e)
        {
            await this.TracksKeyUpHandlerAsync(sender, e);
        }

        private async void ListBoxTracks_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await this.ListActionHandler(sender, e.OriginalSource as DependencyObject, true);
            }
        }

        private void ArtistsButton_Click(object sender, RoutedEventArgs e)
        {
            this.ListBoxArtists.SelectedItem = null;
        }

        private void AlbumsButton_Click(object sender, RoutedEventArgs e)
        {
            this.ListBoxAlbums.SelectedItem = null;
        }

        private void Unsubscribe()
        {
            // Commands
            Core.Prism.ApplicationCommands.SemanticJumpCommand.UnregisterCommand(this.SemanticJumpCommand);

            // Events
            this.eventAggregator.GetEvent<ScrollToPlayingTrack>().Unsubscribe(this.scrollToPlayingTrackToken);
        }

        private void Subscribe()
        {
            // Prevents subscribing twice
            this.Unsubscribe();

            // Commands
            Core.Prism.ApplicationCommands.SemanticJumpCommand.RegisterCommand(this.SemanticJumpCommand);

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
            SettingsClient.Set<int>("FullPlayer", "SelectedPage", (int)SelectedPage.Artists);
        }
        #endregion
    }
}
