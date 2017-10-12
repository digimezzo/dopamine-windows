using Digimezzo.Utilities.Settings;
using Dopamine.Common.Enums;
using Dopamine.Common.Presentation.Utils;
using Dopamine.Common.Presentation.Views;
using Digimezzo.Utilities.Log;
using Dopamine.Common.Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Windows;
using System.Windows.Input;
using Dopamine.Common.Presentation.Views.Base;

namespace Dopamine.CollectionModule.Views
{
    public partial class CollectionArtists : TracksViewBase, INavigationAware
    {
        #region Variables
        private SubscriptionToken scrollToPlayingTrackToken;
        #endregion

        #region Construction
        public CollectionArtists()
        {
            InitializeComponent();

            // Commands
            this.ViewInExplorerCommand = new DelegateCommand(() => this.ViewInExplorer(this.ListBoxTracks));
            this.JumpToPlayingTrackCommand = new DelegateCommand(async () => await this.ScrollToPlayingTrackAsync(this.ListBoxTracks));
         
            // PubSub Events
            this.eventAggregator.GetEvent<PerformSemanticJump>().Subscribe(async(data) => {
                try
                {
                    if (data.Item1.Equals("Artists"))
                    {
                        await SemanticZoomUtils.SemanticScrollAsync(this.ListBoxArtists, data.Item2);
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not perform semantic zoom on Artists. Exception: {0}", ex.Message);
                }
            });
        }
        #endregion

        #region Private
        private async void ListBoxArtists_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await this.ActionHandler(sender, e.OriginalSource as DependencyObject, true);
        }

        private async void ListBoxArtists_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await this.ActionHandler(sender, e.OriginalSource as DependencyObject, true);
            }
        }

        private async void ListBoxAlbums_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await this.ActionHandler(sender, e.OriginalSource as DependencyObject, true);
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

        private async void ListBoxTracks_KeyUp(object sender, KeyEventArgs e)
        {
            await this.KeyUpHandlerAsync(sender, e);
        }

        private async void ListBoxTracks_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await this.ActionHandler(sender, e.OriginalSource as DependencyObject, true);
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
            SettingsClient.Set<int>("FullPlayer", "SelectedPage", (int)SelectedPage.Artists);
        }
        #endregion
    }
}
