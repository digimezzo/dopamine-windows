using System.Collections.Generic;
using SQLite;

namespace Dopamine.Data.Entities
{
    public class Artist
    {
        [PrimaryKey()]
        public long ArtistID { get; set; }
        public string ArtistName { get; set; }

        [Ignore()]
        public string ArtistNameTrim
        {
            get { return ArtistName.Trim(); }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.ArtistNameTrim.Equals(((Artist)obj).ArtistNameTrim);
        }

        public override int GetHashCode()
        {
            return new { this.ArtistNameTrim }.GetHashCode();
        }

        public IList<Artist> ToList()
        {
            List<Artist> l = new List<Artist>();
            l.Add(this);

            return l;
        }
    }
}
