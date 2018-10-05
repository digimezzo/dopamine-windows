using Dopamine.Core.IO;
using Prism.Mvvm;

namespace Dopamine.Services.Entities
{
    public class SmartPlaylistLimitViewModel : BindableBase
    {
        private SmartPlaylistLimitType type;
        private string displayName;
        private int value;

        public SmartPlaylistLimitType MyProperty
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
    }
}
