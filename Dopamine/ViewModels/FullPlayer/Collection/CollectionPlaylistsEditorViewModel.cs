using Digimezzo.Utilities.Utils;
using Dopamine.Services.Entities;
using Dopamine.Services.Playlist;
using Prism.Commands;
using Prism.Mvvm;

namespace Dopamine.ViewModels.FullPlayer.Collection
{
    public class CollectionPlaylistsEditorViewModel : BindableBase
    {
        private PlaylistType playlistType;
        private IPlaylistService playlistService;
        private int selectedIndex;
        private EditablePlaylistViewModel editablePlaylist;

        public CollectionPlaylistsEditorViewModel(IPlaylistService playlistService)
        {
            this.playlistService = playlistService;

            this.AddRuleCommand = new DelegateCommand(() => this.AddRule());
            this.RemoveRuleCommand = new DelegateCommand<SmartPlaylistRuleViewModel>((rule) => this.RemoveRule(rule));
            this.InitializeAsync();
        }

        public DelegateCommand AddRuleCommand { get; set; }

        public DelegateCommand<SmartPlaylistRuleViewModel> RemoveRuleCommand { get; set; }

        public EditablePlaylistViewModel EditablePlaylist
        {
            get { return this.editablePlaylist; }
            set { SetProperty<EditablePlaylistViewModel>(ref this.editablePlaylist, value); }
        }

        public int SelectedIndex
        {
            get { return this.selectedIndex; }
            set {
                SetProperty<int>(ref this.selectedIndex, value);

                // Direct binding of the pivot page selection to editablePlaylist.Type
                // doesn't work well. So we're changing editablePlaylist.Type in this setter.
                if (this.editablePlaylist != null)
                {
                    this.editablePlaylist.Type = (PlaylistType)value;
                }
            }
        }

        public PlaylistType PlaylistType
        {
            get { return this.playlistType; }
            set { SetProperty<PlaylistType>(ref this.playlistType, value); }
        }

        private async void InitializeAsync()
        {
            this.EditablePlaylist = new EditablePlaylistViewModel(
                await this.playlistService.GetUniquePlaylistNameAsync(ResourceUtils.GetString("Language_New_Playlist")),
                PlaylistType.Static
                );
        }

        private void AddRule()
        {
            this.editablePlaylist.AddRule();
        }

        private void RemoveRule(SmartPlaylistRuleViewModel rule)
        {
            this.editablePlaylist.RemoveRule(rule);
        }
    }
}
