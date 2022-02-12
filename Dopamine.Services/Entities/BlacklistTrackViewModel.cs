using Dopamine.Data.Entities;
using Prism.Mvvm;

namespace Dopamine.Services.Entities
{
    public class BlacklistTrackViewModel : BindableBase
    {
        private BlacklistTrack blacklistTrack;

        public BlacklistTrackViewModel(BlacklistTrack blacklistTrack)
        {
            this.blacklistTrack = blacklistTrack;
        }

        public BlacklistTrack Blacklist => this.blacklistTrack;

        public string Path => this.blacklistTrack.Path;

        public string SafePath => this.blacklistTrack.SafePath;

        public long BlacklistTrackId => this.blacklistTrack.BlacklistTrackID;

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return string.Equals(this.SafePath, ((BlacklistTrackViewModel)obj).SafePath);
        }

        public override int GetHashCode()
        {
            return this.SafePath.GetHashCode();
        }

        public override string ToString()
        {
            return this.Path;
        }
    }
}
