using System.Collections.Generic;
using SQLite;

namespace Dopamine.Core.Database.Entities
{
    public class Genre
    {
        #region Properties
        [PrimaryKey()]
        public long GenreID { get; set; }
        public string GenreName { get; set; }
        #endregion

        #region ReadOnly Properties
        [Ignore()]
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
