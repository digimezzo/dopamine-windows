using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Log;
using Dopamine.Services.Entities;
using System;
using System.Windows.Controls;

namespace Dopamine.Views.Common.Base
{
    public class PlaylistsViewBase : TracksViewBase
    {
        protected void ViewPlaylistInExplorer(Object sender)
        {
            try
            {
                // Cast sender to ListBox
                ListBox lb = (ListBox)sender;

                if (lb.SelectedItem != null)
                {
                    PlaylistViewModel playlist = lb.SelectedItem as PlaylistViewModel;

                    Actions.TryViewInExplorer(playlist.Path);
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not view playlist in Windows Explorer. Exception: {0}", ex.Message);
            }
        }
    }
}
