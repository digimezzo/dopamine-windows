using System.Collections.Generic;
using SQLite;

namespace Dopamine.Core.Database.Entities
{
    public class Artist
    {
        #region Properties
        [PrimaryKey()]
        public long ArtistID { get; set; }
        public string ArtistName { get; set; }
        #endregion

        #region ReadOnly Properties
        [Ignore()]
        public string ArtistNameTrim
        {
            get { return ArtistName.Trim(); }
        }
        #endregion

        #region Overrides
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
        #endregion

        #region Public
        public IList<Artist> ToList()
        {
            List<Artist> l = new List<Artist>();
            l.Add(this);

            return l;
        }
        #endregion
    }
}
