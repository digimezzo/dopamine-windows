using Dopamine.Core.IO;
using Prism.Mvvm;

namespace Dopamine.Services.Entities
{
    public class SmartPlaylistLimitViewModel : BindableBase
    {
        private SmartPlaylistLimitType type;
        private int value;
        private bool isEnabled;

        public SmartPlaylistLimitViewModel(SmartPlaylistLimitType type, int value)
        {
            this.type = type;
            this.value = value;
        }

        public SmartPlaylistLimitType Type
        {
            get { return this.type; }
            set { SetProperty<SmartPlaylistLimitType>(ref this.type, value); }
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

        public SmartPlaylistLimit ToSmartPlaylistLimit()
        {
            return new SmartPlaylistLimit(this.type, this.isEnabled ? this.value : 0);
        }
    }
}
