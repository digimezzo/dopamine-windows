using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Utils;
using Dopamine.Core.Base;
using Dopamine.Common.Presentation.Utils;
using Dopamine.Common.Presentation.ViewModels.Entities;
using Dopamine.Common.Presentation.Views.Base;
using Dopamine.Core.Prism;
using Prism.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dopamine.Views.FullPlayer.Collection
{
    public partial class CollectionTracks : CommonViewBase
    {
        public CollectionTracks() : base()
        {
            InitializeComponent();

            // Commands
            this.ViewInExplorerCommand = new DelegateCommand(() => this.ViewInExplorer(this.DataGridTracks));
            this.JumpToPlayingTrackCommand = new DelegateCommand(async () => await this.ScrollToPlayingTrackAsync(this.DataGridTracks));

            // PubSub Events
            this.eventAggregator.GetEvent<ScrollToPlayingTrack>().Subscribe(async (_) => await this.ScrollToPlayingTrackAsync(this.DataGridTracks));
        }

        private async void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await this.ActionHandler(sender, null, false);
        }

        private async void DataGridTracks_KeyUp(object sender, KeyEventArgs e)
        {
            await this.KeyUpHandlerAsync(sender, e);
        }

        private async void DataGridTracks_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Prevent DataGrid.KeyDown to make the selection to go to the next row when pressing Enter
                e.Handled = true;

                // Makes sure that this action is triggered by a DataGridCell. This prevents  
                // enqueuing when clicking other ListBox elements (e.g. the ScrollBar)
                DataGridCell dataGridCell = VisualTreeUtils.FindAncestor<DataGridCell>((DependencyObject)e.OriginalSource);

                if (dataGridCell == null) return;

                DataGrid dg = (DataGrid)sender;
                await this.playbackService.EnqueueAsync(dg.Items.OfType<TrackViewModel>().ToList().Select(vm => vm.Track).ToList(), ((TrackViewModel)dg.SelectedItem).Track);
            }
        }

        protected async override Task ActionHandler(Object sender, DependencyObject source, bool enqueue)
        {
            try
            {
                var dg = VisualTreeUtils.FindAncestor<DataGrid>((DataGridRow)sender);
                await this.playbackService.EnqueueAsync(dg.Items.OfType<TrackViewModel>().ToList().Select(vm => vm.Track).ToList(), ((TrackViewModel)dg.SelectedItem).Track);
            }
            catch (Exception ex)
            {
                LogClient.Error("Error while handling DataGrid action. Exception: {0}", ex.Message);
            }
        }

        protected override async Task KeyUpHandlerAsync(object sender, KeyEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;

            if (e.Key == Key.J && Keyboard.Modifiers == ModifierKeys.Control)
            {
                await this.ScrollToPlayingTrackAsync(dg);

            }
            else if (e.Key == Key.E && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (dg.SelectedItem != null)
                {
                    Actions.TryViewInExplorer(((TrackViewModel)dg.SelectedItem).Track.Path);
                }
            }
        }

        protected override async Task ScrollToPlayingTrackAsync(object sender)
        {
            try
            {
                // Cast sender to ListBox
                DataGrid dg = (DataGrid)sender;

                // This should provide a smoother experience because after this wait,
                // other animations on the UI should have finished executing.
                await Task.Delay(Convert.ToInt32(Constants.ScrollToPlayingTrackTimeoutSeconds * 1000));

                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    await ScrollUtils.ScrollToPlayingTrackAsync(dg);
                });
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not scroll to the playing track. Exception: {1}", ex.Message);
            }
        }

        protected override void ViewInExplorer(object sender)
        {
            try
            {
                // Cast sender to DataGrid
                DataGrid dg = (DataGrid)sender;

                if (dg.SelectedItem != null)
                {
                    Actions.TryViewInExplorer(((TrackViewModel)dg.SelectedItem).Track.Path);
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not view track in Windows Explorer. Exception: {0}", ex.Message);
            }
        }
    }
}
