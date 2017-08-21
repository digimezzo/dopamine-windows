using Digimezzo.Utilities.IO;
using Dopamine.Core.Logging;
using Digimezzo.WPFControls;
using Dopamine.Common.Base;
using Dopamine.Common.Database;
using Dopamine.Common.Presentation.Utils;
using Dopamine.Common.Presentation.ViewModels.Entities;
using Dopamine.Common.Prism;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Dopamine.Common.Presentation.Views.Base
{
    public abstract class TracksViewBase : CommonViewBase
    {
        #region Overrides
        protected override async Task KeyUpHandlerAsync(object sender, KeyEventArgs e)
        {
            ListBox lb = (ListBox)sender;

            if (e.Key == Key.J && Keyboard.Modifiers == ModifierKeys.Control)
            {
                await this.ScrollToPlayingTrackAsync(lb);

            }
            else if (e.Key == Key.E && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (lb.SelectedItem != null)
                {
                    Actions.TryViewInExplorer(((TrackViewModel)lb.SelectedItem).Track.Path);
                }
            }
        }

        protected override async Task ActionHandler(Object sender, DependencyObject source, bool enqueue)
        {
            try
            {
                // Check if an item is selected
                ListBox lb = (ListBox)sender;

                if (lb.SelectedItem == null) return;

                // Confirm that the user double clicked a valid item (and not on the scrollbar for example)
                if (source == null) return;

                while (source != null && !(source is MultiSelectListBox.MultiSelectListBoxItem))
                {
                    source = VisualTreeHelper.GetParent(source);
                }

                if (source == null || source.GetType() != typeof(MultiSelectListBox.MultiSelectListBoxItem)) return;

                // The user double clicked a valid item
                if (!enqueue)
                {
                    // The user just wants to play the selected item. Don't enqueue.
                    if (lb.SelectedItem.GetType().Name == typeof(TrackViewModel).Name)
                    {
                        await this.playbackService.PlaySelectedAsync(((TrackViewModel)lb.SelectedItem).Track);
                    }

                    return;
                };

                // The user wants to enqueue tracks for the selected item
                if (lb.SelectedItem.GetType().Name == typeof(TrackViewModel).Name)
                {
                    await this.playbackService.EnqueueAsync(
                        lb.Items.OfType<TrackViewModel>().ToList().Select((vm) => vm.Track).ToList(),
                        ((TrackViewModel)lb.SelectedItem).Track
                        );
                }
                else if (lb.SelectedItem.GetType().Name == typeof(ArtistViewModel).Name)
                {
                    await this.playbackService.EnqueueAsync(((ArtistViewModel)lb.SelectedItem).Artist.ToList(), false, false);
                }
                else if (lb.SelectedItem.GetType().Name == typeof(GenreViewModel).Name)
                {
                    await this.playbackService.EnqueueAsync(((GenreViewModel)lb.SelectedItem).Genre.ToList(), false, false);
                }
                else if (lb.SelectedItem.GetType().Name == typeof(AlbumViewModel).Name)
                {
                    await this.playbackService.EnqueueAsync(((AlbumViewModel)lb.SelectedItem).Album.ToList(), false, false);
                }
            }
            catch (Exception ex)
            {
                CoreLogger.Current.Error("Error while handling action. Exception: {0}", ex.Message);
            }
        }

        protected override async Task ScrollToPlayingTrackAsync(Object sender)
        {
            try
            {
                // Cast sender to ListBox
                ListBox lb = (ListBox)sender;

                // This should provide a smoother experience because after this wait,
                // other animations on the UI should have finished executing.
                await Task.Delay(Convert.ToInt32(Constants.ScrollToPlayingTrackTimeoutSeconds * 1000));

                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    await ScrollUtils.ScrollToPlayingTrackAsync(lb, typeof(TrackViewModel));
                });
            }
            catch (Exception ex)
            {
                CoreLogger.Current.Error("Could not scroll to the playing track. Exception: {0}", ex.Message);
            }
        }

        protected override void ViewInExplorer(Object sender)
        {
            try
            {
                // Cast sender to ListBox
                ListBox lb = (ListBox)sender;

                if (lb.SelectedItem != null)
                {
                    Actions.TryViewInExplorer(((TrackViewModel)lb.SelectedItem).Track.Path);
                }
            }
            catch (Exception ex)
            {
                CoreLogger.Current.Error("Could not view track in Windows Explorer. Exception: {0}", ex.Message);
            }
        }
        #endregion
    }
}
