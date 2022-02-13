using Digimezzo.Foundation.Core.Utils;
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

        public string ArtistAndTitle
        {
            get
            {
                string artist = string.IsNullOrWhiteSpace(this.blacklistTrack.Artist) ? ResourceUtils.GetString("Language_Unknown_Artist") : this.blacklistTrack.Artist;
                string title = string.IsNullOrWhiteSpace(this.blacklistTrack.Title) ? ResourceUtils.GetString("Language_Unknown_Title") : this.blacklistTrack.Title;

                return $"{artist} - {title}";
            }
        }

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
