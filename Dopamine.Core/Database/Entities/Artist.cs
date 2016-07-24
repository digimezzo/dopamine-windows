using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dopamine.Core.Database.Entities
{
    public class Artist
    {
        #region Properties
        [Key()]
        public long ArtistID { get; set; }
        public string ArtistName { get; set; }
        #endregion

        #region ReadOnly Properties
        [NotMapped()]
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
