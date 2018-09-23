using Digimezzo.Utilities.Utils;
using Dopamine.Services.Entities;
using Dopamine.Services.Playlist;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Dopamine.ViewModels.FullPlayer.Collection
{
    public class CollectionPlaylistsCreatorViewModel : BindableBase
    {
        private PlaylistType playlistType;
        private IPlaylistService playlistService;
        private string playlistName;
        private ObservableCollection<SmartPlaylistRuleViewModel> rules;

        public CollectionPlaylistsCreatorViewModel(IPlaylistService playlistService)
        {
            this.playlistService = playlistService;

            this.LoadedCommand = new DelegateCommand(() => this.LoadedAsync());
            this.AddRuleCommand = new DelegateCommand(() => this.AddRule());
            this.RemoveRuleCommand = new DelegateCommand<SmartPlaylistRuleViewModel>((rule) => this.RemoveRule(rule));
            this.AddFirstRule();
        }

        public DelegateCommand LoadedCommand { get; set; }

        public DelegateCommand AddRuleCommand { get; set; }

        public DelegateCommand<SmartPlaylistRuleViewModel> RemoveRuleCommand { get; set; }

        public ObservableCollection<SmartPlaylistRuleViewModel> Rules
        {
            get { return this.rules; }
            set { SetProperty<ObservableCollection<SmartPlaylistRuleViewModel>>(ref this.rules, value); }
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

        private void AddFirstRule()
        {
            this.Rules = new ObservableCollection<SmartPlaylistRuleViewModel>();
            this.Rules.Add(new SmartPlaylistRuleViewModel());
        }

        private void AddRule()
        {
            this.Rules.Add(new SmartPlaylistRuleViewModel());
        }

        private void RemoveRule(SmartPlaylistRuleViewModel rule)
        {
            this.Rules.Remove(rule);
        }
    }
}
