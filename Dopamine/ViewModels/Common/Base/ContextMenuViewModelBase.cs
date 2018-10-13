using Digimezzo.Utilities.Utils;
using Dopamine.Data.Entities;
using Dopamine.ViewModels;
using Dopamine.Services.Dialog;
using Dopamine.Services.Playback;
using Dopamine.Services.Playlist;
using Dopamine.Services.Provider;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Dopamine.Services.Entities;

namespace Dopamine.ViewModels.Common.Base
{
    public abstract class ContextMenuViewModelBase : BindableBase
    {
        private IProviderService providerService;
        private IPlaylistService playlistService;
        private IPlaybackService playbackService;
        private IDialogService dialogService;
        private ObservableCollection<SearchProvider> contextMenuSearchProviders;
        private ObservableCollection<PlaylistViewModel> contextMenuPlaylists;

        public DelegateCommand<string> SearchOnlineCommand { get; set; }
        public DelegateCommand<string> AddPlayingTrackToPlaylistCommand { get; set; }

        public bool HasContextMenuPlaylists => this.ContextMenuPlaylists != null && this.ContextMenuPlaylists.Count > 0;

        public ObservableCollection<SearchProvider> ContextMenuSearchProviders
        {
            get { return this.contextMenuSearchProviders; }
            set
            {
                SetProperty<ObservableCollection<SearchProvider>>(ref this.contextMenuSearchProviders, value);
                RaisePropertyChanged(nameof(this.HasContextMenuSearchProviders));
            }
        }

        public ObservableCollection<PlaylistViewModel> ContextMenuPlaylists
        {
            get { return this.contextMenuPlaylists; }
            set
            {
                SetProperty<ObservableCollection<PlaylistViewModel>>(ref this.contextMenuPlaylists, value);
                RaisePropertyChanged(nameof(this.HasContextMenuPlaylists));
            }
        }

        public ContextMenuViewModelBase(IContainerProvider container)
        {
            // Dependency injection
            this.providerService = container.Resolve<IProviderService>();
            this.playlistService = container.Resolve<IPlaylistService>();
            this.playbackService = container.Resolve<IPlaybackService>();
            this.dialogService = container.Resolve<IDialogService>();

            // Commands
            this.SearchOnlineCommand = new DelegateCommand<string>((id) => this.SearchOnline(id));
            this.AddPlayingTrackToPlaylistCommand = new DelegateCommand<string>(
            async (playlistName) => await this.AddPlayingTrackToPlaylistAsync(playlistName), (_) => this.playbackService.HasCurrentTrack);

            // Events
            this.providerService.SearchProvidersChanged += (_, __) => { this.GetSearchProvidersAsync(); };
            this.playbackService.PlaybackFailed += (_, __) => this.AddPlayingTrackToPlaylistCommand.RaiseCanExecuteChanged();
            this.playbackService.PlaybackSuccess += (_, __) => this.AddPlayingTrackToPlaylistCommand.RaiseCanExecuteChanged();
            this.playbackService.PlaybackStopped += (_, __) => this.AddPlayingTrackToPlaylistCommand.RaiseCanExecuteChanged();
            this.playbackService.PlaybackPaused += (_, __) => this.AddPlayingTrackToPlaylistCommand.RaiseCanExecuteChanged();
            this.playbackService.PlaybackResumed += (_, __) => this.AddPlayingTrackToPlaylistCommand.RaiseCanExecuteChanged();
            this.playlistService.PlaylistFolderChanged += (_, __) => this.GetContextMenuPlaylistsAsync();

            // Initialize the search providers in the ContextMenu
            this.GetSearchProvidersAsync();

            // Initialize the playlists in the ContextMenu
            this.GetContextMenuPlaylistsAsync();
        }

        private async Task AddPlayingTrackToPlaylistAsync(string playlistName)
        {
            if (!this.playbackService.HasCurrentTrack)
            {
                return;
            }

            var playingTrack = new List<TrackViewModel>() { this.playbackService.CurrentTrack };
            await this.AddTracksToPlaylistAsync(playlistName, playingTrack);
        }

        private async void GetSearchProvidersAsync()
        {
            this.ContextMenuSearchProviders = null;

            List<SearchProvider> providersList = await this.providerService.GetSearchProvidersAsync();
            var localProviders = new ObservableCollection<SearchProvider>();

            await Task.Run(() =>
            {
                foreach (SearchProvider vp in providersList)
                {
                    localProviders.Add(vp);
                }
            });

            this.ContextMenuSearchProviders = localProviders;
        }

        public async void GetContextMenuPlaylistsAsync()
        {
            try
            {
                // Unbind to improve UI performance
                this.ContextMenuPlaylists = null;
                
                // Populate an ObservableCollection
                var playlistViewModels = new ObservableCollection<PlaylistViewModel>(await this.playlistService.GetStaticPlaylistsAsync());

                // Re-bind to update the UI
                this.ContextMenuPlaylists = playlistViewModels;

            }
            catch (Exception)
            {
                // If loading from the database failed, create and empty Collection.
                this.ContextMenuPlaylists = new ObservableCollection<PlaylistViewModel>();
            }
        }

        protected bool HasContextMenuSearchProviders => this.ContextMenuSearchProviders != null && this.ContextMenuSearchProviders.Count > 0;

        protected async Task AddTracksToPlaylistAsync(string playlistName, IList<TrackViewModel> tracks)
        {
            CreateNewPlaylistResult addPlaylistResult = CreateNewPlaylistResult.Success; // Default Success

            // If no playlist is provided, first create one.
            if (playlistName == null)
            {
                var responseText = ResourceUtils.GetString("Language_New_Playlist");

                if (this.dialogService.ShowInputDialog(
                    0xea37,
                    16,
                    ResourceUtils.GetString("Language_New_Playlist"),
                    ResourceUtils.GetString("Language_Enter_Name_For_Playlist"),
                    ResourceUtils.GetString("Language_Ok"),
                    ResourceUtils.GetString("Language_Cancel"),
                    ref responseText))
                {
                    playlistName = responseText;
                    addPlaylistResult = await this.playlistService.CreateNewPlaylistAsync(new EditablePlaylistViewModel(playlistName, PlaylistType.Static));
                }
            }

            // If playlist name is still null, the user clicked cancel on the previous dialog. Stop here.
            if (playlistName == null) return;

            // Verify if the playlist was added
            switch (addPlaylistResult)
            {
                case CreateNewPlaylistResult.Success:
                case CreateNewPlaylistResult.Duplicate:
                    // Add items to playlist
                    AddTracksToPlaylistResult result = await this.playlistService.AddTracksToStaticPlaylistAsync(tracks, playlistName);

                    if (result == AddTracksToPlaylistResult.Error)
                    {
                        this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Adding_Songs_To_Playlist").Replace("{playlistname}", "\"" + playlistName + "\""), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
                    }
                    break;
                case CreateNewPlaylistResult.Error:
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetString("Language_Error"),
                        ResourceUtils.GetString("Language_Error_Adding_Playlist"),
                        ResourceUtils.GetString("Language_Ok"),
                        true,
                        ResourceUtils.GetString("Language_Log_File"));
                    break;
                case CreateNewPlaylistResult.Blank:
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

        protected abstract void SearchOnline(string id);
    }
}
