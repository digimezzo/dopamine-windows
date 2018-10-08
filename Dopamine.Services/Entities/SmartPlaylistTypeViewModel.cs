using Dopamine.Core.IO;
using Prism.Mvvm;

namespace Dopamine.Services.Entities
{
    public class SmartPlaylistTypeViewModel : BindableBase
    {
        private SmartPlaylistLimitType type;
        private string displayName;

        public SmartPlaylistTypeViewModel(SmartPlaylistLimitType type, string displayName)
        {
            this.type = type;
            this.displayName = displayName;
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

        public override string ToString()
        {
            return this.DisplayName;
        }

    }
}
