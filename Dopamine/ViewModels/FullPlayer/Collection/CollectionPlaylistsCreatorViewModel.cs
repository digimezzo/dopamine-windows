using Digimezzo.Foundation.Core.Helpers;
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
        private int limit;
        private bool limitEnabled;
        private ObservableCollection<NameValue> limitTypes;

        public CollectionPlaylistsCreatorViewModel(IPlaylistService playlistService)
        {
            this.playlistService = playlistService;

            this.AddRuleCommand = new DelegateCommand(() => this.AddRule());
            this.RemoveRuleCommand = new DelegateCommand<SmartPlaylistRuleViewModel>((rule) => this.RemoveRule(rule));
            this.InitializeAsync();
        }

        public DelegateCommand AddRuleCommand { get; set; }

        public DelegateCommand<SmartPlaylistRuleViewModel> RemoveRuleCommand { get; set; }

        public ObservableCollection<SmartPlaylistRuleViewModel> Rules
        {
            get { return this.rules; }
            set { SetProperty<ObservableCollection<SmartPlaylistRuleViewModel>>(ref this.rules, value); }
        }

        public int Limit
        {
            get { return this.limit; }
            set { SetProperty<int>(ref this.limit, value); }
        }

        public bool LimitEnabled
        {
            get { return this.limitEnabled; }
            set { SetProperty<bool>(ref this.limitEnabled, value); }
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

        public ObservableCollection<NameValue> LimitTypes
        {
            get { return this.limitTypes; }
            set { SetProperty<ObservableCollection<NameValue>>(ref this.limitTypes, value); }
        }

        private async void InitializeAsync()
        {
            this.Rules = new ObservableCollection<SmartPlaylistRuleViewModel>();
            this.Rules.Add(new SmartPlaylistRuleViewModel());
            this.Limit = 1;
            this.PlaylistName = await this.playlistService.GetUniquePlaylistNameAsync(ResourceUtils.GetString("Language_New_Playlist"));
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
