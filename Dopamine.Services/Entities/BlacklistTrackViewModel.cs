using Dopamine.Data.Entities;
using Prism.Mvvm;

namespace Dopamine.Services.Entities
{
    public class BlacklistTrackViewModel : BindableBase
    {
        private BlacklistTrack blacklist;

        public BlacklistTrackViewModel(BlacklistTrack blacklist)
        {
            this.blacklist = blacklist;
        }

        public BlacklistTrack Blacklist => this.blacklist;

        public string Path => this.blacklist.Path;

        public string SafePath => this.blacklist.SafePath;

        public long BlacklistTrackId => this.blacklist.BlacklistTrackID;

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return string.Equals(this.SafePath, ((FolderViewModel)obj).SafePath);
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
