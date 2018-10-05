using Prism.Mvvm;

namespace Dopamine.Services.Entities
{
    public enum SmartPlaylistRuleFieldDataType
    {
        Text = 0,
        Numeric = 1,
        Boolean = 2
    }

    public class SmartPlaylistRuleFieldViewModel : BindableBase
    {
        private string displayName;
        private string name;
        private SmartPlaylistRuleFieldDataType dataType;

        public SmartPlaylistRuleFieldViewModel(string displayName, string name, SmartPlaylistRuleFieldDataType dataType)
        {
            this.displayName = displayName;
            this.name = name;
            this.dataType = dataType;
        }

        public string DisplayName
        {
            get { return this.displayName; }
            set { SetProperty<string>(ref this.displayName, value); }
        }

        public string QueryName
        {
            get { return this.name; }
            set { SetProperty<string>(ref this.name, value); }
        }

        public SmartPlaylistRuleFieldDataType DataType
        {
            get { return this.dataType; }
            set { SetProperty<SmartPlaylistRuleFieldDataType>(ref this.dataType, value); }
        }

        public override string ToString()
        {
            return this.displayName;
        }
    }
}