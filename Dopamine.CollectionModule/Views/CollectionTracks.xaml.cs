using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Enums;
using Dopamine.Common.Presentation.Utils;
using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Presentation.Views;
using Dopamine.Common.Base;
using Digimezzo.Utilities.Log;
using Dopamine.Common.Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dopamine.CollectionModule.Views
{
    public partial class CollectionTracks : CommonTracksView, INavigationAware
    {
        #region Variables
        private SubscriptionToken scrollToPlayingTrackToken;
        #endregion

        #region Construction
        public CollectionTracks()
        {
            InitializeComponent();

            this.screenName = typeof(CollectionTracks).FullName;

            // Commands
            this.ViewInExplorerCommand = new DelegateCommand(() => this.ViewInExplorer(this.DataGridTracks));
            this.JumpToPlayingTrackCommand = new DelegateCommand(async () => await this.ScrollToPlayingTrackAsync(this.DataGridTracks));

            // Events and Commands
            this.Subscribe();
        }
        #endregion

        #region Private
        private async void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await this.DataGridActionHandler(sender);
        }

        private async Task ScrollToPlayingTrackAsync(DataGrid dataGrid)
        {
            // This should provide a smoother experience because after this wait,
            // other animations on the UI should have finished executing.
            await Task.Delay(Convert.ToInt32(Constants.ScrollToPlayingTrackTimeoutSeconds * 1000));

            try
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    await ScrollUtils.ScrollToPlayingTrackAsync(dataGrid);
                });

            }
            catch (Exception ex)
            {
                LogClient.Error("Could not scroll to the playing track. Exception: {1}", ex.Message);
            }
        }

        private void ViewInExplorer(DataGrid iDataGrid)
        {
            if (iDataGrid.SelectedItem != null)
            {
                try
                {
                    Actions.TryViewInExplorer(((MergedTrackViewModel)iDataGrid.SelectedItem).Track.Path);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not view Track in Windows Explorer. Exception: {0}", ex.Message);
                }
            }
        }

        private async void DataGridTracks_KeyUp(object sender, KeyEventArgs e)
        {
            await this.TracksKeyUpHandlerAsync(sender, e);
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
                await this.playBackService.Enqueue(dg.Items.OfType<MergedTrackViewModel>().ToList().Select(vm => vm.Track).ToList(), ((MergedTrackViewModel)dg.SelectedItem).Track);
            }
        }

        private void Unsubscribe()
        {
            // Events
            this.eventAggregator.GetEvent<ScrollToPlayingTrack>().Unsubscribe(scrollToPlayingTrackToken);
        }


        private void Subscribe()
        {
            // Prevents subscribing twice
            this.Unsubscribe();

            // Events
            this.scrollToPlayingTrackToken = this.eventAggregator.GetEvent<ScrollToPlayingTrack>().Subscribe(async (s) => await this.ScrollToPlayingTrackAsync(this.DataGridTracks));
        }
        #endregion

        #region Protected
        protected async Task TracksKeyUpHandlerAsync(object sender, KeyEventArgs e)
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
                    Actions.TryViewInExplorer(((MergedTrackViewModel)dg.SelectedItem).Track.Path);
                }
            }
            else if (e.Key == Key.Delete)
            {
                this.eventAggregator.GetEvent<RemoveSelectedTracksWithKeyDelete>().Publish(this.screenName);
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
            SettingsClient.Set<int>("FullPlayer", "SelectedPage", (int)SelectedPage.Tracks);
        }
        #endregion
    }
}
