using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dopamine.Core.Database.Entities
{
    public class Genre
    {
        #region Properties
        [Key()]
        public long GenreID { get; set; }
        public string GenreName { get; set; }
        #endregion

        #region ReadOnly Properties
        [NotMapped()]
        public string GenreNameTrim
        {
            get { return GenreName.Trim(); }
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.GenreNameTrim.Equals(((Genre)obj).GenreNameTrim);
        }

        public override int GetHashCode()
        {
            return new { this.GenreNameTrim }.GetHashCode();
        }
        #endregion

        #region Public
        public IList<Genre> ToList()
        {
            List<Genre> l = new List<Genre>();
            l.Add(this);

            return l;
        }
        #endregion
    }
}
