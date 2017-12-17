using Digimezzo.Utilities.Log;
using Dopamine.Presentation.Utils;
using Dopamine.Common.Presentation.ViewModels.Entities;
using Dopamine.Common.Presentation.Views.Base;
using Dopamine.Core.Prism;
using Prism.Commands;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dopamine.Views.FullPlayer.Collection
{
    public partial class CollectionGenres : TracksViewBase
    {
        public CollectionGenres() : base()
        {
            InitializeComponent();

            // Commands
            this.ViewInExplorerCommand = new DelegateCommand(() => this.ViewInExplorer(this.ListBoxTracks));
            this.JumpToPlayingTrackCommand = new DelegateCommand(async () => await this.ScrollToPlayingTrackAsync(this.ListBoxTracks));

            // PubSub Events
            this.eventAggregator.GetEvent<ScrollToPlayingTrack>().Subscribe(async (_) => await this.ScrollToPlayingTrackAsync(this.ListBoxTracks));

            this.eventAggregator.GetEvent<PerformSemanticJump>().Subscribe(async (data) => {
                try
                {
                    if (data.Item1.Equals("Genres"))
                    {
                        await SemanticZoomUtils.SemanticScrollAsync(this.ListBoxGenres, data.Item2);
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not perform semantic zoom on Genres. Exception: {0}", ex.Message);
                }
            });
        }

        protected async Task SemanticScrollToGenreAsync(ListBox listBox, string letter)
        {
            await Task.Run(() =>
            {
                try
                {
                    foreach (GenreViewModel genre in listBox.Items)
                    {

                        if (SemanticZoomUtils.GetGroupHeader(genre.GenreName).ToLower().Equals(letter.ToLower()))
                        {
                            // We can only access the ListBox from the UI Thread
                            Application.Current.Dispatcher.Invoke(() => listBox.ScrollIntoView(genre));
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not perform semantic scroll Genre. Exception: {0}", ex.Message);
                }

            });
        }

        private async void ListBoxGenres_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await this.ActionHandler(sender, e.OriginalSource as DependencyObject, true);
        }

        private async void ListBoxGenres_PreviewKeyDown(object sender, KeyEventArgs e)
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

        private void GenresButton_Click(object sender, RoutedEventArgs e)
        {
            this.ListBoxGenres.SelectedItem = null;
        }

        private void AlbumsButton_Click(object sender, RoutedEventArgs e)
        {
            this.ListBoxAlbums.SelectedItem = null;
        }
    }
}
