using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Core.Base;
using Dopamine.Data;
using Dopamine.Services.Dialog;
using Dopamine.Services.Entities;
using Dopamine.Services.Playlist;
using Prism.Commands;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace Dopamine.ViewModels.Common.Base
{
    public abstract class PlaylistsViewModelBase : TracksViewModelBaseWithTrackArt
    {
        private ObservableCollection<PlaylistViewModel> playlists;
        private PlaylistViewModel selectedPlaylist;
        private IDialogService dialogService;
        private IPlaylistServiceBase playlistServiceBase;

        public DelegateCommand NewPlaylistCommand { get; set; }

        public DelegateCommand RenameSelectedPlaylistCommand { get; set; }

        public DelegateCommand ImportPlaylistsCommand { get; set; }

        public DelegateCommand DeleteSelectedPlaylistCommand { get; set; }

        public DelegateCommand<PlaylistViewModel> DeletePlaylistCommand { get; set; }

        public PlaylistsViewModelBase(IContainerProvider container, IDialogService dialogService, IPlaylistServiceBase playlistServiceBase) : base(container)
        {
            this.dialogService = dialogService;
            this.playlistServiceBase = playlistServiceBase;

            // Events
            this.playlistServiceBase.PlaylistFolderChanged += PlaylistServiceBase_PlaylistFolderChanged; ;
            this.playlistServiceBase.PlaylistAdded += PlaylistServiceBase_PlaylistAdded;
            this.playlistServiceBase.PlaylistDeleted += PlaylistServiceBase_PlaylistDeleted;
            this.playlistServiceBase.PlaylistRenamed += PlaylistServiceBase_PlaylistRenamed;

            // Commands
            this.RenameSelectedPlaylistCommand = new DelegateCommand(async () => await this.RenameSelectedPlaylistAsync());
            this.DeletePlaylistCommand = new DelegateCommand<PlaylistViewModel>(async (playlist) => await this.ConfirmDeletePlaylistAsync(playlist));
            this.ImportPlaylistsCommand = new DelegateCommand(async () => await this.ImportPlaylistsAsync());

            this.DeleteSelectedPlaylistCommand = new DelegateCommand(async () =>
            {
                if (this.IsPlaylistSelected)
                {
                    await this.ConfirmDeletePlaylistAsync(this.SelectedPlaylist);
                }
            });

            // Settings changed
            SettingsClient.SettingChanged += (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "Behaviour", "EnableRating"))
                {
                    this.EnableRating = (bool)e.SettingValue;
                }

                if (SettingsClient.IsSettingChanged(e, "Behaviour", "EnableLove"))
                {
                    this.EnableLove = (bool)e.SettingValue;
                }
            };
        }

        private void PlaylistServiceBase_PlaylistRenamed(PlaylistViewModel oldPlaylist, PlaylistViewModel newPlaylist)
        {
            // Remove the old playlist
            if (this.Playlists.Contains(oldPlaylist))
            {
                this.Playlists.Remove(oldPlaylist);
            }

            // Add the new playlist
            this.Playlists.Add(newPlaylist);
        }

        private void PlaylistServiceBase_PlaylistDeleted(PlaylistViewModel deletedPlaylist)
        {
            this.Playlists.Remove(deletedPlaylist);

            // If the selected playlist was deleted, select the first playlist.
            if (this.SelectedPlaylist == null)
            {
                this.TrySelectFirstPlaylist();
            }

            // Notify that the count has changed
            this.RaisePropertyChanged(nameof(this.PlaylistsCount));
        }

        private void PlaylistServiceBase_PlaylistAdded(PlaylistViewModel addedPlaylist)
        {
            this.Playlists.Add(addedPlaylist);

            // If there is only 1 playlist, automatically select it.
            if (this.Playlists != null && this.Playlists.Count == 1)
            {
                this.TrySelectFirstPlaylist();
            }

            // Notify that the count has changed
            this.RaisePropertyChanged(nameof(this.PlaylistsCount));
        }

        private async void PlaylistServiceBase_PlaylistFolderChanged(object sender, EventArgs e)
        {
            await this.FillListsAsync();
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

        protected async Task ImportPlaylistsAsync(IList<string> playlistPaths = null)
        {
            if (playlistPaths == null)
            {
                // Set up the file dialog box
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.Title = Application.Current.FindResource("Language_Import_Playlists").ToString();
                dlg.DefaultExt = FileFormats.M3U; // Default file extension
                dlg.Multiselect = true;

                // Filter files by extension
                dlg.Filter = $"{ResourceUtils.GetString("Language_Playlists")} {this.playlistServiceBase.DialogFileFilter}";

                // Show the file dialog box
                bool? dialogResult = dlg.ShowDialog();

                // Process the file dialog box result
                if (!(bool)dialogResult)
                {
                    return;
                }

                playlistPaths = dlg.FileNames;
            }

            ImportPlaylistResult result = await this.playlistServiceBase.ImportPlaylistsAsync(playlistPaths);

            if (result == ImportPlaylistResult.Error)
            {
                this.dialogService.ShowNotification(
                    0xe711,
                    16,
                    ResourceUtils.GetString("Language_Error"),
                    ResourceUtils.GetString("Language_Error_Importing_Playlists"),
                    ResourceUtils.GetString("Language_Ok"),
                    true,
                    ResourceUtils.GetString("Language_Log_File"));
            }
        }

        protected abstract Task DeletePlaylistAsync(PlaylistViewModel playlist);

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

        private async Task ConfirmDeletePlaylistAsync(PlaylistViewModel playlist)
        {
            if (this.dialogService.ShowConfirmation(
                0xe11b,
                16,
                ResourceUtils.GetString("Language_Delete"),
                ResourceUtils.GetString("Language_Are_You_Sure_To_Delete_Playlist").Replace("{playlistname}", playlist.Name),
                ResourceUtils.GetString("Language_Yes"),
                ResourceUtils.GetString("Language_No")))
            {
                await this.DeletePlaylistAsync(playlist);
            }
        }

        private async Task RenameSelectedPlaylistAsync()
        {
            if (!this.IsPlaylistSelected)
            {
                return;
            }

            PlaylistViewModel oldPlaylist = this.SelectedPlaylist;
            string newPlaylistName = oldPlaylist.Name;

            if (this.dialogService.ShowInputDialog(
                0xea37,
                16,
                ResourceUtils.GetString("Language_Rename_Playlist"),
                ResourceUtils.GetString("Language_Enter_New_Name_For_Playlist").Replace("{playlistname}", oldPlaylist.Name),
                ResourceUtils.GetString("Language_Ok"),
                ResourceUtils.GetString("Language_Cancel"),
                ref newPlaylistName))
            {
                RenamePlaylistResult result = await this.playlistServiceBase.RenamePlaylistAsync(this.SelectedPlaylist, newPlaylistName);

                switch (result)
                {
                    case RenamePlaylistResult.Duplicate:
                        this.dialogService.ShowNotification(
                            0xe711,
                            16,
                            ResourceUtils.GetString("Language_Already_Exists"),
                            ResourceUtils.GetString("Language_Already_Playlist_With_That_Name").Replace("{playlistname}", newPlaylistName),
                            ResourceUtils.GetString("Language_Ok"),
                            false,
                            string.Empty);
                        break;
                    case RenamePlaylistResult.Error:
                        this.dialogService.ShowNotification(
                            0xe711,
                            16,
                            ResourceUtils.GetString("Language_Error"),
                            ResourceUtils.GetString("Language_Error_Renaming_Playlist"),
                            ResourceUtils.GetString("Language_Ok"),
                            true,
                            ResourceUtils.GetString("Language_Log_File"));
                        break;
                    case RenamePlaylistResult.Blank:
                        this.dialogService.ShowNotification(
                            0xe711,
                            16,
                            ResourceUtils.GetString("Language_Error"),
                            ResourceUtils.GetString("Language_Provide_Playlist_Name"),
                            ResourceUtils.GetString("Language_Ok"),
                            false,
                            string.Empty);
                        break;
                    default:
                        // Never happens
                        break;
                }
            }
        }
    }
}
