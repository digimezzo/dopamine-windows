using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Dopamine.Services.Entities;
using Dopamine.Services.Playlist;
using Dopamine.ViewModels.Common.Base;
using GongSolutions.Wpf.DragDrop;
using Prism.Ioc;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.FullPlayer.Playlists
{
    public class PlaylistsSmartPlaylistsViewModel : PlaylistsViewModelBase, IDropTarget
    {
        private ISmartPlaylistService smartPlaylistService;
        private double leftPaneWidthPercent;

        public PlaylistsSmartPlaylistsViewModel(IContainerProvider container, ISmartPlaylistService smartPlaylistService) : base(container)
        {
            // Dependency injection
            this.smartPlaylistService = smartPlaylistService;

            // Events
            this.smartPlaylistService.PlaylistFolderChanged += SmartPlaylistService_PlaylistFolderChanged; ;

            // Load settings
            this.LeftPaneWidthPercent = SettingsClient.Get<int>("ColumnWidths", "SmartPlaylistsLeftPaneWidthPercent");
        }

        private async void SmartPlaylistService_PlaylistFolderChanged(object sender, EventArgs e)
        {
            await this.FillListsAsync();
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

        protected override async Task GetPlaylistsAsync()
        {
            try
            {
                // Populate an ObservableCollection
                var playlistViewModels = new ObservableCollection<PlaylistViewModel>(await this.smartPlaylistService.GetPlaylistsAsync());

                // Unbind and rebind to improve UI performance
                this.Playlists = null;
                this.Playlists = playlistViewModels;
            }
            catch (Exception ex)
            {
                LogClient.Error("An error occurred while getting Smart Playlists. Exception: {0}", ex.Message);

                // If loading from the database failed, create and empty Collection.
                this.Playlists = new ObservableCollection<PlaylistViewModel>();
            }

            // Notify that the count has changed
            this.RaisePropertyChanged(nameof(this.PlaylistsCount));

            // Select the firts playlist
            this.TrySelectFirstPlaylist();
        }

        protected override async Task GetTracksAsync()
        {
            // TODO
        }
    }
}
