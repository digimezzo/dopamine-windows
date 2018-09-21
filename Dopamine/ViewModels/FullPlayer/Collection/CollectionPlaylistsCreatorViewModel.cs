using Digimezzo.Utilities.Utils;
using Dopamine.Services.Playlist;
using Prism.Commands;
using Prism.Mvvm;

namespace Dopamine.ViewModels.FullPlayer.Collection
{
    public class CollectionPlaylistsCreatorViewModel : BindableBase
    {
        private PlaylistType playlistType;
        private IPlaylistService playlistService;
        private string playlistName;

        public DelegateCommand LoadedCommand { get; set; }

        public CollectionPlaylistsCreatorViewModel(IPlaylistService playlistService)
        {
            this.playlistService = playlistService;

            this.LoadedCommand = new DelegateCommand(() => this.LoadedAsync());
        }

        private async void LoadedAsync()
        {
            this.PlaylistName = await this.playlistService.GetUniquePlaylistNameAsync(ResourceUtils.GetString("Language_New_Playlist"));
        }

        public PlaylistType PlaylistType
        {
            get { return this.playlistType; }
            set { SetProperty<PlaylistType>(ref this.playlistType, value); }
        }

        public string PlaylistName
        {
            get { return this.playlistName; }
            set { SetProperty<string>(ref this.playlistName, value); }
        }
    }
}
