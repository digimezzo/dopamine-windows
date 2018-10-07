using Prism.Mvvm;

namespace Dopamine.Services.Entities
{
    public class SmartPlaylistRuleOperatorViewModel : BindableBase
    {
        private string displayName;
        private string name;

        public SmartPlaylistRuleOperatorViewModel(string displayName, string name)
        {
            this.displayName = displayName;
            this.name = name;
        }

        public string DisplayName
        {
            get { return this.displayName; }
            set { SetProperty<string>(ref this.displayName, value); }
        }

        public string Name
        {
            get { return this.name; }
            set { SetProperty<string>(ref this.name, value); }
        }

        public override string ToString()
        {
            return this.displayName;
        }
    }
}
