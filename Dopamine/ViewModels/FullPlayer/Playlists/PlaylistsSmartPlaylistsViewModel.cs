using Digimezzo.Utilities.Settings;
using Dopamine.ViewModels.Common.Base;
using GongSolutions.Wpf.DragDrop;
using Prism.Ioc;
using System;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.FullPlayer.Playlists
{
    public class PlaylistsSmartPlaylistsViewModel : PlaylistsViewModelBase, IDropTarget
    {
        private double leftPaneWidthPercent;

        public PlaylistsSmartPlaylistsViewModel(IContainerProvider container) : base(container)
        {
            // Load settings
            this.LeftPaneWidthPercent = SettingsClient.Get<int>("ColumnWidths", "SmartPlaylistsLeftPaneWidthPercent");
        }

        public double LeftPaneWidthPercent
        {
            get { return this.leftPaneWidthPercent; }
            set
            {
                SetProperty<double>(ref this.leftPaneWidthPercent, value);
                SettingsClient.Set<int>("ColumnWidths", "SmartPlaylistsLeftPaneWidthPercent", Convert.ToInt32(value));
            }
        }

        public void DragOver(IDropInfo dropInfo)
        {
            // TODO
        }

        public void Drop(IDropInfo dropInfo)
        {
            // TODO
        }

        protected override async Task GetTracksAsync()
        {
            // TODO
        }
    }
}
