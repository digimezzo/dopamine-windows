using Dopamine.Core.Database.Entities;
using System.Collections.Generic;

namespace Dopamine.Core.Database
{
    public class TrackInfo
    {
        #region Track
        public Track Track { get; set; }
        #endregion

        #region Artist
        public Artist Artist { get; set; }
        #endregion

        #region Genre
        public Genre Genre { get; set; }
        #endregion

        #region Album
        public Album Album { get; set; }
        #endregion

        #region Public
        public List<TrackInfo> ToList()
        {

            List<TrackInfo> l = new List<TrackInfo>();
            l.Add(this);

            return l;
        }
        #endregion

        #region Overrides
        public override int GetHashCode()
        {
            return this.Track.Path.ToLower().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            // We're on Windows, so we're not case sensitive
            return this.Track.Path.Equals(((TrackInfo)obj).Track.Path);
        }
        #endregion
    }
}
