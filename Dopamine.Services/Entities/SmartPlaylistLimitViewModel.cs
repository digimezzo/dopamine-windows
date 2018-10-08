using Dopamine.Core.IO;
using Prism.Mvvm;

namespace Dopamine.Services.Entities
{
    public class SmartPlaylistLimitViewModel : BindableBase
    {
        public SmartPlaylistLimit limit { get; }

        public SmartPlaylistLimitViewModel(SmartPlaylistLimitType type, int value)
        {
            this.limit = new SmartPlaylistLimit(type, value);
            this.IsEnabled = false; // Explicitly disable limit for starters (it is enabled in the constructor)
        }

        public SmartPlaylistLimitType Type
        {
            get { return this.limit.Type; }
            set
            {
                this.limit.Type = value;
                this.RaisePropertyChanged(nameof(this.Type));
            }
        }

        public int Value
        {
            get { return this.limit.Value; }
            set
            {
                this.limit.Value = value;
                this.RaisePropertyChanged(nameof(this.Value));
            }
        }

        public bool IsEnabled
        {
            get { return this.limit.IsEnabled; }
            set
            {
                this.limit.IsEnabled = value;
                this.RaisePropertyChanged(nameof(this.IsEnabled));
            }
        }
    }
}
