using Digimezzo.Utilities.Log;
using Dopamine.Core.Base;
using Dopamine.Data;
using Dopamine.Services.Entities;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.Common.Base
{
    public abstract class PlaylistsViewModelBase : TracksViewModelBaseWithTrackArt
    {
        private ObservableCollection<PlaylistViewModel> playlists;
        private PlaylistViewModel selectedPlaylist;

        public PlaylistsViewModelBase(IContainerProvider container) : base(container)
        {
        }

        public string PlaylistsTarget => "ListBoxPlaylists";

        public string TracksTarget => "ListBoxTracks";

        public bool IsPlaylistSelected => this.selectedPlaylist != null;

        public long PlaylistsCount => this.playlists == null ? 0 : this.playlists.Count;

        public ObservableCollection<PlaylistViewModel> Playlists
        {
            get { return this.playlists; }
            set { SetProperty<ObservableCollection<PlaylistViewModel>>(ref this.playlists, value); }
        }

        public PlaylistViewModel SelectedPlaylist
        {
            get { return this.selectedPlaylist; }
            set
            {
                SetProperty<PlaylistViewModel>(ref this.selectedPlaylist, value);

                if (value != null)
                {
                    this.GetTracksAsync();
                }
                else
                {
                    this.ClearTracks();
                }
            }
        }

        public string SelectedPlaylistName
        {
            get
            {
                if (this.SelectedPlaylist != null && !string.IsNullOrEmpty(this.SelectedPlaylist.Name))
                {
                    return this.SelectedPlaylist.Name;
                }

                return null;
            }
        }

        protected abstract Task GetTracksAsync();

        protected abstract Task GetPlaylistsAsync();

        private async Task ClearTracks()
        {
            await this.GetTracksCommonAsync(new List<TrackViewModel>(), TrackOrder.None);
        }

        protected void TrySelectFirstPlaylist()
        {
            try
            {
                if (this.Playlists.Count > 0)
                {
                    this.SelectedPlaylist = this.Playlists[0];
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("An error occurred while selecting the playlist. Exception: {0}", ex.Message);
            }
        }

        protected override async Task FillListsAsync()
        {
            await this.GetPlaylistsAsync();
            await this.GetTracksAsync();
        }

        protected override async Task LoadedCommandAsync()
        {
            if (!this.IsFirstLoad())
            {
                return;
            }

            await Task.Delay(Constants.CommonListLoadDelay); // Wait for the UI to slide in
            await this.FillListsAsync(); // Fill all the lists
        }
    }
}
