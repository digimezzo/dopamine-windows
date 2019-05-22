using Dopamine.Views.Common.Base;
using System.Windows;
using System.Windows.Input;
using System;
using Digimezzo.Foundation.Core.Logging;
using System.Windows.Controls;
using Digimezzo.WPFControls;
using System.Windows.Media;
using Dopamine.Services.Entities;
using Dopamine.Core.Prism;
using Prism.Commands;

namespace Dopamine.Views.FullPlayer.Collection
{
    public partial class CollectionFolders : TracksViewBase
    {
        public CollectionFolders()
        {
            InitializeComponent();

            // Commands
            this.ViewInExplorerCommand = new DelegateCommand(() => this.ViewInExplorer(this.ListBoxTracks));
            this.JumpToPlayingTrackCommand = new DelegateCommand(async () => await this.ScrollToPlayingTrackAsync(this.ListBoxTracks));

            // PubSub Events
            this.eventAggregator.GetEvent<ScrollToPlayingTrack>().Subscribe(async (_) => await this.ScrollToPlayingTrackAsync(this.ListBoxTracks));
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

        private void ListBoxSubfolders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Check if an item is selected
                ListBox lb = (ListBox)sender;

                if (lb.SelectedItem == null)
                {
                    return;
                }

                // Confirm that the user double clicked a valid item (and not on the scrollbar for example)
                DependencyObject source = e.OriginalSource as DependencyObject;

                if (source == null)
                {
                    return;
                }

                while (source != null && !(source is MultiSelectListBox.MultiSelectListBoxItem))
                {
                    source = VisualTreeHelper.GetParent(source);
                }

                if (source == null || source.GetType() != typeof(MultiSelectListBox.MultiSelectListBoxItem))
                {
                    return;
                }

                // The user double clicked a valid item
                this.eventAggregator.GetEvent<ActiveSubfolderChanged>().Publish((SubfolderViewModel)lb.SelectedItem);
            }
            catch (Exception ex)
            {
                LogClient.Error("Error while handling subfolder double click. Exception: {0}", ex.Message);
            }
        }
    }
}
