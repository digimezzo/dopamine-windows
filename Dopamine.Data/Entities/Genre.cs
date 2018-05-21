using System.Collections.Generic;
using SQLite;

namespace Dopamine.Data.Entities
{
    public class Genre
    {
        [PrimaryKey()]
        public long GenreID { get; set; }
        public string GenreName { get; set; }

        [Ignore()]
        public string GenreNameTrim
        {
            get { return GenreName.Trim(); }
        }

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
       
        public IList<Genre> ToList()
        {
            List<Genre> l = new List<Genre>();
            l.Add(this);

            return l;
        }
    }
}
