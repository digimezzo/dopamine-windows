using Dopamine.Common.Presentation.Utils;
using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Base;
using Dopamine.Core.IO;
using Dopamine.Core.Logging;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Events;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dopamine.Common.Presentation.Views
{
    public class CommonTracksView : UserControl
    {
        #region Variables
        protected IEventAggregator eventAggregator;
        protected IPlaybackService playBackService;
        #endregion

        #region Commands
        public DelegateCommand ViewInExplorerCommand { get; set; }
        public DelegateCommand JumpToPlayingTrackCommand { get; set; }
        #endregion

        #region Construction
        public CommonTracksView()
        {
            // We need a parameterless constructor to be able to use this UserControl in other UserControls without dependency injection.
            // So for now there is no better solution than to find the EventAggregator by using the ServiceLocator.
            this.eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            this.playBackService = ServiceLocator.Current.GetInstance<IPlaybackService>();
        }
        #endregion

        #region Protected
        protected async Task TracksKeyUpHandlerAsync(object sender, KeyEventArgs e)
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
                    Actions.TryViewInExplorer(((TrackInfoViewModel)lb.SelectedItem).TrackInfo.Path);
                }
            }
            else if (e.Key == Key.Delete)
            {
                Core.Prism.ApplicationCommands.RemoveSelectedTracksCommand.Execute(null);
            }
        }

        protected async Task DataGridActionHandler(object sender)
        {
            try
            {
                DataGrid dg = (DataGrid)sender;

                if (dg.SelectedItem == null) return;

                await this.playBackService.Enqueue(dg.Items.OfType<TrackInfoViewModel>().ToList().Select(tivm => tivm.TrackInfo).ToList(), ((TrackInfoViewModel)dg.SelectedItem).TrackInfo);

            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Error while handling DataGrid action. Exception: {0}", ex.Message);
            }
        }

        protected async Task ListActionHandler(Object sender)
        {
            try
            {
                ListBox lb = (ListBox)sender;

                if (lb.SelectedItem == null) return;

                if (lb.SelectedItem.GetType().Name == typeof(TrackInfoViewModel).Name)
                {
                    await this.playBackService.Enqueue(lb.Items.OfType<TrackInfoViewModel>().ToList().Select((tivm) => tivm.TrackInfo).ToList(), ((TrackInfoViewModel)lb.SelectedItem).TrackInfo);
                }
                else if (lb.SelectedItem.GetType().Name == typeof(ArtistViewModel).Name)
                {
                    await this.playBackService.Enqueue(((ArtistViewModel)lb.SelectedItem).Artist);
                }
                else if (lb.SelectedItem.GetType().Name == typeof(GenreViewModel).Name)
                {
                    await this.playBackService.Enqueue(((GenreViewModel)lb.SelectedItem).Genre);
                }
                else if (lb.SelectedItem.GetType().Name == typeof(AlbumViewModel).Name)
                {
                    await this.playBackService.Enqueue(((AlbumViewModel)lb.SelectedItem).Album);
                }
                else if (lb.SelectedItem.GetType().Name == typeof(PlaylistViewModel).Name)
                {
                    await this.playBackService.Enqueue(((PlaylistViewModel)lb.SelectedItem).Playlist);
                }
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Error while handling ListBox action. Exception: {0}", ex.Message);
            }
        }

        protected async Task ScrollToPlayingTrackAsync(ListBox listBox)
        {
            // This should provide a smoother experience because after this wait,
            // other animations on the UI should have finished executing.
            await Task.Delay(Convert.ToInt32(Constants.ScrollToPlayingTrackTimeoutSeconds * 1000));

            try
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    await ScrollUtils.ScrollToPlayingTrackAsync(listBox);
                });
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not scroll to the playing track. Exception: {1}", ex.Message);
            }
        }

        /// <summary>
        /// Overridable because some classes might use another type of Items Control than a ListBox.
        /// in such cases, that class can Override this method and provide its own implementation.
        /// </summary>
        /// <param name="listBox"></param>
        /// <remarks></remarks>
        protected virtual void ViewInExplorer(ListBox listBox)
        {
            if (listBox.SelectedItem != null)
            {
                try
                {
                    Actions.TryViewInExplorer(((TrackInfoViewModel)listBox.SelectedItem).TrackInfo.Path);
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not view Track in Windows Explorer. Exception: {0}", ex.Message);
                }
            }
        }
        #endregion
    }
}
