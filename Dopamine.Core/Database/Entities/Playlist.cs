using System.Collections.Generic;
using SQLite.Net.Attributes;

namespace Dopamine.Core.Database.Entities
{
    public class Playlist
    {
        #region Properties
        [PrimaryKey(), AutoIncrement()]
        public long PlaylistID { get; set; }
        public string PlaylistName { get; set; }
        #endregion

        #region ReadOnly Properties
        [Ignore()]
        public string PlaylistNameTrim
        {
            get { return PlaylistName.Trim(); }
        }
        #endregion

        #region Override
        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.PlaylistNameTrim.Equals(((Playlist)obj).PlaylistNameTrim);
        }
        
        public override int GetHashCode()
        {
            return new { this.PlaylistNameTrim }.GetHashCode();
        }
        #endregion

        #region Public
        public IList<Playlist> ToList()
        {
            List<Playlist> l = new List<Playlist>();
            l.Add(this);

            return l;
        }
        #endregion
    }
}
