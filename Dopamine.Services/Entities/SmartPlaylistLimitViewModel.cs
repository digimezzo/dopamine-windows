using Dopamine.Core.IO;
using Prism.Mvvm;

namespace Dopamine.Services.Entities
{
    public class SmartPlaylistLimitViewModel : BindableBase
    {
        private SmartPlaylistLimitType type;
        private string displayName;
        private int value;
        private bool isEnabled;

        public SmartPlaylistLimitViewModel(string displayName, SmartPlaylistLimitType type, int value)
        {
            this.displayName = displayName.ToLower();
            this.type = type;
            this.value = value;
            this.isEnabled = false;
        }

        public SmartPlaylistLimitType Type
        {
            get { return this.type; }
            set { SetProperty<SmartPlaylistLimitType>(ref this.type, value); }
        }

        public string DisplayName
        {
            get { return this.displayName; }
            set { SetProperty<string>(ref this.displayName, value); }
        }

        public int Value
        {
            get { return this.value; }
            set { SetProperty<int>(ref this.value, value); }
        }

        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set { SetProperty<bool>(ref this.isEnabled, value); }
        }
    }
}
