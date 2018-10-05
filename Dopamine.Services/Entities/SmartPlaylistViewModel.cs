using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Dopamine.Services.Entities
{
    public class SmartPlaylistViewModel : BindableBase
    {
        private string playlistName;
        private string match;
        private string order;
        private SmartPlaylistLimitViewModel limit;
        private ObservableCollection<SmartPlaylistRuleViewModel> rules;

        public string PlaylistName
        {
            get { return this.playlistName; }
            set { SetProperty<string>(ref this.playlistName, value); }
        }

        public string Match
        {
            get { return this.match; }
            set { SetProperty<string>(ref this.match, value); }
        }

        public string Order
        {
            get { return this.order; }
            set { SetProperty<string>(ref this.order, value); }
        }

        public SmartPlaylistLimitViewModel Limit
        {
            get { return this.limit; }
            set { SetProperty<SmartPlaylistLimitViewModel>(ref this.limit, value); }
        }

        public ObservableCollection<SmartPlaylistRuleViewModel> Rules
        {
            get { return this.rules; }
            set { SetProperty<ObservableCollection<SmartPlaylistRuleViewModel>>(ref this.rules, value); }
        }
    }
}
