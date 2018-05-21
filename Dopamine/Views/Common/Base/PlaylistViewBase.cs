using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Log;
using Digimezzo.WPFControls;
using Dopamine.Core.Base;
using Dopamine.Data.Contracts.Entities;
using Dopamine.Utils;
using Dopamine.ViewModels;
using Dopamine.Services.Contracts.Playlist;
using CommonServiceLocator;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Dopamine.Views.Common.Base
{
    public abstract class PlaylistViewBase : CommonViewBase
    {
        protected IPlaylistService playlistService;

        public DelegateCommand ViewPlaylistInExplorerCommand { get; set; }
     
        public PlaylistViewBase()
        {
            // We need a parameterless constructor to be able to use this UserControl in other UserControls without dependency injection.
            // So for now there is no better solution than to find the EventAggregator by using the ServiceLocator.
            this.playlistService = ServiceLocator.Current.GetInstance<IPlaylistService>();
        }
  
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
                    Actions.TryViewInExplorer(((KeyValuePair<string, TrackViewModel>)lb.SelectedItem).Value.Track.Path);
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

                if (!enqueue)
                {
                    // The user just wants to play the selected item. Don't enqueue.
                    if (lb.SelectedItem.GetType().Name == typeof(KeyValuePair<string, PlayableTrack>).Name)
                    {
                        var selectedTrack = (KeyValuePair<string, TrackViewModel>)lb.SelectedItem;
                        await this.playbackService.PlaySelectedAsync(new KeyValuePair<string, PlayableTrack>(selectedTrack.Key, selectedTrack.Value.Track));
                    }

                    return;
                }
                else
                {
                    // The user wants to enqueue tracks for the selected item
                    if (lb.SelectedItem.GetType().Name == typeof(KeyValuePair<string, TrackViewModel>).Name)
                    {
                        List<KeyValuePair<string, TrackViewModel>> items = lb.Items.OfType<KeyValuePair<string, TrackViewModel>>().ToList();
                        KeyValuePair<string, TrackViewModel> selectedItem = (KeyValuePair<string, TrackViewModel>)lb.SelectedItem;

                        var tracks = new List<KeyValuePair<string, PlayableTrack>>();

                        foreach (KeyValuePair<string, TrackViewModel> item in items)
                        {
                            tracks.Add(new KeyValuePair<string, PlayableTrack>(item.Key, item.Value.Track));
                        }

                        await this.playbackService.EnqueueAsync(tracks, new KeyValuePair<string, PlayableTrack>(selectedItem.Key, selectedItem.Value.Track));
                    }
                    else if (lb.SelectedItem.GetType().Name == typeof(PlaylistViewModel).Name)
                    {
                        List<PlayableTrack> tracks = await this.playlistService.GetTracks(((PlaylistViewModel)lb.SelectedItem).Name);
                        await this.playbackService.EnqueueAsync(tracks);
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Error while handling action. Exception: {0}", ex.Message);
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
                await Task.Delay(Convert.ToInt32(Core.Base.Constants.ScrollToPlayingTrackTimeoutSeconds * 1000));

                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    await ScrollUtils.ScrollToPlayingTrackAsync(lb, typeof(KeyValuePair<string, TrackViewModel>));
                });
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not scroll to the playing track. Exception: {0}", ex.Message);
            }
        }

        protected void ViewPlaylistInExplorer(Object sender)
        {
            try
            {
                // Cast sender to ListBox
                ListBox lb = (ListBox)sender;

                if (lb.SelectedItem != null)
                {
                    string playlist = ((PlaylistViewModel)lb.SelectedItem).Name;

                    Actions.TryViewInExplorer(Path.Combine(this.playlistService.PlaylistFolder, playlist + FileFormats.M3U));
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not view playlist in Windows Explorer. Exception: {0}", ex.Message);
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
                    Actions.TryViewInExplorer(((KeyValuePair<string, TrackViewModel>)lb.SelectedItem).Value.Track.Path);
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not view track in Windows Explorer. Exception: {0}", ex.Message);
            }
        }
    }
}
